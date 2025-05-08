using MCGalaxy;
using PVPZone.Game.Player;
using System;

namespace PVPZone.Game.Commands
{
    // -------------------------
    // Command: /giveitem
    // -------------------------
    public class CmdGiveItem : Command
    {
        public override string name { get { return "giveitem"; } }
        public override string type { get { return CommandTypes.Information; } }

        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public override void Use(MCGalaxy.Player p, string message)
        {
            string trimmed = message.Trim();
            if (trimmed == "")
            {
                Help(p);
                return;
            }
            string[] parts = trimmed.Split(' ');
            if (parts.Length < 2)
            {
                Help(p);
                return;
            }
            MCGalaxy.Player target = PlayerInfo.FindMatches(p, parts[0]);
            if (target == null) return;
            PVPPlayer targetpvp = PVPPlayer.Get(target);
            if (targetpvp == null) return;

            ushort block = Block.Parse(p, parts[1]);
            if (block == Block.Invalid)
            {
                p.Message($"%c Couldn't find block \"{parts[1]}\"!");
                return;
            }

            ushort amount = 1;
            if (parts.Length > 2)
                amount = ushort.Parse(parts[2]);

            string blockname = Block.GetName(p, block);
            if (block > 256)
                block = (ushort)(block - 256);

            targetpvp.Inventory.Add(block, amount);
            target.Message($"%e[Server] {p.ColoredName} gave you {blockname} x{amount}!");
            p.Message($"%eYou gave {target.ColoredName} {blockname} x{amount}!");
        }

        public override void Help(MCGalaxy.Player p)
        {
            p.Message("&T/giveitem player block amount");
        }
    }
}
