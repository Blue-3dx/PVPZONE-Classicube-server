using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Maths;
using MCGalaxy.Tasks;
using PVPZone.Game.Item;
using PVPZone.Game.Player;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PVPZone.Game.Map
{
    public class SpawnedLoot
    {
        public Vec3U16 Position;
        public Level level;
        public ushort ItemId;
        public bool moving;
        public DateTime expire;
        public SpawnedLoot(Vec3U16 Position, Level level, ushort ItemId)
        {
            this.Position = Position;
            this.level = level;
            this.ItemId = (ushort)(256 + ItemId);
            moving = true;
            expire = DateTime.Now.AddSeconds(MCGalaxy.PVPZone.Config.Item.LootItemExpiryTime);
        }
        public void RemoveBlock()
        {
            this.level.BroadcastRevert( Position.X, Position.Y, Position.Z);
        }
        public bool Move()
        {
            if (DateTime.Now > expire) return true;
            if (!moving) return false;
            if (Position.Y == 0) {this.moving = false; return false; }
            Vec3U16 nextPosition = new Vec3U16(Position.X, (ushort)(Position.Y-1), Position.Z);
            if (!this.level.IsValidPos(nextPosition)) { return true; }

            if (this.level.FastGetBlock(nextPosition.X, nextPosition.Y, nextPosition.Z) != 0) { this.moving = false; return false; }

            RemoveBlock();
            this.level.BroadcastChange( nextPosition.X, nextPosition.Y, nextPosition.Z, this.ItemId);
            Position = nextPosition;
            return false;
        }
    }

    public class LootManager
    {
        public static SchedulerTask Task;

        public static Dictionary<Level, List<SpawnedLoot>> SpawnedLoot = new Dictionary<Level, List<SpawnedLoot>>();
        public static void Load()
        {
            Task = Server.MainScheduler.QueueRepeat(LootTick, null, TimeSpan.FromMilliseconds(60));
            MCGalaxy.Events.PlayerEvents.OnPlayerClickEvent.Register(PlayerClick, MCGalaxy.Priority.Normal);
        }
        public static void Unload()
        {
            MCGalaxy.Events.PlayerEvents.OnPlayerClickEvent.Unregister(PlayerClick);
            Server.MainScheduler.Cancel(Task);
        }
        static DateTime nextLoot = DateTime.Now;
        static System.Random rnd = new System.Random();
        private static void SpawnLoot(Level level)
        {
            if (level.players.Count == 0) { return; }
            //MCGalaxy.Player.Console.Message($"Spawning loot");
            

            if (!SpawnedLoot.ContainsKey(level))
                SpawnedLoot.Add(level, new List<SpawnedLoot>());

            if (SpawnedLoot[level].Count > MCGalaxy.PVPZone.Config.Item.LootItemMax)
                return;


            ItemManager.PVPZoneItems[] items = Enum.GetValues(typeof(ItemManager.PVPZoneItems)).Cast<ItemManager.PVPZoneItems>().ToArray();
            ushort block = (ushort)items[rnd.Next(items.Count() - 1)];

            ushort x = (ushort)rnd.Next(0, level.Width - 1);
            ushort z = (ushort)rnd.Next(0, level.Length - 1);
            ushort y = (ushort)(level.Height - 1);

            SpawnedLoot[level].Add(new SpawnedLoot(new Vec3U16(x, y, z), level, block));

           // MCGalaxy.Player.Console.Message($"Spawned loot {block} at {x} {y} {z}");
        }
        private static void MoveLoot(Level level)
        {
            if (!SpawnedLoot.ContainsKey(level))
                return;
            if (level.players.Count == 0)
            {
                SpawnedLoot.Remove(level);
                return;
            }
            for (int i = 0; i < SpawnedLoot[level].Count; i++)
            {
                var a = SpawnedLoot[level][i];
                if (!a.Move())
                    continue;
                a.RemoveBlock();
                SpawnedLoot[level].Remove(a);
                break;


            }
        }

        private static void LootTick(SchedulerTask task)
        {
            Task = task;
            bool spawnLoot = DateTime.Now > nextLoot;
            if (spawnLoot) nextLoot = DateTime.Now.AddSeconds(MCGalaxy.PVPZone.Config.Item.LootItemSpawnInteveral);
            try
            {
                foreach (var a in MCGalaxy.LevelInfo.Loaded.Items) // MCGalaxy.Games.IGame.RunningGames.Items)
                {
                    if (spawnLoot)
                        SpawnLoot(a);
                    MoveLoot(a);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void PlayerClick(MCGalaxy.Player player, MouseButton button, MouseAction act, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face)
        {

            if (!SpawnedLoot.ContainsKey(player.level)) return;
            PVPPlayer pvpPl = PVPPlayer.Get(player);
            if (pvpPl == null) return;

            for (int i = 0; i < SpawnedLoot[player.level].Count;i++)
            {
                var loot = SpawnedLoot[player.level][i];
                if (loot.Position.X != x)
                    continue;
                if (loot.Position.Y != y)
                    continue;
                if (loot.Position.Z != z)
                    continue;
                loot.RemoveBlock();
                pvpPl.Inventory.Add((ushort)(loot.ItemId - 256));
                SpawnedLoot[player.level].Remove(loot);
                pvpPl.SetHeldBlock(loot.ItemId);
                break;
            }
        }
    }
}
