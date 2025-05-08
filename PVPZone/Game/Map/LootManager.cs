using MCGalaxy;
using MCGalaxy.Tasks;
using PVPZone.Game.Item;
using PVPZone.Game.Projectile;
using System;

namespace PVPZone.Game.Map
{
    public class LootManager
    {
    
        public DateTime nextLoot = DateTime.Now;
        public Level level;
        static System.Random rnd = new System.Random();
        
        public static void SpawnLoot(Level level)
        {
            if (level == null) return;
            if (level.players.Count == 0) { return; }
            if (ProjectileManager.GetProjectiles(level).Count >= MCGalaxy.PVPZone.Config.Item.LootItemMax)
                return;
            ushort block = (ushort)ItemManager.GetRandomItem();

            ushort x = (ushort)rnd.Next(0, level.Width - 1);
            ushort z = (ushort)rnd.Next(0, level.Length - 1);
            ushort y = (ushort)(level.Height - 1);
            
            var prj = new Projectile.Projectiles.Loot(block, (ushort)(ItemManager.Items.ContainsKey(block) ? ItemManager.Items[block].PickupAmount : 1));
            Projectile.Projectile.Drop(prj, level, x, y, z);
        }

        public void LootTick(SchedulerTask task)
        {
            if (DateTime.Now < nextLoot)
                return;
            nextLoot = DateTime.Now.AddSeconds(MCGalaxy.PVPZone.Config.Item.LootItemSpawnInteveral);

            SpawnLoot(level);
        }

        public LootManager(Level level)
        {
            this.level = level;
        }
       
    }
}
