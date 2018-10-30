using System;
using System.Collections.Generic;

namespace HttpInterface
{
	public class JsonEventParser
	{
		public static SubscribedEvent ParseEventFromJsonPaylaod(IList<string> payload)
		{
			//Example Event
			//{
			//    "EventId" : "00000000-0000-0000-0000-000000000033",
			//    "Timestamp" : "19.06.2015 3:28:44",
			//    "EventDescription" : "Detected Motion",
			//    "IsAlarmEvent" : "True",
			//    "ChannelId" : "b7685f31-adca-4cea-b744-12056c6f9a80",
			//    "ChannelName" : "Channel 1",
			//    "Zoneid" : "748a50ef-d196-40c3-a430-e2d40c8ec6e4"
			//}
			var eventIdPayload = SeparateJsonLine(payload[1]);
			Guid eventlId;
			if (!Guid.TryParse(eventIdPayload, out eventlId))
				return null;

			var data = SeparateTimeJsonLine(payload[2]);
			var eventName = SeparateJsonLine(payload[3]);
			List<string> eventDescriptions = new List<string>();
			for (int i = 7; i < payload.Count; i++)
			{
				var payloadString = SeparateJsonLine(payload[i]);
				if (!String.IsNullOrEmpty(payloadString))
					eventDescriptions.Add(payloadString);
			}

			var subscribedEvent = new SubscribedEvent
			{
				Id = eventlId,
				Name = eventName,
				Description = eventDescriptions,
				Data = data
			};

			return subscribedEvent;
		}

		private static string SeparateJsonLine(string paylaod)
		{
			const char commaToSplit = ':';
			const char commaToGetPayload = '"';
			paylaod = paylaod.Trim();

			var strings = paylaod.Split(commaToSplit);
			if (strings.Length < 2)
				return String.Empty;

			var resultPayload = strings[1];
			int foundIndex = resultPayload.IndexOf(commaToGetPayload);
			if (foundIndex == -1)
				return String.Empty;

			int endIndex = resultPayload.IndexOf(commaToGetPayload, foundIndex + 1);
			if (endIndex == -1)
				return String.Empty;

			resultPayload = resultPayload.Substring(foundIndex + 1, endIndex - foundIndex - 1);
			return resultPayload;
		}

		private static string SeparateTimeJsonLine(string paylaod)
		{
			const char commaToSplit = ':';
			const char commaToGetPayload = '"';
			paylaod = paylaod.Trim();

			int foundIndex = paylaod.IndexOf(commaToSplit);
			if (foundIndex == -1)
				return String.Empty;

			int endIndex = paylaod.IndexOf(commaToGetPayload, foundIndex + 4);
			if (endIndex == -1)
				return String.Empty;

			paylaod = paylaod.Substring(foundIndex + 3, endIndex - foundIndex - 3);
			return paylaod;
		}
	}
}
