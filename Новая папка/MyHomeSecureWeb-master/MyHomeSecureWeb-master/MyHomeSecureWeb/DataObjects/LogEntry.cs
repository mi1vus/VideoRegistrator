using Microsoft.WindowsAzure.Mobile.Service;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyHomeSecureWeb.DataObjects
{
    public class LogEntry : EntityData
    {
        public string Severity { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; }

        public string HomeHubId { get; set; }
        [ForeignKey("HomeHubId")]
        public virtual HomeHub HomeHub { get; set; }
    }

    public class LogEntryResponse
    {
        public string Message { get; set; }
        public DateTime Time { get; set; }
    }
}
