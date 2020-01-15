using System;
using System.Net;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using TShockAPI;
using Microsoft.Xna.Framework;

namespace Statistics
{
    public class sCommands
    {
        /* Yama's suggestions */

        #region UI Extended
        public static void UI_Extended(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                if (args.Parameters[0] == "self")
                {
                    if (STools.GetPlayer(args.Player.Index) != null)
                    {
                        SPlayer player = STools.GetPlayer(args.Player.Index);

                        if (player.TSPlayer.IsLoggedIn)
                        {
                            int pageNumber;
                            if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNumber))
                                return;
                            else
                            {
                                var uixInfo = new List<string>();
                                var time_1 = DateTime.Now.Subtract(DateTime.Parse(player.firstLogin));

                                uixInfo.Add(string.Format("UIX info for {0}.", player.Name));

                                //uixInfo.Add(string.Format("{0} is a member of group {1}.", player.Name, player.TSPlayer.Group.Name));
                                uixInfo.Add(string.Format("Group: {0}.", player.TSPlayer.Group.Name));

                                uixInfo.Add(string.Format("First login: {0} ({1}ago).",
                                    player.firstLogin, STools.TimeSpanPlayed(time_1)));

                                uixInfo.Add("Last seen: Now");
                                uixInfo.Add(string.Format("Overall play time: {0}.", STools.TimePlayed(player.TimePlayed)));
                                uixInfo.Add(string.Format("Logged in {0} times since registering.", player.loginCount));
                                try
                                {
                                    uixInfo.Add(string.Format("Known accounts: {0}.", player.knownAccounts));
                                }
                                catch { uixInfo.Add("No known accounts found."); }
                                try
                                {
                                    uixInfo.Add(string.Format("Known IPs: {0}.", player.knownIPs));
                                }
                                catch { uixInfo.Add("No known IPs found."); }

                                PaginationTools.SendPage(args.Player, pageNumber, uixInfo, new PaginationTools.Settings
                                {
                                    //HeaderFormat = "Extended User Information [Page {0} of {1}]",
                                    HeaderFormat = "Extended User Information ({0}/{1})",
                                    HeaderTextColor = Color.Lime,
                                    LineTextColor = Color.White,
                                    FooterFormat = string.Format("Type /uix {0} {1} for more.", args.Parameters[0], pageNumber + 1),
                                    FooterTextColor = Color.Lime
                                });
                            }
                        }
                        else
                            args.Player.SendErrorMessage("You must be logged in to use this on yourself.");
                    }
                    else
                    {
                        args.Player.SendErrorMessage("Something broke. Please try again later.");
                    }
                }
                else
                {
                    string name = "";
                    bool needNumber = false;
                    if (args.Parameters.Count > 1)
                    {
                        var newArgs = new List<string>(args.Parameters);
                        newArgs.RemoveAt(newArgs.Count - 1);
                        name = string.Join(" ", newArgs);
                        needNumber = true;
                    }
                    else
                        name = string.Join(" ", args.Parameters);

                    int pageNumber;
                    if (!PaginationTools.TryParsePageNumber(args.Parameters,
                        needNumber ? args.Parameters.Count - 1 : args.Parameters.Count + 1, args.Player, out pageNumber))
                        return;

                    IPAddress IP;
                    if (IPAddress.TryParse(name, out IP))
                    {
                        if (STools.GetPlayerByIP(IP.ToString()).Count == 1)
                        {
                            SPlayer player = STools.GetPlayerByIP(IP.ToString())[0];

                            if (player.TSPlayer.IsLoggedIn)
                            {
                                var uixInfo = new List<string>();
                                var time_1 = DateTime.Now.Subtract(DateTime.Parse(player.firstLogin));

                                uixInfo.Add(string.Format("UIX info for {0}.", player.Name));
                                uixInfo.Add(string.Format("Group: {0}.", player.TSPlayer.Group.Name));
                                uixInfo.Add(string.Format("First login: {0} ({1} ago).",
                                    player.firstLogin, STools.TimeSpanPlayed(time_1)));

                                uixInfo.Add("Last seen: Now");
                                uixInfo.Add(string.Format("Overall play time: {0}.", STools.TimePlayed(player.TimePlayed)));
                                uixInfo.Add(string.Format("Logged in {0} times since registering.", player.loginCount));
                                try
                                {
                                    uixInfo.Add(string.Format("Known accounts: {0}.", string.Join(", ", player.knownAccounts.Split(','))));
                                }
                                catch { uixInfo.Add("No known accounts found."); }
                                try
                                {
                                    uixInfo.Add(string.Format("Known IPs: {0}.", string.Join(", ", player.knownIPs.Split(','))));
                                }
                                catch { uixInfo.Add("No known IPs found."); }

                                PaginationTools.SendPage(args.Player, pageNumber, uixInfo, new PaginationTools.Settings
                                {
                                    HeaderFormat = "Extended User Information ({0}/{1})",
                                    HeaderTextColor = Color.Lime,
                                    LineTextColor = Color.White,
                                    FooterFormat = string.Format("Type /uix {0} {1} for more.", player.Name, pageNumber + 1),
                                    FooterTextColor = Color.Lime
                                });
                            }
                            else
                                args.Player.SendErrorMessage("{0} is not logged in.", player.Name);

                        }
                        else if (STools.GetPlayerByIP(IP.ToString()).Count > 1)
                            //TShock.Utils.SendMultipleMatchError(args.Player,
                            args.Player.SendMultipleMatchError(
                                STools.GetPlayerByIP(IP.ToString()).Select(p => p.Name));
                        else
                            if (STools.GetStoredPlayerByIP(IP.ToString()).Count == 1)
                        {
                            StoredPlayer StoredPlayer = STools.GetStoredPlayerByIP(IP.ToString())[0];
                            var uixInfo = new List<string>();
                            var time_1 = DateTime.Now.Subtract(DateTime.Parse(StoredPlayer.firstLogin));

                            uixInfo.Add(string.Format("UIX info for {0}.", StoredPlayer.name));
                            //uixInfo.Add(string.Format("{0} is a member of group {1}", StoredPlayer.name, TShock.Users.GetUserByName(StoredPlayer.name).Group));
                            uixInfo.Add(string.Format("Group: {0}.", TShock.UserAccounts.GetUserAccountByName(StoredPlayer.name).Group));
                            uixInfo.Add(string.Format("First login: {0} ({1} ago).",
                                StoredPlayer.firstLogin, STools.TimeSpanPlayed(time_1)));

                            uixInfo.Add(string.Format("Last seen: {0} ({1} ago).", StoredPlayer.lastSeen,
                                STools.TimeSpanPlayed(DateTime.Now.Subtract(DateTime.Parse(StoredPlayer.lastSeen)))));

                            uixInfo.Add(string.Format("Overall play time: {0}.", STools.TimePlayed(StoredPlayer.totalTime)));

                            uixInfo.Add(string.Format("Logged in {0} times since registering.", StoredPlayer.loginCount));
                            try
                            {
                                uixInfo.Add(string.Format("Known accounts: {0}.", string.Join(", ", StoredPlayer.knownAccounts.Split(','))));
                            }
                            catch { uixInfo.Add("No known accounts found."); }
                            try
                            {
                                uixInfo.Add(string.Format("Known IPs: {0}.", string.Join(", ", StoredPlayer.knownIPs.Split(','))));
                            }
                            catch { uixInfo.Add("No known IPs found."); }

                            PaginationTools.SendPage(args.Player, pageNumber, uixInfo, new PaginationTools.Settings
                            {
                                HeaderFormat = "Extended User Information ({0}/{1})",
                                HeaderTextColor = Color.Lime,
                                LineTextColor = Color.White,
                                FooterFormat = string.Format("Type /uix {0} {1} for more.", StoredPlayer.name, pageNumber + 1),
                                FooterTextColor = Color.Lime
                            });
                        }

                        else if (STools.GetStoredPlayerByIP(IP.ToString()).Count > 1)
                            //TShock.Utils.SendMultipleMatchError(args.Player,
                            args.Player.SendMultipleMatchError(
                                STools.GetStoredPlayerByIP(IP.ToString()).Select(p => p.name));

                        else
                            //args.Player.SendErrorMessage("Invalid IP address. Try /check ip {0} to make sure you're using the right IP address", name);
                            args.Player.SendErrorMessage("Invalid IP address. Try /check ip {0}.", name);
                    }

                    else
                    {
                        if (STools.GetPlayer(name).Count == 1)
                        {
                            SPlayer player = STools.GetPlayer(name)[0];

                            if (player.TSPlayer.IsLoggedIn)
                            {
                                var uixInfo = new List<string>();
                                var time_1 = DateTime.Now.Subtract(DateTime.Parse(player.firstLogin));

                                uixInfo.Add(string.Format("UIX info for {0}.", player.Name));
                                uixInfo.Add(string.Format("Group: {0}.", player.TSPlayer.Group.Name));

                                uixInfo.Add(string.Format("First login: {0} ({1} ago).",
                                    player.firstLogin, STools.TimeSpanPlayed(time_1)));

                                uixInfo.Add("Last seen: Now");

                                uixInfo.Add(string.Format("Logged in {0} times since registering.", player.loginCount));
                                try
                                {
                                    uixInfo.Add(string.Format("Known accounts: {0}.", string.Join(", ", player.knownAccounts.Split(','))));
                                }
                                catch { uixInfo.Add("No known accounts found."); }
                                try
                                {
                                    uixInfo.Add(string.Format("Known IPs: {0}.", string.Join(", ", player.knownIPs.Split(','))));
                                }
                                catch { uixInfo.Add("No known IPs found."); }

                                PaginationTools.SendPage(args.Player, pageNumber, uixInfo, new PaginationTools.Settings
                                {
                                    HeaderFormat = "Extended User Information ({0}/{1})",
                                    HeaderTextColor = Color.Lime,
                                    LineTextColor = Color.White,
                                    FooterFormat = string.Format("Type /uix {0} {1} for more.", player.Name, pageNumber + 1),
                                    FooterTextColor = Color.Lime
                                });
                            }
                            else
                                args.Player.SendErrorMessage("{0} is not logged in.", player.Name);
                        }
                        else if (STools.GetPlayer(name).Count > 1)
                        {
                            //TShock.Utils.SendMultipleMatchError(args.Player, STools.GetPlayer(name).Select(p => p.Name));
                            args.Player.SendMultipleMatchError(STools.GetPlayer(name).Select(p => p.Name));
                        }
                        else
                        {
                            if (STools.GetStoredPlayer(name).Count == 1)
                            {
                                StoredPlayer StoredPlayer = STools.GetStoredPlayer(name)[0];
                                var uixInfo = new List<string>();
                                var time_1 = DateTime.Now.Subtract(DateTime.Parse(StoredPlayer.firstLogin));

                                uixInfo.Add(string.Format("UIX info for {0}.", StoredPlayer.name));
                                //uixInfo.Add(string.Format("{0} is a member of group {1}", StoredPlayer.name, TShock.Users.GetUserByName(StoredPlayer.name).Group));
                                uixInfo.Add(string.Format("Group: {0}.", TShock.UserAccounts.GetUserAccountByName(StoredPlayer.name).Group));
                                uixInfo.Add(string.Format("First login: {0} ({1} ago).",
                                    StoredPlayer.firstLogin, STools.TimeSpanPlayed(time_1)));

                                uixInfo.Add(string.Format("Last seen: {0} ({1} ago).", StoredPlayer.lastSeen,
                                    STools.TimeSpanPlayed(DateTime.Now.Subtract(DateTime.Parse(StoredPlayer.lastSeen)))));

                                uixInfo.Add(string.Format("Overall play time: {0}.", STools.TimePlayed(StoredPlayer.totalTime)));

                                uixInfo.Add(string.Format("Logged in {0} times since registering.", StoredPlayer.loginCount));
                                try
                                {
                                    uixInfo.Add(string.Format("Known accounts: {0}.", string.Join(", ", StoredPlayer.knownAccounts.Split(','))));
                                }
                                catch { uixInfo.Add("No known accounts found."); }
                                try
                                {
                                    uixInfo.Add(string.Format("Known IPs: {0}.", string.Join(", ", StoredPlayer.knownIPs.Split(','))));
                                }
                                catch { uixInfo.Add("No known IPs found."); }

                                PaginationTools.SendPage(args.Player, pageNumber, uixInfo, new PaginationTools.Settings
                                {
                                    HeaderFormat = "Extended User Information ({0}/{1})",
                                    HeaderTextColor = Color.Lime,
                                    LineTextColor = Color.White,
                                    FooterFormat = string.Format("Type /uix {0} {1} for more.", StoredPlayer.name, pageNumber + 1),
                                    FooterTextColor = Color.Lime
                                });
                            }
                            else if (STools.GetStoredPlayer(name).Count > 1)
                            {
                                args.Player.SendMultipleMatchError(STools.GetStoredPlayer(name).Select(
                                    p => p.name));
                            }
                            else
                                args.Player.SendErrorMessage("Invalid player! Try /check name {0}.",
                                name);
                        }
                    }
                }
            }
            else
                //args.Player.SendErrorMessage("Invalid syntax. Try /uix [playerName]");
                args.Player.SendErrorMessage("Invalid syntax! Try /uix <player/self>");
        }
        #endregion

        #region UI Character
        public static void UI_Character(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                if (args.Parameters[0] == "self")
                {
                    if (STools.GetPlayer(args.Player.Index) != null)
                    {
                        SPlayer player = STools.GetPlayer(args.Player.Index);

                        if (player.TSPlayer.IsLoggedIn)
                        {
                            int pageNumber;
                            if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNumber))
                                return;
                            else
                            {
                                var uicInfo = new List<string>();
                                var time_1 = DateTime.Now.Subtract(DateTime.Parse(player.firstLogin));

                                uicInfo.Add(string.Format("Character info for {0}.", args.Parameters[0]));

                                uicInfo.Add(string.Format("First login: {0} ({1} ago).",
                                    player.firstLogin, STools.TimeSpanPlayed(time_1)));

                                uicInfo.Add("Last seen: Now");

                                uicInfo.Add(string.Format("Logged in {0} times since registering.  Overall play time: {1}.",
                                    player.loginCount, STools.TimePlayed(player.TimePlayed)));

                                PaginationTools.SendPage(args.Player, pageNumber, uicInfo, new PaginationTools.Settings
                                {
                                    HeaderFormat = "Character Information ({0}/{1})",
                                    HeaderTextColor = Color.Lime,
                                    LineTextColor = Color.White,
                                    FooterFormat = string.Format("Type /uic {0} {1} for more.", args.Parameters[0], pageNumber + 1),
                                    FooterTextColor = Color.Lime
                                });
                            }
                        }
                        else
                            args.Player.SendErrorMessage("You must be logged in to use this on yourself.");
                    }
                    else
                    {
                        args.Player.SendErrorMessage("Something broke. Please try again later.");
                    }
                }

                else
                {
                    string name = "";
                    bool needNumber = false;
                    if (args.Parameters.Count > 1)
                    {
                        var newArgs = new List<string>(args.Parameters);
                        newArgs.RemoveAt(newArgs.Count - 1);
                        name = string.Join(" ", newArgs);
                        needNumber = true;
                    }
                    else
                        name = string.Join(" ", args.Parameters);

                    int pageNumber;
                    if (!PaginationTools.TryParsePageNumber(args.Parameters,
                        needNumber ? args.Parameters.Count - 1 : args.Parameters.Count + 1, args.Player, out pageNumber))
                        return;

                    IPAddress IP;
                    if (IPAddress.TryParse(name, out IP))
                    {
                        if (STools.GetPlayerByIP(IP.ToString()).Count == 1)
                        {
                            SPlayer player = STools.GetPlayerByIP(IP.ToString())[0];

                            var uicInfo = new List<string>();
                            var time_1 = DateTime.Now.Subtract(DateTime.Parse(player.firstLogin));

                            uicInfo.Add(string.Format("Character info for {0}.", player.Name));

                            uicInfo.Add(string.Format("First login: {0} ({1} ago).",
                                player.firstLogin, STools.TimeSpanPlayed(time_1)));

                            uicInfo.Add("Last seen: Now");

                            uicInfo.Add(string.Format("Logged in {0} times since registering.  Overall play time: {1}.",
                                    player.loginCount, STools.TimePlayed(player.TimePlayed)));

                            PaginationTools.SendPage(args.Player, pageNumber, uicInfo, new PaginationTools.Settings
                            {
                                HeaderFormat = "Extended User Information ({0}/{1})",
                                HeaderTextColor = Color.Lime,
                                LineTextColor = Color.White,
                                FooterFormat = string.Format("Type /uic {0} {1} for more.", player.Name, pageNumber + 1),
                                FooterTextColor = Color.Lime
                            });

                        }
                        else if (STools.GetPlayerByIP(IP.ToString()).Count > 1)
                            //TShock.Utils.SendMultipleMatchError(args.Player,
                            args.Player.SendMultipleMatchError(
                                STools.GetPlayerByIP(IP.ToString()).Select(p => p.Name));
                        else
                            if (STools.GetStoredPlayerByIP(IP.ToString()).Count == 1)
                        {
                            StoredPlayer StoredPlayer = STools.GetStoredPlayerByIP(IP.ToString())[0];
                            var uicInfo = new List<string>();
                            var time_1 = DateTime.Now.Subtract(DateTime.Parse(StoredPlayer.firstLogin));
                            var time_2 = DateTime.Now.Subtract(DateTime.Parse(StoredPlayer.lastSeen));

                            uicInfo.Add(string.Format("Character info for {0}.", StoredPlayer.name));

                            uicInfo.Add(string.Format("First login: {0} ({1} ago).",
                                StoredPlayer.firstLogin, STools.TimeSpanPlayed(time_1)));

                            uicInfo.Add(string.Format("Last seen: {0} ({1} ago).", StoredPlayer.lastSeen,
                                STools.TimeSpanPlayed(time_2)));

                            uicInfo.Add(string.Format("Logged in {0} times since registering.  Overall play time: {1}.",
                                StoredPlayer.loginCount, STools.TimePlayed(StoredPlayer.totalTime)));

                            PaginationTools.SendPage(args.Player, pageNumber, uicInfo, new PaginationTools.Settings
                            {
                                HeaderFormat = "Character Information ({0}/{1})",
                                HeaderTextColor = Color.Lime,
                                LineTextColor = Color.White,
                                FooterFormat = string.Format("Type /uic {0} {1} for more.", StoredPlayer.name, pageNumber + 1),
                                FooterTextColor = Color.Lime
                            });
                        }

                        else if (STools.GetStoredPlayerByIP(IP.ToString()).Count > 1)
                            //TShock.Utils.SendMultipleMatchError(args.Player,
                            args.Player.SendMultipleMatchError(
                                STools.GetStoredPlayerByIP(IP.ToString()).Select(p => p.name));

                        else
                            args.Player.SendErrorMessage("Invalid IP address! Try /check ip \"{0}\".",
                        name);
                    }
                    else
                    {
                        if (STools.GetPlayer(name).Count == 1)
                        {
                            SPlayer player = STools.GetPlayer(name)[0];

                            var uicInfo = new List<string>();
                            var time_1 = DateTime.Now.Subtract(DateTime.Parse(player.firstLogin));

                            uicInfo.Add(string.Format("Character info for {0}.", player.Name));

                            uicInfo.Add(string.Format("First login: {0} ({1} ago).",
                                player.firstLogin, STools.TimeSpanPlayed(time_1)));

                            uicInfo.Add("Last seen: Now");

                            uicInfo.Add(string.Format("Logged in {0} times since registering.  Overall play time: {1}.",
                                    player.loginCount, STools.TimePlayed(player.TimePlayed)));

                            PaginationTools.SendPage(args.Player, pageNumber, uicInfo, new PaginationTools.Settings
                            {
                                HeaderFormat = "Extended User Information ({0}/{1})",
                                HeaderTextColor = Color.Lime,
                                LineTextColor = Color.White,
                                FooterFormat = string.Format("Type /uic {0} {1} for more.", player.Name, pageNumber + 1),
                                FooterTextColor = Color.Lime
                            });
                        }
                        else if (STools.GetPlayer(name).Count > 1)
                        {
                            //TShock.Utils.SendMultipleMatchError(args.Player, STools.GetPlayer(name).Select(p => p.Name));
                            args.Player.SendMultipleMatchError(STools.GetPlayer(name).Select(p => p.Name));
                        }
                        else
                        {
                            if (STools.GetStoredPlayer(name).Count == 1)
                            {
                                StoredPlayer StoredPlayer = STools.GetStoredPlayer(name)[0];

                                var uicInfo = new List<string>();
                                var time_1 = DateTime.Now.Subtract(DateTime.Parse(StoredPlayer.firstLogin));
                                var time_2 = DateTime.Now.Subtract(DateTime.Parse(StoredPlayer.lastSeen));

                                uicInfo.Add(string.Format("Character info for {0}.", StoredPlayer.name));

                                uicInfo.Add(string.Format("First login: {0} ({1} ago).",
                                    StoredPlayer.firstLogin, STools.TimeSpanPlayed(time_1)));

                                uicInfo.Add(string.Format("Last seen: {0} ({1} ago).", StoredPlayer.lastSeen,
                                    STools.TimeSpanPlayed(time_2)));

                                uicInfo.Add(string.Format("Logged in {0} times since registering.  Overall play time: {1}.",
                                    StoredPlayer.loginCount, STools.TimePlayed(StoredPlayer.totalTime)));

                                PaginationTools.SendPage(args.Player, pageNumber, uicInfo, new PaginationTools.Settings
                                {
                                    HeaderFormat = "Character Information ({0}/{1})",
                                    HeaderTextColor = Color.Lime,
                                    LineTextColor = Color.White,
                                    FooterFormat = string.Format("Type /uic {0} {1} for more.", StoredPlayer.name, pageNumber + 1),
                                    FooterTextColor = Color.Lime
                                });
                            }
                            else if (STools.GetStoredPlayer(name).Count > 1)
                            {
                                //TShock.Utils.SendMultipleMatchError(args.Player, STools.GetStoredPlayer(name).Select(
                                args.Player.SendMultipleMatchError(STools.GetStoredPlayer(name).Select(
                                    p => p.name));
                            }
                            else
                                args.Player.SendErrorMessage("Invalid player! Try /check name {0}.",
                                name);
                        }
                    }
                }
            }
            else
                //args.Player.SendErrorMessage("Invalid syntax. Try /uic [playerName]");
                args.Player.SendErrorMessage("Invalid syntax! Try /uic <player/self>");
        }
        #endregion

        /* ------------------ */

        #region Check Time
        public static void Check_Time(CommandArgs args)
        {
            if (args.Parameters.Count == 1)
            {
                if (STools.GetPlayer(args.Player.Index) != null)
                {
                    SPlayer player = STools.GetPlayer(args.Player.Index);
                    if (player.TSPlayer.IsLoggedIn)
                    {
                        args.Player.SendInfoMessage("You have played for {0}.", STools.TimePlayed(player.TimePlayed));
                    }
                    else
                        args.Player.SendErrorMessage("You must be logged in to use this command.");
                }
                else if (TSServerPlayer.Server.Name == args.Player.Name)
                    args.Player.SendErrorMessage("The console has no stats to check.");
                else
                    args.Player.SendErrorMessage("Something broke. Please try again later.");
            }

            if (args.Parameters.Count > 1 && args.Parameters.Count != 1)
            {
                args.Parameters.RemoveAt(0);
                string name = string.Join(" ", args.Parameters);

                if (STools.GetPlayer(name).Count == 1)
                {
                    SPlayer player = STools.GetPlayer(name)[0];
                    if (player.TSPlayer.IsLoggedIn)
                    {
                        args.Player.SendInfoMessage("{0} has played for {1}.", player.TSPlayer.Account.Name, STools.TimePlayed(player.TimePlayed));
                    }
                    else
                        args.Player.SendErrorMessage("{0} is not logged in.", player.Name);
                }
                else if (STools.GetPlayer(name).Count > 1)
                {
                    args.Player.SendMultipleMatchError(STools.GetPlayer(name).Select(p => p.Name));
                }
                else
                {
                    if (STools.GetStoredPlayer(name).Count == 1)
                    {
                        StoredPlayer StoredPlayer = STools.GetStoredPlayer(name)[0];
                        args.Player.SendInfoMessage("{0} has played for {1}.", StoredPlayer.name, STools.TimePlayed(StoredPlayer.totalTime));
                    }
                    else if (STools.GetStoredPlayer(name).Count > 1)
                    {
                        args.Player.SendMultipleMatchError(STools.GetStoredPlayer(name).Select(p => p.name));
                    }
                    else
                    {
                        args.Player.SendErrorMessage("Player {0} is not in the database.", name);
                    }
                }
            }
        }
        #endregion

        #region Check Name
        public static void Check_Name(CommandArgs args)
        {
            if (args.Parameters.Count > 1)
            {
                args.Parameters.RemoveAt(0);
                string name = string.Join(" ", args.Parameters);
                var player = TSPlayer.FindByNameOrID(name);

                if (player.Count > 1)
                    args.Player.SendMultipleMatchError(player.Select(ply => ply.Name));
                else if (player.Count == 1)
                    if (player[0].IsLoggedIn)
                        args.Player.SendInfoMessage("Username of {0} is \"{1}\".", player[0].Name, player[0].Account.Name);
                    else
                        args.Player.SendErrorMessage("{0} is not logged in.", player[0].Name);
                else
                    args.Player.SendErrorMessage("Could not find any players named \"{0}\".", name);
            }
            else
                args.Player.SendErrorMessage("Proper syntax: /check name <player>");
        }
        #endregion

        #region Check Kills
        public static void Check_Kills(CommandArgs args)
        {
            if (args.Parameters.Count == 1)
            {
                if (STools.GetPlayer(args.Player.Index) != null)
                {
                    SPlayer player = STools.GetPlayer(args.Player.Index);

                    if (player.TSPlayer.IsLoggedIn)
                    {
                        args.Player.SendInfoMessage("You have killed {0} player{4}, {1} mob{5}, {2} boss{6} and died {3} time{7}.", player.kills, player.mobkills, player.bosskills, player.deaths, STools.Suffix(player.kills), STools.Suffix(player.mobkills), STools.Suffix2(player.bosskills), STools.Suffix(player.deaths));
                    }
                    else
                        args.Player.SendErrorMessage("You must be logged in to use this on yourself.");
                }
                else if (TSServerPlayer.Server.Name == args.Player.Name)
                    args.Player.SendErrorMessage("The console has no stats to check.");
                else
                    args.Player.SendErrorMessage("Something broke. Please try again later.");
            }

            if (args.Parameters.Count > 1)
            {
                args.Parameters.RemoveAt(0); // fixes command fail when checking another player's kills
                string name = string.Join(" ", args.Parameters);

                if (STools.GetPlayer(name).Count == 1)
                {
                    SPlayer player = STools.GetPlayer(name)[0];
                    if (player.TSPlayer.IsLoggedIn)
                    {
                        args.Player.SendInfoMessage("{0} has killed {1} player{5}, {2} mob{6}, {3} boss{7} and died {4} time{8}.", player.TSPlayer.Account.Name, player.kills, player.mobkills, player.bosskills, player.deaths, STools.Suffix(player.kills), STools.Suffix(player.mobkills), STools.Suffix2(player.bosskills), STools.Suffix(player.deaths));
                    }
                    else
                        args.Player.SendErrorMessage("{0} is not logged in.", player.Name);
                }
                else if (STools.GetPlayer(name).Count > 1)
                {
                    args.Player.SendMultipleMatchError(STools.GetPlayer(name).Select(p => p.Name));
                }
                else
                {
                    if (STools.GetStoredPlayer(name).Count == 1)
                    {
                        StoredPlayer StoredPlayer = STools.GetStoredPlayer(name)[0];
                        args.Player.SendInfoMessage("{0} has killed {1} player{5}, {2} mob{6}, {3} boss{7} and died {4} time{8}.", StoredPlayer.name, StoredPlayer.kills, StoredPlayer.mobkills, StoredPlayer.bosskills, StoredPlayer.deaths, STools.Suffix(StoredPlayer.kills), STools.Suffix(StoredPlayer.mobkills), STools.Suffix2(StoredPlayer.bosskills), STools.Suffix(StoredPlayer.deaths));
                    }
                    else if (STools.GetStoredPlayer(name).Count > 1)
                    {
                        args.Player.SendMultipleMatchError(STools.GetStoredPlayer(name).Select(p => p.name));
                    }
                    else
                    {
                        args.Player.SendErrorMessage("Player {0} is not in the database.", name);
                    }
                }
            }
        }
        #endregion

        #region Check AFK
        public static void Check_Afk(CommandArgs args)
        {
            if (args.Parameters.Count == 1)
            {
                if (STools.GetPlayer(args.Player.Index) != null)
                {
                    SPlayer player = STools.GetPlayer(args.Player.Index);
                    if (player.AFK)
                        args.Player.SendInfoMessage("You have been AFK for {0} seconds.", player.AFKcount);
                    else
                        args.Player.SendInfoMessage("You are not AFK.");
                }
                else if (TSServerPlayer.Server.Name == args.Player.Name)
                    args.Player.SendErrorMessage("The console has no stats to check.");
                else
                    args.Player.SendErrorMessage("Something broke. Please try again later.");
            }

            if (args.Parameters.Count > 1)
            {
                args.Parameters.RemoveAt(0);
                string name = string.Join(" ", args.Parameters);
                var players = TSPlayer.FindByNameOrID(name); // fix command fail when afk checking an offline player

                if (players.Count == 0)
                {
                    args.Player.SendErrorMessage("Invalid player!");
                    return;
                }

                if (STools.GetPlayer(name).Count == 1)
                {
                    SPlayer player = STools.GetPlayer(name)[0];
                    if (player.AFK)
                        args.Player.SendInfoMessage("{0} has been AFK for {1} second{2}.", player.TSPlayer.Account.Name, player.AFKcount, STools.Suffix(player.AFKcount));
                    else
                        args.Player.SendInfoMessage("{0} is not AFK.", player.TSPlayer.Account.Name);
                }
                else if (STools.GetPlayer(name).Count > 1)
                {
                    args.Player.SendMultipleMatchError(STools.GetPlayer(name).Select(p => p.Name));
                }
                else
                {
                    args.Player.SendErrorMessage("Player {0} is not in the database.", args.Parameters[1]);
                }
            }
        }
        #endregion
    }
}
