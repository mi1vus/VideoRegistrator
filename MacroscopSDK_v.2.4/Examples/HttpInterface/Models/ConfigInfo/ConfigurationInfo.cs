using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace HttpInterface
{
	[Serializable]
	[XmlRoot("Configuration", IsNullable = true)]
	public class Configuration
	{
		[XmlArray("Servers")]
		[XmlArrayItem("ServerInfo")]
		public List<ServerInfo> Servers { get; set; }

		[XmlArray("Channels")]
		[XmlArrayItem("ChannelInfo")]
		public List<ChannelInfo> Channels { get; set; }
	}
}
