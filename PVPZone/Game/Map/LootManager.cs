using MCGalaxy;
using MCGalaxy.Maths;
using MCGalaxy.Tasks;
using System;
using System.Collections.Generic;

namespace PVPZone.Game.Map
{
    public class SpawnedLoot
    {
        Vec3U16 Position;
        Level level;
        ushort ItemId;
        bool moving;
        public SpawnedLoot(Vec3U16 Position, Level level, ushort ItemId)
        {
            this.Position = Position;
            this.level = level;
            this.ItemId = ItemId;
            moving = true;
        }
        public void RemoveBlock()
        {
            this.level.UpdateBlock(MCGalaxy.Player.Console, Position.X, Position.Y, Position.Z, 0);
        }
        public bool Move()
        {
            if (!moving) return false;
            if (Position.Y == 0) {this.moving = false; return false; }
            Vec3U16 nextPosition = new Vec3U16(Position.X, Position.Y, Position.Z);
            if (!this.level.IsValidPos(nextPosition)) { return true; }

            if (this.level.FastGetBlock(Position.X, (ushort)(Position.Y - 1), Position.Z) != 0) { this.moving = false; return false; }

            RemoveBlock();
            this.level.UpdateBlock(MCGalaxy.Player.Console, nextPosition.X, nextPosition.Y, nextPosition.Z, this.ItemId);
            return false;
        }
    }

    public class LootManager
    {
        public static SchedulerTask Task;

        public Dictionary<string, List<SpawnedLoot>> SpawnedLoot = new Dictionary<string, List<SpawnedLoot>>();
        public static void Load()
        {
            Task = Server.MainScheduler.QueueRepeat(LootTick, null, TimeSpan.FromMilliseconds(50));
        }
        public static void Unload()
        {
            Server.MainScheduler.Cancel(Task);
        }

        private static void MoveLoot(string level)
        {

        }

        private static void LootTick(SchedulerTask task)
        {
            Task = task;
        }
    }
}
