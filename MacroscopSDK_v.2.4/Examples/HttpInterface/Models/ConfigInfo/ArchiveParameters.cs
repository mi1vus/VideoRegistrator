using System;

namespace HttpInterface
{
	public class ArchiveParameters
	{
		public Guid ChannelId { get; set; }
		public Guid EventId { get; set; }
		public String DateTimeStart { get; set; }
		public String DateTimeEnd { get; set; }
	}
}