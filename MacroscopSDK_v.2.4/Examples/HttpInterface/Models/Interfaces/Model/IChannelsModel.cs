using System.Collections.Generic;
using System.Threading.Tasks;

namespace HttpInterface
{
	public interface IChannelsModel
	{
		Task<IEnumerable<ChannelInfo>> GetChannelsInfosTask();
	}
}
