using MCGalaxy;
using MCGalaxy.Tasks;
using System;

namespace PVPZone.Game.Map
{
    public class LootManager
    {
        public static SchedulerTask Task;
        public static void Load()
        {
            Task = Server.MainScheduler.QueueRepeat(LootTick, null, TimeSpan.FromMilliseconds(50));
        }
        public static void Unload()
        {
            Server.MainScheduler.Cancel(Task);
        }

        private static void LootTick(SchedulerTask task)
        {
            Task = task;


        }
    }
}
