using System;
using System.Collections.Generic;

namespace HttpInterface
{
	public class SubscribedEvent
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string Data { get; set; }
		public List<string> Description { get; set; }
	}
}
