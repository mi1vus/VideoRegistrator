using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.Repositories;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace MyHomeSecureWeb.Utilities
{
    public class LookupToken : ILookupToken
    {
        private const string GoogleTokenUrl = "https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={0}";

        private static String _clientId = AppSettings.GetClientId();

        public async Task<string> GetEmailAddress(IPrincipal user)
        {
            try
            {
                var google = Thread.CurrentPrincipal.Identity as GoogleAuthorisationIdentity;
                if (google != null)
                {
                    var cachedEmail = LookupEmailFromToken(google.AuthToken);
                    if (!string.IsNullOrEmpty(cachedEmail))
                    {
                        return cachedEmail;
                    }

                    var googleInfo = await GetProviderInfo(google.AuthToken);
                    var userEmail = googleInfo.Value<string>("email");

                    if (!string.IsNullOrEmpty(_clientId))
                    {
                        var aud = googleInfo.Value<string>("aud");
                        if (string.Compare(_clientId, aud) != 0)
                        {
                            // Not valid
                            return null;
                        }
                    }

                    await StoreToken(google.AuthToken, userEmail);

                    return userEmail;
                }
            }
            catch(HttpRequestException requestException)
            {
                // Swallow error and return null
            }
            return null;
        }

        public async Task<string> GetHomeHubId(IPrincipal user)
        {
            var emailAddress = await GetEmailAddress(user);

            return GetHomeHubIdFromEmail(emailAddress);
        }

        public string GetHomeHubIdFromEmail(string emailAddress)
        {
            if (emailAddress != null)
            {
                using (var awayStatusRepository = new AwayStatusRepository())
                {
                    var awayStatus = awayStatusRepository.GetStatus(emailAddress);
                    if (awayStatus != null)
                    {
                        return awayStatus.HomeHubId;
                    }
                }
            }
            return null;
        }

        private string LookupEmailFromToken(string authToken)
        {
            using (IAwayStatusRepository awayStatusRepository = new AwayStatusRepository())
            {
                var awayStatus = awayStatusRepository.LookupGoogleToken(authToken);
                return awayStatus != null ? awayStatus.UserName : null;
            }
        }
        private async Task StoreToken(string authToken, string emailAddress)
        {
            using (IAwayStatusRepository awayStatusRepository = new AwayStatusRepository())
            {
                await awayStatusRepository.SetGoogleTokenAsync(emailAddress, authToken);
            }
        }

        private async Task<JToken> GetProviderInfo(string authToken)
        {
            string url = string.Format(GoogleTokenUrl, authToken);
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return JToken.Parse(await response.Content.ReadAsStringAsync());
            }
        }
    }


}
