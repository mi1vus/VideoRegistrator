using MyHomeSecureWeb.DataObjects;
using MyHomeSecureWeb.Models;
using System;
using System.Linq;

namespace MyHomeSecureWeb.Repositories
{
    public class HomeHubRepository : IHomeHubRepository
    {
        private MobileServiceContext db = new MobileServiceContext();

        public HomeHub GetHub(string name)
        {
            return db.HomeHubs.SingleOrDefault(h => h.Name == name);
        }
        public HomeHub GetHubById(string id)
        {
            return db.HomeHubs.SingleOrDefault(h => h.Id == id);
        }

        public HomeHub AddHub(string name, byte[] tokenHash, byte[] salt)
        {
            var hub = new HomeHub
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                TokenHash = tokenHash,
                TokenSalt = salt
            };

            var newHub = db.HomeHubs.Add(hub);
            db.SaveChanges();

            return newHub;
        }

        public void SetLocation(string homeHubId, double latitude, double longitude, float radius)
        {
            var hub = db.HomeHubs.Single(h => h.Id == homeHubId);

            hub.Latitude = latitude;
            hub.Longitude = longitude;
            hub.Radius = radius;

            db.SaveChanges();
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
