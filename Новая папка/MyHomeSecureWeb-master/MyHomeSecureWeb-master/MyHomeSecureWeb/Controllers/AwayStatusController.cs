using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using MyHomeSecureWeb.DataObjects;
using MyHomeSecureWeb.Models;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.Utilities;
using System.Web.Http.Cors;
using System;
using MyHomeSecureWeb.Repositories;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Mobile.Service;

namespace MyHomeSecureWeb.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    [GoogleAuthorisation(AuthorizationLevel.Anonymous)]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RequireHttps]
    public class AwayStatusController : ApiController
    {
        public ApiServices Services { get; set; }

        private IAwayStatusRepository _awayStatusRepository = new AwayStatusRepository();
        private ILogRepository _logRepository = new LogRepository();
        private IPasswordHash _passwordHash = new PasswordHash();
        private ILookupToken _lookupToken = new LookupToken();

        private static string ActionEntered = "entered";
        private static string ActionExited = "exited";

        // POST: api/AwayStatus
        [ResponseType(typeof(AwayStatus))]
        public async Task<IHttpActionResult> PostAwayStatus(AwayStatusRequest awayStatus)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(awayStatus.UserName) && string.IsNullOrEmpty(awayStatus.Token))
            {
                var emailAddress = await _lookupToken.GetEmailAddress(this.User);
                if (string.IsNullOrEmpty(emailAddress))
                {
                    Services.Log.Error("No logged in user", null, "AwayStatus");
                    return Unauthorized();
                }

                var existingEntry = _awayStatusRepository.GetStatus(emailAddress);
                if (existingEntry == null)
                {
                    Services.Log.Error(string.Format("No status found for user '{0}'", emailAddress), null, "AwayStatus");
                    return NotFound();
                }

                UpdateAwayStatus(existingEntry, awayStatus.Action);
            }
            else
            {
                AwayStatus existingEntry = existingEntry = _awayStatusRepository.GetStatus(awayStatus.UserName);
                if (existingEntry == null)
                {
                    Services.Log.Error(string.Format("No status found for user '{0}'", awayStatus.UserName), null, "AwayStatus");
                    return NotFound();
                }

                if (string.IsNullOrEmpty(awayStatus.Token))
                {
                    Services.Log.Error("Missing Token", null, "AwayStatus");
                    return Unauthorized();
                }

                var tokenHash = _passwordHash.Hash(awayStatus.Token, existingEntry.TokenSalt);
                if (!tokenHash.SequenceEqual(existingEntry.TokenHash))
                {
                    Services.Log.Error(string.Format("Invalid Token - {0}", awayStatus.Token), null, "AwayStatus");
                    return Unauthorized();
                }

                UpdateAwayStatus(existingEntry, awayStatus.Action);
            }


            return StatusCode(HttpStatusCode.NoContent);
        }

        private void UpdateAwayStatus(AwayStatus existingEntry, string awayStatusAction)
        {
            Services.Log.Info(string.Format("User '{0}' {1}", existingEntry.UserName, awayStatusAction), null, "AwayStatus");
            var newAwayStatus = string.Equals(awayStatusAction, ActionExited, StringComparison.OrdinalIgnoreCase);
            if (newAwayStatus != existingEntry.Away)
            {
                _awayStatusRepository.UpdateStatus(existingEntry.UserName, newAwayStatus);
                CheckInOutMonitor.UserInOut(existingEntry.HomeHubId, existingEntry.UserName, newAwayStatus);
                _logRepository.Priority(existingEntry.HomeHubId,
                            string.Format("{0} {1}", existingEntry.UserName, newAwayStatus ? ActionExited : ActionEntered));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _awayStatusRepository.Dispose();
                _logRepository.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}