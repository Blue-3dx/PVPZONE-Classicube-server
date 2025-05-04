//pluginref exp.dll
using System;
using System.Collections.Generic;
using System.Threading;
using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Maths;
using MCGalaxy.Network;
using MCGalaxy.Tasks;

using BlockID = System.UInt16;

namespace MCGalaxy
{
    public class Weapons : Plugin
    {
        public static float EnderpearlPower = 1.5f;     // Matches Minecraft Enderpearl speed
        public static float EnderpearlGravity = 0.05f;  // Matches Minecraft Enderpearl gravity

        static Dictionary<string, bool> cooldowns = new Dictionary<string, bool>();

        public override string name { get { return "Enderpearl"; } }
        public override string MCGalaxy_Version { get { return "1.9.4.9"; } }
        public override string creator { get { return "Blue3dx + Venk"; } }
        public override bool LoadAtStartup { get { return true; } }

        public override void Load(bool startup)
        {
            OnPlayerClickEvent.Register(HandlePlayerClick, Priority.Low);
        }

        public override void Unload(bool shutdown)
        {
            OnPlayerClickEvent.Unregister(HandlePlayerClick);
        }

        void HandlePlayerClick(Player p, MouseButton button, MouseAction action, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face)
        {
            if (button == MouseButton.Left && action == MouseAction.Pressed)
            {
                BlockID held = p.ClientHeldBlock;
                if (held == Block.FromRaw(128)) // Block 128 is the one to throw
                {
                    // Check level requirement (must be level 30+)
                    if (XPPlugin.GetLevel(p) < 30)
                    {
                        p.Message("&cYou Must Be At Least Level 30+ To Use This Item!");
                        return;
                    }
                    
                    if (cooldowns.ContainsKey(p.name) && cooldowns[p.name])
                    {
                        p.Message("%cYou must wait 15 seconds before throwing another Ender Pearl!");
                        return;
                    }

                    cooldowns[p.name] = true;
                    new Thread(() =>
                    {
                        Thread.Sleep(15000); // 15 seconds cooldown
                        cooldowns[p.name] = false;
                    }).Start();

                    Enderpearl Enderpearl = new Enderpearl();
                    Enderpearl.Throw(p, EnderpearlPower);
                }
            }
        }

        #region Enderpearl

        public class Enderpearl
        {
            public void Throw(Player player, float power)
            {
                Vec3F32 dir = DirUtils.GetDirVector(player.Rot.RotY, player.Rot.HeadX);
                EnderpearlData data = MakeArgs(player, dir, power);

                SchedulerTask task = new SchedulerTask(EnderpearlCallback, data, TimeSpan.FromMilliseconds(85), true);
                player.CriticalTasks.Add(task);
            }

            private EnderpearlData MakeArgs(Player player, Vec3F32 dir, float power)
            {
                EnderpearlData args = new EnderpearlData();
                args.block = Block.FromRaw(128); // Using block ID 128
                args.drag = new Vec3F32(0.99f, 0.99f, 0.99f);
                args.gravity = Weapons.EnderpearlGravity;
                args.pos = player.Pos.BlockCoords;
                args.last = Round(args.pos);
                args.next = Round(args.pos);
                args.vel = new Vec3F32(dir.X * power, dir.Y * power, dir.Z * power);
                args.player = player;
                return args;
            }

            private void RevertLast(Player player, EnderpearlData data)
            {
                player.level.BroadcastRevert(data.last.X, data.last.Y, data.last.Z);
            }

            private void UpdateNext(Player player, EnderpearlData data)
            {
                player.level.BroadcastChange(data.next.X, data.next.Y, data.next.Z, data.block);
            }

            private void OnHitBlock(EnderpearlData data, Vec3U16 pos, BlockID block)
            {
                // Teleport player to the block's coordinates + Y + 1
                Vec3F32 newPos = new Vec3F32(pos.X, pos.Y + 1, pos.Z);
                Command.Find("tp").Use(data.player, newPos.X + " " + newPos.Y + " " + newPos.Z);
            }

            private void EnderpearlCallback(SchedulerTask task)
            {
                EnderpearlData data = (EnderpearlData)task.State;
                if (TickEnderpearl(data)) return;

                RevertLast(data.player, data);
                task.Repeating = false;
            }

            private static Vec3U16 Round(Vec3F32 v)
            {
                unchecked { return new Vec3U16((ushort)Math.Round(v.X), (ushort)Math.Round(v.Y), (ushort)Math.Round(v.Z)); }
            }

            private bool TickEnderpearl(EnderpearlData data)
            {
                Player player = data.player;
                Vec3U16 pos = data.next;
                BlockID cur = player.level.GetBlock(pos.X, pos.Y, pos.Z);

                if (cur == Block.Invalid) return false;
                if (cur != Block.Air) { OnHitBlock(data, pos, cur); return false; }

                data.pos += data.vel;
                data.vel.X *= data.drag.X; data.vel.Y *= data.drag.Y; data.vel.Z *= data.drag.Z;
                data.vel.Y -= data.gravity;

                data.next = Round(data.pos);
                if (data.last == data.next) return true;

                RevertLast(data.player, data);
                UpdateNext(data.player, data);
                data.last = data.next;
                return true;
            }
        }

        public class EnderpearlData
        {
            public Player player;
            public BlockID block;
            public Vec3F32 pos, vel, drag;
            public Vec3U16 last, next;
            public float gravity;
        }

        #endregion
    }
}
