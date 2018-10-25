using Microsoft.WindowsAzure.Mobile.Service;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyHomeSecureWeb.DataObjects
{
    public class HubCamera : EntityData
    {
        public string Name { get; set; }
        public string Node { get; set; }

        public string HomeHubId { get; set; }
        [ForeignKey("HomeHubId")]
        public virtual HomeHub HomeHub { get; set; }
    }

    public class HubCameraResponse
    {
        public string Name { get; set; }
        public string Node { get; set; }
    }
}
