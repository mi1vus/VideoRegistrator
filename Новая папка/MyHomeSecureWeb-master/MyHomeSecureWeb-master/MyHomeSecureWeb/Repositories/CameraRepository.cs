using MyHomeSecureWeb.DataObjects;
using MyHomeSecureWeb.Models;
using System;
using System.Linq;

namespace MyHomeSecureWeb.Repositories
{
    public class CameraRepository : ICameraRepository
    {
        private MobileServiceContext db = new MobileServiceContext();

        public void AddCamera(string name, string node, string homeHubId)
        {
            if (db.HubCameras.SingleOrDefault(c => c.Name == name && c.HomeHubId == homeHubId) != null)
            {
                throw new Exception(string.Format("The camera '{0}' already exists", name));
            }

            db.HubCameras.Add(new HubCamera
            {
                Id = Guid.NewGuid().ToString(),
                HomeHubId = homeHubId,
                Name = name,
                Node = node
            });
            db.SaveChanges();
        }

        public void RemoveCamera(string name, string homeHubId)
        {
            db.HubCameras.Remove(db.HubCameras.Single(c => c.Node == name && c.HomeHubId == homeHubId));
            db.SaveChanges();
        }

        public IQueryable<HubCamera> GetAllForHub(string homeHubId)
        {
            return db.HubCameras.Where(s => s.HomeHubId == homeHubId);
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
