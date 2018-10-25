using MyHomeSecureWeb.Models;
using MyHomeSecureWeb.Repositories;
using MyHomeSecureWeb.Utilities;
using System.Diagnostics;
using System.Linq;

namespace MyHomeSecureWeb.WebSockets
{
    public class CameraInitialise : ISocketTarget
    {
        private ICameraSocket _cameraSocket;
        private ILogRepository _logRepository = new LogRepository();
        private IHomeHubRepository _homeHubRepository = new HomeHubRepository();
        private IPasswordHash _passwordHash = new PasswordHash();

        public CameraInitialise(ICameraSocket cameraSocket)
        {
            _cameraSocket = cameraSocket;
        }

        public void Initialise(CameraInitialiseRequest request)
        {
            if (string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Token))
            {
                return;
            }
            Debug.WriteLine(string.Format("Initialising camera at hub: {0}", request.Name));

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
            _cameraSocket.initialise(hub.Id, request.Node);
        }

        public void Dispose()
        {
            _logRepository.Dispose();
            _homeHubRepository.Dispose();
        }
    }
}