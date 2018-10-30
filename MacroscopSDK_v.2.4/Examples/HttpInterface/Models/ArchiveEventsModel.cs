using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HttpInterface
{
	public class ArchiveEventsModel : IArchiveEventsModel
	{
		private readonly IConnectionParametersModel _connectionParametersModel;
		private readonly IUrlViewer _urlViewer;
		private readonly IWebRequestFactory _webRequestFactory;
		private const string ArchiveEventsUrl = "http://{0}:{1}/specialarchiveevents?login={2}&password={3}&startTime={4}&endTime={5}";

		public ArchiveEventsModel(IConnectionParametersModel connectionParametersModel, IUrlViewer urlViewer, IWebRequestFactory webRequestFactory)
		{
			_connectionParametersModel = connectionParametersModel;
			_urlViewer = urlViewer;
			_webRequestFactory = webRequestFactory;
		}

		public Task<IEnumerable<SubscribedEvent>> GetArchiveEventsTask(ArchiveParameters archiveParameters)
		{
			var task = new Task<IEnumerable<SubscribedEvent>>(() =>
			{
				var result = GetArchiveEvents(archiveParameters);
				return result;
			});

			return task;
		}

		private IEnumerable<SubscribedEvent> GetArchiveEvents(ArchiveParameters archiveParameters)
		{
			var connectionParameters = _connectionParametersModel.ConnectionParameter;
			var url = string.Format(ArchiveEventsUrl, connectionParameters.ServerIp,
				connectionParameters.Port,
				connectionParameters.Login,
				Md5Helper.Md5Hash(connectionParameters.Password),
				archiveParameters.DateTimeStart,
				archiveParameters.DateTimeEnd);

			if (archiveParameters.ChannelId != Guid.Empty)
				url += string.Concat("&channelid=", archiveParameters.ChannelId);
			if (archiveParameters.EventId != Guid.Empty)
				url += string.Concat("&eventid=", archiveParameters.EventId);

			_urlViewer.Url = url;

			var responseString = _webRequestFactory.CreateAndGetResult(url);
			if (string.IsNullOrEmpty(responseString))
				return new List<SubscribedEvent>();

			var resultsSubscribedEvents = new List<SubscribedEvent>();
			var stringBuilder = new List<string>();

			using (var reader = new StringReader(responseString))
			{
				while (reader.Peek() > -1)
				{
					var line = reader.ReadLine();
					if (string.IsNullOrEmpty(line))
					{
						stringBuilder.Clear();
						continue;
					}

					stringBuilder.Add(line);

					if (line != "}") 
						continue;

					var subscribedEvent = JsonEventParser.ParseEventFromJsonPaylaod(stringBuilder);
					if (subscribedEvent == null)
						continue;

					resultsSubscribedEvents.Add(subscribedEvent);
				}
			}

			return resultsSubscribedEvents;
		}
	}
}
