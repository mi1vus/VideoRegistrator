using MyHomeSecureWeb.Models;
using System;
using System.Collections.Generic;

namespace MyHomeSecureWeb.Utilities
{
    public class ChatHub : IDisposable
    {
        public delegate void ChatHubMessage(SocketMessageBase message);
        public event ChatHubMessage HomeMessage;
        public event ChatHubMessage ClientMessage;

        private static Dictionary<string, ChatHub> _instances = new Dictionary<string, ChatHub>();

        public static ChatHub Get(string homeHubId)
        {
            lock (_instances)
            {
                ChatHub chatHub = null;
                if (_instances.ContainsKey(homeHubId))
                {
                    chatHub = _instances[homeHubId];
                }
                else {
                    chatHub = new ChatHub(homeHubId);
                    _instances[homeHubId] = chatHub;
                }

                chatHub.AddRef();
                return chatHub;
            }
        }

        public void MessageToHome(SocketMessageBase message)
        {
            if (HomeMessage != null)
            {
                HomeMessage(message);
            }
        }

        public void MessageToClients(SocketMessageBase message)
        {
            if (ClientMessage != null)
            {
                ClientMessage(message);
            }
        }

        public bool HubConnected
        {
            get { return HomeMessage != null; }
        }

        private string _homeHubId;
        private int _refCount;

        private ChatHub(string homeHubId)
        {
            _homeHubId = homeHubId;
            _refCount = 0;
        }

        private void AddRef()
        {
            _refCount++;
        }

        public void Dispose()
        {
            _refCount--;
            if (_refCount == 0)
            {
                _instances.Remove(_homeHubId);
            }
        }
    }
}
