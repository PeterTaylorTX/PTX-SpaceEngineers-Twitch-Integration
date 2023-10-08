using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Engine.Platform;
using Sandbox.Game;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace PTX
{
    // This object is always present, from the world load to world unload.
    // NOTE: all clients and server run mod scripts, keep that in mind.
    // NOTE: this and gamelogic comp's update methods run on the main game thread, don't do too much in a tick or you'll lower sim speed.
    // NOTE: also mind allocations, avoid realtime allocations, re-use collections/ref-objects (except value types like structs, integers, etc).
    //
    // The MyUpdateOrder arg determines what update overrides are actually called.
    // Remove any method that you don't need, none of them are required, they're only there to show what you can use.
    // Also remove all comments you've read to avoid the overload of comments that is this file.
    //[MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TextPanel), false)]
    public class PTX_Twitch_SE_Mod : MyGameLogicComponent
    {
        /// <summary>
        /// The Steam Name of the person running the Twitch bot
        /// </summary>
        string streamer;
        /// <summary>
        /// The last time the streamer was seen by the Twitch Bot
        /// </summary>
        DateTime streamerLastSeen;
        MyObjectBuilder_EntityBase m_objectBuilder = null;
        /// <summary>
        /// The in-game block 
        /// </summary>
        internal Sandbox.ModAPI.Ingame.IMyTextPanel block;
        /// <summary>
        /// Number of text lines removed for scrolling
        /// </summary>
        internal Int32 textLinesRemoved = -2;
        /// <summary>
        /// The last error
        /// </summary>
        public static string lastErrorMessage { get; set; }
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            try
            {
                m_objectBuilder = objectBuilder;
                streamer = readFileFromHDD("PTX_Twitch_Channel.txt");
                //if (streamer.ToLower().Contains("file not found")) { return; } // Make mod inactive if the Twitch bot is not running

                this.NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;
                this.block = (Sandbox.ModAPI.Ingame.IMyTextPanel)this.Entity;
                MyLog.Default.WriteLineAndConsole($"[PTX_INFO] LOADED");
                writeFileToHDD("info.txt", "Find the mod author live on https://twitch.tv/PetereTaylorTX");
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"[ERROR]{e.Message}\n{e.StackTrace}");
            }
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();

            try
            {
                streamer = readFileFromHDD("PTX_Twitch_Channel.txt");
                //if (String.IsNullOrWhiteSpace(streamer) || streamer.ToLower().Contains("file not found")) { return; } // Make mod inactive if the Twitch bot is not running

                if (!block.DisplayNameText.ToLower().Contains("ptx-twitch")) { return; }
                if (MyAPIGateway.Session.Player == null) { return; }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"[ERROR]{e.Message}\n{e.StackTrace}");
            }

            try
            {
                StringBuilder output = new StringBuilder();
                block.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;

                Int32 intActiveCommands = 0;
                foreach (string commandLine in this.block.CustomData.ToLower().Split(NewLines(), StringSplitOptions.None))
                {
                    if (commandLine.Contains("@subscribers")) { output = GetSubscribers(output); intActiveCommands += 1; }
                    if (commandLine.Contains("@lastsubscriber")) { output = GetLastSubscriber(output); intActiveCommands += 1; }
                    if (commandLine.Contains("@lastchatmessage")) { output = GetLastChatMessage(output); intActiveCommands += 1; }
                    if (commandLine.Contains("@chatmessages")) { output = GetChatMessages(output); intActiveCommands += 1; }
                    if (commandLine.Contains("@echo")) { output.AppendLine(); }
                    if (commandLine.Contains("@text=")) { output.Append(commandLine.Replace("@text=", string.Empty)); }
                    if (commandLine.Contains("@debug")) { output = GetDebugInfo(output); intActiveCommands += 1; }
                }



                if (output.Length == 0)
                {
                    output.AppendLine("No Custom Data specified");
                    output.AppendLine("Options:");
                    output.AppendLine("@Subscribers - A list of Subscribers");
                    output.AppendLine("@LastSubscriber - The last Subscriber");
                    output.AppendLine("@ChatMessages - The last few chat messages");
                    output.AppendLine("@LastChatMessage - The last chat message");
                    output.AppendLine("@echo - Add a blank line");
                    output.AppendLine("@text=The this text");
                }

                TextProcessing txtProc = new TextProcessing(this, intActiveCommands);
                block.WriteText(txtProc.Text(output.ToString()), false);
            }
            catch (Exception e) // NOTE: never use try-catch for code flow or to ignore errors! catching has a noticeable performance impact.
            {
                MyLog.Default.WriteLineAndConsole($"[ERROR]{e.Message}\n{e.StackTrace}");
                lastErrorMessage = e.Message;

                if (MyAPIGateway.Session?.Player != null)
                    MyAPIGateway.Utilities.ShowNotification($"[ ERROR: {GetType().FullName}: {e.Message} | Send SpaceEngineers.Log to mod author]", 10000, MyFontEnum.Red);
            }
        }

        private StringBuilder GetDebugInfo(StringBuilder output)
        {
            try
            {
                output.AppendLine("Debug Info");
                output.AppendLine("==========");
                output.AppendLine($"Streamer: {streamer}");
                if (this.NeedsUpdate == MyEntityUpdateEnum.EACH_100TH_FRAME) { output.AppendLine($"Update Interval: Every 100 ticks"); }
                output.AppendLine($"Last Error: {lastErrorMessage}");

                //if (values == null) { return output; }

                //foreach (string value in values)
                //{
                //    output.AppendLine(value);
                //}

            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"[ERROR]{e.Message}\n{e.StackTrace}");
            }

            return output;
        }

        /// <summary>
        /// Get the subscribers list
        /// </summary>
        /// <param name="output"></param>
        protected StringBuilder GetSubscribers(StringBuilder output)
        {
            try
            {
                string rawText = readFileFromHDD("PTX_Twitch_Subscribers.txt");
                char[] delims = new[] { '\r', '\n' };
                string[] values = rawText.Split(delims, StringSplitOptions.RemoveEmptyEntries);


                output.AppendLine("Subscribers");
                output.AppendLine("==========");
                if (values == null) { return output; }

                foreach (string value in values)
                {
                    output.AppendLine(value);
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"[ERROR]{e.Message}\n{e.StackTrace}");
            }

            return output;
        }

        /// <summary>
        /// Last Subscriber
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        private StringBuilder GetLastSubscriber(StringBuilder output)
        {
            try
            {
                string value = readFileFromHDD("PTX_Twitch_lastSubscriber.txt");
                output.AppendLine("Last Subscriber");
                output.AppendLine("===============");
                output.AppendLine(value);
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"[ERROR]{e.Message}\n{e.StackTrace}");
            }
            return output;
        }

        /// <summary>
        /// Last Chat Message
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        private StringBuilder GetChatMessages(StringBuilder output)
        {
            try
            {
                string value = readFileFromHDD("PTX_Twitch_ChatMessages.txt");
                output.AppendLine("Twitch Chat");
                output.AppendLine("===========");
                output.AppendLine(value);
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"[ERROR]{e.Message}\n{e.StackTrace}");
            }
            return output;
        }

        /// <summary>
        /// Last Chat Message
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        private StringBuilder GetLastChatMessage(StringBuilder output)
        {
            try
            {
                string value = readFileFromHDD("PTX_Twitch_lastChatMessage.txt");
                output.AppendLine("Last Chat Message");
                output.AppendLine("=================");
                output.AppendLine(value);
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"[ERROR]{e.Message}\n{e.StackTrace}");
            }
            return output;
        }

        protected string readFileFromHDD(string filename)
        {
            try
            {
                if (!MyAPIGateway.Utilities.FileExistsInLocalStorage(filename, typeof(PTX_Twitch_SE_Mod)))
                {
                    lastErrorMessage = $"{filename} File not found";
                    return $"{filename} File not found";
                }
                var reader = MyAPIGateway.Utilities.ReadFileInLocalStorage(filename, typeof(PTX_Twitch_SE_Mod));
                string rawText = reader.ReadToEnd();
                reader.Close();
                return rawText;
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"[ERROR]{e.Message}\n{e.StackTrace}");
                return string.Empty;
            }
        }

        protected void writeFileToHDD(string filename, string data)
        {
            try
            {
                var writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(filename, typeof(PTX_Twitch_SE_Mod));
                writer.Write(data);
                writer.Flush();
                writer.Close();
                writer.Dispose();
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"[ERROR]{e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// Need a way to know the person has exited, or another way to see if the twitch bot is active
        /// </summary>
        private void game_unloading()
        {
            streamer = string.Empty;
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return m_objectBuilder;
        }

        /// <summary>
        /// New line chars
        /// </summary>
        protected string[] NewLines()
        {
            string[] delim = { Environment.NewLine, "\n" }; // "\n" added in case you manually appended a newline
            return delim;
        }

    }






    public class TextProcessing
    {
        /// <summary>
        /// The target block
        /// </summary>
        PTX_Twitch_SE_Mod myObject;
        /// <summary>
        /// The number of active commands
        /// </summary>
        Int32 intActiveCommands;
        /// <summary>
        /// Process text for a block
        /// </summary>
        /// <param name="Block">The target block</param>
        public TextProcessing(PTX_Twitch_SE_Mod MyObject, Int32 IntActiveCommands) { this.myObject = MyObject; this.intActiveCommands = IntActiveCommands; }

        /// <summary>
        /// Process and wrap text as needed
        /// </summary>
        /// <param name="text">The text to test</param>
        /// <param name="font">Selected font</param>
        /// <param name="scale">Fone Size</param>
        /// <returns></returns>
        public String Text(String text)
        {
            StringBuilder newText = new StringBuilder();

            Vector2 blockSize = this.myObject.block.SurfaceSize;

            // Wrap Text
            foreach (string row in text.Split(NewLines(), StringSplitOptions.None))
            {
                newText.Append(wrapText(new StringBuilder(row), this.myObject.block.Font, this.myObject.block.FontSize, blockSize));
                newText.AppendLine();
            }
            // Wrap Text

            //Scroll Text
            newText = this.scrollText(newText, this.myObject.block.Font, this.myObject.block.FontSize, blockSize);
            //Scroll Text

            return newText.ToString();
        }

        /// <summary>
        /// Wrap the text to fit the screen
        /// </summary>
        /// <param name="text">The text to test</param>
        /// <param name="font">Selected font</param>
        /// <param name="scale">Fone Size</param>
        /// <param name="blockSize">Size of the target block</param>
        protected StringBuilder wrapText(StringBuilder text, string font, float scale, Vector2 blockSize)
        {
            if (text.Length == 0) { return text; }
            Vector2 textSize = this.myObject.block.MeasureStringInPixels(text, font, scale);
            if (this.textToWide(textSize, blockSize))
            {
                string[] wordSeperator = new string[] { space };
                StringBuilder newText = new StringBuilder();
                StringBuilder lineText = new StringBuilder();
                foreach (string word in text.ToString().Split(wordSeperator, StringSplitOptions.None))
                {
                    lineText.Append(word);
                    lineText.Append(space);
                    Vector2 lineSize = this.myObject.block.MeasureStringInPixels(lineText, font, scale);

                    if (textToWide(lineSize, blockSize))
                    {
                        newText.AppendLine();
                        lineText.Clear();
                        lineText.Append(word);
                        lineText.Append(space);
                    }
                    newText.Append(word);
                    newText.Append(space);
                }
                return newText;
            }
            else
            {
                return text; // Text Fits
            }
        }

        /// <summary>
        /// Scroll the text if too tall for the screen
        /// </summary>
        /// <param name="text">The text to test</param>
        /// <param name="font">Selected font</param>
        /// <param name="scale">Fone Size</param>
        /// <param name="blockSize">Size of the target block</param>
        /// <returns></returns>
        protected StringBuilder scrollText(StringBuilder text, string font, float scale, Vector2 blockSize)
        {
            try
            {
                StringBuilder sbHeader = new StringBuilder();
                Int32 charsToRemoveForHeader = 0, charsToRemoveForDivider = 0;
                Vector2 headerSize = Vector2.Zero;
                // Remove the Header is it is the only active command
                if (this.intActiveCommands == 1)
                {
                    charsToRemoveForHeader = text.ToString().IndexOf(Environment.NewLine) + Environment.NewLine.Length;
                    sbHeader.Append(text.ToString(0, charsToRemoveForHeader));
                    charsToRemoveForDivider = text.ToString(charsToRemoveForHeader, text.Length - charsToRemoveForHeader).IndexOf(Environment.NewLine) + Environment.NewLine.Length;
                    sbHeader.Append(text.ToString(charsToRemoveForHeader, charsToRemoveForDivider));

                    text.Remove(0, charsToRemoveForHeader);
                    text.Remove(0, charsToRemoveForDivider);
                    headerSize = this.myObject.block.MeasureStringInPixels(sbHeader, font, scale);
                }
                // Remove the Header is it is the only active command


                StringBuilder originalText = new StringBuilder(text.ToString());
                if (text.Length == 0) { return text; }
                Vector2 textSize = this.myObject.block.MeasureStringInPixels(text, font, scale);
                textSize.Y += headerSize.Y; // Add the height of the header onto the size calculation
                if (textToTall(textSize, blockSize))
                {
                    this.myObject.textLinesRemoved += 1;
                    if (this.myObject.textLinesRemoved <= 0) { return text; } //Remove nothing
                    for (int i = 1; i < this.myObject.textLinesRemoved; i++)
                    {
                        Int32 charsToRemove = text.ToString().IndexOf(Environment.NewLine) + Environment.NewLine.Length;
                        if (text.Length > charsToRemove) { text.Remove(0, charsToRemove); }
                    }

                    textSize = this.myObject.block.MeasureStringInPixels(text, font, scale);
                    textSize.Y += headerSize.Y; // Add the height of the header onto the size calculation
                    if (!textToTall(textSize, blockSize))
                    {
                        this.myObject.textLinesRemoved = -1;
                        return originalText;
                    }
                    return sbHeader.Append(text);
                }
                else
                {
                    this.myObject.textLinesRemoved = -1;
                    return sbHeader.Append(text); // Text Fits
                }
            }
            catch (Exception ex)
            {
                MyAPIGateway.Utilities.ShowNotification($"ERROR: {GetType().FullName}: [{this.myObject.block.CustomName}] Lines {this.myObject.textLinesRemoved} | {ex.Message}", 10000, MyFontEnum.Red);
                return text;
            }
        }

        #region Helpers
        /// <summary>
        /// The space value
        /// </summary>
        protected const string space = " ";

        /// <summary>
        /// Is the text too wide for the screen
        /// </summary>
        /// <param name="text">The text to test</param>
        /// <param name="block">The target block</param>
        protected bool textToWide(Vector2 text, Vector2 block)
        {
            if (text.X > block.X) { return true; }
            return false;
        }

        /// <summary>
        /// Is the text too tall for the screen
        /// </summary>
        /// <param name="text">The text to test</param>
        /// <param name="block">The target block</param>
        protected bool textToTall(Vector2 text, Vector2 block)
        {
            if (text.Y > block.Y) { return true; }
            return false;
        }

        /// <summary>
        /// New line chars
        /// </summary>
        protected string[] NewLines()
        {
            string[] delim = { Environment.NewLine, "\n" }; // "\n" added in case you manually appended a newline
            return delim;
        }
        #endregion
    }
}