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
        public static float BallPower = 1.5f;     // Matches Minecraft Ball speed
        public static float BallGravity = 0.05f;  // Matches Minecraft Ball gravity

        static Dictionary<string, bool> cooldowns = new Dictionary<string, bool>();

        public override string name { get { return "Ball"; } }
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
                // Only proceed if holding a snowball (ID 488)
                if (held == Block.FromRaw(488))
                {
                    // Check level requirement
                    if (XPPlugin.GetLevel(p) < 10)
                    {
                        p.Message("&cYou Must Be At Least Level 10+ To Use This Item!");
                        return;
                    }

                    // Enforce cooldown between throws
                    if (cooldowns.ContainsKey(p.name) && cooldowns[p.name])
                    {
                        p.Message("%cYou must wait 3 seconds before throwing another Ball!");
                        return;
                    }

                    cooldowns[p.name] = true;
                    new Thread(() =>
                    {
                        Thread.Sleep(3000); // 3 seconds cooldown
                        cooldowns[p.name] = false;
                    }).Start();

                    Ball ball = new Ball();
                    ball.Throw(p, BallPower);
                }
            }
        }

        #region Ball

        public class Ball
        {
            public void Throw(Player player, float power)
            {
                Vec3F32 dir = DirUtils.GetDirVector(player.Rot.RotY, player.Rot.HeadX);
                BallData data = MakeArgs(player, dir, power);

                SchedulerTask task = new SchedulerTask(BallCallback, data, TimeSpan.FromMilliseconds(85), true);
                player.CriticalTasks.Add(task);
            }

            private BallData MakeArgs(Player player, Vec3F32 dir, float power)
            {
                return new BallData {
                    block   = Block.FromRaw(488),
                    drag    = new Vec3F32(0.99f, 0.99f, 0.99f),
                    gravity = Weapons.BallGravity,
                    pos     = player.Pos.BlockCoords,
                    last    = Round(player.Pos.BlockCoords),
                    next    = Round(player.Pos.BlockCoords),
                    vel     = new Vec3F32(dir.X * power, dir.Y * power, dir.Z * power),
                    player  = player
                };
            }

            private void RevertLast(Player player, BallData data)
            {
                player.Level.BroadcastRevert(data.last.X, data.last.Y, data.last.Z);
            }

            private void UpdateNext(Player player, BallData data)
            {
                player.Level.BroadcastChange(data.next.X, data.next.Y, data.next.Z, data.block);
            }

            private void OnHitBlock(BallData data, Vec3U16 pos, BlockID block)
            {
                data.player.Message("");
            }

            private void OnHitPlayer(BallData data, Player pl)
            {
                Player attacker = data.player;

                // Check if the player being hit has the shield model
                if (pl.Model == "shieldb3")
                {
                    // If the player has the shield model, do nothing (no knockback)
                    return;
                }

                int dx = attacker.Pos.X - pl.Pos.X;
                int dy = attacker.Pos.Y - pl.Pos.Y;
                int dz = attacker.Pos.Z - pl.Pos.Z;
                Vec3F32 dir = Vec3F32.Normalise(new Vec3F32(dx, dy, dz));

                float strength = 1.5f;
                if (pl.Supports(CpeExt.VelocityControl) && attacker.Supports(CpeExt.VelocityControl))
                {
                    pl.Send(Packet.VelocityControl(
                        -dir.X * strength,
                         0.5f * strength,
                        -dir.Z * strength,
                        0, 1, 0));
                }
                else
                    attacker.Message("Knockback failed: client lacks VelocityControl.");
            }

            private void BallCallback(SchedulerTask task)
            {
                BallData data = (BallData)task.State;
                if (TickBall(data)) return;
                RevertLast(data.player, data);
                task.Repeating = false;
            }

            private static Vec3U16 Round(Vec3F32 v)
            {
                return new Vec3U16((ushort)Math.Round(v.X), (ushort)Math.Round(v.Y), (ushort)Math.Round(v.Z));
            }

            private bool TickBall(BallData data)
            {
                Player player = data.player;
                Vec3U16 pos = data.next;
                BlockID cur = player.Level.GetBlock(pos.X, pos.Y, pos.Z);

                if (cur == Block.Invalid) return false;
                if (cur != Block.Air) { OnHitBlock(data, pos, cur); return false; }
                Player pl = PlayerAt(player, pos, true);
                if (pl != null) { OnHitPlayer(data, pl); return false; }

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

            private Player PlayerAt(Player player, Vec3U16 pos, bool skipSelf)
            {
                foreach (Player pl in PlayerInfo.Online.Items)
                {
                    if (pl.Level != player.Level) continue;
                    if (skipSelf && pl == player) continue;
                    if (Math.Abs(pl.Pos.BlockX - pos.X) <= 1 && Math.Abs(pl.Pos.BlockY - pos.Y) <= 1 && Math.Abs(pl.Pos.BlockZ - pos.Z) <= 1)
                        return pl;
                }
                return null;
            }
        }

        public class BallData
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
