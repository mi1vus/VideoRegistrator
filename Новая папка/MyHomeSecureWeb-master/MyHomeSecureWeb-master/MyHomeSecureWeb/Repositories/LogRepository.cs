using MyHomeSecureWeb.DataObjects;
using MyHomeSecureWeb.Models;
using System;
using System.Data.SqlClient;
using System.Linq;

namespace MyHomeSecureWeb.Repositories
{
    public class LogRepository : ILogRepository
    {
        private MobileServiceContext db = new MobileServiceContext();

        public void Info(string homeHubId, string message)
        {
            LogEntry(homeHubId, "Info", message);
        }

        public void Priority(string homeHubId, string message)
        {
            LogEntry(homeHubId, "Priority", message);
        }

        public void Error(string homeHubId, string message)
        {
            LogEntry(homeHubId, "Error", message);
        }

        public void LogEntry(string homeHubId, string severity, string message)
        {
            db.LogEntries.Add(new LogEntry
            {
                Id = Guid.NewGuid().ToString(),
                Severity = severity,
                HomeHubId = homeHubId,
                Message = message,
                Time = DateTime.Now
            });

            db.SaveChanges();
        }

        public IQueryable<LogEntry> GetLogEntries(string homeHubId, bool priority)
        {
            return db.LogEntries.Where(l => 
                    string.Equals(l.HomeHubId, homeHubId)
                    && (!priority || l.Severity == "Priority" || l.Severity == "Error")
                );
        }

        public void PurgeOldLogEntries()
        {
            db.Database.ExecuteSqlCommand(
                "DELETE FROM ogadai_secure.LogEntries WHERE Time < @OldTime",
                new SqlParameter("@OldTime", DateTime.Today.AddDays(-2)));
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
