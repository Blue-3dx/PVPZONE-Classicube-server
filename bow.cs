//pluginref exp.dll
using System;
using System.Collections.Generic;
using System.Threading;
using MCGalaxy;
using MCGalaxy.Commands;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Maths;
using MCGalaxy.Network;
using MCGalaxy.Tasks;

using BlockID = System.UInt16;

namespace MCGalaxy
{
    public class Weapons : Plugin
    {
        public static float ArrowPower = 1.5f;     // Matches Minecraft arrow speed
        public static float ArrowGravity = 0.05f;  // Matches Minecraft arrow gravity

        static Dictionary<string, bool> cooldowns = new Dictionary<string, bool>();

        public override string name { get { return "Arrow"; } }
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
            if (button == MouseButton.Right && action == MouseAction.Pressed)
            {
                BlockID held = p.ClientHeldBlock;
                if (held == Block.FromRaw(156)) // Block 156 is arrow
                {
                    // Check level requirement (must be level 50+)
                    if (XPPlugin.GetLevel(p) < 50)
                    {
                        p.Message("&cYou Must Be At Least Level 50+ To Use This Item!");
                        return;
                    }

                    if (cooldowns.ContainsKey(p.name) && cooldowns[p.name])
                    {
                        p.Message("%cYou must wait 60 seconds before using an arrow again!");
                        return;
                    }

                    cooldowns[p.name] = true;
                    new Thread(() =>
                    {
                        Thread.Sleep(60000); // 60 seconds cooldown
                        cooldowns[p.name] = false;
                    }).Start();

                    Arrow arrow = new Arrow();
                    arrow.Throw(p, ArrowPower);
                }
            }
        }

        #region Arrow

        public class Arrow
        {
            public void Throw(Player player, float power)
            {
                Vec3F32 dir = DirUtils.GetDirVector(player.Rot.RotY, player.Rot.HeadX);
                ArrowData data = MakeArgs(player, dir, power);

                SchedulerTask task = new SchedulerTask(ArrowCallback, data, TimeSpan.FromMilliseconds(85), true);
                player.CriticalTasks.Add(task);
            }

            private ArrowData MakeArgs(Player player, Vec3F32 dir, float power)
            {
                return new ArrowData {
                    block   = Block.FromRaw(147), // Arrow visual block
                    drag    = new Vec3F32(0.99f, 0.99f, 0.99f),
                    gravity = Weapons.ArrowGravity,
                    pos     = player.Pos.BlockCoords,
                    last    = Round(player.Pos.BlockCoords),
                    next    = Round(player.Pos.BlockCoords),
                    vel     = new Vec3F32(dir.X * power, dir.Y * power, dir.Z * power),
                    player  = player
                };
            }

            private void RevertLast(Player player, ArrowData data)
            {
                player.level.BroadcastRevert(data.last.X, data.last.Y, data.last.Z);
            }

            private void UpdateNext(Player player, ArrowData data)
            {
                player.level.BroadcastChange(data.next.X, data.next.Y, data.next.Z, data.block);
            }

            private void OnHitBlock(ArrowData data, Vec3U16 pos, BlockID block)
            {
                data.player.Message("");
            }

private void OnHitPlayer(ArrowData data, Player pl)
{
    Player attacker = data.player;

    // Skip if the player has model "shieldb3"
                if (pl.Model == "shieldb3")
    {
        return;  // Exit the method and do nothing
    }

    // Knockback
    int dx = attacker.Pos.X - pl.Pos.X;
    int dy = attacker.Pos.Y - pl.Pos.Y;
    int dz = attacker.Pos.Z - pl.Pos.Z;
    Vec3F32 dir = new Vec3F32(dx, dy, dz);
    if (dir.Length > 0) dir = Vec3F32.Normalise(dir);

    float strength = 1.5f;
    if (pl.Supports(CpeExt.VelocityControl) && attacker.Supports(CpeExt.VelocityControl))
    {
        pl.Send(Packet.VelocityControl(
            -dir.X * strength,
            0.5f * strength,
            -dir.Z * strength,
            0, 1, 0
        ));
    }
    else
    {
        attacker.Message("Knockback failed: client lacks VelocityControl.");
    }

    // Trigger /kill on the hit player via console
    Command cmd = Command.Find("kill");
    if (cmd != null)
    {
        cmd.Use(Player.Console, pl.name);
    }
}


            private void ArrowCallback(SchedulerTask task)
            {
                ArrowData data = (ArrowData)task.State;
                if (TickArrow(data)) return;

                RevertLast(data.player, data);
                task.Repeating = false;
            }

            private static Vec3U16 Round(Vec3F32 v)
            {
                unchecked { return new Vec3U16((ushort)Math.Round(v.X), (ushort)Math.Round(v.Y), (ushort)Math.Round(v.Z)); }
            }

            private bool TickArrow(ArrowData data)
            {
                Player player = data.player;
                Vec3U16 pos = data.next;
                BlockID cur = player.level.GetBlock(pos.X, pos.Y, pos.Z);

                if (cur == Block.Invalid) return false;
                if (cur != Block.Air) { OnHitBlock(data, pos, cur); return false; }

                Player pl = PlayerAt(player, pos, true);
                if (pl != null) { OnHitPlayer(data, pl); return false; }

                data.pos += data.vel;
                data.vel.X *= data.drag.X; data.vel.Y *= data.drag.Y; data.vel.Z *= data.drag.Z;
                data.vel.Y -= data.gravity;

                data.next = Round(data.pos);
                if (data.last == data.next) return true;

                RevertLast(data.player, data);
                UpdateNext(player, data);
                data.last = data.next;
                return true;
            }

            private Player PlayerAt(Player player, Vec3U16 pos, bool skipSelf)
            {
                foreach (Player pl in PlayerInfo.Online.Items)
                {
                    if (pl.level != player.level) continue;
                    if (skipSelf && pl == player) continue;
                    if (Math.Abs(pl.Pos.BlockX - pos.X) <= 1 &&
                        Math.Abs(pl.Pos.BlockY - pos.Y) <= 1 &&
                        Math.Abs(pl.Pos.BlockZ - pos.Z) <= 1)
                        return pl;
                }
                return null;
            }
        }

        public class ArrowData
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
