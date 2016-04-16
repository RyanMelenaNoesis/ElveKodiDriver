using System;
using XBMCRPC;

namespace NoesisLabs.Elve.Kodi
{
	public class PlatformServices : IPlatformServices
	{
		public ISocketFactory SocketFactory
		{
			get
			{
				return new SocketFactory();
			}
		}
	}
}