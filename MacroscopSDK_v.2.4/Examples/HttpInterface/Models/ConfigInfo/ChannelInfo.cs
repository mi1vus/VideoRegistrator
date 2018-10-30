using System;
using System.Xml.Serialization;

namespace HttpInterface
{
	[Serializable]
	public class ChannelInfo
	{
		[XmlAttribute("Id")]
		public Guid Id { get; set; }
	}
}