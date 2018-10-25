using Microsoft.WindowsAzure.Mobile.Service;
using System;

namespace MyHomeSecureWeb.WebSockets
{
    public interface ISocketSender
    {
        void SendMessage<T>(T message);

        ApiServices Services { get; }
    }
}
