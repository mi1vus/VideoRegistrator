namespace MyHomeSecureWeb.Notifications
{
    public class StatusMessage
    {
        public string Message { get; set; }
        public string HomeHubId { get; set; }
        public string State { get; set; }
        public bool Active { get; set; }
        public string Node { get; set; }
        public string Rule { get; set; }
    }
}
