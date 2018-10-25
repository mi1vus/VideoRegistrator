using MyHomeSecureWeb.Models;
using MyHomeSecureWeb.Repositories;
using MyHomeSecureWeb.Utilities;
using System.Diagnostics;
using System.Linq;

namespace MyHomeSecureWeb.WebSockets
{
    public class HomeHubReconnect : ISocketTarget
    {
        private IHomeHubSocket _homeHubSocket;
        private ILogRepository _logRepository = new LogRepository();
        private IHomeHubRepository _homeHubRepository = new HomeHubRepository();
        private IPasswordHash _passwordHash = new PasswordHash();

        public HomeHubReconnect(IHomeHubSocket homeHubSocket)
        {
            _homeHubSocket = homeHubSocket;
        }

        public void Initialise(HubReconnectRequest request)
        {
            if (string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Token))
            {
                return;
            }
            Debug.WriteLine(string.Format("Reconnect hub: {0}", request.Name));

            var hub = _homeHubRepository.GetHub(request.Name);
            if (hub != null)
            {
                // Validate access to hub
                var tokenHash = _passwordHash.Hash(request.Token, hub.TokenSalt);
                if (!tokenHash.SequenceEqual(hub.TokenHash))
                {
                    _logRepository.Error(hub.Id, "Attempt to access hub with invalid token");
                    return;
                }
            }

            // Set the initialised home hub id
            _homeHubSocket.HomeHubId = hub.Id;
        }

        public void Dispose()
        {
            _logRepository.Dispose();
            _homeHubRepository.Dispose();
        }
    }
}
