using System;
using System.Net;
using System.Net.Sockets;
using XBMCRPC;

namespace NoesisLabs.Elve.Kodi
{
	public class SocketFactory : ISocketFactory
	{
		public ISocket GetSocket()
		{
			return new SocketWrapper(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
		}

		public string[] ResolveHostname(string hostname)
		{
			var hostEntry = Dns.GetHostEntry(hostname);

			return hostEntry.Aliases;
		}
	}
}