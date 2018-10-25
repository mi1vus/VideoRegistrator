using Microsoft.WindowsAzure.Mobile.Service;
using MyHomeSecureWeb.Models;
using MyHomeSecureWeb.Repositories;
using MyHomeSecureWeb.Utilities;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;

namespace MyHomeSecureWeb.WebSockets
{
    public class UserAppSocket : SocketBase, IUserAppSocket, IDisposable
    {
        private ChatHub _chatHub;
        private CheckInOutMonitor _checkInOutMonitor;
        private string _userEmail;

        public UserAppSocket(WebSocket socket, ApiServices services, string homeHubId, string userEmail) : base(socket, services)
        {
            Debug.WriteLine("UserApp Conection opened");
            _userEmail = userEmail;

            _chatHub = ChatHub.Get(homeHubId);
            _chatHub.ClientMessage += _chatHub_ClientMessage;

            _checkInOutMonitor = CheckInOutMonitor.Create(homeHubId);
            _checkInOutMonitor.CheckInOut += _checkInOutMonitor_CheckInOut; ;

            SendInitialStates(homeHubId);
            SendInitialConnectionStatus();
            SendInitialUserInOut();
        }

        private void _checkInOutMonitor_CheckInOut(string userName, bool away)
        {
            bool currentUser = string.Equals(_userEmail, userName, StringComparison.OrdinalIgnoreCase);

            SendMessage(new UserCheckInOut { UserName = userName, CurrentUser = currentUser, Away = away });
        }

        private void _chatHub_ClientMessage(SocketMessageBase message)
        {
            SendMessage(message);
        }

        public override ISocketTarget CreateMessageInstance(Type type)
        {
            return Activator.CreateInstance(type, this) as ISocketTarget;
        }

        private void SendInitialStates(string homeHubId)
        {
            using (var hubStateRepository = new HubStateRepository())
            {
                var hubStates = hubStateRepository.GetAllForHub(homeHubId);
                var messageStates = new HubChangeStates {
                    States = hubStates.Select((s) => new HubChangeState {
                        Name = s.Name,
                        Active = s.Active
                    }).ToArray()
                };

                SendMessage(messageStates);
            }
        }

        private void SendInitialConnectionStatus()
        {
            SendMessage(new HubConnectionStatus { Connected = _chatHub.HubConnected });
        }

        private void SendInitialUserInOut()
        {
            using (IAwayStatusRepository awayStatusRepository = new AwayStatusRepository())
            {
                var status = awayStatusRepository.GetStatus(_userEmail);
                var away = status != null && status.Away;

                SendMessage(new UserCheckInOut { UserName = _userEmail, CurrentUser = true, Away = away });
            }
        }

        public void Dispose()
        {
            _checkInOutMonitor.CheckInOut -= _checkInOutMonitor_CheckInOut;
            _checkInOutMonitor.Dispose();

            _chatHub.ClientMessage -= _chatHub_ClientMessage;
            _chatHub.Dispose();
        }
    }
}
