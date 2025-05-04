using MCGalaxy;
using MCGalaxy.DB;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Tasks;
using PVPZone.Game.Item;
using System;
using BlockID = System.UInt16;
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
            MCGalaxy.Events.PlayerEvents.OnPlayerSpawningEvent.Register(PlayerSpawn, MCGalaxy.Priority.Normal);
            MCGalaxy.Events.PlayerEvents.OnPlayerDiedEvent.Register(PlayerDie, MCGalaxy.Priority.Normal);
            Task = Server.MainScheduler.QueueRepeat(PlayerTick, null, TimeSpan.FromMilliseconds(100));
        }
        public static void Unload()
        {
            MCGalaxy.Events.PlayerEvents.OnPlayerConnectEvent.Unregister(PlayerJoin);
            MCGalaxy.Events.PlayerEvents.OnPlayerDisconnectEvent.Unregister(PlayerDisconnect);
            MCGalaxy.Events.PlayerEvents.OnPlayerClickEvent.Unregister(PlayerClick);
            MCGalaxy.Events.PlayerEvents.OnPlayerSpawningEvent.Unregister(PlayerSpawn);
            MCGalaxy.Events.PlayerEvents.OnPlayerDiedEvent.Unregister(PlayerDie);
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
        private static void PlayerSpawn(MCGalaxy.Player player, ref Position pos, ref byte yaw, ref byte pitch, bool respawning)
        {
            PVPPlayer pl = PVPPlayer.Get(player);
            if (pl == null) return;
            pl.Spawn();
        }
        private static void PlayerDie(MCGalaxy.Player player, BlockID cause, ref TimeSpan cooldown)
        {
            PVPPlayer pl = PVPPlayer.Get(player);
            if (pl == null) return;
            pl.OnDeath();
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
