using MyHomeSecureWeb.Utilities;
using System;

namespace MyHomeSecureWeb.WebSockets
{
    public interface IHomeHubSocket : ISocketSender
    {
        string HomeHubId { get; set; }
        ChatHub ChatHub { get; }
    }
}
