using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
namespace XBMCRPC.Setting.Details
{
   public class ControlRange : XBMCRPC.Setting.Details.ControlBase
   {
       public string formatlabel { get; set; }
       public string formatvalue { get; set; }
       public XBMCRPC.Setting.Details.ControlRange_type type { get; set; }
    }
}
