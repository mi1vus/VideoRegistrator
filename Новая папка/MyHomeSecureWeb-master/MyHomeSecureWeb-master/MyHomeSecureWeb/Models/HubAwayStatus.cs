namespace MyHomeSecureWeb.Models
{
    public class HubLastUserAway : SocketMessageBase
    {
        public string UserName { get; set; }
    }

    public class HubFirstUserHome : SocketMessageBase
    {
        public string UserName { get; set; }
    }
}
