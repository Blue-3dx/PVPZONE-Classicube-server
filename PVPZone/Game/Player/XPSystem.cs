using MCGalaxy;
using MCGalaxy.SQL;
using System;
using System.Collections.Generic;

namespace PVPZone.Game.Player
{
    public class XPSystem
    {
        public static void Load()
        {
            Database.CreateTable("pvpzonexp", createInventories);
            MCGalaxy.Events.PlayerEvents.OnPlayerChatEvent.Register(OnPlayerChatHandler, Priority.High);

        }
        public static void Unload()
        {
            originalPrefix.Clear();
            MCGalaxy.Events.PlayerEvents.OnPlayerChatEvent.Unregister(OnPlayerChatHandler);
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
            string msg = MCGalaxy.PVPZone.Config.XP.XPMsg_LevelUp.Replace("{lvl}", newLevel.ToString());
            string msgGlobal = msg.Replace("{pl}", pl.ColoredName);
            pl.Message(msg.Replace("{pl}", "%aYou"));
            foreach(var p in PlayerInfo.Online.Items)
            {
                if (pl == p || p.level != pl.level)
                    continue;
                p.Message(msgGlobal);
            }
        }
        public static void ExpUp(MCGalaxy.Player pl, uint amount = 1)
        {
            uint newXP = GetXP(pl) + amount;
            SetXP(pl, newXP);

            PVPPlayer pvpPl = PVPPlayer.Get(pl);
            if (pvpPl != null)
                pvpPl.GuiHint(MCGalaxy.PVPZone.Config.XP.XPMsg_XPUp.Replace("{xp}", amount.ToString()));

            uint oldLevel = GetLevel(pl)+1;
            uint lvlAmount = 0;
            while (newXP >= GetLevelXPRequired(oldLevel + lvlAmount))
                lvlAmount++;
            
            if (lvlAmount == 0) return;
            LevelUp(pl, lvlAmount);
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
}
