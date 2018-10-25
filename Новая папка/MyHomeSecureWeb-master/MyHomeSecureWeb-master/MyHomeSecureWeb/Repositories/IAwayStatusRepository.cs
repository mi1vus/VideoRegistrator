using MyHomeSecureWeb.DataObjects;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyHomeSecureWeb.Repositories
{
    public interface IAwayStatusRepository: IDisposable
    {
        AwayStatus GetStatus(string userName);
        void UpdateStatus(string userName, bool away);
        void SetToken(string userName, byte[] tokenHash, byte[] salt);
        void AddUser(string userName, string homeHubId);
        void RemoveUser(string userName);
        IQueryable<AwayStatus> GetAllForHub(string homeHubId);
        IQueryable<AwayStatus> GetAll();

        Task SetGoogleTokenAsync(string userName, string googleToken);
        AwayStatus LookupGoogleToken(string googleToken);
        Task SetDriveTokensAsync(string userName, string accessToken, string refreshToken = null);
    }
}