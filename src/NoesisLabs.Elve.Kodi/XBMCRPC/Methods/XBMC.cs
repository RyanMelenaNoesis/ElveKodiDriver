using System;
using Newtonsoft.Json.Linq;
namespace XBMCRPC.Methods
{
   public partial class XBMC
   {
        private readonly Client _client;
          public XBMC(Client client)
          {
              _client = client;
          }

                /// <summary>
                /// Retrieve info booleans about Kodi and the system
                /// </summary>
        public XBMCRPC.XBMC.GetInfoBooleansResponse GetInfoBooleans(global::System.Collections.Generic.List<string> booleans=null)
        {
            var jArgs = new JObject();
             if (booleans != null)
             {
                 var jpropbooleans = JToken.FromObject(booleans, _client.Serializer);
                 jArgs.Add(new JProperty("booleans", jpropbooleans));
             }
            return _client.GetData<XBMCRPC.XBMC.GetInfoBooleansResponse>("XBMC.GetInfoBooleans", jArgs);
        }

                /// <summary>
                /// Retrieve info labels about Kodi and the system
                /// </summary>
        public XBMCRPC.XBMC.GetInfoLabelsResponse GetInfoLabels(global::System.Collections.Generic.List<string> labels=null)
        {
            var jArgs = new JObject();
             if (labels != null)
             {
                 var jproplabels = JToken.FromObject(labels, _client.Serializer);
                 jArgs.Add(new JProperty("labels", jproplabels));
             }
            return _client.GetData<XBMCRPC.XBMC.GetInfoLabelsResponse>("XBMC.GetInfoLabels", jArgs);
        }
   }
}
