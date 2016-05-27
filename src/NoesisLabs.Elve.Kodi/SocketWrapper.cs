using System;
using System.IO;
using System.Net.Sockets;
using XBMCRPC;

namespace NoesisLabs.Elve.Kodi
{
	public class SocketWrapper : ISocket
	{
		private readonly Socket socket;

		public SocketWrapper(Socket socket)
		{
			this.socket = socket;
		}

		public void Connect(string hostName, int port, AsyncCallback callback)
		{
			this.socket.BeginConnect(hostName, port, callback, this.socket);
		}

		public void Dispose()
		{
		}

		public Stream GetInputStream()
		{
			return new NetworkStream(this.socket);
		}

		public bool IsConnected()
		{
			bool part1 = this.socket.Poll(1000, SelectMode.SelectRead);
			bool part2 = (this.socket.Available == 0);
			return !(part1 && part2);
		}
	}
}