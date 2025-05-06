using MCGalaxy;
using PVPZone.Game.Player;
using System;

namespace PVPZone.Game.Commands
{
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
