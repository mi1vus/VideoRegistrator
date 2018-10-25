using MyHomeSecureWeb.Models;
using MyHomeSecureWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHomeSecureWeb.WebSockets
{
    public class HomeHubAwayChange : IDisposable
    {
        private IHomeHubSocket _homeHubSocket;

        private AwayStatusRepository _awayStatusRepository = new AwayStatusRepository();

        public HomeHubAwayChange(IHomeHubSocket homeHubSocket)
        {
            _homeHubSocket = homeHubSocket;
        }

        public void InitialiseHub()
        {
            var allStates = _awayStatusRepository.GetAllForHub(_homeHubSocket.HomeHubId).ToList();
            if (allStates.Count > 0)
            {
                if (allStates.All(s => s.Away))
                {
                    _homeHubSocket.SendMessage(new HubLastUserAway { UserName = allStates[0].UserName });
                }
                else
                {
                    _homeHubSocket.SendMessage(new HubFirstUserHome { UserName = allStates.First(s => !s.Away).UserName });
                }
            }
        }

        public void UserCheckInOut(string userName, bool away)
        {
            var allStates = _awayStatusRepository.GetAllForHub(_homeHubSocket.HomeHubId).ToList();
            if (away && allStates.All(s => s.Away))
            {
                _homeHubSocket.SendMessage(new HubLastUserAway { UserName = userName });
            }
            else if (!away && allStates.Count(s => !s.Away) == 1)
            {
                _homeHubSocket.SendMessage(new HubFirstUserHome { UserName = userName });
            }
        }

        public void Dispose()
        {
            _awayStatusRepository.Dispose();
        }
    }
}
