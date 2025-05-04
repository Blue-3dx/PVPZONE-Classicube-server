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
        public static float BlastBallPower = 3.0f;
        public static float BlastBallGravity = 0.05f;

        static Dictionary<string, bool> cooldowns = new Dictionary<string, bool>();
        static Random rand = new Random();

        public override string name { get { return "BlastBallRocket"; } }
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
                if (held == Block.FromRaw(501))
                {
                    if (XPPlugin.GetLevel(p) < 10)
                    {
                        p.Message("&cYou Must Be At Least Level 10+ To Use This Item!");
                        return;
                    }

                    if (cooldowns.ContainsKey(p.name) && cooldowns[p.name])
                    {
                        p.Message("%cYou must wait 10 seconds before launching another BlastBall!");
                        return;
                    }

                    cooldowns[p.name] = true;
                    new Thread(() =>
                    {
                        Thread.Sleep(10000);
                        cooldowns[p.name] = false;
                    }).Start();

                    BlastBall BlastBall = new BlastBall();
                    BlastBall.Throw(p, BlastBallPower);
                }
            }
        }

        #region BlastBall

        public class BlastBall
        {
            public void Throw(Player player, float power)
            {
                Vec3F32 dir = DirUtils.GetDirVector(player.Rot.RotY, player.Rot.HeadX);
                BlastBallData data = MakeArgs(player, dir, power);
                data.startY = player.Pos.BlockY;

                SchedulerTask task = new SchedulerTask(BlastBallCallback, data, TimeSpan.FromMilliseconds(85), true);
                player.CriticalTasks.Add(task);
            }

            private BlastBallData MakeArgs(Player player, Vec3F32 dir, float power)
            {
                return new BlastBallData {
                    block   = Block.FromRaw(501),
                    drag    = new Vec3F32(0.99f, 0.99f, 0.99f),
                    gravity = Weapons.BlastBallGravity,
                    pos     = player.Pos.BlockCoords,
                    last    = Round(player.Pos.BlockCoords),
                    next    = Round(player.Pos.BlockCoords),
                    vel     = new Vec3F32(dir.X * power, dir.Y * power, dir.Z * power),
                    player  = player
                };
            }

            private void BlastBallCallback(SchedulerTask task)
            {
                BlastBallData data = (BlastBallData)task.State;
                if (TickBlastBall(data)) return;
                RevertLast(data.player, data);
                task.Repeating = false;
            }

            private bool TickBlastBall(BlastBallData data)
            {
                Player player = data.player;
                Vec3U16 pos = data.next;
                BlockID cur = player.Level.GetBlock(pos.X, pos.Y, pos.Z);

                if (cur == Block.Invalid) return false;

                if (cur != Block.Air)
                {
                    SpawnHitEffect(data);
                    return false;
                }

                Player hitPlayer = PlayerAt(player, pos, true);
                if (hitPlayer != null)
                {
                    OnHitPlayer(data, hitPlayer);
                    SpawnHitEffect(data);
                    return false;
                }

                data.pos += data.vel;
                data.vel.X *= data.drag.X; data.vel.Y *= data.drag.Y; data.vel.Z *= data.drag.Z;
                data.vel.Y -= data.gravity;
                data.next = Round(data.pos);

                if (data.last == data.next) return true;
                RevertLast(player, data);
                UpdateNext(player, data);
                data.last = data.next;
                return true;
            }

            private void RevertLast(Player player, BlastBallData data)
            {
                player.Level.BroadcastRevert(data.last.X, data.last.Y, data.last.Z);
            }

            private void UpdateNext(Player player, BlastBallData data)
            {
                player.Level.BroadcastChange(data.next.X, data.next.Y, data.next.Z, data.block);
            }

            private void SpawnHitEffect(BlastBallData data)
            {
                string[] effects = { "bluefirework", "greenfirework", "purplefirework", "rainbowfirework", "redfirework", "yellowfirework" };
                string effect = effects[rand.Next(effects.Length)];

                foreach (Player viewer in PlayerInfo.Online.Items)
                {
                    if (viewer.Level == data.player.Level)
                    {
                        GoodlyEffects.SpawnEffectFor(viewer, effect, data.next.X, data.next.Y, data.next.Z, 0f, 0f, 0f);
                    }
                }
            }

private void OnHitPlayer(BlastBallData data, Player pl)
{
    Player attacker = data.player;
	
                if (pl.Model == "shieldb3")
    {
        return;  // Exit the method and do nothing
    }
    int dx = attacker.Pos.X - pl.Pos.X;
    int dy = attacker.Pos.Y - pl.Pos.Y;
    int dz = attacker.Pos.Z - pl.Pos.Z;
    Vec3F32 dir = Vec3F32.Normalise(new Vec3F32(dx, dy, dz));

    float strength = 15f; // 10x chaos
    float upwardBoost = (float)(rand.NextDouble() * 2.5 + 1.5); // Adds 1.5 - 4.0 Y velocity

    if (pl.Supports(CpeExt.VelocityControl) && attacker.Supports(CpeExt.VelocityControl))
    {
        pl.Send(Packet.VelocityControl(
            -dir.X * strength,
             upwardBoost,
            -dir.Z * strength,
            0, 1, 0));
    }
    else
    {
        attacker.Message("Knockback failed: client lacks VelocityControl.");
    }
}

            private Player PlayerAt(Player player, Vec3U16 pos, bool skipSelf)
            {
                foreach (Player pl in PlayerInfo.Online.Items)
                {
                    if (pl.Level != player.Level) continue;
                    if (skipSelf && pl == player) continue;
                    if (Math.Abs(pl.Pos.BlockX - pos.X) <= 1 &&
                        Math.Abs(pl.Pos.BlockY - pos.Y) <= 1 &&
                        Math.Abs(pl.Pos.BlockZ - pos.Z) <= 1)
                        return pl;
                }
                return null;
            }

            private static Vec3U16 Round(Vec3F32 v)
            {
                return new Vec3U16((ushort)Math.Round(v.X), (ushort)Math.Round(v.Y), (ushort)Math.Round(v.Z));
            }
        }

        public class BlastBallData
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
