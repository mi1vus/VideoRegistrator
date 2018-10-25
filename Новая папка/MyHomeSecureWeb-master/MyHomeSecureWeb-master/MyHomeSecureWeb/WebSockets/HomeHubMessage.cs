using System;
using System.Diagnostics;

namespace MyHomeSecureWeb.WebSockets
{
    public class MessageRequest
    {
        public string Message { get; set; }
    }

    public class HomeHubMessage : ISocketTarget
    {
        private IHomeHubSocket _homeHubSocket;

        public HomeHubMessage(IHomeHubSocket homeHubSocket)
        {
            _homeHubSocket = homeHubSocket;
        }

        public void Message(MessageRequest request)
        {
            Debug.WriteLine(string.Format("Received message: {0}", request.Message));
            _homeHubSocket.SendMessage(new MessageRequest {
                Message = string.Format("You sent: {0}", request.Message)
            });
        }

        public void Dispose()
        {
            
        }
    }
}
