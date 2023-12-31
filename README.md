# PTX Space Engineers Twitch Integration
This mod works to bring Twitch integration into your Space Engineers game  
The Steam mod can be found at https://steamcommunity.com/sharedfiles/filedetails/?id=3039895610  

## Main Features
* Show a subscriber list on Text Panels
* Show the last subscriber on Text Panels
* Show recent chat messages on Text Panels
* Show the last chat message on Text Panels

# Chat Bot
## Setup 
1. When first running the bot, you will be asked for the Twitch channel name.  
1. Copy the Access Token and paste it at the "Access Token" prompt.
  
  
# In Game Usage
## Text Panel
### Setup
Add "ptx-twitch" to the name of a Text Panel.  
The panel will update with a list of options.  
Enter the desired options into the "Custom Data" for the Text Panel.

<img src="PTX-Twitch_SE_Mod/Content/TwitchChat.png"/>

### Options
@Subscribers - A list of all current Twitch Subscribers  
@LastSubscriber - The last Twitch Subscriber  
@ChatMessages - The last few messages in chat. Number set in the bot config  
@LastChatMessage - The last message in chat  
@echo - Adds a blank line  
@text - Adds custom text | example "@text=this is an example" (without the ")  
*Mods clearing chat will clear the chat message screens*  

### Chat Commands 
Chat can run commands to trigger in-game actions like Opening your helmet, turning your Jeypack On/Off, and more (configurable in via a config file in the bot directory)
The commands available are listed here: https://docs.google.com/spreadsheets/d/16qNz9PngmQSWH5euSEXnryt6pJpW4wvDKCZp4Pa7-Rg/

## Multi-player
This mod works in multi-player.

## Notes
This mod requires the use of the Twitch ChatBot to monitor Twitch chat and update the data used by the mod.  
  
If you would like to use your own ChatBot, the files this mod reads for the Text Panel updates are located at: %appdata%\SpaceEngineers\Storage\PTX-Twitch_Chat_Integration  


## Framework
There are two downloads in the release folder.  
PTX-SpaceEngineers-Twitch-Bot-dotNet6 requires dotNet6 is installed.  
PTX-SpaceEngineers-Twitch-Bot-IncFramework includes the framework, but it a larger file.
