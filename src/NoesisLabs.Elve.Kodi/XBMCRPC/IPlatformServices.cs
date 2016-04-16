using System;

namespace XBMCRPC
{
	public interface IPlatformServices
	{
		ISocketFactory SocketFactory { get; }
	}
}