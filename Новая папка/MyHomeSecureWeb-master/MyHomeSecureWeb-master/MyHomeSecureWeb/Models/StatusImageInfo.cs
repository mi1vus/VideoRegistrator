using System;

namespace MyHomeSecureWeb.Models
{
    public class StatusImageInfo
    {
        public string State { get; set; }
        public string FileName { get; set; }
        public bool Active { get; set; }
        public DateTime Updated { get; set; }
        public int ZIndex { get; set; }
    }
}
