using MCGalaxy;
using MCGalaxy.SQL;
using System;
using System.Collections.Generic;

namespace PVPZone.Game.Player
{
    public class XPSystem
    {
        private static Cmdxp xpCmd;
        public static void Load()
        {
            Database.CreateTable("pvpzonexp", createInventories);
            MCGalaxy.Events.PlayerEvents.OnPlayerChatEvent.Register(OnPlayerChatHandler, Priority.High);
            // Register /xp command.
            xpCmd = new Cmdxp();
            Command.Register(xpCmd);
        }
        public static void Unload()
        {
            originalPrefix.Clear();
            MCGalaxy.Events.PlayerEvents.OnPlayerChatEvent.Unregister(OnPlayerChatHandler);

           Command.Unregister(xpCmd);
        }
        private static ColumnDesc[] createInventories = new ColumnDesc[] {
            new ColumnDesc("Name", ColumnType.VarChar, 16),
            new ColumnDesc("Level", ColumnType.UInt32),
            new ColumnDesc("Exp", ColumnType.UInt32),
        };
        
        public static uint GetXP(MCGalaxy.Player pl)
        {
            List<string[]> pRows = Database.GetRows("pvpzonexp", "*", "WHERE Name=@0", pl.name);
            /*if (pRows.Count != 0)
                pl.Message(pRows[0][0]);*/
            return pRows.Count == 0 ? 0 : uint.Parse(pRows[0][2]);
        }
        public static uint GetLevel(MCGalaxy.Player pl)
        {
            List<string[]> pRows = Database.GetRows("pvpzonexp", "*", "WHERE Name=@0", pl.name);
            /*if (pRows.Count != 0)
                pl.Message(pRows[0][0]);*/
            return pRows.Count == 0 ? 0 : uint.Parse(pRows[0][1]);
        }
        public static uint GetLevelXPRequired(uint level)
        {
            if (level == 0) return 0;
            if (level == 1) return 5;
            if (level == 2) return 10;
            if (level == 3) return 20;
            uint cumulative = 15;
            for (uint L = 4; L <= level; L++)
                cumulative += 10 * (L - 2);
            return cumulative;
        }
        public static void LevelUp(MCGalaxy.Player pl, uint amount=1)
        {
            uint newLevel = GetLevel(pl) + amount;
            SetLevel(pl, newLevel);

            pl.Message(MCGalaxy.PVPZone.Config.XP.XPMsg_LevelUp.Replace("{lvl}", newLevel.ToString()));
        }
        public static void ExpUp(MCGalaxy.Player pl, uint amount = 1)
        {
            uint newXP = GetXP(pl) + amount;
            SetXP(pl, newXP);
            
            uint nextLevel = GetLevel(pl) + 1;
            if (newXP < GetLevelXPRequired(nextLevel))
                return;
            LevelUp(pl);
          //  SetLevel(pl, nextLevel);
        }
        private static void SetLevel(MCGalaxy.Player pl, uint level)
        {
            List<string[]> pRows = Database.GetRows("pvpzonexp", "*", "WHERE Name=@0", pl.name);

            if (pRows.Count == 0)
            {
                Database.AddRow("pvpzonexp", "Name,  Level, Exp", pl.name, level, 0);
                return;
            }
            Database.UpdateRows("pvpzonexp", "Level=@0", "WHERE Name=@1", level, pl.name);
        }
        private static void SetXP(MCGalaxy.Player pl, uint Experience)
        {
            List<string[]> pRows = Database.GetRows("pvpzonexp", "*", "WHERE Name=@0", pl.name);

            if (pRows.Count == 0)
            {
                Database.AddRow("pvpzonexp", "Name,  Level, Exp", pl.name, 0, Experience);
                return;
            }
            Database.UpdateRows("pvpzonexp", "Exp=@0", "WHERE Name=@1", Experience, pl.name);

        }

