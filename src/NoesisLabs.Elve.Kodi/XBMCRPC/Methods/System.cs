using System;
using Newtonsoft.Json.Linq;
namespace XBMCRPC.Methods
{
   public partial class System
   {
        private readonly Client _client;
          public System(Client client)
          {
              _client = client;
          }

                /// <summary>
                /// Ejects or closes the optical disc drive (if available)
                /// </summary>
        public string EjectOpticalDrive()
        {
            var jArgs = new JObject();
            return _client.GetData<string>("System.EjectOpticalDrive", jArgs);
        }

                /// <summary>
                /// Retrieves the values of the given properties
                /// </summary>
        public XBMCRPC.System.Property.Value GetProperties(XBMCRPC.System.GetProperties_properties properties=null)
        {
            var jArgs = new JObject();
             if (properties != null)
             {
                 var jpropproperties = JToken.FromObject(properties, _client.Serializer);
                 jArgs.Add(new JProperty("properties", jpropproperties));
             }
            return _client.GetData<XBMCRPC.System.Property.Value>("System.GetProperties", jArgs);
        }

                /// <summary>
                /// Puts the system running Kodi into hibernate mode
                /// </summary>
        public string Hibernate()
        {
            var jArgs = new JObject();
            return _client.GetData<string>("System.Hibernate", jArgs);
        }

                /// <summary>
                /// Reboots the system running Kodi
                /// </summary>
        public string Reboot()
        {
            var jArgs = new JObject();
            return _client.GetData<string>("System.Reboot", jArgs);
        }

                /// <summary>
                /// Shuts the system running Kodi down
                /// </summary>
        public string Shutdown()
        {
            var jArgs = new JObject();
            return _client.GetData<string>("System.Shutdown", jArgs);
        }

                /// <summary>
                /// Suspends the system running Kodi
                /// </summary>
        public string Suspend()
        {
            var jArgs = new JObject();
            return _client.GetData<string>("System.Suspend", jArgs);
        }

        public delegate void OnLowBatteryDelegate(string sender=null, object data=null);
        public event OnLowBatteryDelegate OnLowBattery;
        internal void RaiseOnLowBattery(string sender=null, object data=null)
        {
            if (OnLowBattery != null)
            {
                OnLowBattery.BeginInvoke(sender, data, null, null);
            }
        }

        public delegate void OnQuitDelegate(string sender=null, XBMCRPC.System.OnQuit_data data=null);
        public event OnQuitDelegate OnQuit;
        internal void RaiseOnQuit(string sender=null, XBMCRPC.System.OnQuit_data data=null)
        {
            if (OnQuit != null)
            {
                OnQuit.BeginInvoke(sender, data, null, null);
            }
        }

        public delegate void OnRestartDelegate(string sender=null, object data=null);
        public event OnRestartDelegate OnRestart;
        internal void RaiseOnRestart(string sender=null, object data=null)
        {
            if (OnRestart != null)
            {
                OnRestart.BeginInvoke(sender, data, null, null);
            }
        }

        public delegate void OnSleepDelegate(string sender=null, object data=null);
        public event OnSleepDelegate OnSleep;
        internal void RaiseOnSleep(string sender=null, object data=null)
        {
            if (OnSleep != null)
            {
                OnSleep.BeginInvoke(sender, data, null, null);
            }
        }

        public delegate void OnWakeDelegate(string sender=null, object data=null);
        public event OnWakeDelegate OnWake;
        internal void RaiseOnWake(string sender=null, object data=null)
        {
            if (OnWake != null)
            {
                OnWake.BeginInvoke(sender, data, null, null);
            }
        }
   }
}
