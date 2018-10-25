namespace MyHomeSecureWeb.Models
{
    public class CameraInitialiseRequest : SocketMessageBase
    {
        public string Name { get; set; }
        public string Token { get; set; }
        public string Node { get; set; }
    }
}
