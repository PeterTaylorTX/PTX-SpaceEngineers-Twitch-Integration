using System.Diagnostics;

namespace PTX_SpaceEngineers_Twitch_Bot.Helpers
{
    class WriteFiles
    {
        public const string file_Subscribers = "PTX_Twitch_Subscribers.txt";
        public const string file_lastSubscriber = "PTX_Twitch_lastSubscriber.txt";
        public const string file_lastChatMessage = "PTX_Twitch_lastChatMessage.txt";
        public const string file_ChatMessages = "PTX_Twitch_ChatMessages.txt";
        public const string file_TwitchChannel = "PTX_Twitch_Channel.txt";
        static string folderName = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\SpaceEngineers\\Storage\\3039895610.sbm_Chat_Integration";

        public static void writeSpaceEngineersFiles(string filename, string data, bool tryAgain = true)
        {
            try
            {
                System.IO.File.WriteAllText($"{folderName}\\{filename}", data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                if (tryAgain)
                {
                    System.Threading.Thread.Sleep(1000);
                    writeSpaceEngineersFiles(filename, data, false);
                }
            }
        }

        protected static void checkFolder()
        {
            try
            {
                if (!System.IO.Directory.Exists($"{folderName}"))
                {
                    System.IO.Directory.CreateDirectory($"{folderName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
        }

        public static bool fileExistsSpaceEngineersFiles(string filename)
        {
            try
            {
                checkFolder();
                return System.IO.File.Exists($"{folderName}\\{filename}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
            return false;
        }

    }
}
