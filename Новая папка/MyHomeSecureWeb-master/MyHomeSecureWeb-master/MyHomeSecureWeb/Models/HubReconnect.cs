namespace MyHomeSecureWeb.Models
{
    public class HubReconnectRequest : SocketMessageBase
    {
        public string Name { get; set; }
        public string Token { get; set; }
    }
}
