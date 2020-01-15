using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Threading;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;

namespace Statistics
{
    [ApiVersion(2, 1)]
    public class Statistics : TerrariaPlugin
    {
        public override string Author
        { get { return "WhiteX"; } }

        public override string Description
        { get { return "Statistics for players"; } }

        public override string Name
        { get { return "Statistics"; } }

        public override Version Version
        { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    Thread dispose = new Thread(new ThreadStart(DisposeThread));

                    dispose.Start();
                    dispose.Join();
                }
                catch (Exception x)
                {
                    TShock.Log.ConsoleError(x.ToString());
                }
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnGreet);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
                ServerApi.Hooks.GamePostInitialize.Register(this, STools.PostInitialize);
                ServerApi.Hooks.ServerChat.Deregister(this, OnChat);
                ServerApi.Hooks.NetGetData.Deregister(this, GetData);
                TShockAPI.Hooks.PlayerHooks.PlayerPostLogin -= PostLogin;

            }
            base.Dispose(disposing);
        }

        public static void DisposeThread()
        {
            STools.SaveDatabase();
            Thread.Sleep(600);
        }

        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreet);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
            ServerApi.Hooks.GamePostInitialize.Register(this, STools.PostInitialize);
            ServerApi.Hooks.ServerChat.Register(this, OnChat);
            ServerApi.Hooks.NetGetData.Register(this, GetData);
            TShockAPI.Hooks.PlayerHooks.PlayerPostLogin += PostLogin;

            GetDataHandlers.InitGetDataHandler();
        }

        #region OnInitialize
        public void OnInitialize(EventArgs args)
        {
            Commands.ChatCommands.Add(new Command("stats.check", Stat_Check, "check")
            {
                HelpText = "Base command for the Statistics plugin. Allows you to view statistics about players and yourself."
            });

            Commands.ChatCommands.Add(new Command("stats.uix", sCommands.UI_Extended, "uix")
            {
                HelpText = "Extended user information."
            });

            Commands.ChatCommands.Add(new Command("stats.uic", sCommands.UI_Character, "uic")
            {
                HelpText = "Provides information about a player's character."
            });

            STools.RegisterSubs();
            /*Commands.ChatCommands.Add(new Command("graph.set", _Graph.graphCommand, "graph") { AllowServer = false });
            Commands.ChatCommands.Add(new Command("graph.add", _Graph.createGraph, "cgraph") { AllowServer = false });*/

            STools.DatabaseInit();
        }
        #endregion

        #region PostLogin
        public void PostLogin(TShockAPI.Hooks.PlayerPostLoginEventArgs args)
        {
            if (STools.GetPlayer(args.Player.Index) != null)
            {
                SPlayer player = STools.GetPlayer(args.Player.Index);

                if (STools.GetStoredPlayer(args.Player.Account.Name).Count == 1)
                {
                    StoredPlayer StoredPlayer = STools.GetStoredPlayer(args.Player.Account.Name)[0];

                    if (StoredPlayer.knownIPs.Length > 0)
                    {
                        if (!StoredPlayer.knownIPs.Contains(args.Player.IP))
                            StoredPlayer.knownIPs += ", " + args.Player.IP;
                    }
                    else
                        StoredPlayer.knownIPs = args.Player.IP;

                    if (StoredPlayer.knownAccounts.Length > 0)
                    {
                        if (!StoredPlayer.knownAccounts.Contains(args.Player.Account.Name))
                            StoredPlayer.knownAccounts += ", " + args.Player.Account.Name;
                    }
                    else
                        StoredPlayer.knownAccounts = args.Player.Account.Name;

                    STools.PopulatePlayerStats(player, StoredPlayer);
                    TShock.Log.ConsoleInfo("Successfully linked account {0} with stored player {1}.", args.Player.Account.Name, StoredPlayer.name);
                    return;
                }
                else if (STools.GetStoredPlayer(args.Player.Account.Name).Count > 1)
                {
                    TShock.Log.ConsoleError("Multiple match error! --Attempting to obtain stored player for {0} resulted in" + " {1} matches: {2}", args.Player.Account.Name, STools.GetStoredPlayer(args.Player.Account.Name).Count, STools.GetStoredPlayer(args.Player.Account.Name).Select(p => p.name));
                }
                else
                {
                    TShock.Log.ConsoleInfo("New stored player named {0} has been added", args.Player.Account.Name);
                    STools.StoredPlayers.Add(new StoredPlayer(args.Player.Account.Name, DateTime.Now.ToString("G"), DateTime.Now.ToString("G"), 0, 1, args.Player.Account.Name, args.Player.IP, 0, 0, 0, 0));

                    STools.db.Query("INSERT INTO Stats (Name, FirstLogin, LastSeen, Time, LoginCount, KnownAccounts, KnownIPs, Kills, Deaths, MobKills, BossKills)" + " VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10)", args.Player.Account.Name, DateTime.Now.ToString("G"), DateTime.Now.ToString("G"), 0, 1, args.Player.Account.Name, args.Player.IP, 0, 0, 0, 0);

                    STools.PopulatePlayerStats(player, STools.StoredPlayers[STools.StoredPlayers.Count - 1]);
                    TShock.Log.ConsoleInfo("Successfully linked account {0} with stored player {1}", args.Player.Account.Name, STools.StoredPlayers[STools.StoredPlayers.Count - 1].name);
                }
            }
            else
            {
                STools.SPlayers.Add(new SPlayer(args.Player.Index));
                PostLogin(new TShockAPI.Hooks.PlayerPostLoginEventArgs(args.Player));

                /*Log.ConsoleError("Error encountered while checking '" + args.Player.Name + "'");
                Log.ConsoleError("Unable to find a stat player with index '" + args.Player.Index + "'");
                Log.ConsoleError("Player at statistic index " + args.Player.Index + " is " + STools.SPlayers[args.Player.Index]);
                */
            }
        }
        #endregion

        #region OnLeave
        public void OnLeave(LeaveEventArgs args)
        {
            if (STools.GetPlayer(args.Who) != null)
            {
                STools.UpdatePlayer(STools.GetPlayer(args.Who));

                STools.SPlayers.RemoveAll(p => p.Index == args.Who);
            }
        }
        #endregion

        #region OnGreet
        public void OnGreet(GreetPlayerEventArgs args)
        {
            if (STools.GetPlayer(args.Who) == null)
            {
                STools.SPlayers.Add(new SPlayer(args.Who));

                if (!TShock.Config.DisableUUIDLogin)
                    if (TShock.Players[args.Who].IsLoggedIn)
                        PostLogin(new TShockAPI.Hooks.PlayerPostLoginEventArgs(TShock.Players[args.Who]));
            }
        }
        #endregion

        #region GetData
        public void GetData(GetDataEventArgs args)
        {
            PacketTypes type = args.MsgID;
            var player = TShock.Players[args.Msg.whoAmI];

            if (player == null)
            {
                args.Handled = true;
                return;
            }

            if (!player.ConnectionAlive)
            {
                args.Handled = true;
                return;
            }

            using (var data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length))
            {
                try
                {
                    if (GetDataHandlers.HandlerGetData(type, player, data))
                        args.Handled = true;
                }
                catch (Exception ex)
                {
                    TShock.Log.ConsoleError(ex.ToString());
                }
            }
        }
        #endregion

        #region OnChat
        public void OnChat(ServerChatEventArgs args)
        {
            try
            {
                var player = STools.GetPlayer(args.Who);
                if (player != null)
                {
                    if (!args.Text.StartsWith("/check"))
                    {
                        if (player.AFKcount > 0)
                            player.AFKcount = 0;

                        if (player.AFK)
                        {
                            player.AFK = false;
                        }
                    }
                }
            }
            catch { }
        }
        #endregion

        public Statistics(Main game) : base(game)
        {
            //Order = 100;
        }

        public void Stat_Check(CommandArgs args)
        {
            STools.handler.RunSubCommand(args);
        }
    }
}
