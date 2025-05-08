using MCGalaxy.Maths;
using MCGalaxy;
using System;
using MCGalaxy.Network;
using PVPZone.Game.Projectile;
using PVPZone.Game.Projectile.Projectiles;
using PVPZone.Game.Gamemodes;
using PVPZone.Game.Player;

namespace PVPZone
{
    public class Util
    {
        public static bool IsPVPLevel(Level level)
        {
            return level.Config.MOTD.Contains("+pvp") || PVPZoneGame.Instance.Map == level;
        }
        public static bool IsNoInventoryLevel(Level level)
        {
            return level.Config.MOTD.Contains("-inventory");
        }
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
                if (IsPVPLevel(pl.Level))
                {
                    PVPPlayer pvppl = PVPPlayer.Get(pl);
                    if (pvppl != null && pvppl.Dead)
                        continue;
                }
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
                if (IsPVPLevel(pl.Level))
                {
                    PVPPlayer pvppl = PVPPlayer.Get(pl);
                    if (pvppl != null && pvppl.Dead)
                        continue;
                }
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
                if (pl.EntityID == id)
                    return pl;
            }
            return null;
        }
        static System.Random rnd = new System.Random();

        static int rndDirection { get { return rnd.Next(0, 2) == 1 ? -1 : 1; } }
        public static void FakeExplosionEffect(Level lvl, int cx, int cy, int cz, ushort radius = 1)
        {
            for (int x = -radius; x <= radius; x++)
                for (int y = -radius; y <= radius; y++) // cy to cy + 2, effectively cy-1 to cy+1
                    for (int z = -radius; z <= radius; z++)
                    {
                        int px = cx + x, py = cy + y, pz = cz + z;
                        if (!lvl.IsValidPos(px, py, pz)) continue;
                        ushort block = lvl.GetBlock((ushort)px, (ushort)py, (ushort)pz);
                        if (block == Block.Air || block== Block.Invalid)
                            continue;

                        Projectile.Throw(new Debris() { BlockId = block}, lvl, new Vec3F32((float)px, (float)py+2, (float)pz), new Vec3F32((float)(rnd.NextDouble() * rndDirection * 0.4f), 0.5f + (float)(rnd.NextDouble()* 0.25f), (float)(rnd.NextDouble() * rndDirection * 0.4f)), 0.5f);
                    }
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

        public static void SetSpectator(MCGalaxy.Player p)
        {
            p.Send(Packet.HackControl(true, true, true, true, true, -1));
            Entities.GlobalDespawn(p, false);

            if (p.Extras.Contains("spectator"))
                return;
            p.Extras["spectator"] = true;
            p.Message("%eYou are %cdead! %fSpectate the game to your liking!");
        }
        public static void UnsetSpectator(MCGalaxy.Player p)
        {
            p.Send(Packet.HackControl(false, false, false, false, true, -1));

            Entities.GlobalSpawn(p, false);

            if (p.Extras.Contains("spectator"))
                p.Extras.Remove("spectator");
        }
    }
}
