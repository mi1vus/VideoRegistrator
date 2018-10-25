using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;
using MyHomeSecureWeb.Repositories;
using System.Linq;
using MyHomeSecureWeb.Utilities;
using System.Net;
using System;

namespace MyHomeSecureWeb.ScheduledJobs
{
    // A simple scheduled job which can be invoked manually by submitting an HTTP
    // POST request to the path "/jobs/purgelog".
    public class PurgeLogJob : ScheduledJob
    {
        private const int ExpiryAgeDays = 7;

        public async override Task ExecuteAsync()
        {
            try
            {
                await RemoveExpiredSnapshots();
            }
            catch(Exception e)
            {
                Services.Log.Error("Error removing expired snapshots from Drive", e);
            }

            try
            {
                PurgeLogs();
            }
            catch (Exception e)
            {
                Services.Log.Error("Error purging log", e);
            }
        }

        private void PurgeLogs()
        {
            Services.Log.Info("Running Job: Purge old log entries");
            using (var logRepository = new LogRepository())
            {
                logRepository.PurgeOldLogEntries();
            }
        }

        public async Task RemoveExpiredSnapshots()
        {
            Services.Log.Info("Running Job: Remove Expired snapshots");
            using (IAwayStatusRepository awayStatusRepository = new AwayStatusRepository())
            {
                var allWithToken = awayStatusRepository.GetAll()
                    .Where(s => s.DriveRefreshToken != null && s.DriveRefreshToken != "")
                    .Select(s => s.UserName)
                    .ToList();

                foreach (var user in allWithToken)
                {
                    await RemoveExpiredForUser(user);
                }
            }
        }

        private async Task RemoveExpiredForUser(string emailAddress)
        {
            await new GoogleDriveAuthHelper(Services).AccessDrive(emailAddress, RemoveExpiredWithAccessToken);
        }

        private async Task RemoveExpiredWithAccessToken(string accessToken)
        {
            IGoogleDriveHelper driveHelper = new GoogleDriveHelper();

            // Get all the top level folders
            var snapshotFolders = await driveHelper.GetChildrenIDs(accessToken);

            // Decode the folder names for the dates
            foreach (var folder in snapshotFolders.Items)
            {
                DateTime nameAsDate;
                if (DateTime.TryParse(folder.Title, out nameAsDate))
                {
                    if (nameAsDate < DateTime.Today.AddDays(-ExpiryAgeDays))
                    {
                        // Delete this folder
                        Services.Log.Info(string.Format("Deleting expired folder '{0}'", folder.Title));
                        await driveHelper.Delete(accessToken, folder.Id);
                    }
                }
            }

        }
    }
}