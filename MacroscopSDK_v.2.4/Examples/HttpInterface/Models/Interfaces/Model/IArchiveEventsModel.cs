using System.Collections.Generic;
using System.Threading.Tasks;

namespace HttpInterface
{
	public interface IArchiveEventsModel
	{
		Task<IEnumerable<SubscribedEvent>> GetArchiveEventsTask(ArchiveParameters archiveParameters);
	}
}
