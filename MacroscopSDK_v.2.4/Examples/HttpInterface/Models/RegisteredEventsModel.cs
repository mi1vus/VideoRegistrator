using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HttpInterface
{
	public class RegisteredEventsModelModel : IRegisteredEventsModel
	{
		private readonly IConnectionParametersModel _connectionParametersModel;
		private readonly IUrlViewer _urlViewer;
		private readonly IWebRequestFactory _webRequestFactory;
		private const string RegisteredEventsUrl = "http://{0}:{1}/command?type=getallregisteredevents&login={2}&password={3}";

		const string XmlHeaderToFind = @"<?xml version=""1.0"" encoding=""utf-8""?>";

		public RegisteredEventsModelModel(IConnectionParametersModel connectionParametersModel, IUrlViewer urlViewer, IWebRequestFactory webRequestFactory)
		{
			_connectionParametersModel = connectionParametersModel;
			_urlViewer = urlViewer;
			_webRequestFactory = webRequestFactory;
		}

		public Task<IEnumerable<EventInfo>> GetRegisteredEventInfosTask()
		{
			var task = new Task<IEnumerable<EventInfo>>(() =>
			{
				var result = GetRegisteredEventInfos();
				return result;
			});

			return task;
		}

		private IEnumerable<EventInfo> GetRegisteredEventInfos()
		{
			var connectionParameters = _connectionParametersModel.ConnectionParameter;
			var url = string.Format(RegisteredEventsUrl, connectionParameters.ServerIp, 
				connectionParameters.Port,
				connectionParameters.Login,
				Md5Helper.Md5Hash(connectionParameters.Password));

			_urlViewer.Url = url;

			var responseString = _webRequestFactory.CreateAndGetResult(url);
			if (string.IsNullOrEmpty(responseString))
				return new List<EventInfo>();

			var ser = new XmlSerializer(typeof(List<EventInfo>));

			var bytesToSeek = responseString.IndexOf(XmlHeaderToFind, StringComparison.Ordinal);
			if (bytesToSeek == -1)
				return new List<EventInfo>();

			responseString = responseString.Substring(bytesToSeek, responseString.Length - bytesToSeek);

			using (TextReader reader = new StringReader(responseString))
			{
				var eventsList = ser.Deserialize(reader) as List<EventInfo>;
				return eventsList;
			}
		}
	}
}
