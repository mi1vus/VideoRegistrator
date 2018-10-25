using MyHomeSecureWeb.DataObjects;
using MyHomeSecureWeb.Models;
using System;
using System.Linq;

namespace MyHomeSecureWeb.Repositories
{
    public class HubStateRepository : IHubStateRepository
    {
        private MobileServiceContext db = new MobileServiceContext();

        public bool GetState(string homeHubId, string name)
        {
            return GetStateEntry(homeHubId, name).Active;
        }

        public bool SetState(string homeHubId, string name, bool state)
        {
            var existing = GetStateEntry(homeHubId, name);
            if (existing.Active != state)
            {
                existing.Active = state;
                db.SaveChanges();
                return true;
            }
            return false;
        }
        
        public void AddState(string homeHubId, string name)
        {
            db.HubStates.Add(new HubState
            {
                Id = Guid.NewGuid().ToString(),
                HomeHubId = homeHubId,
                Name = name,
                Active = false
            });
            db.SaveChanges();
        }

        public void RemoveState(string homeHubId, string name)
        {
            db.HubStates.Remove(GetStateEntry(homeHubId, name));
            db.SaveChanges();
        }

        public IQueryable<HubState> GetAllForHub(string homeHubId)
        {
            return db.HubStates.Where(s => s.HomeHubId == homeHubId);
        }

        private HubState GetStateEntry(string homeHubId, string name)
        {
            return db.HubStates.Single(s => s.HomeHubId == homeHubId && s.Name == name);
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
