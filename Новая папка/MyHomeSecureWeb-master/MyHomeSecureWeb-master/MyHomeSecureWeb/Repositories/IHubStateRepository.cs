using System.Linq;
using MyHomeSecureWeb.DataObjects;
using System;

namespace MyHomeSecureWeb.Repositories
{
    public interface IHubStateRepository : IDisposable
    {
        void AddState(string homeHubId, string name);
        IQueryable<HubState> GetAllForHub(string homeHubId);
        bool GetState(string homeHubId, string name);
        void RemoveState(string homeHubId, string name);
        bool SetState(string homeHubId, string name, bool state);
    }
}