using System.Security.Principal;
using System.Threading.Tasks;

namespace MyHomeSecureWeb.Utilities
{
    public interface ILookupToken
    {
        Task<string> GetEmailAddress(IPrincipal user);
        Task<string> GetHomeHubId(IPrincipal user);
        string GetHomeHubIdFromEmail(string emailAddress);
    }
}