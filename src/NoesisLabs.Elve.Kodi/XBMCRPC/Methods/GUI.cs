using System;
using Newtonsoft.Json.Linq;
namespace XBMCRPC.Methods
{
   public partial class GUI
   {
        private readonly Client _client;
          public GUI(Client client)
          {
              _client = client;
          }

                /// <summary>
                /// Activates the given window
                /// </summary>
        public string ActivateWindow(XBMCRPC.GUI.Window window=0, global::System.Collections.Generic.List<string> parameters=null)
        {
            var jArgs = new JObject();
             if (window != null)
             {
                 var jpropwindow = JToken.FromObject(window, _client.Serializer);
                 jArgs.Add(new JProperty("window", jpropwindow));
             }
             if (parameters != null)
             {
                 var jpropparameters = JToken.FromObject(parameters, _client.Serializer);
                 jArgs.Add(new JProperty("parameters", jpropparameters));
             }
            return _client.GetData<string>("GUI.ActivateWindow", jArgs);
        }

                /// <summary>
                /// Retrieves the values of the given properties
                /// </summary>
        public XBMCRPC.GUI.Property.Value GetProperties(XBMCRPC.GUI.GetProperties_properties properties=null)
        {
            var jArgs = new JObject();
             if (properties != null)
             {
                 var jpropproperties = JToken.FromObject(properties, _client.Serializer);
                 jArgs.Add(new JProperty("properties", jpropproperties));
             }
            return _client.GetData<XBMCRPC.GUI.Property.Value>("GUI.GetProperties", jArgs);
        }

                /// <summary>
                /// Returns the supported stereoscopic modes of the GUI
                /// </summary>
        public XBMCRPC.GUI.GetStereoscopicModesResponse GetStereoscopicModes()
        {
            var jArgs = new JObject();
            return _client.GetData<XBMCRPC.GUI.GetStereoscopicModesResponse>("GUI.GetStereoscopicModes", jArgs);
        }

                /// <summary>
                /// Toggle fullscreen/GUI
                /// </summary>
        public bool SetFullscreen(bool fullscreen)
        {
            var jArgs = new JObject();
             if (fullscreen != null)
             {
                 var jpropfullscreen = JToken.FromObject(fullscreen, _client.Serializer);
                 jArgs.Add(new JProperty("fullscreen", jpropfullscreen));
             }
            return _client.GetData<bool>("GUI.SetFullscreen", jArgs);
        }

                /// <summary>
                /// Toggle fullscreen/GUI
                /// </summary>
        public bool SetFullscreen(XBMCRPC.Global.Toggle2 fullscreen)
        {
            var jArgs = new JObject();
             if (fullscreen != null)
             {
                 var jpropfullscreen = JToken.FromObject(fullscreen, _client.Serializer);
                 jArgs.Add(new JProperty("fullscreen", jpropfullscreen));
             }
            return _client.GetData<bool>("GUI.SetFullscreen", jArgs);
        }

                /// <summary>
                /// Toggle fullscreen/GUI
                /// </summary>
        public bool SetFullscreen()
        {
            var jArgs = new JObject();
            return _client.GetData<bool>("GUI.SetFullscreen", jArgs);
        }

                /// <summary>
                /// Sets the stereoscopic mode of the GUI to the given mode
                /// </summary>
        public string SetStereoscopicMode(XBMCRPC.GUI.SetStereoscopicMode_mode mode=0)
        {
            var jArgs = new JObject();
             if (mode != null)
             {
                 var jpropmode = JToken.FromObject(mode, _client.Serializer);
                 jArgs.Add(new JProperty("mode", jpropmode));
             }
            return _client.GetData<string>("GUI.SetStereoscopicMode", jArgs);
        }

