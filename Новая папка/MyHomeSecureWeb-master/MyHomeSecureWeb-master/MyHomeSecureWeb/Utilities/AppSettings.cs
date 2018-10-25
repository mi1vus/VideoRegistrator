using System.Web.Configuration;

namespace MyHomeSecureWeb.Utilities
{
    public class AppSettings
    {
        public static string GetApplicationKey()
        {
            return WebConfigurationManager.AppSettings["ApplicationKey"];
        }
        public static string GetClientId()
        {
            return WebConfigurationManager.AppSettings["ClientId"];
        }
        public static string GetClientSecret()
        {
            return WebConfigurationManager.AppSettings["ClientSecret"];
        }
        public static string GetFirebaseServerKey()
        {
            return WebConfigurationManager.AppSettings["FirebaseServerKey"];
        }
    }
}
