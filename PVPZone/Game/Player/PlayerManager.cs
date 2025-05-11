using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Tasks;
using PVPZone.Game.Item;
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
            OnPlayerConnectEvent.Register(PlayerJoin, MCGalaxy.Priority.High);
            OnPlayerDisconnectEvent.Register(PlayerDisconnect, MCGalaxy.Priority.High);
            OnPlayerClickEvent.Register(PlayerClick, MCGalaxy.Priority.Normal);
            OnPlayerSpawningEvent.Register(PlayerSpawn, MCGalaxy.Priority.Normal);
            OnPlayerDiedEvent.Register(PlayerDie, MCGalaxy.Priority.Normal);
            OnSentMapEvent.Register(PlayerSentMap, MCGalaxy.Priority.Normal);
            OnBlockChangingEvent.Register(PlayerChangingBlock, MCGalaxy.Priority.High);
            OnJoinedLevelEvent.Register(PlayerJoinedLevel, Priority.High);

            Task = Server.MainScheduler.QueueRepeat(PlayerTick, null, TimeSpan.FromMilliseconds(100));
        }
        public static void Unload()
        {
            OnPlayerConnectEvent.Unregister(PlayerJoin);
            OnPlayerDisconnectEvent.Unregister(PlayerDisconnect);
            OnPlayerClickEvent.Unregister(PlayerClick);
            OnPlayerSpawningEvent.Unregister(PlayerSpawn);
            OnPlayerDiedEvent.Unregister(PlayerDie);
            OnSentMapEvent.Unregister(PlayerSentMap);
            OnBlockChangingEvent.Unregister(PlayerChangingBlock);
            OnJoinedLevelEvent.Unregister(PlayerJoinedLevel);

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
            if (!Util.CanBreakBlocks(p.level) && !placing)
            {
                p.RevertBlock(x, y, z);
                cancel = true;
                return;
            }
            if (p.Game.Referee || block == Block.Air)
                return;

            if (!Util.IsPVPLevel(p.level))
                return;
            
            if (ItemManager.Items.ContainsKey(block) && !ItemManager.Items[block].Placeable)
            {
                cancel = true;
                p.RevertBlock(x, y, z);
                return;   
            }

            if (Util.IsNoInventoryLevel(p.level))
                return;

            PVPPlayer pvppl = PVPPlayer.Get(p);

            if (pvppl == null) return;

            if (pvppl.Inventory.Has(block))
            {
                pvppl.Inventory.Remove(block);
                return;
            }
            cancel = true;
            p.RevertBlock(x, y, z);
            
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
            var pvppl = PVPPlayer.Get(p);
            if (pvppl != null)
                pvppl.Inventory.SendCanPlaceBreak();
        }
        private static void PlayerSentMap(MCGalaxy.Player p, Level prevLevel, Level level)
        {
            SendMiningUnbreakableMessage(p);
            ProjectileManager.SendProjectileData(p);
        }
        private static void PlayerJoinedLevel(MCGalaxy.Player p, Level prevLevel, Level level, ref bool announce)
        {
            ProjectileManager.SendProjectileData(p);
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
