using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.Utilities;
using MyHomeSecureWeb.WebSockets;
using System.Linq;
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
    public class UserAppController : ApiController
    {
        public ApiServices Services { get; set; }

        private ILookupToken _LookupToken = new LookupToken();

        [HttpGet]
        public async Task<HttpResponseMessage> Get()
        {
            var userEmail = await _LookupToken.GetEmailAddress(this.User);
            var homeHubId = _LookupToken.GetHomeHubIdFromEmail(userEmail);
            if (string.IsNullOrEmpty(homeHubId))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            HttpContext currentContext = HttpContext.Current;
            if (currentContext.IsWebSocketRequest ||
                currentContext.IsWebSocketRequestUpgrading)
            {
                currentContext.AcceptWebSocketRequest((context) => ProcessWSChat(context, homeHubId, userEmail));
                return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        private async Task ProcessWSChat(AspNetWebSocketContext context, string homeHubId, string userEmail)
        {
            WebSocket socket = context.WebSocket;
            using (var userAppSocket = new UserAppSocket(context.WebSocket, Services, homeHubId, userEmail))
            {
                await userAppSocket.Process();
            }
        }
    }
}
