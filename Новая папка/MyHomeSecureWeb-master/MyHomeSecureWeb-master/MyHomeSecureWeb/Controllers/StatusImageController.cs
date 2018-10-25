using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.Models;
using MyHomeSecureWeb.Repositories;
using MyHomeSecureWeb.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace MyHomeSecureWeb.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    [GoogleAuthorisation(AuthorizationLevel.User)]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RequireHttps]
    public class StatusImageController : ApiController
    {
        public ApiServices Services { get; set; }
        private ILookupToken _lookupToken = new LookupToken();

        private static readonly string[] ActiveKeyWords = new[] { "active", "on" };

        private const string ImagesFolder = "images";

        [HttpGet]
        [Route("api/statusimage")]
        public async Task<IHttpActionResult> GetList()
        {
            var userEmails = await UsersForHub();
            if (userEmails == null)
            {
                Services.Log.Error("No logged in user", null, "SetupToken");
                return Unauthorized();
            }

            var imageList = new List<StatusImageInfo>();
            foreach(var email in userEmails)
            {
                var userImages = await GetStatusImagesForUser(email);
                if (userImages != null)
                {
                    imageList.AddRange(userImages);
                }
            }

            return Ok(imageList.ToArray());
        }
        
        [HttpGet]
        [Route("api/statusimage/{name}")]
        public async Task<HttpResponseMessage> GetImage(string name)
        {
            var userEmails = await UsersForHub();
            if (userEmails == null)
            {
                Services.Log.Error("No logged in user", null, "SetupToken");
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            byte[] imageBytes = null;
            foreach (var email in userEmails)
            {
                if (imageBytes == null)
                {
                    imageBytes = await GetStatusImageBytesForUser(email, name);
                }
            }

            if (imageBytes != null)
            {
                var content = new ByteArrayContent(imageBytes);
                content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

                return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
            }
            else
            {
                Services.Log.Error(string.Format("Status image {0} not found", name));
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }

        private async Task<string[]> UsersForHub()
        {
            var hubId = await _lookupToken.GetHomeHubId(this.User);
            if (string.IsNullOrEmpty(hubId))
            {
                return null;
            }

            using (IAwayStatusRepository awayStatusRepository = new AwayStatusRepository())
            {
                var users = awayStatusRepository.GetAllForHub(hubId);
                return users.Select(u => u.UserName).ToArray();
            }
        }


        private async Task<StatusImageInfo[]> GetStatusImagesForUser(string emailAddress)
        {
            return await new GoogleDriveAuthHelper(Services).AccessDrive(emailAddress, GetStatusImagesWithAccessToken);
        }

        private async Task<StatusImageInfo[]> GetStatusImagesWithAccessToken(string accessToken)
        {
            IGoogleDriveHelper driveHelper = new GoogleDriveHelper();

            // Get the "images" folder, if it exists
            var imagesFolderId = await GetImageFolderId(accessToken);
            if (!string.IsNullOrEmpty(imagesFolderId))
            {
                var imageFiles = await driveHelper.GetChildrenIDs(accessToken, imagesFolderId);

                return imageFiles.Items.Select(ToImageInfo).ToArray();
            }

            return null;
        }

        private async Task<byte[]> GetStatusImageBytesForUser(string emailAddress, string name)
        {
            return await new GoogleDriveAuthHelper(Services).AccessDrive(emailAddress, (accessToken) => 
                    GetStatusImageBytesWithAccessToken(accessToken, name));
        }

        private async Task<byte[]> GetStatusImageBytesWithAccessToken(string accessToken, string name)
        {
            IGoogleDriveHelper driveHelper = new GoogleDriveHelper();
            // Get the "images" folder, if it exists
            var imagesFolderId = await GetImageFolderId(accessToken);
            if (!string.IsNullOrEmpty(imagesFolderId))
            {
                var query = string.Format("title = '{0}.png' and '{1}' in parents", name, imagesFolderId);

                var searchResult = await driveHelper.Search(accessToken, query);
                if (searchResult.Items.Length > 0)
                {
                    var imageId = searchResult.Items[0].Id;
                    return await driveHelper.GetFileContent(accessToken, imageId);
                }

            }
            return null;
        }

        private async Task<string> GetImageFolderId(string accessToken)
        {
            IGoogleDriveHelper driveHelper = new GoogleDriveHelper();
            var cacheKey = string.Format("statusimage|{0}", accessToken);

            var cachedId = HttpContext.Current.Cache[cacheKey] as string;
            if (!string.IsNullOrEmpty(cachedId))
            {
                return cachedId;
            }

            var lookupId = await driveHelper.GetFolderId(accessToken, ImagesFolder);

            HttpContext.Current.Cache[cacheKey] = lookupId;
            return lookupId;
        }

        private StatusImageInfo ToImageInfo(GoogleSearchItem result)
        {
            var fileName = result.Title.IndexOf('.') != -1 ? result.Title.Substring(0, result.Title.IndexOf('.')) : result.Title;
            var nameParts = fileName.Split('-');

            var state = nameParts[0];
            var active = nameParts.Length > 1 && ActiveKeyWords.Contains(nameParts[1].ToLowerInvariant());
            var zIndex = 0;
            if (nameParts.Length > 2)
            {
                int.TryParse(nameParts[2], out zIndex);
            }

            return new StatusImageInfo
            {
                FileName = fileName,
                State = state,
                Active = active,
                Updated = result.ModifiedDate,
                ZIndex = zIndex
            };
        }
    }
}
