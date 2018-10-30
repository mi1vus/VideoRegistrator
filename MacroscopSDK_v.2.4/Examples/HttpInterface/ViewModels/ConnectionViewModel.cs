namespace HttpInterface
{
	public class ConnectionViewModel : IConnectionParametersModel
	{
		private readonly ConnectionParameter _connectionParameter;

		public ConnectionParameter ConnectionParameter
		{
			get  { return _connectionParameter; }
		}

		public ConnectionViewModel()
		{
			_connectionParameter = new ConnectionParameter
			{
				Login = "root",
				Password = string.Empty,
				Port = 1235,
				ServerIp = "91.230.153.2"
            };
		}
	}
}
