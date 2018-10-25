using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.Models;
using MyHomeSecureWeb.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace MyHomeSecureWeb.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    [GoogleAuthorisation(AuthorizationLevel.User)]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RequireHttps]
    public class CameraSnapshotController : ApiController
    {
        public ApiServices Services { get; set; }

        private ILookupToken _lookupToken = new LookupToken();

        private const int LongTimelapse = 10000;
        private const int ShortTimelapse = 1000;
        private const int ThumbnailWidth = 512;

        [HttpGet]
        public async Task<HttpResponseMessage> Get(string node, bool thumbnail = false, bool singleImage = false)
        {
            string hubId = await _lookupToken.GetHomeHubId(User);

            if (string.IsNullOrEmpty(hubId))
            {
                Services.Log.Error("No logged in user", null, "CameraSnapshot");
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new PushStreamContent(async (outputStream, httpContent, transportContext) =>
                {
                    try
                    {
                        await PipeSnapshotImage(hubId, node, outputStream, thumbnail, singleImage);
                    }
                    catch (HttpException ex)
                    {
                        if (ex.ErrorCode == -2147023667) // The remote host closed the connection. 
                        {
                            return;
                        }
                    }
                    finally
                    {
                        // Close output stream as we are done
                        outputStream.Close();
                    }
                }, new MediaTypeHeaderValue("image/jpeg"))
            };

            return response;
        }

        private async Task TestSnapshotImage(string hubId, string node, Stream outputStream)
        {
            var filePath = HttpContext.Current.Server.MapPath(@"~/test.jpg");
            var filePath2 = @"D:\home\site\wwwroot\test.jpg";
            byte[] videoData = File.ReadAllBytes(filePath2);

            await outputStream.WriteAsync(videoData, 0, videoData.Length);
        }

        private async Task PipeSnapshotImage(string hubId, string node, Stream outputStream, bool thumbnail, bool singleImage)
        {
            int duration = thumbnail || singleImage ? ShortTimelapse : LongTimelapse;
            CameraActivator.Trigger(hubId, node, duration);

            using (var videoHub = VideoHub.Get(hubId, node))
            {
                using (var videoWaitable = new VideoHubWaitable(videoHub, true))
                {
                    var imageSize = 0;
                    var videoData = await videoWaitable.WaitData();

                    if (videoData.Length != 0 &&
                            !(videoData.Length == 1 && videoData.Bytes[0] == 0))
                    {
                        var videoBytes = videoData.Bytes;
                        var videoLength = videoData.Length;
                        if (thumbnail)
                        {
                            videoBytes = CreateThumbnail(videoBytes, videoLength);
                            videoLength = videoBytes.Length;
                        }

                        await outputStream.WriteAsync(videoBytes, 0, videoLength);
                        imageSize += videoData.Length;

                        CameraActivator.Received(hubId, node);
                    }
//                    Services.Log.Info(string.Format("Sent camera snapshot with {0} bytes", imageSize));
                }
            }
        }

        private byte[] CreateThumbnail(byte[] source, int sourceLength)
        {
            Image sourceImage = null;
            using (var ms = new MemoryStream(source, 0, sourceLength))
            {
                sourceImage = Image.FromStream(ms);
            }
            var sWidth = sourceImage.Width;
            var sHeight = sourceImage.Height;

            var tWidth = ThumbnailWidth;
            var tHeight = (int)((float)tWidth * ((float)sHeight / (float)sWidth));

            var targetImage = sourceImage.GetThumbnailImage(tWidth, tHeight, () => { return true; }, IntPtr.Zero);

            using (var ms = new MemoryStream())
            {
                targetImage.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        private class CameraActivator
        {
            private static Dictionary<string, CameraActivator> _activators = new Dictionary<string, CameraActivator>();
            public static void Trigger(string hub, string node, int duration = 10000)
            {
                var id = Id(hub, node);
                if (!_activators.ContainsKey(id))
                {
                    _activators[id] = new CameraActivator(hub, node);
                }
                _activators[id].Trigger(duration);
            }
            public static void Received(string hub, string node)
            {
                var id = Id(hub, node);
                if (_activators.ContainsKey(id))
                {
                    _activators[id].Received();
                }
            }
            private static string Id(string hub, string node)
            {
                return string.Format("{0}|{1}", hub, node);
            }

            private ChatHub _chatHub;
            private string _hub;
            private string _node;
            private Deactivator _deactivator;

            private bool _received;
            private bool _timedOut;
            private DateTime _timeoutTime;

            protected CameraActivator(string hub, string node)
            {
                _chatHub = ChatHub.Get(hub);
                _hub = hub;
                _node = node;

                _chatHub.MessageToHome(new HubCameraCommand
                {
                    Node = _node,
                    Active = true,
                    Type = "timelapse"
                });
            }

            public void Trigger(int duration)
            {
                _received = false;
                _timedOut = false;
                if (_deactivator != null)
                {
                    _deactivator.Cancelled = true;
                }
                _deactivator = new Deactivator(duration, TimedOut);
            }

            public void Received()
            {
                if (_timedOut)
                {
                    if (_deactivator != null)
                    {
                        _deactivator.Cancelled = true;
                    }
                    Deactivate();
                }
                else
                {
                    _received = true;
                }
            }

            private void TimedOut()
            {
                if (_received)
                {
                    Deactivate();
                }
                else
                {
                    if (!_timedOut)
                    {
                        _timedOut = true;
                        _timeoutTime = DateTime.Now;
                    }

                    if (DateTime.Now.Subtract(_timeoutTime) > TimeSpan.FromSeconds(10))
                    {
                        Deactivate();
                    }
                    else
                    {
                        _deactivator = new Deactivator(ShortTimelapse, TimedOut);
                    }
                }
            }

            private void Deactivate()
            {
                _activators.Remove(Id(_hub, _node));
                _chatHub.MessageToHome(new HubCameraCommand
                {
                    Node = _node,
                    Active = false,
                    Type = "timelapse"
                });
            }

            private class Deactivator
            {
                public bool Cancelled { get; set; }
                public Deactivator(int duration, Action callback)
                {
                    new Thread(new ThreadStart(() =>
                    {
                        Thread.Sleep(duration);
                        if (!Cancelled)
                        {
                            callback();
                        }
                    })).Start();
                }
            }
        }

    }
}
