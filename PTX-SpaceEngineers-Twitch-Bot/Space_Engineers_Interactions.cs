using Newtonsoft.Json.Linq;
using PTX_SpaceEngineers_Twitch_Bot.Structures;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PTX_SpaceEngineers_Twitch_Bot.Structures.Interaction_Settings;

namespace PTX_SpaceEngineers_Twitch_Bot
{
    public class Space_Engineers_Interactions
    {
        /// <summary>
        /// The config file location
        /// </summary>
        private static string configFile = $"{Environment.CurrentDirectory}/_InteractionConfig.json";

        static Dictionary<string, Structures.Interaction_Settings> keyboardCommands = new Dictionary<string, Structures.Interaction_Settings>();

        /// <summary>
        /// Game interaction via Commands
        /// </summary>
        /// <param name="command">The issued command</param>
        /// <returns></returns>
        public static bool Game_Interaction(string command, string User)
        {
            if (keyboardCommands.ContainsKey(command))
            {
                if (command == "example") { return true; }
                Structures.Interaction_Settings selectedCommand = keyboardCommands[command];
                if (selectedCommand.TriggerMode.ToLower() == "command" || selectedCommand.TriggerMode.ToLower() == "all")
                {
                    if (inCooldown(selectedCommand, User)) { throw new Space_Engineers_Interactions_Cooldown_Exception(command, selectedCommand, User); } //Don't run if in cooldown

                    Int32 duration = 0;
                    if (command.StartsWith("move")) { duration = 5000; }
                    Helpers.Game_Interaction.Interact(selectedCommand.Key, duration);

                    ApplyCooldown(selectedCommand, User);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Game interaction via Bits
        /// </summary>
        /// <param name="Bits">The bits amount</param>
        /// <returns></returns>
        public static bool Game_Interaction(Int32 Bits)
        {
            foreach (KeyValuePair<string, Structures.Interaction_Settings> command in keyboardCommands.Where(c => (c.Value.TriggerMode.ToLower() == "bits" || c.Value.TriggerMode.ToLower() == "all") && c.Value.BitsPrice == Bits))
            {
                Int32 duration = 0;
                if (command.Key.StartsWith("move")) { duration = 5000; }
                Helpers.Game_Interaction.Interact(command.Key, duration);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Apply the scooldown to the command
        /// </summary>
        /// <param name="selectedCommand">The command</param>
        /// <param name="user">The user</param>
        private static void ApplyCooldown(Interaction_Settings selectedCommand, string user)
        {
            selectedCommand.Global_Cooldown_Time = DateTime.Now.AddSeconds(selectedCommand.Cooldown_Global_Seconds); // Calculate Global Cooldown
            if (selectedCommand.lastRunUser.ContainsKey(user))
            {
                selectedCommand.lastRunUser[user] = DateTime.Now.AddSeconds(selectedCommand.CoolDown_User_Seconds);
            }
            else
            {
                selectedCommand.lastRunUser.Add(user, DateTime.Now.AddSeconds(selectedCommand.CoolDown_User_Seconds));
            }
        }

        /// <summary>
        /// Is the command in cooldown?
        /// </summary>
        /// <param name="selectedCommand">The Command</param>
        /// <param name="user">The requesting user</param>
        private static bool inCooldown(Interaction_Settings selectedCommand, string user)
        {
            if (selectedCommand.Global_Cooldown_Time > DateTime.Now) { Console.WriteLine($"[COOLDOWN] Command in global cooldown"); return true; }
            if (!selectedCommand.lastRunUser.ContainsKey(user)) { return false; }
            DateTime userCooldown = selectedCommand.lastRunUser[user];
            if (userCooldown > DateTime.Now) { Console.WriteLine($"[COOLDOWN] Command in user cooldown"); return true; }
            return false;
        }

        /// <summary>
        /// Setup redeems
        /// </summary>
        /// <returns></returns>
        public static bool Setup()
        {
            if (!System.IO.File.Exists(configFile)) { UpdateConfig(); } // Make new config file is none exists
            string configData = System.IO.File.ReadAllText(configFile);
            if (configData == null) { return false; }
            keyboardCommands = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Structures.Interaction_Settings>>(configData) ?? new Dictionary<string, Structures.Interaction_Settings>();
            UpdateConfig(); // Update the config file 
            return true;
        }

        /// <summary>
        /// Write a new config file
        /// </summary>
        private static void UpdateConfig()
        {
            AddKeyBoardCommand("example", new Structures.Interaction_Settings() { Key = "X", TriggerMode = "Command|Bits|All|None", BitsPrice = 101 });
            AddKeyBoardCommand("jetpack", new Structures.Interaction_Settings() { Key = "X", TriggerMode = "Command", BitsPrice = 0 });
            AddKeyBoardCommand("dampeners", new Structures.Interaction_Settings() { Key = "Z", TriggerMode = "Command", BitsPrice = 0 });
            AddKeyBoardCommand("helmet", new Structures.Interaction_Settings() { Key = "J", TriggerMode = "Command", BitsPrice = 0 });
            AddKeyBoardCommand("moveforward", new Structures.Interaction_Settings() { Key = "W", TriggerMode = "Command", BitsPrice = 0 });
            AddKeyBoardCommand("moveback", new Structures.Interaction_Settings() { Key = "S", TriggerMode = "Command", BitsPrice = 0 });
            AddKeyBoardCommand("moveleft", new Structures.Interaction_Settings() { Key = "A", TriggerMode = "Command", BitsPrice = 0 });
            AddKeyBoardCommand("moveright", new Structures.Interaction_Settings() { Key = "D", TriggerMode = "Command", BitsPrice = 0 });
            AddKeyBoardCommand("moveup", new Structures.Interaction_Settings() { Key = " ", TriggerMode = "Command", BitsPrice = 0 });
            AddKeyBoardCommand("movedown", new Structures.Interaction_Settings() { Key = "C", TriggerMode = "Command", BitsPrice = 0 });
            AddKeyBoardCommand("power", new Structures.Interaction_Settings() { Key = "Y", TriggerMode = "Command", BitsPrice = 0 });
            AddKeyBoardCommand("interact", new Structures.Interaction_Settings() { Key = "F", TriggerMode = "Command", BitsPrice = 0 });
            AddKeyBoardCommand("toolbaritem", new Structures.Interaction_Settings() { Key = "1", TriggerMode = "Command", BitsPrice = 0 });
            AddKeyBoardCommand("toolbar+", new Structures.Interaction_Settings() { Key = ".", TriggerMode = "Command", BitsPrice = 0 });
            AddKeyBoardCommand("toolbar-", new Structures.Interaction_Settings() { Key = ",", TriggerMode = "Command", BitsPrice = 0 });
            AddKeyBoardCommand("leftmouse", new Structures.Interaction_Settings() { Key = "MOUSE_LEFT", TriggerMode = "Command", BitsPrice = 0 });
            AddKeyBoardCommand("rightmouse", new Structures.Interaction_Settings() { Key = "MOUSE_RIGHT", TriggerMode = "Command", BitsPrice = 0 });
            AddKeyBoardCommand("inventory", new Structures.Interaction_Settings() { Key = "I", TriggerMode = "Command", BitsPrice = 0 });
            AddKeyBoardCommand("respawn", new Structures.Interaction_Settings() { Key = "BACKSPACE", TriggerMode = "None", BitsPrice = 500 });
            AddKeyBoardCommand("camera", new Structures.Interaction_Settings() { Key = "V", TriggerMode = "Command", BitsPrice = 0 });
            AddKeyBoardCommand("park", new Structures.Interaction_Settings() { Key = "P", TriggerMode = "Command", BitsPrice = 0 });

            string configData = Newtonsoft.Json.JsonConvert.SerializeObject(keyboardCommands, Newtonsoft.Json.Formatting.Indented);
            System.IO.File.WriteAllText(configFile, configData);
            Console.WriteLine($"Interaction Config File Saved at {Path.GetFullPath(configFile)}");
        }

        /// <summary>
        /// Add command to the list
        /// </summary>
        /// <param name="name">Name of the command</param>
        /// <param name="value">The command event</param>
        protected static void AddKeyBoardCommand(string name, Structures.Interaction_Settings value)
        {
            if (keyboardCommands == null) { keyboardCommands = new Dictionary<string, Structures.Interaction_Settings>(); }
            if (!keyboardCommands.ContainsKey(name)) { keyboardCommands.Add(name, value); }
        }


    }

    public class Space_Engineers_Interactions_Cooldown_Exception : Exception
    {
        public string Command { get; }
        public Structures.Interaction_Settings Settings { get; }
        public Int32 RemainingCooldown { get; }
        public Space_Engineers_Interactions_Cooldown_Exception(string command, Structures.Interaction_Settings commandSettings, String user)
        {
            Command = command;
            Settings = commandSettings;
            RemainingCooldown = (Int32)(commandSettings.Global_Cooldown_Time - DateTime.Now).TotalSeconds;

            if (commandSettings.lastRunUser.ContainsKey(user))
            {
                DateTime userCooldown = commandSettings.lastRunUser[user];
                Int32 userCooldownSeconds = (Int32)(userCooldown - DateTime.Now).TotalSeconds;
                if(userCooldownSeconds > RemainingCooldown) { RemainingCooldown = userCooldownSeconds; }
            }
        }

    }
}
