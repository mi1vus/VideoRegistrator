using System.Linq;
using MyHomeSecureWeb.DataObjects;
using System;

namespace MyHomeSecureWeb.Repositories
{
    public interface ICameraRepository : IDisposable
    {
        void AddCamera(string name, string node, string homeHubId);
        void RemoveCamera(string name, string homeHubId);
        IQueryable<HubCamera> GetAllForHub(string homeHubId);
    }
}