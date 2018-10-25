using Microsoft.WindowsAzure.Mobile.Service;
using MyHomeSecureWeb.Models;
using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace MyHomeSecureWeb.WebSockets
{
    public abstract class SocketBase
    {
        private WebSocket _socket;
        private ApiServices _services;

        public SocketBase(WebSocket socket, ApiServices services)
        {
            _socket = socket;
            _services = services;
        }

        public ApiServices Services {
            get
            {
                return _services;
            }
        }

        public async Task Process()
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
            while (true)
            {
                WebSocketReceiveResult result = await _socket.ReceiveAsync(
                    buffer, CancellationToken.None);
                if (_socket.State == WebSocketState.Open)
                {
                    OnReceived(buffer.Array, result.Count);
                }
                else
                {
                    break;
                }
            }
        }

        protected virtual void OnReceived(byte[] bytes, int length)
        {
            string message = Encoding.UTF8.GetString(bytes, 0, length);
            ReceivedMessage(message);
        }

        private void ReceivedMessage(string message)
        {
            var decoded = JsonConvert.DeserializeObject<SocketMessageBase>(message);

            var currentTypeName = this.GetType().FullName;
            var baseTypeName = currentTypeName.Substring(0, currentTypeName.LastIndexOf("Socket"));
            var targetName = string.Format("{0}{1}", baseTypeName, decoded.Method);
            var target = CreateMessageInstance(Type.GetType(targetName));
            using (target)
            {
                var methodInfo = target.GetType().GetMethod(decoded.Method);
                if (methodInfo != null)
                {
                    var parameters = methodInfo.GetParameters();
                    if (parameters.Length > 0)
                    {
                        try {
                            var methodParams = new[]
                            {
                            JsonConvert.DeserializeObject(message, parameters[0].ParameterType)
                        };
                            methodInfo.Invoke(target, methodParams);
                        }
                        catch (Exception e)
                        {
                            _services.Log.Error(e, null, "Socket error");
                        }
                    }
                }
            }
        }

        public abstract ISocketTarget CreateMessageInstance(Type type);

        public void SendMessage<T>(T message)
        {
            try
            {
                SendMessage(JsonConvert.SerializeObject(message));
            }
            catch (Exception ex)
            {
                _services.Log.Error("Error sending Websocket message to client", ex);
            }
        }

        public void SendMessageRaw<T>(T message)
        {
            SendMessage(JsonConvert.SerializeObject(message));
        }

        private void SendMessage(string message)
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
            buffer = new ArraySegment<byte>(
                Encoding.UTF8.GetBytes(message));

            _socket.SendAsync(
                buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

    }
}
