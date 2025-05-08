using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Tasks;
using PVPZone.Game.Player;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PVPZone.Game.Projectile
{
    public class ProjectileManager
    {
        public static Dictionary<Level, ProjectileLevel> ProjectileLevels = new Dictionary<Level, ProjectileLevel>();
        public static SchedulerTask Task;
        
        public static void Load()
        {
            ProjectileLevels.Clear();
            Task = Server.MainScheduler.QueueRepeat(ProjectileTick, null, TimeSpan.FromMilliseconds(50));
        }
        public static void Unload()
        {
            Server.MainScheduler.Cancel(Task);
            ProjectileLevels.Clear();
        }
        public static void ClearMap(Level level)
        {
            ProjectileLevels.Remove(level);
        }
        public static void SendProjectileData(MCGalaxy.Player pl)
        {
            if (pl.level == null || !ProjectileLevels.ContainsKey(pl.level))
                return;

            if (!ProjectileLevels.TryGetValue(pl.level, out var projectilelevel))
                return;

            projectilelevel.SendVisualUpdate(pl);
        }
        public static List<Projectile> GetProjectiles(Level level)
        {
            if (level == null || !ProjectileLevels.ContainsKey(level))
                return new List<Projectile>();

            if (!ProjectileLevels.TryGetValue(level, out var projectilelevel))
                return new List<Projectile>();

            return projectilelevel.Projectiles.Items.ToList();

        }
        public static bool PlayerClick(MCGalaxy.Player player, MouseButton button, MouseAction act, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face)
        {
            if (!ProjectileLevels.ContainsKey(player.level))
                return false;
            PVPPlayer pvpPlayer = PVPPlayer.Get(player);
            if (pvpPlayer == null)
                return false;
            ProjectileLevels.TryGetValue(player.level, out ProjectileLevel prLevel);
            if (prLevel == null)
                return false;
            Projectile projectile = prLevel.ProjectileAt(x, y, z);
            if (projectile == null)
                return false;
            projectile.OnClick(pvpPlayer);
            return true;
        }
        public static void ProjectileTick(SchedulerTask task)
        {
            Task = task;
            for (int i = 0; i < ProjectileLevels.Count; i++)
            {
                if (i >= ProjectileLevels.Count) return;
                Level lvl = ProjectileLevels.Keys.ElementAt(i);
                if (lvl == null || lvl.players.Count == 0)
                {
                    ProjectileLevels.Remove(lvl);
                    i--;
                    continue;
                }
                ProjectileLevels.TryGetValue(lvl, out ProjectileLevel prlevel);
                if (prlevel == null)
                {
                    ProjectileLevels.Remove(lvl);
                    i--;
                }
                try
                {
                    prlevel.Tick();
                }
                catch(Exception e)
                {
                    MCGalaxy.Player.Console.Message(e.ToString());
                }
            }
        }
        public static void ProjectileAdd(Level level, Projectile projectile)
        {
            if (!ProjectileLevels.ContainsKey(level))
                ProjectileLevels.Add(level, new ProjectileLevel(level));

            ProjectileLevels[level].Projectiles.Add(projectile);

        }
        public static void ProjectileAdd(Projectile projectile)
        {
            ProjectileAdd(projectile.Level, projectile);
        }
    }
}