                /// <summary>
                /// Shows a GUI notification
                /// </summary>
        public string ShowNotification(XBMCRPC.GUI.ShowNotification_image1 image, string title=null, string message=null, int displaytime=0)
        {
            var jArgs = new JObject();
             if (title != null)
             {
                 var jproptitle = JToken.FromObject(title, _client.Serializer);
                 jArgs.Add(new JProperty("title", jproptitle));
             }
             if (message != null)
             {
                 var jpropmessage = JToken.FromObject(message, _client.Serializer);
                 jArgs.Add(new JProperty("message", jpropmessage));
             }
             if (image != null)
             {
                 var jpropimage = JToken.FromObject(image, _client.Serializer);
                 jArgs.Add(new JProperty("image", jpropimage));
             }
             if (displaytime != null)
             {
                 var jpropdisplaytime = JToken.FromObject(displaytime, _client.Serializer);
                 jArgs.Add(new JProperty("displaytime", jpropdisplaytime));
             }
            return _client.GetData<string>("GUI.ShowNotification", jArgs);
        }

                /// <summary>
                /// Shows a GUI notification
                /// </summary>
        public string ShowNotification(string image, string title=null, string message=null, int displaytime=0)
        {
            var jArgs = new JObject();
             if (title != null)
             {
                 var jproptitle = JToken.FromObject(title, _client.Serializer);
                 jArgs.Add(new JProperty("title", jproptitle));
             }
             if (message != null)
             {
                 var jpropmessage = JToken.FromObject(message, _client.Serializer);
                 jArgs.Add(new JProperty("message", jpropmessage));
             }
             if (image != null)
             {
                 var jpropimage = JToken.FromObject(image, _client.Serializer);
                 jArgs.Add(new JProperty("image", jpropimage));
             }
             if (displaytime != null)
             {
                 var jpropdisplaytime = JToken.FromObject(displaytime, _client.Serializer);
                 jArgs.Add(new JProperty("displaytime", jpropdisplaytime));
             }
            return _client.GetData<string>("GUI.ShowNotification", jArgs);
        }

                /// <summary>
                /// Shows a GUI notification
                /// </summary>
        public string ShowNotification(string title=null, string message=null, int displaytime=0)
        {
            var jArgs = new JObject();
             if (title != null)
             {
                 var jproptitle = JToken.FromObject(title, _client.Serializer);
                 jArgs.Add(new JProperty("title", jproptitle));
             }
             if (message != null)
             {
                 var jpropmessage = JToken.FromObject(message, _client.Serializer);
                 jArgs.Add(new JProperty("message", jpropmessage));
             }
             if (displaytime != null)
             {
                 var jpropdisplaytime = JToken.FromObject(displaytime, _client.Serializer);
                 jArgs.Add(new JProperty("displaytime", jpropdisplaytime));
             }
            return _client.GetData<string>("GUI.ShowNotification", jArgs);
        }

        public delegate void OnDPMSActivatedDelegate(string sender=null, object data=null);
        public event OnDPMSActivatedDelegate OnDPMSActivated;
        internal void RaiseOnDPMSActivated(string sender=null, object data=null)
        {
            if (OnDPMSActivated != null)
            {
                OnDPMSActivated.BeginInvoke(sender, data, null, null);
            }
        }

        public delegate void OnDPMSDeactivatedDelegate(string sender=null, object data=null);
        public event OnDPMSDeactivatedDelegate OnDPMSDeactivated;
        internal void RaiseOnDPMSDeactivated(string sender=null, object data=null)
        {
            if (OnDPMSDeactivated != null)
            {
                OnDPMSDeactivated.BeginInvoke(sender, data, null, null);
            }
        }

        public delegate void OnScreensaverActivatedDelegate(string sender=null, object data=null);
        public event OnScreensaverActivatedDelegate OnScreensaverActivated;
        internal void RaiseOnScreensaverActivated(string sender=null, object data=null)
        {
            if (OnScreensaverActivated != null)
            {
                OnScreensaverActivated.BeginInvoke(sender, data, null, null);
            }
        }

        public delegate void OnScreensaverDeactivatedDelegate(string sender=null, XBMCRPC.GUI.OnScreensaverDeactivated_data data=null);
        public event OnScreensaverDeactivatedDelegate OnScreensaverDeactivated;
        internal void RaiseOnScreensaverDeactivated(string sender=null, XBMCRPC.GUI.OnScreensaverDeactivated_data data=null)
        {
            if (OnScreensaverDeactivated != null)
            {
                OnScreensaverDeactivated.BeginInvoke(sender, data, null, null);
            }
        }
   }
}
