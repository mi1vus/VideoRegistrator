using Microsoft.WindowsAzure.Mobile.Service;

namespace MyHomeSecureWeb.DataObjects
{
    public class TodoItem : EntityData
    {
        public string Text { get; set; }

        public bool Complete { get; set; }
    }
}