using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.Repositories;
using MyHomeSecureWeb.Utilities;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System;
using NReco.VideoConverter;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

namespace MyHomeSecureWeb.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    [GoogleAuthorisation(AuthorizationLevel.Anonymous)]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RequireHttps]
    public class UploadStreamController : ApiController
    {
        public ApiServices Services { get; set; }

        private IHomeHubRepository _homeHubRepository = new HomeHubRepository();
        private IPasswordHash _passwordHash = new PasswordHash();

        // POST: api/AwayStatus
        [HttpPost]
        public async Task<IHttpActionResult> UploadStream([FromUri]string hub, [FromUri]string token, [FromUri]string node)
        {
            // Validate access to this hub
            var homeHub = _homeHubRepository.GetHub(hub);
            if (homeHub == null)
            {
                return NotFound();
            }

            var tokenHash = _passwordHash.Hash(token, homeHub.TokenSalt);
            if (!tokenHash.SequenceEqual(homeHub.TokenHash))
            {
                Services.Log.Error(string.Format("Invalid Token - {0}", token), null, "UploadSnapshot");
                return Unauthorized();
            }

            // Hand over the uploaded file data
            using (var videoHub = VideoHub.Get(homeHub.Id, node))
            {
                var requestStream = new VideoHubStream(homeHub.Id, node);
                await ConvertStream(Request.Content, requestStream);
            }
            
            return Ok();
        }

        private Task ConvertStream(HttpContent httpContent, Stream outputStream)
        {
            Task convertTask = new Task(() => {

                var convertSettings = new ConvertSettings {
                    CustomOutputArgs = "-map 0",
                    CustomInputArgs = "-vcodec h264"
                };

                var ffMpeg = new FFMpegConverter();
                ffMpeg.ConvertProgress += FfMpeg_ConvertProgress;
                ffMpeg.LogReceived += FfMpeg_LogReceived;

                //var task = ffMpeg.ConvertLiveMedia(Format.h264, "C:\\Work\\Test\\converted.avi", Format.avi, convertSettings);
                var task = ffMpeg.ConvertLiveMedia(Format.h264, outputStream, Format.mpeg, convertSettings);

                task.Start();

                var ffmpegStream = new FFMPegStream(task);
                var copyTask = httpContent.CopyToAsync(ffmpegStream);
                copyTask.Wait();
                ffmpegStream.Close();

                task.Wait();

                //                ffMpeg.ConvertMedia(@"C:\Work\Test\MyHomeSecureNode\devices\test\video.h264", "C:\\Work\\Test\\converted.avi", Format.avi);

                outputStream.Close();
            });

            convertTask.Start();
            return convertTask;
        }

        private void FfMpeg_ConvertProgress(object sender, ConvertProgressEventArgs e)
        {
            Debug.WriteLine(string.Format("ffmpeg progress: {0}", e.Processed.ToString()));
        }
        
        private void FfMpeg_LogReceived(object sender, FFMpegLogEventArgs e)
        {
            Debug.WriteLine(string.Format("ffmpeg: {0}", e.Data));
        }

        protected override void Dispose(bool disposing)
        {
            _homeHubRepository.Dispose();
            base.Dispose(disposing);
        }

        private class FFMPegStream : Stream
        {
            private ConvertLiveMediaTask mMediaTask;
            public FFMPegStream(ConvertLiveMediaTask mediaTask)
            {
                mMediaTask = mediaTask;
            }


            public override void Write(byte[] buffer, int offset, int count)
            {
                mMediaTask.Write(buffer, offset, count);
            }

            public override void Close()
            {
                mMediaTask.Stop();
            }

            public override bool CanRead
            {
                get
                {
                    return false;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return false;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return true;
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public override long Length
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override long Position
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            public override void Flush()
            {
                throw new NotImplementedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }
        }
        
        private class VideoHubStream : Stream
        {
            private VideoHub _videoHub;
            public VideoHubStream(string hubId, string node)
            {
                _videoHub = VideoHub.Get(hubId, node);
            }

            public override bool CanRead
            {
                get
                {
                    return false;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return false;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return true;
                }
            }

            public override long Length
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override long Position
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            public override void Flush()
            {
                throw new NotImplementedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                var copy = buffer.Skip(offset).ToArray();
                _videoHub.ReceivedData(copy, count);
            }

            public override void Close()
            {
                base.Close();
                if (_videoHub != null)
                {
                    _videoHub.Closed();
                    _videoHub.Dispose();
                    _videoHub = null;
                }
            }
        }
    }
}
