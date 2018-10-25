using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.DataObjects;
using MyHomeSecureWeb.Repositories;
using MyHomeSecureWeb.Utilities;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace MyHomeSecureWeb.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    [GoogleAuthorisation(AuthorizationLevel.User)]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RequireHttps]
    public class CameraListController : ApiController
    {
        private IHomeHubRepository _homeHubRepository = new HomeHubRepository();
        private ICameraRepository _cameraRepository = new CameraRepository();
        private IPasswordHash _passwordHash = new PasswordHash();
        private ILookupToken _lookupToken = new LookupToken();

        // GET api/cameralist
        public async Task<IHttpActionResult> GetCameras()
        {
            var hubId = await _lookupToken.GetHomeHubId(this.User);
            if (string.IsNullOrEmpty(hubId))
            {
                return Unauthorized();
            }

            return GetCamerasForHub(hubId);
        }

        private IHttpActionResult GetCamerasForHub(string hubId)
        {
            return Ok(_cameraRepository.GetAllForHub(hubId)
                    .Select(c => new HubCameraResponse { Name = c.Name, Node = c.Node }));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _homeHubRepository.Dispose();
                _cameraRepository.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
