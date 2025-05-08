using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using PVPZone.Game.Item;
using PVPZone.Game.Map;
using PVPZone.Game.Projectile;
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
            MCGalaxy.Events.PlayerEvents.OnSentMapEvent.Register(PlayerSentMap, MCGalaxy.Priority.Normal);
            MCGalaxy.Events.PlayerEvents.OnBlockChangingEvent.Register(PlayerChangingBlock, MCGalaxy.Priority.Normal);

            Task = Server.MainScheduler.QueueRepeat(PlayerTick, null, TimeSpan.FromMilliseconds(100));
        }
        public static void Unload()
        {
            MCGalaxy.Events.PlayerEvents.OnPlayerConnectEvent.Unregister(PlayerJoin);
            MCGalaxy.Events.PlayerEvents.OnPlayerDisconnectEvent.Unregister(PlayerDisconnect);
            MCGalaxy.Events.PlayerEvents.OnPlayerClickEvent.Unregister(PlayerClick);
            MCGalaxy.Events.PlayerEvents.OnPlayerSpawningEvent.Unregister(PlayerSpawn);
            MCGalaxy.Events.PlayerEvents.OnPlayerDiedEvent.Unregister(PlayerDie);
            MCGalaxy.Events.PlayerEvents.OnSentMapEvent.Unregister(PlayerSentMap);
            MCGalaxy.Events.PlayerEvents.OnBlockChangingEvent.Unregister(PlayerChangingBlock);

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
            if (!Util.IsPVPLevel(player.level)) return;
            PVPPlayer pl = PVPPlayer.Get(player);
            if (pl == null) return;
            pl.Spawn();
        }
        private static void PlayerChangingBlock(MCGalaxy.Player p, ushort x, ushort y, ushort z, BlockID block, bool placing, ref bool cancel)
        {
            if (Util.IsPVPLevel(p.level))
            {
                cancel = true;
                p.RevertBlock(x,y,z);
            }
        }
        private static void PlayerDie(MCGalaxy.Player player, BlockID cause, ref TimeSpan cooldown)
        {
            if (!Util.IsPVPLevel(player.level)) return;
            PVPPlayer pl = PVPPlayer.Get(player);
            if (pl == null) return;
            pl.OnDeath();
        }
        private static void PlayerClick(MCGalaxy.Player player, MouseButton button, MouseAction act, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face)
        {
            if (!Util.IsPVPLevel(player.level)) return;
            PVPPlayer pl = PVPPlayer.Get(player);

            if (pl == null) return;

            if (ProjectileManager.PlayerClick(player, button, act, yaw, pitch, entity, x, y, z, face))
                return;

            if (button == MouseButton.Left)
            {
                pl.Punch(entity);
                return;
            }
            if (button == MouseButton.Right && act == MouseAction.Pressed)
            {
                pl.UseItem();
                return;
            }
        }
        private static void SendMiningUnbreakableMessage(MCGalaxy.Player p)
        {
            if (!Util.IsPVPLevel(p.level)) return;
            bool extBlocks = p.Session.hasExtBlocks;
            int count = p.Session.MaxRawBlock + 1;
            int size = extBlocks ? 5 : 4;
            byte[] bulk = new byte[count * size];
            Level level = p.level;
            for (int i = 0; i < count; i++)
            {
                bool canPlace = p.Game.Referee;
                bool canBreak = p.Game.Referee;
          
                Packet.WriteBlockPermission((BlockID)i, i != 0 ? canPlace : true, i == 0 ? true : canBreak, p.Session.hasExtBlocks, bulk, i * size);
            }
            p.Send(bulk);
        }
        private static void PlayerSentMap(MCGalaxy.Player p, Level prevLevel, Level level)
        {
            SendMiningUnbreakableMessage(p);
        }
        private static void PlayerTick(SchedulerTask task)
        {
            Task = task;
            try
            {
                foreach (var pl in PVPPlayer.Players)
                {
                    if (!Util.IsPVPLevel(pl.MCGalaxyPlayer.level))
                        continue;
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
