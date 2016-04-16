using CodecoreTechnologies.Elve.DriverFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoesisLabs.Elve.Kodi.TestHarness
{
	public class TestLogger : ILogger
	{
		public void Debug(object message)
		{
			Console.WriteLine(message.ToString());
		}

		public void Debug(object message, Exception ex)
		{
			Console.WriteLine(message.ToString());
		}

		public void Debug(object message, byte[] data)
		{
			Console.WriteLine(message.ToString());
		}

		public void Debug(object message, byte[] data, Exception ex)
		{
			Console.WriteLine(message.ToString());
		}

		public void Debug(object message, byte[] data, int startIndex, int length)
		{
			Console.WriteLine(message.ToString());
		}

		public void DebugFormat(string format, params object[] args)
		{
			Console.WriteLine(String.Format(format, args));
		}

		public void Error(object message)
		{
			Console.WriteLine(message.ToString());
		}

		public void Error(object message, Exception ex)
		{
			   Console.WriteLine(message.ToString());
		}

		public void Error(object message, byte[] data)
		{
			   Console.WriteLine(message.ToString());
		}

		public void Error(object message, byte[] data, Exception ex)
		{
			   Console.WriteLine(message.ToString());
		}

		public void Error(object message, byte[] data, int startIndex, int length)
		{
			   Console.WriteLine(message.ToString());
		}

		public void ErrorFormat(string format, params object[] args)
		{
			Console.WriteLine(String.Format(format, args));
		}

		public void Fatal(object message)
		{
			   Console.WriteLine(message.ToString());
		}

		public void Fatal(object message, Exception ex)
		{
			   Console.WriteLine(message.ToString());
		}

		public void Fatal(object message, byte[] data)
		{
			   Console.WriteLine(message.ToString());
		}

		public void Fatal(object message, byte[] data, Exception ex)
		{
			   Console.WriteLine(message.ToString());
		}

		public void Fatal(object message, byte[] data, int startIndex, int length)
		{
			   Console.WriteLine(message.ToString());
		}

		public void FatalFormat(string format, params object[] args)
		{
			Console.WriteLine(String.Format(format, args));
		}

		public void Info(object message)
		{
			   Console.WriteLine(message.ToString());
		}

		public void Info(object message, Exception ex)
		{
			   Console.WriteLine(message.ToString());
		}

		public void Info(object message, byte[] data)
		{
			   Console.WriteLine(message.ToString());
		}

		public void Info(object message, byte[] data, Exception ex)
		{
			   Console.WriteLine(message.ToString());
		}

		public void Info(object message, byte[] data, int startIndex, int length)
		{
			   Console.WriteLine(message.ToString());
		}

		public void InfoFormat(string format, params object[] args)
		{
			Console.WriteLine(String.Format(format, args));
		}

		public void SystemMsg(object message)
		{
			   Console.WriteLine(message.ToString());
		}

		public void SystemMsg(object message, Exception ex)
		{
			   Console.WriteLine(message.ToString());
		}

		public void SystemMsg(object message, byte[] data)
		{
			   Console.WriteLine(message.ToString());
		}

		public void SystemMsg(object message, byte[] data, Exception ex)
		{
			   Console.WriteLine(message.ToString());
		}

		public void SystemMsg(object message, byte[] data, int startIndex, int length)
		{
			   Console.WriteLine(message.ToString());
		}

		public void SystemMsgFormat(string format, params object[] args)
		{
			Console.WriteLine(String.Format(format, args));
		}

		public void Warning(object message)
		{
			   Console.WriteLine(message.ToString());
		}

		public void Warning(object message, Exception ex)
		{
			   Console.WriteLine(message.ToString());
		}

		public void Warning(object message, byte[] data)
		{
			   Console.WriteLine(message.ToString());
		}

		public void Warning(object message, byte[] data, Exception ex)
		{
			   Console.WriteLine(message.ToString());
		}

		public void Warning(object message, byte[] data, int startIndex, int length)
		{
			   Console.WriteLine(message.ToString());
		}

		public void WarningFormat(string format, params object[] args)
		{
			Console.WriteLine(String.Format(format, args));
		}
	}
}
