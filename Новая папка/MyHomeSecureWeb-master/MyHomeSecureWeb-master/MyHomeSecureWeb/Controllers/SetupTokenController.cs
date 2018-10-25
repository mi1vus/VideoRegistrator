using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.DataObjects;
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
    public class SetupTokenController : ApiController
    {
        public ApiServices Services { get; set; }

        private IAwayStatusRepository _awayStatusRepository = new AwayStatusRepository();
        private IPasswordHash _passwordHash = new PasswordHash();
        private ILookupToken _lookupToken = new LookupToken();

        // POST: api/AwayStatus
        [HttpPost]
        [ResponseType(typeof(SetupTokenResponse))]
        public async Task<IHttpActionResult> RequestNewToken()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var emailAddress = await _lookupToken.GetEmailAddress(this.User);
            if (string.IsNullOrEmpty(emailAddress))
            {
                Services.Log.Error("No logged in user", null, "SetupToken");
                return Unauthorized();
            }

            // Create the new token
            var salt = _passwordHash.CreateSalt(32);
            var token = _passwordHash.CreateToken(128);

            // Update the user's token
            _awayStatusRepository.SetToken(emailAddress, _passwordHash.Hash(token, salt), salt);

            Services.Log.Info(string.Format("Assigned new token to user {0}", emailAddress), null, "SetupToken");

            return Ok(new SetupTokenResponse {
                UserName = emailAddress,
                Token = token
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _awayStatusRepository.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
