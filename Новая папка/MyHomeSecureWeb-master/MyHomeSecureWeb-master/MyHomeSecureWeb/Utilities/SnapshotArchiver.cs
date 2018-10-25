using MyHomeSecureWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyHomeSecureWeb.Utilities
{
    public class SnapshotArchiver
    {
        private static Dictionary<string, SnapshotArchiver> _archivers = new Dictionary<string, SnapshotArchiver>();
        private static object _globalSync = new object();
        public static void Queue(string hubId, string node, byte[] imageBytes)
        {
            lock(_globalSync)
            {
                if (!_archivers.ContainsKey(hubId))
                {
                    _archivers[hubId] = new SnapshotArchiver(hubId);
                }
                _archivers[hubId].Queue(node, imageBytes);
            }
        }

        private List<UploadTask> _uploadTasks = new List<UploadTask>();
        private object _localSync = new object();
        private string _hubId;
        private Task _executionTask;
        private string[] emailAddresses;

        private SnapshotArchiver(string hubId)
        {
            _hubId = hubId;
            _executionTask = null;

            using (IAwayStatusRepository awayStatusRepository = new AwayStatusRepository())
            {
                var users = awayStatusRepository.GetAllForHub(hubId);
                emailAddresses = users.Select(u => u.UserName).ToArray();
            }
        }

        private void Queue(string node, byte[] imageBytes)
        {
            Task newTask = null;
            lock(_localSync)
            {
                _uploadTasks.Add(CreateTask(node, imageBytes));

                if (_executionTask == null)
                {
                    _executionTask = new Task(async () => { await UploadQueueOnThread(); });
                    newTask = _executionTask;
                }
            }

            if (newTask != null) newTask.Start();
        }

        private UploadTask CreateTask(string node, byte[] imageBytes)
        {
            var folderPath = string.Format("{0}/{1}", GetDateFolderName(), node);

            return new UploadTask
            {
                Node = node,
                ImageBytes = imageBytes,
                FolderPath = string.Format("{0}/{1}", GetDateFolderName(), node),
                FileName = GetTimeFileName()
            };
        }

        private async Task UploadQueueOnThread()
        {
            using (var driveUploader = new GoogleDriveUploader())
            {
                UploadTask task = null;
                while (NextTask(ref task))
                {
                    try
                    {
                        foreach (var emailAddress in emailAddresses)
                        {
                            await driveUploader.UploadFile(emailAddress, task.FolderPath, task.FileName, task.ImageBytes);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }

        private string GetDateFolderName()
        {
            return DateTime.Today.ToString("yyyy-MM-dd");
        }

        private string GetTimeFileName()
        {
            return DateTime.Now.ToString("HH-mm-ss.fff");
        }

        private bool NextTask(ref UploadTask task)
        {
            lock (_localSync)
            {
                if (_uploadTasks.Count > 0)
                {
                    task = _uploadTasks[0];
                    _uploadTasks.RemoveAt(0);
                }
                else
                {
                    _executionTask = null;
                    task = null;
                }
                return task != null;
            }
        }

        private class UploadTask
        {
            public string Node { get; set; }

            public string FolderPath { get; set; }
            public string FileName { get; set; }

            public byte[] ImageBytes { get; set; }
        }
    }
}
