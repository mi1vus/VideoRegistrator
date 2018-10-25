using Microsoft.WindowsAzure.Mobile.Service;
using MyHomeSecureWeb.Utilities;
using System;
using System.Diagnostics;
using System.Net.WebSockets;

namespace MyHomeSecureWeb.WebSockets
{
    public class CameraSocket : SocketBase, ICameraSocket, IDisposable
    {
        private VideoHub _videoHub;

        public CameraSocket(WebSocket socket, ApiServices services) : base(socket, services)
        {
            Debug.WriteLine("Camera Conection opened");
        }

        public void initialise(string homeHubId, string node)
        {
            if (_videoHub != null)
            {
                _videoHub.Closed();
                _videoHub.Dispose();
            }
            _videoHub = VideoHub.Get(homeHubId, node);
            Debug.WriteLine(string.Format("Initialised camera {0} on hub {1}", node, homeHubId));
        }

        protected override void OnReceived(byte[] bytes, int length)
        {
            if (_videoHub == null)
            {
                base.OnReceived(bytes, length);
            }
            else
            {
                _videoHub.ReceivedData(bytes, length);
            }
        }

        public override ISocketTarget CreateMessageInstance(Type type)
        {
            return Activator.CreateInstance(type, this) as ISocketTarget;
        }

        public virtual void Dispose()
        {
            if (_videoHub != null)
            {
                _videoHub.Closed();
                _videoHub.Dispose();
                _videoHub = null;
            }

            Debug.WriteLine("Camera Conection closed");
        }
    }
}
