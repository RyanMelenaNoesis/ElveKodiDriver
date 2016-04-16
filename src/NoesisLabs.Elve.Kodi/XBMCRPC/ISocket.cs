using System;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace XBMCRPC
{

    public interface ISocket : IDisposable
    {
		void Connect(string hostName, int port, AsyncCallback callback);
		Stream GetInputStream();
		bool IsConnected();
    }
}