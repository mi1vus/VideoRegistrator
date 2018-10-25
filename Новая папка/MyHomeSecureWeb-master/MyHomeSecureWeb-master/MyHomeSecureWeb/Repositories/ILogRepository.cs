using MyHomeSecureWeb.DataObjects;
using System;
using System.Linq;

namespace MyHomeSecureWeb.Repositories
{
    public interface ILogRepository: IDisposable
    {
        void Info(string homeHubId, string message);
        void Priority(string homeHubId, string message);
        void Error(string homeHubId, string message);
        void LogEntry(string homeHubId, string severity, string message);

        IQueryable<LogEntry> GetLogEntries(string homeHubId, bool priority = false);
        void PurgeOldLogEntries();
    }
}