using System.IO;

namespace HttpInterface
{
	public interface IWebRequestFactory
	{
		string CreateAndGetResult(string url);
		Stream CreateInifinityWebRequest(string url);
	}
}
