using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.Models;
using MyHomeSecureWeb.Repositories;
using MyHomeSecureWeb.Utilities;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;

namespace MyHomeSecureWeb.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    [GoogleAuthorisation(AuthorizationLevel.User)]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RequireHttps]
    public class HubLocationController : ApiController
    {
        public ApiServices Services { get; set; }

        private IHomeHubRepository _homeHubRepository = new HomeHubRepository();
        private ILookupToken _lookupToken = new LookupToken();

        // GET: api/HubLocation
        [HttpGet]
        [ResponseType(typeof(HubLocation))]
        public async Task<IHttpActionResult> GetHubLocation()
        {
            Services.Log.Info("Getting hub location");

            var hubId = await _lookupToken.GetHomeHubId(this.User);
            if (string.IsNullOrEmpty(hubId))
            {
                Services.Log.Error("No hub for user", null, "HubLocation");
                return Unauthorized();
            }

            var hub = _homeHubRepository.GetHubById(hubId);

            return Ok(new HubLocation {
                HubId = hub.Id,
                Latitude = hub.Latitude,
                Longitude = hub.Longitude,
                Radius = hub.Radius
            });
        }
    }
}
