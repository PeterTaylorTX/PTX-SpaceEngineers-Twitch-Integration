using System.Diagnostics;
using System.Text;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;


namespace PTX_SpaceEngineers_Twitch_Bot
{
    public class Bot
    {
        /// <summary>
        /// Twitch conne connection configuration
        /// </summary>
        public Twitch_Config config;
        /// <summary>
        /// Twitch Client
        /// </summary>
        public TwitchLib.Client.TwitchClient client;
        Twitch_API? twitchAPI;
        /// <summary>
        /// Is the app existing
        /// </summary>
        bool exitApp = false;

        List<string> lastChatMessages = new List<string>();

        /// <summary>
        /// Setup the bot
        /// </summary>
        public Bot()
        {
            Console.WriteLine("Initialising");
            this.config = Twitch_Config.LoadConfig();

            if (!Helpers.WriteFiles.fileExistsSpaceEngineersFiles(Helpers.WriteFiles.file_Subscribers)) { Helpers.WriteFiles.writeSpaceEngineersFiles(Helpers.WriteFiles.file_Subscribers, string.Empty); }
            if (!Helpers.WriteFiles.fileExistsSpaceEngineersFiles(Helpers.WriteFiles.file_lastSubscriber)) { Helpers.WriteFiles.writeSpaceEngineersFiles(Helpers.WriteFiles.file_lastSubscriber, string.Empty); }
            if (!Helpers.WriteFiles.fileExistsSpaceEngineersFiles(Helpers.WriteFiles.file_lastChatMessage)) { Helpers.WriteFiles.writeSpaceEngineersFiles(Helpers.WriteFiles.file_lastChatMessage, string.Empty); }
            if (!Helpers.WriteFiles.fileExistsSpaceEngineersFiles(Helpers.WriteFiles.file_ChatMessages)) { Helpers.WriteFiles.writeSpaceEngineersFiles(Helpers.WriteFiles.file_ChatMessages, string.Empty); }
            Helpers.WriteFiles.writeSpaceEngineersFiles(Helpers.WriteFiles.file_TwitchChannel, this.config.channelName ?? string.Empty);
            
            client = new TwitchLib.Client.TwitchClient();
        }

        /// <summary>
        /// Start the bot
        /// </summary>
        public void Start()
        {
            this.connect();
        }


