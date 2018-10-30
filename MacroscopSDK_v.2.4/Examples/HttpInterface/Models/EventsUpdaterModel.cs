using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Permissions;
using System.Threading;

namespace HttpInterface
{
	public class EventsUpdaterModel : IDisposable
	{
		// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
		public event EventHandler<SubscribedEventArgs> IncomingNewEvent;
		private Thread _backgroundThread;
		private volatile bool _isStarted;
		private readonly IConnectionParametersModel _connectionParametersModel;
		private readonly IChannelsViewModel _channelsViewModel;
		private readonly IUrlViewer _urlViewer;
		private readonly IWebRequestFactory _webRequestFactory;
		private EventInfo _selectedEventforFilter;
		private const string EventsUrl = "http://{0}:{1}/event?login={2}&password={3}&responsetype=xml{4}{5}";

		public EventsUpdaterModel(IConnectionParametersModel connectionParametersModel, IChannelsViewModel channelsViewModel,
			IUrlViewer urlViewer, IWebRequestFactory webRequestFactory)
		{
			_connectionParametersModel = connectionParametersModel;
			_channelsViewModel = channelsViewModel;
			_urlViewer = urlViewer;
			_webRequestFactory = webRequestFactory;
			_selectedEventforFilter = new EventInfo();
			_backgroundThread = new Thread(EventsSubscribeThread)
			{
				IsBackground = true
			};
		}

		public bool IsStarted()
		{
			return _isStarted;
		}

		public void Start(EventInfo selectdEvent)
		{
			if (selectdEvent != null)
				_selectedEventforFilter = selectdEvent;

			if (_isStarted) 
				return;

			try
			{
				_backgroundThread = new Thread(EventsSubscribeThread)
				{
					IsBackground = true
				};
				_backgroundThread.Start();
				_isStarted = true;
			}
			catch
			{
				//
			}
		}
	
		[SecurityPermission(SecurityAction.Demand, ControlThread = true)]
		public void Stop()
		{
			try
			{
				_backgroundThread.Abort();
				_isStarted = false;
			}
			catch
			{
				//
			}
		}

		private void EventsSubscribeThread()
		{
			var connectionParameters = _connectionParametersModel.ConnectionParameter;

			var eventIdForfilter = _selectedEventforFilter != null && _selectedEventforFilter.Id != Guid.Empty ?
				_selectedEventforFilter.Id.ToString() :
				string.Empty;

			if (eventIdForfilter != string.Empty)
				eventIdForfilter = string.Concat("&filter=", eventIdForfilter);

			var channeldId = _channelsViewModel.SelectedChannelId;
			var channelIdForFilter = string.Empty;
			if (channeldId != Guid.Empty)
				channelIdForFilter = string.Concat("&channelid=", channeldId);

			var url = String.Format(EventsUrl,
				connectionParameters.ServerIp,
				connectionParameters.Port,
				connectionParameters.Login,
				Md5Helper.Md5Hash(connectionParameters.Password),
				eventIdForfilter,
				channelIdForFilter);

			_urlViewer.Url = url;

			using (var streamToRead = _webRequestFactory.CreateInifinityWebRequest(url))
			{
				if (streamToRead == null)
					return;

				var stringBuilder = new List<string>();
				using (var sr = new StreamReader(streamToRead))
				{
					while (_isStarted)
					{
						try
						{
							string line = sr.ReadLine();
							if (string.IsNullOrEmpty(line))
							{
								stringBuilder.Clear();
								continue;
							}

							stringBuilder.Add(line);

							if (line == "}")
							{
								var subscribedEvent = JsonEventParser.ParseEventFromJsonPaylaod(stringBuilder);

								OnIncomingNewEvent(subscribedEvent);

								Thread.Sleep(1000);
							}
							//Thread.Sleep(1000);
						}
						catch (Exception)
						{
							Thread.Sleep(1000);
						}
					}
				}
			}
		}

		protected virtual void OnIncomingNewEvent(SubscribedEvent subscribedEvent)
		{
			var handler = IncomingNewEvent;
			if (handler != null)
			{
				var newMessageEventArgs = new SubscribedEventArgs(subscribedEvent);
				handler(this, newMessageEventArgs);
			}
		}

		public void Dispose()
		{
			Stop();
		}
	}
}
