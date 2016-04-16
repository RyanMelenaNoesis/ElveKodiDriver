using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
namespace XBMCRPC.Player
{
   public enum GetPlayersResponseItem_type
   {
       [global::System.Runtime.Serialization.EnumMember(Value="internal")]
       Internal,
       external,
       remote,
   }
}
