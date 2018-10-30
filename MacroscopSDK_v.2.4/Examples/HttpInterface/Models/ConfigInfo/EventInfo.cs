using System;
using System.Xml.Serialization;

namespace HttpInterface
{
	[Serializable]
	public class EventInfo
	{
		[XmlElement("Id")]
		public Guid Id { get; set; }
		[XmlElement("GuiName")]
		public string GuiName { get; set; }
	}
}
