using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.Utilities;
using System.Threading.Tasks;
using System.Web.Http;

namespace MyHomeSecureWeb.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    [GoogleAuthorisation(AuthorizationLevel.Anonymous)]
    public class StaticController : ApiController
    {
        [HttpGet]
        [Route("api/css/{*path}")]
        public IHttpActionResult Css(string path)
        {
            if (path.Contains(@"\\") || path.Contains(@"..") || path.Contains(@":"))
            {
                return InternalServerError();
            }
            return new HtmlActionResult(path + ".css", "text/css");
        }
    }
}
