using Microsoft.WindowsAzure.Mobile.Service;
using MyHomeSecureWeb.Models;
using MyHomeSecureWeb.Repositories;
using MyHomeSecureWeb.Utilities;
using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading;
using System.Web.Http;

namespace MyHomeSecureWeb.WebSockets
{
    public class HomeHubSocket : SocketBase, IHomeHubSocket, IDisposable
    {
        private string _homeHubId;
        private CheckInOutMonitor _checkInOutMonitor;
        private ChatHub _chatHub;

        public HomeHubSocket(WebSocket socket, ApiServices services) : base(socket, services)
        {
            Debug.WriteLine("HomeHub Conection opened");
        }

        public ChatHub ChatHub
        {
            get
            {
                return _chatHub;
            }
        }

        public string HomeHubId
        {
            get
            {
                return _homeHubId;
            }
            set
            {
                StopMonitoringConnection();

                if (_checkInOutMonitor != null)
                {
                    _checkInOutMonitor.CheckInOut -= _checkInOutMonitor_CheckInOut;
                    _checkInOutMonitor.Dispose();
                    LogConnectionMessage("Hub has disconnected");
                }

                if (_chatHub != null)
                {
                    _chatHub.HomeMessage -= _chatHub_HomeMessage;
                    _chatHub.Dispose();
                }

                _homeHubId = value;

                _checkInOutMonitor = CheckInOutMonitor.Create(_homeHubId);
                _checkInOutMonitor.CheckInOut += _checkInOutMonitor_CheckInOut;

                _chatHub = ChatHub.Get(_homeHubId);
                _chatHub.HomeMessage += _chatHub_HomeMessage;

                using (var homeHubAwayChange = new HomeHubAwayChange(this))
                {
                    homeHubAwayChange.InitialiseHub();
                    LogConnectionMessage("Hub has connected");
                }

                _chatHub.MessageToClients(new HubConnectionStatus { Connected = true });
                StartMonitoringConnection();
            }
        }

        private void _chatHub_HomeMessage(SocketMessageBase message)
        {
            SendMessage(message);
        }

        private void _checkInOutMonitor_CheckInOut(string userName, bool away)
        {
            if (!string.IsNullOrEmpty(HomeHubId))
            {
                using (var homeHubAwayChange = new HomeHubAwayChange(this))
                {
                    homeHubAwayChange.UserCheckInOut(userName, away);
                }
            }
        }

        public override ISocketTarget CreateMessageInstance(Type type)
        {
            return Activator.CreateInstance(type, this) as ISocketTarget;
        }
        
        public virtual void Dispose()
        {
            Debug.WriteLine("HomeHub Conection closed");
            StopMonitoringConnection();

            if (_checkInOutMonitor != null)
            {
                _checkInOutMonitor.CheckInOut -= _checkInOutMonitor_CheckInOut;
                _checkInOutMonitor.Dispose();
            }

            if (_chatHub != null)
            {
                _chatHub.MessageToClients(new HubConnectionStatus { Connected = false });

                _chatHub.HomeMessage -= _chatHub_HomeMessage;
                _chatHub.Dispose();
            }

            LogConnectionMessage("Hub has disconnected");
        }

        private void LogConnectionMessage(string message)
        {
            if (!string.IsNullOrEmpty(HomeHubId))
            {
                using (var logRepository = new LogRepository())
                {
                    logRepository.Priority(HomeHubId, message);
                }
            }
        }

        private Timer _monitorTimer = null;
        private const int MonitorIntervalMS = 60 * 1000;

        private void StartMonitoringConnection()
        {
            _monitorTimer = new Timer(new TimerCallback(CheckConnectionStatus), null, MonitorIntervalMS, MonitorIntervalMS);
        }
        private void StopMonitoringConnection()
        {
            if (_monitorTimer != null)
            {
                _monitorTimer.Dispose();
                _monitorTimer = null;
            }
        }

        private void CheckConnectionStatus(object stateObj)
        {
            try
            {
                SendMessageRaw(new HubChangeStates { States = new HubChangeState[] { } });
            }
            catch(Exception ex)
            {
                Services.Log.Error("Error checking hub connection status", ex);
                Dispose();
            }
        }
    }
}
