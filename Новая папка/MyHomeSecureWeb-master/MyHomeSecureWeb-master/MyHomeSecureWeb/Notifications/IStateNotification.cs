using System.Threading.Tasks;

namespace MyHomeSecureWeb.Notifications
{
    public interface IStateNotification
    {
        Task Send(string homeHubId, string state, bool active, string node, string rule);
    }
}