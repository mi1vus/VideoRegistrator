namespace MyHomeSecureWeb.Models
{
    public class HubCameraCommand : SocketMessageBase
    {
        public string Node { get; set; }
        public bool Active { get; set; }
        public string Type { get; set; }
    }
}
