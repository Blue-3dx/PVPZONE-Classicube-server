using MCGalaxy.Maths;
using MCGalaxy;
using System;
using MCGalaxy.Network;

namespace PVPZone
{
    public class Util
    {
        public static Vec3U16 Round(Vec3F32 v)
        {
            unchecked { return new Vec3U16((ushort)Math.Round(v.X), (ushort)Math.Round(v.Y), (ushort)Math.Round(v.Z)); }
        }

        public static MCGalaxy.Player PlayerAt(MCGalaxy.Player player, Vec3U16 pos)
        {
            foreach (MCGalaxy.Player pl in PlayerInfo.Online.Items)
            {
                if (pl == player) continue;
                if (pl.Level != player.Level) continue;
                if (Math.Abs(pl.Pos.BlockX - pos.X) <= 1 && Math.Abs(pl.Pos.BlockY - pos.Y) <= 1 && Math.Abs(pl.Pos.BlockZ - pos.Z) <= 1)
                    return pl;
            }
            return null;
        }
        public static MCGalaxy.Player PlayerAt(Level level, Vec3U16 pos)
        {
            foreach (MCGalaxy.Player pl in PlayerInfo.Online.Items)
            {
                if (pl.Level != level) continue;
                if (Math.Abs(pl.Pos.BlockX - pos.X) <= 1 && Math.Abs(pl.Pos.BlockY - pos.Y) <= 1 && Math.Abs(pl.Pos.BlockZ - pos.Z) <= 1)
                    return pl;
            }
            return null;
        }
        public static MCGalaxy.Player PlayerAt(Level level, int x, int y, int z)
        {
            return PlayerAt(level, new Vec3U16((ushort)x, (ushort)y, (ushort)z));
        }
        public static MCGalaxy.Player PlayerAt(Level level, ushort x, ushort y, ushort z)
        {
            return PlayerAt(level, new Vec3U16((ushort)x, (ushort)y, (ushort)z));
        }
        public static MCGalaxy.Player PlayerAt(MCGalaxy.Player player, int x, int y, int z)
        {
            return PlayerAt(player, new Vec3U16((ushort)x, (ushort)y, (ushort)z));
        }
        public static MCGalaxy.Player PlayerAt(MCGalaxy.Player player, ushort x, ushort y, ushort z)
        {
            return PlayerAt(player, new Vec3U16((ushort)x, (ushort)y, (ushort)z));
        }
        public static MCGalaxy.Player PlayerFrom(byte id)
        {
            foreach (MCGalaxy.Player pl in PlayerInfo.Online.Items)
            {
                if (pl.EntityID == id);
                    return pl;
            }
            return null;
        }

        public static void Effect(Level level, string effect, int bx, int by, int bz )
        {
            foreach (MCGalaxy.Player pl in PlayerInfo.Online.Items)
            {
                if (pl.Level != level) continue;

                if (!pl.Supports(CpeExt.CustomParticles)) continue;

                GoodlyEffects.SpawnEffectFor(pl, effect, bx, by, bz, 0, 0, 0);
            }
        }

        public static void BroadcastMessage(Level level, string message)
        {
            foreach (Player pl in PlayerInfo.Online.Items)
            {
                if (level == pl.level)
                    pl.Message(message);
            }
        }

        public static string HealthBar(string symbol, int amount, int max)
        {
            string bar = "%f";
            for (int i = 0; i < amount; i++) bar += symbol;
            bar += "%0";
            for (int i = amount; i < max; i++) bar += symbol;// (i < amount) ? symbol : "%0" + symbol;
            return bar;
        }
        public static void SetHotbar(Player p, byte slot, ushort block)
        {
            byte[] buffer = Packet.SetHotbar(block, slot, p.Session.hasExtBlocks);
            p.Send(buffer);

        }

        public static void ClearHotbar(Player p)
        {
            for (byte i = 0; i < 9; i++)
            {
                SetHotbar(p, i, 0);
            }
        }
    }
}
