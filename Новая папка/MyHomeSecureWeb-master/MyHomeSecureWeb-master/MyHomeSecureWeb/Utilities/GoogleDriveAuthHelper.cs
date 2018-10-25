using Microsoft.WindowsAzure.Mobile.Service;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace MyHomeSecureWeb.Utilities
{
    public class GoogleDriveAuthHelper
    {
        public ApiServices Services { get; set; }
        public GoogleDriveAuthHelper(ApiServices services = null)
        {
            Services = services;
        }

        public async Task AccessDrive(string emailAddress, Func<string, Task> accessWithToken)
        {
            using (IGoogleDriveAuthorization driveAuth = new GoogleDriveAuthorization())
            {
                string accessToken = driveAuth.GetAccessToken(emailAddress);
                if (!string.IsNullOrEmpty(accessToken))
                {
                    try
                    {
                        await accessWithToken(accessToken);
                    }
                    catch (WebException ex)
                    {
                        var response = ex.Response as HttpWebResponse;
                        if (response != null && response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            try
                            {
                                accessToken = await driveAuth.RefreshAccessToken(emailAddress);

                                // Try again
                                await accessWithToken(accessToken);
                            }
                            catch (Exception e)
                            {
                                if (Services != null) Services.Log.Error("Error removing expired snapshots from Drive", e);
                            }
                        }
                        else
                        {
                            if (Services != null) Services.Log.Error("Error removing expired snapshots from Drive", ex);
                        }
                    }
                    catch (Exception e)
                    {
                        if (Services != null) Services.Log.Error("Error removing expired snapshots from Drive", e);
                    }
                }
            }
        }

        public async Task<T> AccessDrive<T>(string emailAddress, Func<string, Task<T>> accessWithToken)
        {
            using (IGoogleDriveAuthorization driveAuth = new GoogleDriveAuthorization())
            {
                string accessToken = driveAuth.GetAccessToken(emailAddress);
                if (!string.IsNullOrEmpty(accessToken))
                {
                    try
                    {
                        return await accessWithToken(accessToken);
                    }
                    catch (WebException ex)
                    {
                        var response = ex.Response as HttpWebResponse;
                        if (response != null && response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            try
                            {
                                accessToken = await driveAuth.RefreshAccessToken(emailAddress);

                                // Try again
                                return await accessWithToken(accessToken);
                            }
                            catch (Exception e)
                            {
                                if (Services != null) Services.Log.Error("Error removing expired snapshots from Drive", e);
                            }
                        }
                        else
                        {
                            if (Services != null) Services.Log.Error("Error removing expired snapshots from Drive", ex);
                        }
                    }
                    catch (Exception e)
                    {
                        if (Services != null) Services.Log.Error("Error removing expired snapshots from Drive", e);
                    }
                }
                return default(T);
            }
        }
    }
}
