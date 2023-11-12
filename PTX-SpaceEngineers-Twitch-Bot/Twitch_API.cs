using System.Text;
using TwitchLib.Api.Helix;
using TwitchLib.Communication.Interfaces;

namespace PTX_SpaceEngineers_Twitch_Bot
{
    internal class Twitch_API
    {
        Bot twitch;
        TwitchLib.Api.TwitchAPI api;
        TwitchLib.PubSub.TwitchPubSub pub;
        List<string> broadcaster = new();
        string channelID;

        public Twitch_API(Bot _Twitch)
        {
            twitch = _Twitch;

            api = new TwitchLib.Api.TwitchAPI();
            pub = new TwitchLib.PubSub.TwitchPubSub();
            api.Settings.ClientId = twitch.config.clientID;

            pub.OnPubSubServiceConnected += Pub_onPubSubServiceConnected;
            pub.OnChannelPointsRewardRedeemed += Pub_OnChannelPointsRewardRedeemed;
            pub.OnBitsReceivedV2 += twitch.Pub_OnBitsReceivedV2;
            pub.OnListenResponse += Pub_OnListenResponse;
            pub.Connect();


            broadcaster.Add(twitch.config.channelName);
            Task.Run(async () => { await updateSubsList(); });
            
        }

        private void Pub_OnListenResponse(object? sender, TwitchLib.PubSub.Events.OnListenResponseArgs e)
        {
            if (!e.Successful)
                Console.WriteLine($"Failed to listen! Response: {e.Response.Error}|{e.Topic}");
        }

        private void Pub_OnChannelPointsRewardRedeemed(object? sender, TwitchLib.PubSub.Events.OnChannelPointsRewardRedeemedArgs e)
        {
            string reward = e.RewardRedeemed.Redemption.Reward.Title;
            Console.WriteLine($"[Channel Points] {reward} redeemed");
        }

        private async void Pub_onPubSubServiceConnected(object? sender, EventArgs e)
        {
            TwitchLib.Api.Helix.Models.Users.GetUsers.GetUsersResponse user = await api.Helix.Users.GetUsersAsync(null, broadcaster, twitch.config.OAuthToken);
            channelID = user.Users[0].Id;
            pub.ListenToBitsEventsV2(channelID);
            pub.ListenToChannelPoints(channelID);

            pub.SendTopics(twitch.config.OAuthToken);
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