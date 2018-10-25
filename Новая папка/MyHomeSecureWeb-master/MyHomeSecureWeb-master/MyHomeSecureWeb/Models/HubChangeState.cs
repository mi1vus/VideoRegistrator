namespace MyHomeSecureWeb.Models
{
    public class HubChangeStates : SocketMessageBase
    {
        public HubChangeState[] States { get; set; }
    }

    public class HubChangeState
    {
        public string Name { get; set; }
        public bool Active { get; set; }
        public string Node { get; set; }
        public string Rule { get; set; }
    }
}
