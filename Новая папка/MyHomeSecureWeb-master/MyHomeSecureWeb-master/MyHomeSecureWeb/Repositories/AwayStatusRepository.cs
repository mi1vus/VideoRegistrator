using MyHomeSecureWeb.DataObjects;
using MyHomeSecureWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHomeSecureWeb.Repositories
{
    public class AwayStatusRepository : IAwayStatusRepository
    {
        private MobileServiceContext db = new MobileServiceContext();

        private int GoogleTokenExpiryHours = 7 * 24;

        public AwayStatus GetStatus(string userName)
        {
            return db.AwayStatus.SingleOrDefault(s => s.UserName == userName);
        }

        public void UpdateStatus(string userName, bool away)
        {
            db.AwayStatus.Single(s => s.UserName == userName).Away = away;
            db.SaveChanges();
        }

        public void SetToken(string userName, byte[] tokenHash, byte[] salt)
        {
            var user = GetStatus(userName);
            user.TokenHash = tokenHash;
            user.TokenSalt = salt;
            db.SaveChanges();
        }

        public async Task SetGoogleTokenAsync(string userName, string googleToken)
        {
            var user = GetStatus(userName);
            user.GoogleToken = googleToken;
            user.GoogleTokenExpires = DateTime.Now.AddHours(GoogleTokenExpiryHours);
            await db.SaveChangesAsync();
        }

        public AwayStatus LookupGoogleToken(string googleToken)
        {
            var checkDate = DateTime.Now;
            return db.AwayStatus.SingleOrDefault(s => s.GoogleToken == googleToken && s.GoogleTokenExpires > checkDate);
        }

        public async Task SetDriveTokensAsync(string userName, string accessToken, string refreshToken = null)
        {
            var user = GetStatus(userName);
            user.DriveAccessToken = accessToken;
            if (!string.IsNullOrEmpty(refreshToken)) {
                user.DriveRefreshToken = refreshToken;
            }
            await db.SaveChangesAsync();
        }

        public void AddUser(string userName, string homeHubId)
        {
            if (db.AwayStatus.SingleOrDefault(s => s.UserName == userName) != null)
            {
                throw new Exception(string.Format("The user '{0}' already exists", userName));
            }

            db.AwayStatus.Add(new AwayStatus
            {
                Id = Guid.NewGuid().ToString(),
                HomeHubId = homeHubId,
                UserName = userName,
                Away = false
            });
            db.SaveChanges();
        }

        public void RemoveUser(string userName)
        {
            db.AwayStatus.Remove(db.AwayStatus.Single(s => s.UserName == userName));
            db.SaveChanges();
        }

        public IQueryable<AwayStatus> GetAllForHub(string homeHubId)
        {
            return db.AwayStatus.Where(s => s.HomeHubId == homeHubId);
        }

        public IQueryable<AwayStatus> GetAll()
        {
            return db.AwayStatus;
        }

        public void Dispose()
        {
            db.Dispose();
        }

    }
}
