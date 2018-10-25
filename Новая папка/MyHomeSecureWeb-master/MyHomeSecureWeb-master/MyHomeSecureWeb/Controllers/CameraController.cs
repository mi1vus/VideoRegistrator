using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.Utilities;
using MyHomeSecureWeb.WebSockets;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.WebSockets;

namespace MyHomeSecureWeb.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    [GoogleAuthorisation(AuthorizationLevel.User)]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RequireHttps]
    public class CameraController : ApiController
    {
        public ApiServices Services { get; set; }

        [HttpGet]
        public HttpResponseMessage Get()
        {
            HttpContext currentContext = HttpContext.Current;
            if (currentContext.IsWebSocketRequest ||
                currentContext.IsWebSocketRequestUpgrading)
            {

                currentContext.AcceptWebSocketRequest(ProcessWSChat, new AspNetWebSocketOptions { SubProtocol = "echo-protocol" });
                return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        private async Task ProcessWSChat(AspNetWebSocketContext context)
        {
            WebSocket socket = context.WebSocket;
            using (var cameraSocket = new CameraSocket(context.WebSocket, Services))
            {
                await cameraSocket.Process();
            }
        }
    }
}
