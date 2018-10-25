using MyHomeSecureWeb.Models;
using MyHomeSecureWeb.Repositories;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyHomeSecureWeb.Utilities
{
    public class GoogleDriveAuthorization : IGoogleDriveAuthorization
    {
        private IAwayStatusRepository _awayStatusRepository = new AwayStatusRepository();

        private const string _refreshTokenAddress = "https://www.googleapis.com/oauth2/v4/token";
        private const string _refreshTokenBodyTemplate = "client_id={0}&client_secret={1}&refresh_token={2}&grant_type=refresh_token";

        public string GetAccessToken(string emailAddress)
        {
            var user = _awayStatusRepository.GetStatus(emailAddress);
            if (user != null)
            {
                return user.DriveAccessToken;
            }
            return null;
        }

        public async Task<string> RefreshAccessToken(string emailAddress)
        {
            var user = _awayStatusRepository.GetStatus(emailAddress);
            if (user == null) return null;

            var postData = string.Format(_refreshTokenBodyTemplate, AppSettings.GetClientId(),
                    AppSettings.GetClientSecret(), user.DriveRefreshToken);
            var byteArray = Encoding.UTF8.GetBytes(postData);

            // Get the response
            var request = WebRequest.Create(_refreshTokenAddress);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            request.Method = "POST";

            using (var writeStream = await request.GetRequestStreamAsync())
            {
                writeStream.Write(byteArray, 0, byteArray.Length);
            }

            var response = await request.GetResponseAsync();

            string responseContent = null;
            using (var stream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    responseContent = await reader.ReadToEndAsync();
                }
            }

            // Deserialise for the access token and renewal token
            var token = JsonConvert.DeserializeObject<GoogleAccessToken>(responseContent);

            await _awayStatusRepository.SetDriveTokensAsync(emailAddress, token.AccessToken, user.DriveRefreshToken);

            return token.AccessToken;
        }

        public void Dispose()
        {
            _awayStatusRepository.Dispose();
        }

    }
}
