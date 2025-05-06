using MCGalaxy;
using MCGalaxy.Tasks;
using System;
using System.Collections.Generic;

namespace PVPZone.Game.Projectile
{
    public class ProjectileManager
    {
        public static List<Projectile> Projectiles = new List<Projectile>();
        public static SchedulerTask Task;
        
        public static void Load()
        {
            Task = Server.MainScheduler.QueueRepeat(ProjectileTick, null, TimeSpan.FromMilliseconds(50));
        }
        public static void Unload()
        {
            Server.MainScheduler.Cancel(Task);
        }

        public static void ProjectileTick(SchedulerTask task)
        {
            Task = task;
            for (int i = 0; i < Projectiles.Count; i++)
            {
                if (i >= Projectiles.Count) break;

                Projectile projectile = Projectiles[i];
                try
                {
                    if (projectile != null && projectile.Tick())
                        continue;
                }
                catch (Exception e)
                {
                    MCGalaxy.Player.Console.Message(e.ToString());
                }

                Projectiles.RemoveAt(i);
                i--;
            }
        }
        public static void ProjectileAdd(Projectile projectile)
        {
            Projectiles.Add(projectile);
        }
    }
}
