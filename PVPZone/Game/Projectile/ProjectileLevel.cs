using MCGalaxy;
using MCGalaxy.Maths;
using MCGalaxy.Network;
using System;

namespace PVPZone.Game.Projectile
{
    public class ProjectileLevel
    {
        public VolatileArray<Projectile> Projectiles = new VolatileArray<Projectile>();
        public Level Level;
        public BufferedBlockSender BlockSender;

        public ProjectileLevel(Level level)
        {
            Level = level;
            BlockSender = new BufferedBlockSender(level);
        }

        public Projectile ProjectileAt(ushort x, ushort y, ushort z)
        {
            foreach(var p in Projectiles.Items)
            {
                Vec3U16 blockPos = Util.Round(p.Position);
                if (blockPos.X == x && blockPos.Y == y && blockPos.Z == z)
                    return p;
            }
            return null;
        }
        public void SendVisualUpdate(MCGalaxy.Player pl)
        {
            BufferedBlockSender sender = new BufferedBlockSender(pl);
            LoadBufferedBlockSender(sender);
            sender.Flush();
        }
        private void LoadBufferedBlockSender(BufferedBlockSender sender)
        {
            foreach (Projectile prj in Projectiles.Items)
            {
                if (!prj.UpdatePos) continue;

                prj.UpdatePos = false;

                Vec3U16 blockPos = Util.Round(prj.Position);
                Vec3U16 lastBlockPos = prj.PositionLast;

                if (Level.IsValidPos(lastBlockPos))
                    sender.Add(Level.PosToInt(lastBlockPos.X, lastBlockPos.Y, lastBlockPos.Z), Level.GetBlock(blockPos.X, blockPos.Y, blockPos.Z));
                if (Level.IsValidPos(blockPos))
                    sender.Add(Level.PosToInt(blockPos.X, blockPos.Y, blockPos.Z), prj.BlockId);
            }
        }
        public void Tick()
        {

            for (int i = 0; i < Projectiles.Count; i++)
            {
                if (i >= Projectiles.Count) break;

                Projectile projectile = Projectiles.Items[i];
                try
                {
                    if (projectile != null && projectile.Tick() && !projectile.Destroy)
                        continue;
                }
                catch (Exception e)
                {
                    MCGalaxy.Player.Console.Message(e.ToString());
                }
   
                Vec3U16 blockPos = Util.Round(projectile.Position);
                Vec3U16 lastBlockPos = projectile.PositionLast;
                if (Level.IsValidPos(blockPos))
                {
                    BlockSender.Add(Level.PosToInt(lastBlockPos.X, lastBlockPos.Y, lastBlockPos.Z), Level.GetBlock(lastBlockPos.X, lastBlockPos.Y, lastBlockPos.Z));
                    try
                    {
                        projectile.OnDestroy();
                    }
                    catch(Exception e)
                    {
                        MCGalaxy.Player.Console.Message(e.ToString());
                    }
                }
                if (Level.IsValidPos(lastBlockPos))
                    BlockSender.Add(Level.PosToInt(blockPos.X, blockPos.Y, blockPos.Z), Level.GetBlock(blockPos.X, blockPos.Y, blockPos.Z));
                Projectiles.Remove(projectile);
                i--;
            }
            LoadBufferedBlockSender(BlockSender);
            BlockSender.Flush();
        }

    }
}
