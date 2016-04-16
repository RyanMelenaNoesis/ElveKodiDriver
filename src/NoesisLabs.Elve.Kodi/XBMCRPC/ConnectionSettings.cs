using System;

namespace XBMCRPC
{
	public class ConnectionSettings
	{
		public Uri BaseAddress;
		public string Host;
		public Uri JsonInterfaceAddress;
		public string Password;
		public int Port;
		public int TcpPort = 9090;
		public string UserName;

		public ConnectionSettings(string host, int port, string userName, string password)
		{
			Host = host;
			Port = port;
			UserName = userName;
			Password = password;
			JsonInterfaceAddress = new Uri(String.Format("http://{0}:{1}/jsonrpc", host, port));
			BaseAddress = new Uri(String.Format("http://{0}:{1}", host, port));
		}
	}
}