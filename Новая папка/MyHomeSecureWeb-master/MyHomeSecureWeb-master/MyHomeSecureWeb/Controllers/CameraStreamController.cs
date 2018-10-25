using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.Models;
using MyHomeSecureWeb.Repositories;
using MyHomeSecureWeb.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
    public class CameraStreamController : ApiController
    {
        public ApiServices Services { get; set; }

        private ILookupToken _lookupToken = new LookupToken();

        [HttpGet]
        public async Task<HttpResponseMessage> Get(string hubName, string node)
        {

            string hubId = await _lookupToken.GetHomeHubId(User);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new PushStreamContent(async (outputStream, httpContent, transportContext) =>
                {
                    Services.Log.Info(string.Format("Requesting camera stream from {0} - {1}", node, hubId));
                    using (var chatHub = ChatHub.Get(hubId))
                    {
                        chatHub.MessageToHome(new HubCameraCommand
                        {
                            Node = node,
                            Active = true,
                            Type = "h264"
                        });

                        try
                        {
                            var totalData = 0;
                            using (var videoHub = VideoHub.Get(hubId, node))
                            {
                                using (var videoWaitable = new VideoHubWaitable(videoHub))
                                {
                                    var moreData = true;
                                    while (moreData)
                                    {
                                        var videoData = await videoWaitable.WaitData();

                                        if (videoData.Length != 0)
                                        {
                                            totalData += videoData.Length;
                                            Debug.WriteLine(string.Format("uploaded: {0}", totalData));
                                            await outputStream.WriteAsync(videoData.Bytes, 0, videoData.Length);
                                        }
                                        else
                                        {
                                            moreData = false;
                                        }
                                    }
                                }
                            }
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

                            chatHub.MessageToHome(new HubCameraCommand
                            {
                                Node = node,
                                Active = false,
                                Type = "h264"
                            });
                        }
                    }
                }, new MediaTypeHeaderValue("video/webm")) // "text/plain"
            };

            return response;
        }
    }
}
