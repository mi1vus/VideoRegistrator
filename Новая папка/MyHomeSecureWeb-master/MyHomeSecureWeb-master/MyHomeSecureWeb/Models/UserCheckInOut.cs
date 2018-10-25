namespace MyHomeSecureWeb.Models
{
    public class UserCheckInOut : SocketMessageBase
    {
        public string UserName { get; set; }
        public bool CurrentUser { get; set; }
        public bool Away { get; set; }
    }
}
