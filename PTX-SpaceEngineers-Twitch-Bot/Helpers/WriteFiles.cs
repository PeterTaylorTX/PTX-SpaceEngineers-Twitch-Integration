using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTX_SpaceEngineers_Twitch_Bot.Helpers
{
    class WriteFiles
    {
        public const string file_Subscribers = "PTX_Twitch_Subscribers.txt";
        public const string file_lastSubscriber = "PTX_Twitch_lastSubscriber.txt";
        public const string file_lastChatMessage = "PTX_Twitch_lastChatMessage.txt";
        public const string file_ChatMessages = "PTX_Twitch_ChatMessages.txt";
        public const string file_TwitchChannel = "PTX_Twitch_Channel.txt";

        public static void writeSpaceEngineersFiles(string filename, string data, bool tryAgain = true)
        {
            try
            {
                System.IO.File.WriteAllText($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\SpaceEngineers\\Storage\\PTX-Twitch_Chat_Integration\\{filename}", data);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] {ex.Message}");
                if (tryAgain)
                {
                    System.Threading.Thread.Sleep(1000);
                    writeSpaceEngineersFiles(filename, data, false);
                }
            }
        }

        public static bool fileExistsSpaceEngineersFiles(string filename)
        {
            try
            {
                return System.IO.File.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\SpaceEngineers\\Storage\\PTX-Twitch_Chat_Integration\\{filename}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] {ex.Message}");
            }
            return false;
        }

    }
}
