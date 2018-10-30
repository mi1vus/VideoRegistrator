using System;
using Alarus.RealTimeFrameProviding;
using System.Runtime.InteropServices;
using Alarus.Events;

namespace Analyst_PluginExample
{
    [Serializable]
    [GuidAttribute("389EDCE2-54BB-4C2C-9984-51B7516A5DDF")]
    [EventLocalizedName("Тестовое событие")]
	[EventSaveable(EventSaveMode.SpecialAndUnifiedLog, "counter", EventGenerationFrequency.Middle)]
    [EventGeneratesAlarmByDefault]
    public class CounterEvent : RawEvent
    {
        [EventFieldSaveableOrScenariesUsable(0, true)]
        [EventFieldLocalizedName("Целое число")]
        private int Counter;

        public CounterEvent(int count)
        {
            Counter = count;
			Comment = "Число = " + count;
        }		
    }
}
