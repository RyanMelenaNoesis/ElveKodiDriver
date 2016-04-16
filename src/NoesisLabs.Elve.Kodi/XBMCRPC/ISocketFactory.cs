using System;

namespace XBMCRPC
{
	public interface ISocketFactory
	{
		ISocket GetSocket();

		string[] ResolveHostname(string hostname);
	}
}