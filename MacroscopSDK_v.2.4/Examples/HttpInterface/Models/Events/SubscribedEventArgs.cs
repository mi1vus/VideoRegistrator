using System;

namespace HttpInterface
{
	public class SubscribedEventArgs : EventArgs
	{
		public SubscribedEvent SubscribedEvent { get; private set; }

		public SubscribedEventArgs(SubscribedEvent subscribedEvent)
		{
			SubscribedEvent = subscribedEvent;
		}
	}
}