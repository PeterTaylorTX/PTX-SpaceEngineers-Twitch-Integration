using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTX_SpaceEngineers_Twitch_Bot.Structures
{
    public class Interaction_Settings
    {
        public String Key { get; set; }
        public string TriggerMode { get; set; } = string.Empty;
        public Decimal BitsPrice { get; set; }
        public Int32 CoolDown_User_Seconds { get; set;} = 0;
        public Int32 Cooldown_Global_Seconds { get; set; } = 120;
        public bool Cooldown_Applies_to_Bits { get; set; } = false;



        [Newtonsoft.Json.JsonIgnore]
        public DateTime Global_Cooldown_Time = DateTime.MinValue;
        [Newtonsoft.Json.JsonIgnore]
        public Dictionary<string, DateTime> lastRunUser = new Dictionary<string, DateTime>();
    }

}
