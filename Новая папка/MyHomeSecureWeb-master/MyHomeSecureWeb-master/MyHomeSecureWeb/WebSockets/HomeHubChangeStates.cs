using System;
using System.Linq;
using MyHomeSecureWeb.Models;
using MyHomeSecureWeb.Repositories;
using MyHomeSecureWeb.Notifications;
using System.Collections.Generic;
using System.Threading;

namespace MyHomeSecureWeb.WebSockets
{
    public class HomeHubChangeStates : ISocketTarget
    {
        private IHomeHubSocket _homeHubSocket;
        private IHubStateRepository _hubStateRepository = new HubStateRepository();
        private ILogRepository _logRepository = new LogRepository();
        private IStateNotification _statusNotification;

        private string[] _priorityStates = new[] { "Away", "Alert", "Alarm" };
        private Dictionary<string, string> _alertStates = new Dictionary<string, string> { { "Alert", "Alarm" } };
        private string[] _notificationStates = new[] { "Away", "Alert", "Alarm" };

        private const int AlertToAlarmMS = 45000;

        public HomeHubChangeStates(IHomeHubSocket homeHubSocket)
        {
            _homeHubSocket = homeHubSocket;
            _statusNotification = new StateNotification(homeHubSocket.Services);
        }

        public void ChangeStates(HubChangeStates states)
        {
            if (!string.IsNullOrEmpty(_homeHubSocket.HomeHubId))
            {
                foreach(var state in states.States)
                {
                    var changed = _hubStateRepository.SetState(_homeHubSocket.HomeHubId, state.Name, state.Active);

                    if (changed)
                    {
                        if (_priorityStates.Contains(state.Name))
                        {
                            string logMessage = string.Format("{0} changed to {1}", state.Name, state.Active ? "Active" : "Inactive");
                            if (!string.IsNullOrEmpty(state.Node))
                            {
                                logMessage = string.Format("{0} > {1}", logMessage, state.Node);
                            }
                            if (!string.IsNullOrEmpty(state.Rule))
                            {
                                logMessage = string.Format("{0} | {1}", logMessage, state.Rule);
                            }

                            _logRepository.Priority(_homeHubSocket.HomeHubId, logMessage);
                        }

                        if (_alertStates.Keys.Contains(state.Name))
                        {
                            ClearTimerUpdates();
                            if (state.Active)
                            {
                                var timerStates = new HubChangeStates
                                {
                                    States = new HubChangeState[] {
                                    new HubChangeState { Name = state.Name, Active = false },
                                    new HubChangeState { Name = _alertStates[state.Name], Active = true }
                                }
                                };

                                AddTimerUpdate(timerStates, AlertToAlarmMS);
                            }
                        }

                        if (_notificationStates.Contains(state.Name))
                        {
                            // Send a notification to devices
                            _statusNotification.Send(_homeHubSocket.HomeHubId, state.Name, state.Active, state.Node, state.Rule);
                        }
                    }
                }

                _homeHubSocket.ChatHub.MessageToClients(states);
            }
        }

        private static Dictionary<string, ChangeStatesTimerUpdate> _timerUpdates = new Dictionary<string, ChangeStatesTimerUpdate>();
        private void ClearTimerUpdates()
        {
            lock(_timerUpdates)
            {
                if (_timerUpdates.ContainsKey(_homeHubSocket.HomeHubId))
                {
                    _timerUpdates[_homeHubSocket.HomeHubId].Cancel();
                    _timerUpdates.Remove(_homeHubSocket.HomeHubId);
                }
            }
        }

        private void AddTimerUpdate(HubChangeStates states, int milliseconds)
        {
            lock(_timerUpdates)
            {
                _timerUpdates[_homeHubSocket.HomeHubId] =
                        new ChangeStatesTimerUpdate(_homeHubSocket, states, milliseconds);
            }
        }

        public void Dispose()
        {
            _hubStateRepository.Dispose();
            _logRepository.Dispose();
        }

        private class ChangeStatesTimerUpdate
        {
            private IHomeHubSocket _homeHubSocket;
            private HubChangeStates _states;

            private Timer _timer;

            public ChangeStatesTimerUpdate(IHomeHubSocket homeHubSocket, HubChangeStates states, int milliseconds)
            {
                _homeHubSocket = homeHubSocket;
                _states = states;

                _timer = new Timer(new TimerCallback(Execute), null, milliseconds, Timeout.Infinite);
            }

            private void Execute(object arg)
            {
                var changeState = new HomeHubChangeStates(_homeHubSocket);
                changeState.ChangeStates(_states);
            }

            public void Cancel()
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }
    }
}
