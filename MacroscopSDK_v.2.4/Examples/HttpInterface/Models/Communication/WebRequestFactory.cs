using System;
using System.IO;
using System.Net;
using System.Text;

namespace HttpInterface
{
	class WebRequestFactory : IWebRequestFactory
	{
		public string CreateAndGetResult(string url)
		{
			try
			{
				var req = WebRequest.Create(url);
				var resp = req.GetResponse();
				var stream = resp.GetResponseStream();
				if (stream == null)
					return string.Empty;

				using (var sr = new StreamReader(stream, Encoding.UTF8))
				{
					var responseString = sr.ReadToEnd();
					return responseString;
				}
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		public Stream CreateInifinityWebRequest(string url)
		{
			try
			{
				var req = WebRequest.Create(url);
				var resp = req.GetResponse();
				var stream = resp.GetResponseStream();
				return stream;
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
