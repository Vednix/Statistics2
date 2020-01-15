using System;
using System.Timers;
using TShockAPI;

namespace Statistics
{
    public class Stat_Timers
    {
        static Timer aTimer = new Timer(1000);
        static Timer uTimer = new Timer(60 * 1000);
        static Timer databaseSaver = new Timer(600 * 1000);

        public static void Start(EventArgs args)
        {
            aTimer.Enabled = true;
            aTimer.Elapsed += new ElapsedEventHandler(AfkTimer);

            uTimer.Enabled = true;
            uTimer.Elapsed += new ElapsedEventHandler(UpdateTimer);

            databaseSaver.Enabled = true;
            databaseSaver.Elapsed += new ElapsedEventHandler(DatabaseTimer);
        }

        static void DatabaseTimer(object sender, ElapsedEventArgs args)
        {
            STools.SaveDatabase();
        }

        static void AfkTimer(object sender, ElapsedEventArgs args)
        {

            /* Needs advanced afk checks */
            foreach (SPlayer player in STools.SPlayers)
            {
                if (player.TSPlayer.X == player.LastPosX && player.TSPlayer.Y == player.LastPosY)
                {
                    player.AFKcount++;
                    if (player.AFKcount > 300)
                    {
                        if (!player.AFK)
                        {
                            TSPlayer.All.SendInfoMessage("{0} is now AFK.", player.TSPlayer.Name);
                            player.TSPlayer.SendWarningMessage("You are now marked as AFK.");
                            player.TSPlayer.SendWarningMessage("This time is not being counted towards your statistics.");
                            player.AFK = true;
                        }
                    }
                }
                else
                {
                    player.TimePlayed++;
                    if (player.AFK)
                        player.AFK = false;

                    if (player.AFKcount > 0)
                        player.AFKcount = 0;
                }

                player.LastPosX = player.TSPlayer.X;
                player.LastPosY = player.TSPlayer.Y;
            }
        }

        static void UpdateTimer(object sender, ElapsedEventArgs args)
        {
            foreach (SPlayer player in STools.SPlayers)
            {
                if (!player.AFK && player.TSPlayer.IsLoggedIn)
                {
                    STools.UpdatePlayer(player);
                }
            }
        }
    }
}
