using System;
using System.Xml.Serialization;

namespace HttpInterface
{
	[Serializable]
	public class ServerInfo
	{
		[XmlAttribute("Id")]
		public Guid Id { get; set; }
	}
}