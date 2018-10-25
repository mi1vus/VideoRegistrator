using System;
using System.Collections.Generic;

namespace MyHomeSecureWeb.Utilities
{
    public class CheckInOutMonitor : IDisposable
    {
        public delegate void UserCheckedInOut(string userName, bool away);
        public event UserCheckedInOut CheckInOut;

        private static List<CheckInOutMonitor> _instances = new List<CheckInOutMonitor>();

        public static CheckInOutMonitor Create(string homeHubId)
        {
            var newMonitor = new CheckInOutMonitor(homeHubId);
            _instances.Add(newMonitor);
            return newMonitor;
        }
        public static void UserInOut(string homeHubId, string userName, bool away)
        {
            foreach(var instance in _instances)
            {
                if (instance._homeHubId == homeHubId)
                {
                    instance.UserInOut(userName, away);
                }
            }
        }

        private string _homeHubId;

        private CheckInOutMonitor(string homeHubId)
        {
            _homeHubId = homeHubId;
        }

        private void UserInOut(string userName, bool away)
        {
            if (CheckInOut != null)
            {
                CheckInOut(userName, away);
            }
        }

        public void Dispose()
        {
            _instances.Remove(this);
        }
    }
}
