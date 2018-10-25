using MyHomeSecureWeb.Models;
using System.Diagnostics;
using MyHomeSecureWeb.Repositories;
using MyHomeSecureWeb.Utilities;
using System.Linq;
using System.Collections.Generic;

namespace MyHomeSecureWeb.WebSockets
{
    public class HomeHubInitialise : ISocketTarget
    {
        private IHomeHubSocket _homeHubSocket;
        private ILogRepository _logRepository = new LogRepository();
        private IHomeHubRepository _homeHubRepository = new HomeHubRepository();
        private IAwayStatusRepository _awayStatusRepository = new AwayStatusRepository();
        private IHubStateRepository _hubStateRepository = new HubStateRepository();
        private ICameraRepository _cameraRepository = new CameraRepository();
        private IPasswordHash _passwordHash = new PasswordHash();

        public HomeHubInitialise(IHomeHubSocket homeHubSocket)
        {
            _homeHubSocket = homeHubSocket;
        }

        public void Initialise(HubInitialiseRequest request)
        {
            if (string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Token))
            {
                return;
            }
            Debug.WriteLine(string.Format("Initialise hub: {0}", request.Name));

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
            else
            {
                // Create new hub
                var salt = _passwordHash.CreateSalt(32);
                var tokenHash = _passwordHash.Hash(request.Token, salt);
                hub = _homeHubRepository.AddHub(request.Name, tokenHash, salt);
            }

            // Update the location parameters
            _homeHubRepository.SetLocation(hub.Id, request.Latitude, request.Longitude, request.Radius);

            InitialiseUsers(hub.Id, request.Users);
            InitialiseStates(hub.Id, request.States);
            InitialiseCameras(hub.Id, request.Cameras);

            // Set the initialised home hub id
            _homeHubSocket.HomeHubId = hub.Id;
        }

        private void InitialiseUsers(string homeHubId, HubInitialiseUser[] users)
        {
            var hubUsers = _awayStatusRepository.GetAllForHub(homeHubId).ToList();

            foreach(var user in users)
            {
                if (!string.IsNullOrEmpty(user.Name) && !string.IsNullOrEmpty(user.Token))
                {
                    var existingUser = hubUsers.SingleOrDefault(u => u.UserName == user.Name);
                    if (existingUser != null)
                    {
                        hubUsers.Remove(existingUser);
                    }
                    else if (_awayStatusRepository.GetStatus(user.Name) == null)
                    {
                        _awayStatusRepository.AddUser(user.Name, homeHubId);
                    }
                }
            }

            // Remove unused users
            foreach(var user in hubUsers)
            {
                _awayStatusRepository.RemoveUser(user.UserName);
            }
        }

        private void InitialiseStates(string homeHubId, string[] states)
        {
            var hubStates = _hubStateRepository.GetAllForHub(homeHubId).ToList();

            foreach (var stateName in states)
            {
                if (!string.IsNullOrEmpty(stateName))
                {
                    var existingState = hubStates.SingleOrDefault(u => u.Name == stateName);
                    if (existingState != null)
                    {
                        hubStates.Remove(existingState);
                        //if (existingState.Active)
                        //{
                        //    _hubStateRepository.SetState(homeHubId, existingState.Name, false);
                        //}
                    }
                    else
                    {
                        _hubStateRepository.AddState(homeHubId, stateName);
                    }
                }
            }

            // Remove unused states
            foreach (var state in hubStates)
            {
                _hubStateRepository.RemoveState(homeHubId, state.Name);
            }
        }

        private void InitialiseCameras(string homeHubId, HubInitialiseCamera[] cameras)
        {
            var hubCameras = _cameraRepository.GetAllForHub(homeHubId).ToList();
            if (cameras != null)
            {
                foreach (var camera in cameras)
                {
                    if (!string.IsNullOrEmpty(camera.Name) && !string.IsNullOrEmpty(camera.Node))
                    {
                        var existingCamera = hubCameras.SingleOrDefault(c => c.Name == camera.Name);
                        if (existingCamera != null)
                        {
                            hubCameras.Remove(existingCamera);
                        }
                        else
                        {
                            _cameraRepository.AddCamera(camera.Name, camera.Node, homeHubId);
                        }
                    }
                }
            }

            // Remove unused cameras
            foreach (var camera in hubCameras)
            {
                _cameraRepository.RemoveCamera(camera.Name, homeHubId);
            }
        }

        public void Dispose()
        {
            _logRepository.Dispose();
            _homeHubRepository.Dispose();
            _awayStatusRepository.Dispose();
            _hubStateRepository.Dispose();
            _cameraRepository.Dispose();
        }
    }
}
