using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.DataObjects;
using MyHomeSecureWeb.Repositories;
using MyHomeSecureWeb.Utilities;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace MyHomeSecureWeb.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    [GoogleAuthorisation(AuthorizationLevel.User)]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RequireHttps]
    public class LogController : ApiController
    {
        private IHomeHubRepository _homeHubRepository = new HomeHubRepository();
        private ILogRepository _logRepository = new LogRepository();
        private IPasswordHash _passwordHash = new PasswordHash();
        private ILookupToken _lookupToken = new LookupToken();

        // GET api/log
        public async Task<IHttpActionResult> GetLog(bool priority = true)
        {
            var hubId = await _lookupToken.GetHomeHubId(this.User);
            if (string.IsNullOrEmpty(hubId))
            {
                return Unauthorized();
            }

            return GetLogEntriesForHub(hubId, priority);
        }

        private IHttpActionResult GetLogEntriesForHub(string hubId, bool priority)
        {
            return Ok(_logRepository.GetLogEntries(hubId, priority)
                    .Select(l => new LogEntryResponse { Message = l.Message, Time = l.Time }));
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _homeHubRepository.Dispose();
                _logRepository.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
