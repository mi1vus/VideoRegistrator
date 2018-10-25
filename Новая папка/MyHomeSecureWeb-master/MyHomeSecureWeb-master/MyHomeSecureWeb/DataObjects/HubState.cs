using Microsoft.WindowsAzure.Mobile.Service;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyHomeSecureWeb.DataObjects
{
    public class HubState : EntityData
    {
        public string Name { get; set; }
        public bool Active { get; set; }

        public string HomeHubId { get; set; }
        [ForeignKey("HomeHubId")]
        public virtual HomeHub HomeHub { get; set; }
    }
}
