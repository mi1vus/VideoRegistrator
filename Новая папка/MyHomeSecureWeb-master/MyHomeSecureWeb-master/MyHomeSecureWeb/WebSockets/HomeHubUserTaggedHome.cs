using MyHomeSecureWeb.DataObjects;
using MyHomeSecureWeb.Models;
using MyHomeSecureWeb.Repositories;
using MyHomeSecureWeb.Utilities;

namespace MyHomeSecureWeb.WebSockets
{
    public class HomeHubUserTaggedHome : ISocketTarget
    {
        private IHomeHubSocket _homeHubSocket;
        private IAwayStatusRepository _awayStatusRepository = new AwayStatusRepository();
        private ILogRepository _logRepository = new LogRepository();

        private string[] _priorityStates = new[] { "Away", "Alert", "Alarm" };

        public HomeHubUserTaggedHome(IHomeHubSocket homeHubSocket)
        {
            _homeHubSocket = homeHubSocket;
        }

        public void UserTaggedHome(HubUserTagged userTagged)
        {
            if (!string.IsNullOrEmpty(_homeHubSocket.HomeHubId))
            {
                AwayStatus existingEntry = _awayStatusRepository.GetStatus(userTagged.UserName);
                if (string.Equals(existingEntry.HomeHubId, _homeHubSocket.HomeHubId)
                        && existingEntry.Away)
                {
                    _awayStatusRepository.UpdateStatus(existingEntry.UserName, false);
                    CheckInOutMonitor.UserInOut(existingEntry.HomeHubId, existingEntry.UserName, false);
                    _logRepository.Priority(existingEntry.HomeHubId,
                                string.Format("{0} token", existingEntry.UserName));
                }

                _logRepository.Info(_homeHubSocket.HomeHubId, "");
                        
            }
        }

        public void Dispose()
        {
            _awayStatusRepository.Dispose();
            _logRepository.Dispose();
        }
    }
}
