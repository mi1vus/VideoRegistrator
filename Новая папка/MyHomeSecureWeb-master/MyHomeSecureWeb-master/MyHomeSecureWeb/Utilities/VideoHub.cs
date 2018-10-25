using MyHomeSecureWeb.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MyHomeSecureWeb.Utilities
{
    public class VideoHub : IDisposable
    {
        public delegate void VideoDataMessage(VideoHubData data);
        public event VideoDataMessage OnData;
        public event Action OnClosed;

        private static Dictionary<string, VideoHub> _instances = new Dictionary<string, VideoHub>();
        private static Dictionary<string, VideoHubData> _recentData = new Dictionary<string, VideoHubData>();
        private const int RecentDataSeconds = 10;

        public static VideoHub Get(string homeHubId, string node)
        {
            lock (_instances)
            {
                VideoHub videoHub = null;
                var streamId = getStreamId(homeHubId, node);
                if (_instances.ContainsKey(streamId))
                {
                    videoHub = _instances[streamId];
                }
                else {
                    videoHub = new VideoHub(streamId, homeHubId, node);
                    _instances[streamId] = videoHub;
                }

                videoHub.AddRef();
                return videoHub;
            }
        }

        private static string getStreamId(string homeHubId, string node)
        {
            return string.Format("{0}|{1}", homeHubId, node);
        }

        private string _streamId;
        private string _nodeName;
        private int _refCount;
        private ChatHub _chatHub;

        private DateTime _lastRequested = DateTime.MinValue;

        private VideoHub(string streamId, string homeHubId, string nodeName)
        {
            _streamId = streamId;
            _nodeName = nodeName;
            _refCount = 0;
            _chatHub = ChatHub.Get(homeHubId);
        }

        public void ReceivedData(byte[] bytes, int length)
        {
            var videoData = new VideoHubData
            {
                Bytes = bytes,
                Length = length,
                Time = DateTime.Now
            };

            if (OnData != null)
            {
                OnData(videoData);
            }

            _recentData[_streamId] = videoData;
        }

        public VideoHubData GetRecent()
        {
            if (_recentData.ContainsKey(_streamId))
            {
                var recent = _recentData[_streamId];
                if (recent != null && recent.Time > DateTime.Now.AddSeconds(-RecentDataSeconds))
                {
                    return recent;
                }
            }
            return null;
        }

        public void Closed()
        {
            if (OnClosed != null)
            {
                OnClosed();
            }
        }

        private void AddRef()
        {
            _refCount++;
        }

        public void Dispose()
        {
            _refCount--;
            if (_refCount == 0)
            {
                _instances.Remove(_streamId);
                _chatHub.Dispose();
            }
        }
    }

    public class VideoHubWaitable : IDisposable
    {
        private VideoHub _videoHub;
        TaskCompletionSource<VideoHubData> _dataCompletion;

        public VideoHubWaitable(VideoHub videoHub, bool useRecent = false)
        {
            _videoHub = videoHub;

            Push();

            _videoHub.OnData += _videoHub_OnData;
            _videoHub.OnClosed += _videoHub_OnClosed;

            if (useRecent)
            {
                var recentData = _videoHub.GetRecent();
                if (recentData != null)
                {
                    Push(recentData);
                }
            }
        }

        private void _videoHub_OnData(VideoHubData data)
        {
            Push(data);
        }
        private void _videoHub_OnClosed()
        {
            Push(new VideoHubData { Length = 0 });
        }

        public async Task<VideoHubData> WaitData()
        {
            VideoHubData data = await First().Task;
            Pop();
            return data;
        }

        public void Dispose()
        {
            _videoHub.OnData -= _videoHub_OnData;
            _videoHub.OnClosed -= _videoHub_OnClosed;
            _videoHub.Dispose();
        }

        private List<TaskCompletionSource<VideoHubData>> mList = new List<TaskCompletionSource<VideoHubData>>();
        private void Push(VideoHubData data = null)
        {
            lock(_videoHub)
            {
                if (data != null && mList.Count > 0)
                {
                    mList[mList.Count - 1].TrySetResult(data);
                }
                mList.Add(new TaskCompletionSource<VideoHubData>());
            }
        }
        private TaskCompletionSource<VideoHubData> First()
        {
            lock(_videoHub)
            {
                return mList[0];
            }
        }
        private void Pop()
        {
            lock(_videoHub)
            {
                mList.RemoveAt(0);
            }
        }
    }

    public class VideoHubData
    {
        public byte[] Bytes { get; set; }
        public int Length { get; set; }

        public DateTime Time { get; set; }
    }
}
