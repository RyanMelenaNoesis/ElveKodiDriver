using CodecoreTechnologies.Elve.DriverFramework.DriverTestHarness;
using CodecoreTechnologies.Elve.DriverFramework.Scripting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NoesisLabs.Elve.Kodi.TestHarness
{
	internal class Program
	{
		private const string KODI_HOST = ""; // <-- SET KODI HOST/IP
		private const int KODI_PORT = 80; // <-- SET KODI PORT
		private const int REFRESH_NTERVAL_SECONDS = 10;

		private static void Main(string[] args)
		{
			// Prepare any needed configuration files (this is rare).
			Dictionary<string, byte[]> configFiles = new Dictionary<string, byte[]>();
			//configFiles.Add("myfile.xml", ...);

			// Prepare any settings (if the device requires that settings be set)
			TestDeviceSettingDictionary settings = new TestDeviceSettingDictionary();
			settings.Add(new TestDeviceSetting("HostnameSetting", KODI_HOST));
			settings.Add(new TestDeviceSetting("PortSetting", KODI_PORT.ToString()));
			settings.Add(new TestDeviceSetting("RefreshIntervalSetting", REFRESH_NTERVAL_SECONDS.ToString()));
			//settings.Add(new TestDeviceSetting("SerialPortSetting", "COM1"));

			// Prepare any rules (if you wish to test with rules)
			TestRuleDictionary rules = new TestRuleDictionary();
			//rules.Add(new TestHarnessDriverRule("my rule", true, "TheEventMemberName", new StringDictionary()));

			//**************************************************
			// Create and Start the device.
			//**************************************************
			// TODO: Change the "MyDriver" type below below to the type name of your driver.
			KodiDriver device;
			try
			{
				device = (KodiDriver)DeviceFactory.CreateAndStartDevice(typeof(KodiDriver), configFiles, settings, rules, new TestLogger());
			}
			catch (Exception ex)
			{
				// An exception occurred while creating or starting the device.
				throw;
			}

			// Test any properties or method here.
			ScriptNumber stage = device.DeviceLifecycleStage;

			var refreshTimer = new System.Timers.Timer();
			refreshTimer.Interval = 10000;
			refreshTimer.AutoReset = true;
			refreshTimer.Elapsed += new System.Timers.ElapsedEventHandler((sender, e) =>
			{
				Console.WriteLine(JsonConvert.SerializeObject(device.player));
			});
			refreshTimer.Start();

			// Sleep until the user presses enter.
			Console.ReadLine();

			// Stop the device gracefully.
			device.StopDriver();
		}
	}
}