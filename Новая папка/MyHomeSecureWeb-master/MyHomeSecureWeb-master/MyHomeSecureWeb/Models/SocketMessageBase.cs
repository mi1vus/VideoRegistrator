namespace MyHomeSecureWeb.Models
{
    public class SocketMessageBase
    {
        public SocketMessageBase()
        {
            var typeName = GetType().Name;
            if (typeName.StartsWith("Hub"))
            {
                typeName = typeName.Substring(3);
            }
            Method = typeName;
        }
        public string Method { get; set; } 
    }
}
