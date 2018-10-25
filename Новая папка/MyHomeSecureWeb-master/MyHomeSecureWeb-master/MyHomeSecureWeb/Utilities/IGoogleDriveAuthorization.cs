using System;
using System.Threading.Tasks;

namespace MyHomeSecureWeb.Utilities
{
    public interface IGoogleDriveAuthorization : IDisposable
    {
        string GetAccessToken(string emailAddress);
        Task<string> RefreshAccessToken(string emailAddress);
    }
}