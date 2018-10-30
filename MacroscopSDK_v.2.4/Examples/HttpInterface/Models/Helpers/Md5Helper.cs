using System;
using System.Security.Cryptography;
using System.Text;

namespace HttpInterface
{
	public class Md5Helper
	{
		public static string Md5Hash(string input)
		{
			try
			{
				byte[] encodedPassword = new UTF8Encoding().GetBytes(input);
				byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);
				string encoded = BitConverter.ToString(hash)
				   .Replace("-", string.Empty)
				   .ToLower();
				return encoded;

			}
			catch (Exception)
			{
				return string.Empty;
			}
		}
	}
}
