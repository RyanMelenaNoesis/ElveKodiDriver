using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
namespace XBMCRPC.Player
{
   public class GetPlayersResponseItem
   {
       public string name { get; set; }
       public int playercoreid { get; set; }
       public bool playsaudio { get; set; }
       public bool playsvideo { get; set; }
       public XBMCRPC.Player.GetPlayersResponseItem_type type { get; set; }
    }
}