        static Dictionary<string, string> originalPrefix = new Dictionary<string, string>();
        static void UpdateChatPrefix(MCGalaxy.Player p)
        {
            // Store the player's original prefix the first time.
            if (!originalPrefix.ContainsKey(p.name))
                originalPrefix[p.name] = p.prefix;  // Save current prefix that contains team title, etc.

            /*"&2Lvl " + GetLevel(p) + " |%f "*/
            p.prefix = MCGalaxy.PVPZone.Config.XP.ChatPrefix.Replace("{lvl}", GetLevel(p).ToString()) + originalPrefix[p.name] + p.color;
        }

        static void OnPlayerChatHandler(MCGalaxy.Player p, string message)
        {
            UpdateChatPrefix(p);
        }
    }
    // -------------------------
    // Command: /xp
    // -------------------------
    public class Cmdxp : Command
    {
        public override string name { get { return "xp"; } }
        public override string type { get { return CommandTypes.Information; } }

        // /xp shows your XP, level, and how much XP is needed for the next level.
        // /xp playername shows the same info for that player.
        // /xp give password player amount lets you give XP (only for players level 10+)

        private void GiveXP(MCGalaxy.Player p, string[] parts)
        {
            p.Message("&cThis give option is disabled currently!.");
            return; // Disable for now
            // xpected format: /xp give player amount
            if (parts.Length != 4)
            {
                Help(p);
                return;
            }
            if (XPSystem.GetLevel(p) < 10)
            {
                p.Message("&cYou need to be at least level 10 to give XP.");
                return;
            }

            string targetName = parts[1];
            MCGalaxy.Player target = PlayerInfo.FindMatches(p, targetName);
            if (target == null)
            {
                p.Message("&cPlayer not found.");
                return;
            }

            uint amount;
            try
            {
                amount = Convert.ToUInt32(parts[2]);
            }
            catch (Exception)
            {
                p.Message("&cInvalid amount.");
                return;
            }


            XPSystem.ExpUp(target, amount);
            p.Message("&aYou have given " + target.name + " " + amount + " XP.");
            target.Message("&aYou received " + amount + " XP from " + p.name + ".");
            return;
        }
        public override void Use(MCGalaxy.Player p, string message)
        {
            string trimmed = message.Trim();
            if (trimmed == "")
            {
                DisplayPlayerInfo(p, p);
                return;
            }
            string[] parts = trimmed.Split(' ');

            if (parts.Length >= 1 && parts[0].ToLower() == "give")
            {
                GiveXP(p, parts);
                return;
            }
            // Otherwise, assume it's a player name lookup.
            MCGalaxy.Player targetPlayer = PlayerInfo.FindMatches(p, trimmed);
            if (targetPlayer == null)
            {
                p.Message("&cPlayer not found.");
                return;
            }
            DisplayPlayerInfo(p, targetPlayer);
        }

        // Displays player's XP, level, and the XP required for the next level.
        void DisplayPlayerInfo(MCGalaxy.Player viewer, MCGalaxy.Player target)
        {
            uint xp = XPSystem.GetXP(target);
            uint level = XPSystem.GetLevel(target);

            viewer.Message("&a" + target.name + "'s XP: &e" + xp);
            viewer.Message("&a" + target.name + "'s Level: &e" + level);

            uint nextLevelXP = XPSystem.GetLevelXPRequired(level + 1);
            int xpNeeded = (int)nextLevelXP - (int)xp;
            viewer.Message("&aXP needed for next level: &e" + xpNeeded);
        }

        public override void Help(MCGalaxy.Player p)
        {
            p.Message("&T/xp");
            p.Message("&HShows your current XP, level, and how much XP is needed for the next level.");
            p.Message("&T/xp playername");
            p.Message("&HShows the XP info of the specified player.");
            p.Message("&T/xp give password player amount");
            p.Message("&HOnly players of level 10+ can use this to give XP to others.");
        }
    }

}
