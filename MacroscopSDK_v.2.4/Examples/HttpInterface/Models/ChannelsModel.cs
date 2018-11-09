using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HttpInterface
{
	public class ChannelsModelModel : IChannelsModel
	{
		private readonly IConnectionParametersModel _connectionParametersModel;
		private readonly IUrlViewer _urlViewer;
		private readonly IWebRequestFactory _webRequestFactory;
		private const string ChannelsUrl = "http://{0}:{1}/configex?responsetype=xml&login={2}&password={3}";

		const string XmlTextToRemove = @"xmlns=""http://www.macroscop.com""";

		public ChannelsModelModel(IConnectionParametersModel connectionParametersModel, IUrlViewer urlViewer, IWebRequestFactory webRequestFactory)
		{
			_connectionParametersModel = connectionParametersModel;
			_urlViewer = urlViewer;
			_webRequestFactory = webRequestFactory;
		}

		public Task<IEnumerable<ChannelInfo>> GetChannelsInfosTask()
		{
			var task = new Task<IEnumerable<ChannelInfo>>(() =>
			{
				var result = GetChannelsInfos();
				return result;
			});

			return task;
		}

		private IEnumerable<ChannelInfo> GetChannelsInfos()
		{
			var connectionParameters = _connectionParametersModel.ConnectionParameter;
			var url = string.Format(ChannelsUrl, connectionParameters.ServerIp,
				connectionParameters.Port,
				connectionParameters.Login,
                string.IsNullOrWhiteSpace( connectionParameters.Password) ? "" : Md5Helper.Md5Hash(connectionParameters.Password));

			var responseString = _webRequestFactory.CreateAndGetResult(url);

			_urlViewer.Url = url;

			if (string.IsNullOrEmpty(responseString))
				return new List<ChannelInfo>();

			var xRoot = new XmlRootAttribute
			{
				IsNullable = true,
			};

			var ser = new XmlSerializer(typeof(Configuration), xRoot);

			int startPosToRemove = responseString.IndexOf(XmlTextToRemove, StringComparison.Ordinal);
			if (startPosToRemove == -1)
				return new List<ChannelInfo>();

			responseString = responseString.Remove(startPosToRemove, XmlTextToRemove.Length);

			using (TextReader reader = new StringReader(responseString))
			{
				var config = ser.Deserialize(reader) as Configuration;

				if (config == null || config.Channels == null)
					return new List<ChannelInfo>();

				return config.Channels;
			}
		}
	}
}
