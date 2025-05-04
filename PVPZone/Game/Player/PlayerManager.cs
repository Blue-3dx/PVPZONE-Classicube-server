using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Tasks;
using PVPZone.Game.Item;
using System;

namespace PVPZone.Game.Player
{
    public class PlayerManager
    {
        public static SchedulerTask Task;
        public static void Load()
        {
            MCGalaxy.Events.PlayerEvents.OnPlayerConnectEvent.Register(PlayerJoin, MCGalaxy.Priority.High);
            MCGalaxy.Events.PlayerEvents.OnPlayerDisconnectEvent.Register(PlayerDisconnect, MCGalaxy.Priority.High);
            MCGalaxy.Events.PlayerEvents.OnPlayerClickEvent.Register(PlayerClick, MCGalaxy.Priority.Normal);
            Task = Server.MainScheduler.QueueRepeat(PlayerTick, null, TimeSpan.FromMilliseconds(100));
        }
        public static void Unload()
        {
            MCGalaxy.Events.PlayerEvents.OnPlayerConnectEvent.Unregister(PlayerJoin);
            MCGalaxy.Events.PlayerEvents.OnPlayerDisconnectEvent.Unregister(PlayerDisconnect);
            MCGalaxy.Events.PlayerEvents.OnPlayerClickEvent.Unregister(PlayerClick);
            Server.MainScheduler.Cancel(Task);
        }

        private static void PlayerJoin(MCGalaxy.Player player)
        {
            PVPPlayer pl = new PVPPlayer(player);//Auto adds to list
        }
        private static void PlayerDisconnect(MCGalaxy.Player player, string reason)
        {
            PVPPlayer pvpPlayer = PVPPlayer.Get(player);
            if (pvpPlayer == null) return;
            ItemManager.PlayerDisconnect(pvpPlayer);
            PVPPlayer.Players.Remove(pvpPlayer);
        }
        private static void PlayerClick(MCGalaxy.Player player, MouseButton button, MouseAction act, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face)
        {
            PVPPlayer pl = PVPPlayer.Get(player);

            if (pl == null) return;

            if (button == MouseButton.Left && act == MouseAction.Pressed)
            {
                pl.Punch(entity);
                return;
            }
            if (button == MouseButton.Right && act == MouseAction.Pressed)
            {
                pl.UseItem();
            }
        }

        private static void PlayerTick(SchedulerTask task)
        {
            Task = task;
            try
            {
                foreach (var pl in PVPPlayer.Players)
                {
                    pl.Think();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
