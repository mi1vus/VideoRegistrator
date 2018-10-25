using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.Notifications;
using MyHomeSecureWeb.Utilities;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Web.Http;

namespace MyHomeSecureWeb.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    [GoogleAuthorisation(AuthorizationLevel.Anonymous)]
    public class TestController : ApiController
    {
        public ApiServices Services { get; set; }

        //private static string TestHubId = "<hub-id>";
        //private ILookupToken _lookupToken = new LookupToken();

        //[HttpGet]
        //[Route("api/test/notify")]
        //public async Task<IHttpActionResult> notify(string state, bool active, string email = null)
        //{
        //    var hubId = !string.IsNullOrEmpty(email)
        //        ? _lookupToken.GetHomeHubIdFromEmail(email)
        //        : TestHubId;

        //    var statusNotification = new StateNotification(Services);
        //    await statusNotification.Send(hubId, state, active, "garage", "rule");

        //    var message = JsonConvert.SerializeObject(new StatusMessage
        //    {
        //        Message = "StateNotification",
        //        HomeHubId = TestHubId,
        //        State = state,
        //        Active = active,
        //        Node = "garage",
        //        Rule = "rule"
        //    });
        //    return Ok(string.Format("Sent {0}", message));
        //}
    }
}
