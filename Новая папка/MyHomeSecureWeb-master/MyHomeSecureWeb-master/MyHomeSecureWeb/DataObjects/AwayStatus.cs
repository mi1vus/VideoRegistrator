using Microsoft.WindowsAzure.Mobile.Service;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyHomeSecureWeb.DataObjects
{
    public class AwayStatus : EntityData
    {
        public string UserName { get; set; }
        public bool Away { get; set; }

        public byte[] TokenHash { get; set; }
        public byte[] TokenSalt { get; set; }

        public string GoogleToken { get; set; }
        public DateTime GoogleTokenExpires { get; set; }

        public string HomeHubId { get; set; }
        [ForeignKey("HomeHubId")]
        public virtual HomeHub HomeHub { get; set; }

        public string DriveAccessToken { get; set; }
        public string DriveRefreshToken { get; set; }

        public AwayStatus()
        {
            GoogleTokenExpires = DateTime.Now;
        }
    }

    public class AwayStatusRequest
    {
        public string UserName { get; set; }
        public string Action { get; set; }

        public string Token { get; set; }
    }

}
