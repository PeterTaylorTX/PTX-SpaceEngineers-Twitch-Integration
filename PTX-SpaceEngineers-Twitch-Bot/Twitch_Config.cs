﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTX_SpaceEngineers_Twitch_Bot
{
    public class Twitch_Config
    {
        /// <summary>
        /// The Twitch Client ID
        /// </summary>
        public string? clientID { get; set; }
        /// <summary>
        /// Twitch Channel Name
        /// </summary>
        public string? channelName { get; set; }
        /// <summary>
        /// OAuth Token
        /// </summary>
        public string? OAuthToken { get; set; }
        /// <summary>
        /// The number of chat messages to keep for the chat log
        /// </summary>
        public Int32? numberOfChatMessages { get; set; }
        /// <summary>
        /// The config file location
        /// </summary>
        private static string configFile = $"{Environment.CurrentDirectory}/_TwitchConfig.cfg";

        public Twitch_Config()
        {

        }

        /// <summary>
        /// Load Config from file
        /// </summary>
        /// <returns></returns>
        public static Twitch_Config LoadConfig()
        {
            Twitch_Config? value = null;
            try
            {
                if (!System.IO.File.Exists(configFile)) { WriteNewConfig(); } // Make new config file is none exists
                string configData = System.IO.File.ReadAllText(configFile);

                value = Newtonsoft.Json.JsonConvert.DeserializeObject<Twitch_Config>(configData);
                bool newConfig = false;
                if (value.channelName.Contains("Twitch Channel Name")) { value.getChannelName(); newConfig = true; }
                if (value.OAuthToken.Contains("OAuth Token")) { value.getToken(); newConfig = true; }
                if (newConfig)
                {
                    Console.Clear();
                    Console.WriteLine($"Config File Saved at {Path.GetFullPath(configFile)}");
                }

                if (value == null) { return new Twitch_Config(); }
                return value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                if (value == null) { return new Twitch_Config(); }
                return value;
            }
        }

        /// <summary>
        /// Write a new empty config file
        /// </summary>
        public void WriteConfig()
        {
            try
            {
                string configData = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
                System.IO.File.WriteAllText(configFile, configData);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured");
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Write a new empty config file
        /// </summary>
        public static void WriteNewConfig()
        {
            try
            {
                Twitch_Config config = new Twitch_Config()
                {
                    clientID = "t2ln9gd0x4gqgp9na1pxdeh0s2otft", // Public Client ID for this app, change if you want to run your own app version
                    channelName = "Twitch Channel Name",
                    OAuthToken = "OAuth Token",
                    numberOfChatMessages = 5
                };
                string configData = Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
                System.IO.File.WriteAllText(configFile, configData);
                Console.WriteLine($"Config File Saved at {Path.GetFullPath(configFile)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured");
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Get the auth token
        /// </summary>
        public void getToken()
        {
            try
            {
                string authScopes = "chat:edit+chat:read+channel:read:subscriptions";
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"https://id.twitch.tv/oauth2/authorize?client_id={this.clientID}&response_type=token&scope={authScopes}&redirect_uri=http://localhost",
                    UseShellExecute = true
                });
                Console.WriteLine("Please enter the Access Token:");
                this.OAuthToken = Console.ReadLine();
                this.WriteConfig();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        /// <summary>
        /// Get the Channel Name
        /// </summary>
        public void getChannelName()
        {
            try
            {
                Console.WriteLine("Please enter the Channel Name:");
                this.channelName = Console.ReadLine();
                this.WriteConfig();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
}