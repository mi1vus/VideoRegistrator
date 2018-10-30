using System.Collections.Generic;
using System.Threading.Tasks;

namespace HttpInterface
{
	public interface IRegisteredEventsModel
	{
		Task<IEnumerable<EventInfo>> GetRegisteredEventInfosTask();
	}
}
