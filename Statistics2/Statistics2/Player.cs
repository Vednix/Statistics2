using TShockAPI;
using Terraria;
using Microsoft.Xna.Framework;

namespace Statistics
{
    public class SPlayer
    {
        public int Index;

        public TSPlayer TSPlayer { get { return TShock.Players[Index]; } }
        public string Name { get { return Main.player[Index].name; } }

        public bool AFK = false;
        public int AFKcount = 0;
        public float LastPosX { get; set; }
        public float LastPosY { get; set; }

        public int TotalPoints { get; set; }
        public int TimePlayed = 0;

        public string firstLogin;
        public string lastSeen;
        public int loginCount;
        public string knownAccounts;
        public string knownIPs;

        public int deaths;
        public int kills;
        public int mobkills;
        public int bosskills;

        public Vector2 posPoint;

        public SPlayer KillingPlayer = null;

        public SPlayer(int index)
        {
            Index = index;
            LastPosX = TShock.Players[Index].X;
            LastPosY = TShock.Players[Index].Y;
        }
    }

    public class StoredPlayer
    {
        public string name;
        public string firstLogin;
        public string lastSeen;
        public int totalTime;
        public int loginCount;
        public string knownAccounts;
        public string knownIPs;

        public int kills;
        public int deaths;
        public int mobkills;
        public int bosskills;

        public StoredPlayer(string name, string firstLogin, string lastSeen, int totalTime, int loginCount,
            string knownAccounts, string knownIPs, int kills, int deaths, int mobkills, int bosskills)
        {
            this.name = name;
            this.firstLogin = firstLogin;
            this.lastSeen = lastSeen;
            this.totalTime = totalTime;
            this.loginCount = loginCount;
            this.knownAccounts = knownAccounts;
            this.knownIPs = knownIPs;
            this.kills = kills;
            this.deaths = deaths;
            this.mobkills = mobkills;
            this.bosskills = bosskills;
        }
    }
}
