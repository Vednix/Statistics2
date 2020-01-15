using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using TShockAPI;
using TShockAPI.DB;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;

namespace Statistics
{
    public class STools
    {
        public static IDbConnection db;
        public static List<SPlayer> SPlayers = new List<SPlayer>();
        public static List<StoredPlayer> StoredPlayers = new List<StoredPlayer>();

        public static SubCommandHandler handler = new SubCommandHandler();

        #region Database
        public static void DatabaseInit()
        {
            if (TShock.Config.StorageType.ToLower() == "sqlite")
            {
                db = new SqliteConnection(string.Format("uri=file://{0},Version=3", Path.Combine(TShock.SavePath, "Statistics.sqlite")));
            }
            else if (TShock.Config.StorageType.ToLower() == "mysql")
            {
                try
                {
                    var host = TShock.Config.MySqlHost.Split(':');
                    db = new MySqlConnection();
                    db.ConnectionString =
                        String.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4}",
                        host[0],
                        host.Length == 1 ? "3306" : host[1],
                        TShock.Config.MySqlDbName,
                        TShock.Config.MySqlUsername,
                        TShock.Config.MySqlPassword
                        );
                }
                catch (MySqlException x)
                {
                    TShock.Log.Error(x.ToString());
                    throw new Exception("MySQL not setup correctly.");
                }
            }
            else
            {
                throw new Exception("Invalid storage type.");
            }

            SqlTableCreator SQLCreator = new SqlTableCreator(db,
                                             db.GetSqlType() == SqlType.Sqlite
                                             ? (IQueryBuilder)new SqliteQueryCreator()
                                             : new MysqlQueryCreator());

            var table = new SqlTable("Stats",
                new SqlColumn("ID", MySqlDbType.Int32) { Primary = true, AutoIncrement = true },
                new SqlColumn("Name", MySqlDbType.VarChar, 50) { Unique = true },
                new SqlColumn("Time", MySqlDbType.Int32),
                new SqlColumn("FirstLogin", MySqlDbType.Text),
                new SqlColumn("LastSeen", MySqlDbType.Text),
                new SqlColumn("Kills", MySqlDbType.Int32),
                new SqlColumn("Deaths", MySqlDbType.Int32),
                new SqlColumn("MobKills", MySqlDbType.Int32),
                new SqlColumn("BossKills", MySqlDbType.Int32),
                new SqlColumn("KnownAccounts", MySqlDbType.Text),
                new SqlColumn("KnownIPs", MySqlDbType.Text),
                new SqlColumn("LoginCount", MySqlDbType.Int32)
                );
            //SQLCreator.EnsureExists(table);
            SQLCreator.EnsureTableStructure(table);


