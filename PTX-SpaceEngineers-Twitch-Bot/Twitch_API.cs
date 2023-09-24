using System.Text;

namespace PTX_SpaceEngineers_Twitch_Bot
{
    internal class Twitch_API
    {
        Bot twitch;
        TwitchLib.Api.TwitchAPI api;
        List<string> broadcaster = new();

        public Twitch_API(Bot _Twitch)
        {
            twitch = _Twitch;

            api = new TwitchLib.Api.TwitchAPI();
            api.Settings.ClientId = twitch.config.clientID;
            //api.Settings.Secret = twitch.config.clientSecret;
            //api.Settings.AccessToken = twitch.config.OAuthToken;
            broadcaster.Add(twitch.config.channelName);
            Task.Run(async () => { await updateSubsList(); });
        }

        public async Task updateSubsList()
        {
            try
            {
                TwitchLib.Api.Helix.Models.Users.GetUsers.GetUsersResponse user = await api.Helix.Users.GetUsersAsync(null, broadcaster, twitch.config.OAuthToken);
                TwitchLib.Api.Helix.Models.Subscriptions.GetBroadcasterSubscriptionsResponse apiSubs = await api.Helix.Subscriptions.GetBroadcasterSubscriptionsAsync(user.Users[0].Id, 100, null, twitch.config.OAuthToken);

                StringBuilder sbSubs = new();

                //Int32 subCount = 1;
                foreach (var sub in apiSubs.Data)
                {
                    sbSubs.AppendLine(sub.UserName);
                    //Console.WriteLine($"Sub {subCount} | {sub.UserName} | Tier: {sub.Tier}");
                    //subCount += 1;
                }
                Helpers.WriteFiles.writeSpaceEngineersFiles(Helpers.WriteFiles.file_Subscribers, sbSubs.ToString());
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
        }
    }
}