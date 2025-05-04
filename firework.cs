//pluginref exp.dll
//pluginref Goodlyeffects.dll
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
        public static float FireworkPower = 1.5f;

        static Dictionary<string, bool> cooldowns = new Dictionary<string, bool>();
        static Random rand = new Random();

        public override string name { get { return "Firework"; } }
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

        void HandlePlayerClick(Player p, MouseButton button, MouseAction act, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face)
        {
            if (button == MouseButton.Left && act == MouseAction.Pressed)
            {
                BlockID held = p.ClientHeldBlock;
                if (held == Block.FromRaw(483))
                {
                    if (XPPlugin.GetLevel(p) < 10)
                    {
                        p.Message("&cYou Must Be At Least Level 10+ To Use This Item!");
                        return;
                    }

                    if (cooldowns.ContainsKey(p.name) && cooldowns[p.name])
                    {
                        p.Message("%cYou must wait 1 seconds before launching another Firework!");
                        return;
                    }

                    cooldowns[p.name] = true;
                    new Thread(() =>
                    {
                        Thread.Sleep(1000);
                        cooldowns[p.name] = false;
                    }).Start();

                    Firework firework = new Firework();
                    firework.Throw(p, FireworkPower);
                }
            }
        }

        #region Firework

        public class Firework
        {
            public void Throw(Player player, float power)
            {
                Vec3F32 dir = new Vec3F32(0f, 1f, 0f); // Straight up
                FireworkData data = MakeArgs(player, dir, power);
                data.startY = player.Pos.BlockY;

                SchedulerTask task = new SchedulerTask(FireworkCallback, data, TimeSpan.FromMilliseconds(85), true);
                player.CriticalTasks.Add(task);
            }

            private FireworkData MakeArgs(Player player, Vec3F32 dir, float power)
            {
                return new FireworkData {
                    block   = Block.FromRaw(483),
                    drag    = new Vec3F32(1f, 1f, 1f),
                    gravity = 0f,
                    pos     = player.Pos.BlockCoords,
                    last    = Round(player.Pos.BlockCoords),
                    next    = Round(player.Pos.BlockCoords),
                    vel     = new Vec3F32(0f, power, 0f),
                    player  = player
                };
            }

            private void FireworkCallback(SchedulerTask task)
            {
                FireworkData data = (FireworkData)task.State;
                if (TickFirework(data)) return;
                RevertLast(data.player, data);

                // 🎆 Spawn a random Goodly effect at the peak
                string[] effects = { "bluefirework", "greenfirework", "purplefirework", "rainbowfirework", "redfirework", "yellowfirework" };
                string effect = effects[rand.Next(effects.Length)];

                float fx = data.next.X;
                float fy = data.next.Y;
                float fz = data.next.Z;

                foreach (Player viewer in PlayerInfo.Online.Items)
                {
                    if (viewer.Level == data.player.Level)
                    {
                        GoodlyEffects.SpawnEffectFor(viewer, effect, fx, fy, fz, 0f, 0f, 0f);
                    }
                }

                task.Repeating = false;
            }

            private bool TickFirework(FireworkData data)
            {
                Player player = data.player;

                if (data.pos.Y >= data.startY + 10)
                    return false;

                Vec3U16 pos = data.next;
                BlockID cur = player.Level.GetBlock(pos.X, pos.Y, pos.Z);
                if (cur == Block.Invalid || cur != Block.Air) return false;

                data.pos += data.vel;
                data.next = Round(data.pos);

                if (data.last == data.next) return true;
                RevertLast(player, data);
                UpdateNext(player, data);
                data.last = data.next;
                return true;
            }

            private void RevertLast(Player player, FireworkData data)
            {
                player.Level.BroadcastRevert(data.last.X, data.last.Y, data.last.Z);
            }

            private void UpdateNext(Player player, FireworkData data)
            {
                player.Level.BroadcastChange(data.next.X, data.next.Y, data.next.Z, data.block);
            }

            private static Vec3U16 Round(Vec3F32 v)
            {
                return new Vec3U16((ushort)Math.Round(v.X), (ushort)Math.Round(v.Y), (ushort)Math.Round(v.Z));
            }
        }

        public class FireworkData
        {
            public Player player;
            public BlockID block;
            public Vec3F32 pos, vel, drag;
            public Vec3U16 last, next;
            public float gravity;
            public float startY;
        }

        #endregion
    }
}
