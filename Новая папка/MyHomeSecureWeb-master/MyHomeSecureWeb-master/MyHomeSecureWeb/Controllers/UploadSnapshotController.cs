using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.Repositories;
using MyHomeSecureWeb.Utilities;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace MyHomeSecureWeb.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    [GoogleAuthorisation(AuthorizationLevel.Anonymous)]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RequireHttps]
    public class UploadSnapshotController : ApiController
    {
        public ApiServices Services { get; set; }

        private IHomeHubRepository _homeHubRepository = new HomeHubRepository();
        private IPasswordHash _passwordHash = new PasswordHash();

        // POST: api/AwayStatus
        [HttpPost]
        public async Task<IHttpActionResult> UploadSnapshot([FromUri]string hub, [FromUri]string token, [FromUri]string node)
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
                var bodyBytes = await Request.Content.ReadAsByteArrayAsync();
                videoHub.ReceivedData(bodyBytes, bodyBytes.Length);

                SnapshotArchiver.Queue(homeHub.Id, node, bodyBytes);
            }

            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            _homeHubRepository.Dispose();
            base.Dispose(disposing);
        }
    }
}