            /*
            var graphTable = new SqlTable("Graphs",
                new SqlColumn("ID", MySqlDbType.Int32) { Primary = true, AutoIncrement = true },
                new SqlColumn("PointX", MySqlDbType.Int32),
                new SqlColumn("PointY", MySqlDbType.Int32),
                new SqlColumn("Type", MySqlDbType.String),
                new SqlColumn("TopRightPoint", MySqlDbType.Int32)
                );
            SQLCreator.EnsureExists(graphTable);

            var graphData = new SqlTable("GraphData",
                new SqlColumn("Monday", MySqlDbType.Int32),
                new SqlColumn("Tuesday", MySqlDbType.Int32),
                new SqlColumn("Wednesday", MySqlDbType.Int32),
                new SqlColumn("Thursday", MySqlDbType.Int32),
                new SqlColumn("Friday", MySqlDbType.Int32),
                new SqlColumn("Saturday", MySqlDbType.Int32),
                new SqlColumn("Sunday", MySqlDbType.Int32)
            );
            SQLCreator.EnsureExists(graphData);
            */
        }
        #endregion

        public static void PostInitialize(EventArgs args)
        {
            int count = 0;
            using (var reader = db.QueryReader("SELECT * FROM Stats"))
            {
                while (reader.Read())
                {
                    string name = reader.Get<string>("Name");
                    int totalTime = reader.Get<int>("Time");
                    string firstLogin = reader.Get<string>("FirstLogin");
                    string lastSeen = reader.Get<string>("LastSeen");

                    string knownAccounts = reader.Get<string>("KnownAccounts");
                    string knownIPs = reader.Get<string>("KnownIPs");
                    int loginCount = reader.Get<int>("LoginCount");

                    int kills = reader.Get<int>("Kills");
                    int deaths = reader.Get<int>("Deaths");
                    int mobKills = reader.Get<int>("MobKills");
                    int bossKills = reader.Get<int>("BossKills");

                    StoredPlayers.Add(new StoredPlayer(name, firstLogin, lastSeen, totalTime, loginCount, knownAccounts, knownIPs,
                        kills, deaths, mobKills, bossKills));
                    count++;
                }
            }
            TShock.Log.ConsoleInfo("Populated {0} stored player{1}.", count, Suffix(count));

            Stat_Timers.Start(args);
        }


        #region Find player methods
        /// <summary>
        /// Returns an SPlayer through index matching
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static SPlayer GetPlayer(int index)
        {
            foreach (SPlayer player in STools.SPlayers)
                if (player.Index == index)
                    return player;

            return null;
        }
        /// <summary>
        /// Returns an SPlayer through UserAccountName matching
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static List<SPlayer> GetPlayer(string name)
        {
            var matches = new List<SPlayer>();
            foreach (SPlayer player in STools.SPlayers)
                if (player.TSPlayer.IsLoggedIn)
                {
                    if (player.TSPlayer.Account.Name.ToLower().Contains(name.ToLower()) && !matches.Contains(player))
                        matches.Add(player);
                    if (player.TSPlayer.Account.Name.ToLower() == name.ToLower())
                        return new List<SPlayer> { player };
                }

            return matches;
        }

        public static List<SPlayer> GetPlayerByIP(string IP)
        {
            var matches = new List<SPlayer>();
            foreach (SPlayer player in STools.SPlayers)
            {
                if (player.TSPlayer.IP == IP)
                    return new List<SPlayer> { player };
                if (player.TSPlayer.IP.Contains(IP))
                    matches.Add(player);
            }

            return matches;
        }

        public static List<StoredPlayer> GetStoredPlayerByIP(string IP)
        {
            var matches = new List<StoredPlayer>();
            foreach (StoredPlayer StoredPlayer in STools.StoredPlayers)
            {
                if (StoredPlayer.knownIPs.Contains(IP))
                    matches.Add(StoredPlayer);
                if (StoredPlayer.knownIPs == IP)
                    return new List<StoredPlayer> { StoredPlayer };
            }

            return matches;
        }

        public static List<StoredPlayer> GetStoredPlayer(string name)
        {
            var matches = new List<StoredPlayer>();
            foreach (StoredPlayer StoredPlayer in StoredPlayers)
            {
                if (StoredPlayer.name.ToLower() == name.ToLower())
                    return new List<StoredPlayer> { StoredPlayer };
                if (StoredPlayer.name.ToLower().Contains(name.ToLower()) && !matches.Contains(StoredPlayer))
                    matches.Add(StoredPlayer);
            }

            return matches;
        }

        public static StoredPlayer GetStoredPlayer(string AccountName, string AccountIP)
        {
            foreach (StoredPlayer StoredPlayer in StoredPlayers)
                if (StoredPlayer.knownAccounts.Contains(AccountName) && StoredPlayer.knownAccounts.Contains(AccountIP))
                    return StoredPlayer;

            return null;
        }

        #endregion

        /// <summary>
        /// Fills out a player's stats
        /// </summary>
        /// <param name="player"></param>
        /// <param name="StoredPlayer"></param>
        public static void PopulatePlayerStats(SPlayer player, StoredPlayer StoredPlayer)
        {
            if (StoredPlayer != null && player != null)
            {
                player.TimePlayed = StoredPlayer.totalTime;
                player.firstLogin = StoredPlayer.firstLogin;
                player.lastSeen = DateTime.UtcNow.ToString("G");
                player.loginCount = StoredPlayer.loginCount + 1;
                player.knownAccounts = StoredPlayer.knownAccounts;
                player.knownIPs = StoredPlayer.knownIPs;

                player.kills = StoredPlayer.kills;
                player.deaths = StoredPlayer.deaths;
                player.mobkills = StoredPlayer.mobkills;
                player.bosskills = StoredPlayer.bosskills;
            }
        }

        /// <summary>
        /// Updates a stored player's stats
        /// </summary>
        /// <param name="player"></param>
        /// <param name="StoredPlayer"></param>
        public static void PopulateStoredStats(SPlayer player, StoredPlayer StoredPlayer)
        {
            if (player != null && StoredPlayer != null)
            {
                StoredPlayer.totalTime = player.TimePlayed;
                StoredPlayer.firstLogin = player.firstLogin;
                StoredPlayer.lastSeen = DateTime.Now.ToString("G");
                StoredPlayer.loginCount = player.loginCount;
                StoredPlayer.knownAccounts = player.knownAccounts;
                StoredPlayer.knownIPs = player.knownIPs;

                StoredPlayer.kills = player.kills;
                StoredPlayer.deaths = player.deaths;
                StoredPlayer.mobkills = player.mobkills;
                StoredPlayer.bosskills = player.bosskills;
            }
        }

        /// <summary>
        /// Updates a player
        /// </summary>
        /// <param name="player"></param>
        public static void UpdatePlayer(SPlayer player)
        {
            try
            {
                PopulateStoredStats(player, GetStoredPlayer(player.TSPlayer.Account.Name)[0]);
            }
            catch (Exception x)
            {
                TShock.Log.ConsoleError(x.ToString());
            }
        }

        public static void RegisterSubs()
        {
            handler.RegisterSubCommand("time", sCommands.Check_Time, "stats.time", "stats.*");
            handler.RegisterSubCommand("afk", sCommands.Check_Afk, "stats.afk", "stats.*");
            handler.RegisterSubCommand("kills", sCommands.Check_Kills, "stats.kills", "stats.*");
            handler.RegisterSubCommand("name", sCommands.Check_Name, "stats.name", "stats.*");

            //handler.HelpText = "Valid SubCommands of /check:|[time \\ afk \\ kills]|Syntax: /check [option] [playerName \\ self]";
            handler.HelpText = "Syntax: /check <afk/kills/time> [player]";
        }

        public static void SaveDatabase()
        {
            foreach (SPlayer player in SPlayers)
                if (player.TSPlayer.IsLoggedIn)
                    PopulateStoredStats(player, GetStoredPlayer(player.TSPlayer.Account.Name)[0]);

            foreach (StoredPlayer StoredPlayer in StoredPlayers)
            {
                db.Query("UPDATE Stats SET Time = @0, LastSeen = @1, Kills = @2, Deaths = @3, MobKills = @4, " +
                    "BossKills = @5, LoginCount = @6 WHERE Name = @7",
                    StoredPlayer.totalTime, DateTime.Now.ToString("G"), StoredPlayer.kills, StoredPlayer.deaths,
                    StoredPlayer.mobkills, StoredPlayer.bosskills, StoredPlayer.loginCount, StoredPlayer.name);
            }
            TShock.Log.ConsoleInfo("Database save complete.");
        }


        public static string Suffix(int number)
        {
            return number > 1 || number == 0 ? "s" : "";
        }

        public static string Suffix2(int number)
        {
            return number > 1 || number == 0 ? "es" : "";
        }

        public static string TimePlayed(int number)
        {
            double totalTime = (double)number;

            double weeks = Math.Floor(totalTime / 604800);
            double days = Math.Floor(((totalTime / 604800) - weeks) * 7);

            TimeSpan ts = new TimeSpan(0, 0, 0, (int)totalTime);

            return string.Format("{0} week{5} {1} day{6} {2} hour{7} {3} minute{8} {4} second{9}",
            weeks, days, ts.Hours, ts.Minutes, ts.Seconds, Suffix((int)weeks), Suffix((int)days), Suffix(ts.Hours),
            Suffix(ts.Minutes), Suffix(ts.Seconds));



            /*  Broken format
             return string.Format("{0}{5}{1}{6}{2}{7}{3}{8}{10}{4}{9}",
                weeks > 0 ? weeks.ToString() + " week" : "",
                ts.Days > 0 ? ts.Days.ToString() + " day" : "",
                ts.Hours > 0 ? ts.Hours.ToString() + " hour" : "",
                ts.Minutes > 0 ? ts.Minutes.ToString() + " minute" : "",
                ts.Seconds > 0 ? " " + ts.Seconds.ToString() + " second" : "",

                weeks > 1 ? "s " : "",
                ts.Days > 1 ? "s " : (ts.Days == 0 || ts.Days == 1) && weeks != 0 ? " " : "",
                ts.Hours > 1 ? "s " : (ts.Hours == 0 || ts.Hours == 1) && (ts.Days != 0 || weeks != 0) ? " " : "",
                ts.Minutes > 1 ? "s " : (ts.Minutes == 0 || ts.Minutes == 1) && (ts.Hours != 0 || ts.Days != 0 || weeks != 0 || ts.Seconds > 0) ? " " : "",
                ts.Seconds > 1 ? "s " : (ts.Seconds == 0 || ts.Seconds == 1) && (ts.Minutes != 0 || ts.Hours != 0 || ts.Days != 0 || weeks != 0) ? " " : "",
                ts.Seconds > 0 && (weeks != 0 || ts.Days != 0 || ts.Minutes != 0) ? "and" : "").Trim();*/
        }

        public static string TimeSpanPlayed(TimeSpan ts)
        {
            return string.Format("{0}{4}{1}{5}{2}{6}{8}{3}{7}",
                ts.Days > 0 ? ts.Days.ToString() + " day" : "",
                ts.Hours > 0 ? ts.Hours.ToString() + " hour" : "",
                ts.Minutes > 0 ? ts.Minutes.ToString() + " minute" : "",
                ts.Seconds > 0 ? " " + ts.Seconds.ToString() + " second" : "",

                ts.Days > 1 ? "s " : (ts.Days == 0 || ts.Days == 1) ? " " : "",
                ts.Hours > 1 ? "s " : (ts.Hours == 0 || ts.Hours == 1) && (ts.Days != 0) ? " " : "",
                ts.Minutes > 1 ? "s " : (ts.Minutes == 0 || ts.Minutes == 1) && (ts.Hours != 0 || ts.Days != 0 || ts.Seconds > 0) ? " " : "",
                ts.Seconds > 1 ? "s " : (ts.Seconds == 0 || ts.Seconds == 1) && (ts.Minutes != 0 || ts.Hours != 0 || ts.Days != 0) ? " " : "",
                ts.Seconds > 0 && (ts.Days != 0 || ts.Minutes != 0) ? "and" : "").Trim();
        }
    }

    public class SubCommandHandler
    {
        private List<SubCommand> SubCommands = new List<SubCommand>();

        public string HelpText;

        public SubCommandHandler()
        {
            RegisterSubCommand("help", DisplayHelpText);
        }

        private void DisplayHelpText(CommandArgs args)
        {
            foreach (string item in HelpText.Split('|'))
                args.Player.SendInfoMessage(item);
        }

        public void RegisterSubCommand(string command, Action<CommandArgs> func, params string[] permissions)
        {
            SubCommands.Add(new SubCommand(command, func, permissions));
        }
        public void RegisterSubCommand(string command, Action<CommandArgs> func, string permission)
        {
            SubCommands.Add(new SubCommand(command, func, permission));
        }

        public void RunSubCommand(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                CommandArgs newargs = new CommandArgs(args.Message, args.Player, args.Parameters.GetRange(1, args.Parameters.Count - 1));
                try
                {
                    int count = 0;
                    foreach (string perm in SubCommands.Find(command => command.name == args.Parameters[0]).permissions)
                        if (!args.Player.Group.HasPermission(perm))
                        {
                            count++;
                        }
                    if (count == SubCommands.Find(command => command.name == args.Parameters[0]).permissions.Count)
                        args.Player.SendErrorMessage("You do not have permission to use this command!");
                    else
                        SubCommands.Find(command => command.name == args.Parameters[0]).func.Invoke(args);
                }
                catch //(Exception e)
                {
                    //args.Player.SendErrorMessage("Command failed, check logs for more details.");
                    //TShock.Log.Error(e.Message);
                    //SubCommands.Find(command => command.name == "help").func.Invoke(newargs);
                    args.Player.SendErrorMessage("Invalid command!");
                }
            }
            else
                SubCommands.Find(command => command.name == "help").func.Invoke(args);
        }
    }

    public class SubCommand
    {
        public List<string> permissions;
        public string name;
        public Action<CommandArgs> func;

        public SubCommand(string name, Action<CommandArgs> func, params string[] permissions)
        {
            this.permissions = new List<string>(permissions);
            this.name = name;
            this.func = func;
        }
        public SubCommand(string name, Action<CommandArgs> func, string permission)
        {
            this.permissions.Add(permission);
            this.name = name;
            this.func = func;
        }
        public SubCommand(string name, Action<CommandArgs> func)
        {
            this.name = name;
            this.func = func;
        }
    }
}
