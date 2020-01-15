# Statistics2
Statistics plugin for TShock servers (I had to name it Statistics2 cuz I forked CAWCAWCAW's version already)

Updated version of [Statistics](https://github.com/CAWCAWCAW/Statistics) by CAWCAWCAW (which was an outdated fork of a plugin originally made by WhiteX/QuiCM.

## Features
- track play time of all registered players (weeks, days, hours, minutes, seconds)
- AFK players will not gain play time 
- announce when a player goes AFK
- track kills of all registered players (players, mobs, bosses, deaths)

## Commands
- `/check` - shows proper stat checking command syntax
- `/check time [player]` - shows another player's play time, if no player name is specified, then it shows your own time.
- `/check afk [player]` - shows if another player is AFK, if no player name is specified, then it shows if you are AFK or not.
- `/check kills [player]` - shows another player's kills, if no player name is specified, then it shows your own kills.
- `uic [player]` - shows character info (will be changed later)
- `uix [player]` - shows extended info (will be changed later)

## Compatibility
- TShock Mobile (Bellatrix)
- TShock compiled with the same code from `gen-dev` branch
