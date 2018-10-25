namespace MyHomeSecureWeb.Models
{
    public class HubInitialiseRequest : SocketMessageBase
    {
        public string Name { get; set; }
        public string Token { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public float Radius { get; set; }

        public HubInitialiseUser[] Users { get; set; }
        public string[] States { get; set; }
        public HubInitialiseCamera[] Cameras { get; set; }
    }

    public class HubInitialiseUser : SocketMessageBase
    {
        public string Name { get; set; }
        public string Token { get; set; }
    }

    public class HubInitialiseCamera
    {
        public string Name { get; set; }
        public string Node { get; set; }
    }
}