        /// <summary>
        /// Connect to Twitch Chat
        /// </summary>
        public void connect()
        {
            try
            {
                client.Initialize(new ConnectionCredentials(this.config.channelName, this.config.OAuthToken), this.config.channelName);
                Helpers.WriteFiles.writeSpaceEngineersFiles(Helpers.WriteFiles.file_TwitchChannel, this.config.channelName ?? string.Empty);
                client.OnConnectionError += onConnectionError;
                client.OnError += onError;
                client.OnConnected += onConnected;
                client.OnJoinedChannel += onJoinedChannel;
                client.OnDisconnected += onDisconnected;
                client.OnFailureToReceiveJoinConfirmation += onFailureToReceiveJoinConfirmation;
                client.OnIncorrectLogin += onIncorrectLogin;
                client.OnMessageReceived += onMessageReceived;
                client.OnChatCleared += onChatClearedReceived;
                client.OnNewSubscriber += onSubscriberReceived;
                client.OnReSubscriber += onReSubscriberReceived;
                client.OnChatCommandReceived += onChatCommandReceived;
                client.Connect();
                while (!exitApp) { System.Threading.Thread.Sleep(1000); }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        /// <summary>
        /// Mod request to clear Chat messages
        /// </summary>
        private void onChatClearedReceived(object? sender, OnChatClearedArgs e)
        {
            Helpers.WriteFiles.writeSpaceEngineersFiles(Helpers.WriteFiles.file_lastChatMessage, string.Empty);
            Helpers.WriteFiles.writeSpaceEngineersFiles(Helpers.WriteFiles.file_ChatMessages, string.Empty);
            lastChatMessages.Clear();
        }

        /// <summary>
        /// Twitch Command Received
        /// </summary>
        private void onChatCommandReceived(object? sender, OnChatCommandReceivedArgs e)
        {
            Console.WriteLine($"[COMMAND] {e.Command.ChatMessage.DisplayName} | {e.Command.ChatMessage.Message}");
            string command = e.Command.CommandText.ToLower();
            if (command == $"ptx" && e.Command.ArgumentsAsString.ToLower() == "quit" && isValidUserForCommand(e.Command.ChatMessage, mods: true))
            {
                client.SendMessage(this.config.channelName, $"PTX Space Engineers Integration bot shutting down");
                Console.WriteLine("PTX Space Engineers Integration bot shutting down");
                this.exitApp = true;
            };

            if(command.ToLower() == "commands") { client.SendMessage(this.config.channelName, "Space Engineers bot Commands Guide: https://docs.google.com/spreadsheets/d/16qNz9PngmQSWH5euSEXnryt6pJpW4wvDKCZp4Pa7-Rg"); }
            if (command.ToLower() == "se")
            {
                if (!string.IsNullOrWhiteSpace(e.Command.ArgumentsAsString))
                {
                    if (e.Command.ArgumentsAsString.ToLower() == "commands") { client.SendMessage(this.config.channelName, "Space Engineers bot Commands Guide: https://docs.google.com/spreadsheets/d/16qNz9PngmQSWH5euSEXnryt6pJpW4wvDKCZp4Pa7-Rg"); }


                    try
                    {
                        bool handelled = Space_Engineers_Interactions.Game_Interaction(e.Command.ArgumentsAsString.ToLower(), e.Command.ChatMessage.Username);

                        if (handelled) { Console.WriteLine($"[EXECUTED] {e.Command.ArgumentsAsString} command | {e.Command.ChatMessage.Username}"); }
                        else { Console.WriteLine($"[NOT-EXECUTED] Command {e.Command.ArgumentsAsString}"); }
                    }
                    catch (Space_Engineers_Interactions_Cooldown_Exception ex) { client.SendMessage(this.config.channelName, $"Command {ex.Command} in cooldown for {ex.RemainingCooldown} seconds"); }
                    catch (Exception ex) { Console.WriteLine($"[ERROR] Command {e.Command.ArgumentsAsString} faled to execute. | Reason {ex.Message}"); }
                }
            }
        }

        /// <summary>
        /// Is the chatter allowed to use the command
        /// </summary>
        /// <param name="chatMessage">The sent message</param>
        /// <param name="mods">Are mods allowed to use the command</param>
        /// <param name="subs">Are subs allowed to use the command</param>
        /// <param name="vips">Are vips allowed to use the command</param>
        /// <param name="chatter">Are normal chatters allowed to use the command</param>
        /// <returns></returns>
        private bool isValidUserForCommand(ChatMessage chatMessage, bool mods = false, bool subs = false, bool vips = false, bool chatter = false)
        {
            if (chatMessage.IsBroadcaster) { return true; }
            if (chatMessage.IsModerator && mods) { return true; }
            if (chatMessage.IsVip && vips) { return true; }
            if (chatMessage.IsSubscriber && subs) { return true; }
            return false;
        }

        /// <summary>
        /// Chat Message Received
        /// </summary>
        private void onMessageReceived(object? sender, OnMessageReceivedArgs e)
        {
            string currentMessage = $"{e.ChatMessage.DisplayName}{Environment.NewLine}{e.ChatMessage.Message}";
            Helpers.WriteFiles.writeSpaceEngineersFiles(Helpers.WriteFiles.file_lastChatMessage, currentMessage);

            if (this.lastChatMessages.Count >= config.numberOfChatMessages) { this.lastChatMessages.RemoveAt(0); } //Remove the oldest message
            lastChatMessages.Add(currentMessage);
            StringBuilder sbChatMessages = new StringBuilder();
            lastChatMessages.ForEach(c => { sbChatMessages.AppendLine(c); sbChatMessages.AppendLine(""); });
            Helpers.WriteFiles.writeSpaceEngineersFiles(Helpers.WriteFiles.file_ChatMessages, sbChatMessages.ToString());

            Console.WriteLine($"{e.ChatMessage.DisplayName} | {e.ChatMessage.Message}");
        }

        /// <summary>
        /// Bot joined the channel
        /// </summary>
        private void onJoinedChannel(object? sender, OnJoinedChannelArgs e)
        {
            Space_Engineers_Interactions.Setup();
            Console.WriteLine($"joined {e.Channel}");
            Console.WriteLine($"You can now load your save file.");
            Console.WriteLine($"Have fun in Space Engineers!");
            if (!Debugger.IsAttached)
            {
                client.SendMessage(this.config.channelName, $"PTX Space Engineers bot is online");
            }
            this.twitchAPI = new Twitch_API(this);
        }

        /// <summary>
        /// Log in Error
        /// </summary>
        private void onIncorrectLogin(object? sender, OnIncorrectLoginArgs e)
        {
            Console.WriteLine($"[ERROR] Incorrect Log In | {e.Exception.Message}");
        }

        /// <summary>
        /// Join Channel confirmation not received
        /// </summary>
        private void onFailureToReceiveJoinConfirmation(object? sender, OnFailureToReceiveJoinConfirmationArgs e)
        {
            Console.WriteLine($"[ERROR] Failed to join channel {e.Exception.Channel ?? string.Empty}");
        }

        /// <summary>
        /// Disconnected from Twitch
        /// </summary>
        private void onDisconnected(object? sender, OnDisconnectedEventArgs e)
        {
            Console.WriteLine($"DISCONNECTED");
            this.exitApp = true;
        }

        /// <summary>
        /// Connected to Twitch
        /// </summary>
        private void onConnected(object? sender, OnConnectedArgs e)
        {
            Console.WriteLine($"CONNECTED as {e.BotUsername}");
        }

        /// <summary>
        /// On Twitch Error
        /// </summary>
        private void onError(object? sender, OnErrorEventArgs e)
        {
            Console.WriteLine($"[ERROR] {e.Exception.Message}");
        }

        /// <summary>
        /// Connection Error
        /// </summary>
        private void onConnectionError(object? sender, OnConnectionErrorArgs e)
        {
            Console.WriteLine($"[ERROR] Failed to connect: {e.BotUsername}");
        }

        /// <summary>
        /// Resub notification
        /// </summary>
        private async void onReSubscriberReceived(object? sender, OnReSubscriberArgs e)
        {
            Helpers.WriteFiles.writeSpaceEngineersFiles(Helpers.WriteFiles.file_lastSubscriber, e.ReSubscriber.DisplayName);
            await this.twitchAPI.updateSubsList();
        }
        /// <summary>
        /// New Sub notification
        /// </summary>
        private async void onSubscriberReceived(object? sender, OnNewSubscriberArgs e)
        {
            Helpers.WriteFiles.writeSpaceEngineersFiles(Helpers.WriteFiles.file_lastSubscriber, e.Subscriber.DisplayName);
            await this.twitchAPI.updateSubsList();
        }

        /// <summary>
        /// Event on Bits Used
        /// </summary>
        public void Pub_OnBitsReceivedV2(object? sender, TwitchLib.PubSub.Events.OnBitsReceivedV2Args e)
        {
            if (e.TotalBitsUsed == 0) { return; }
            bool handelled = Space_Engineers_Interactions.Game_Interaction(e.TotalBitsUsed);
        }
    }
}
