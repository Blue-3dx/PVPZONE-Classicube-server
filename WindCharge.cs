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

namespace MCGalaxy {
    public class WindChargePlugin : Plugin {
        public override string name { get { return "WindChargePlugin"; } }
        public override string MCGalaxy_Version { get { return "1.9.4.9"; } }
        public override string creator { get { return "Blue3dx + Venk"; } }
        public override bool LoadAtStartup { get { return true; } }

public static float ChargePower = 1.5f;
public static float ChargeGravity = 0.03f;

        static Dictionary<string, bool> cooldowns = new Dictionary<string, bool>();

        public override void Load(bool startup) {
            OnPlayerClickEvent.Register(HandlePlayerClick, Priority.Low);
        }

        public override void Unload(bool shutdown) {
            OnPlayerClickEvent.Unregister(HandlePlayerClick);
        }

        void HandlePlayerClick(Player p, MouseButton button, MouseAction action,
                               ushort yaw, ushort pitch, byte entity,
                               ushort x, ushort y, ushort z, TargetBlockFace face) {
            // Use block ID 95 for WindCharge
            if (p.ClientHeldBlock != Block.FromRaw(95)) return;
            // Level requirement: 150+
            if (XPPlugin.GetLevel(p) < 150) {
                p.Message("&cYou must be at least level 150 to use WindCharges.");
                return;
            }
            // Cooldown check
            if (cooldowns.ContainsKey(p.name) && cooldowns[p.name]) {
                p.Message("&cYou must wait 30 seconds before using this again.");
                return;
            }
            cooldowns[p.name] = true;
            new Thread(() => {
                Thread.Sleep(30000);
                cooldowns[p.name] = false;
            }).Start();

            if (button == MouseButton.Right && action == MouseAction.Pressed) {
                // Launch wind charge projectile
                Charge proj = new Charge();
                proj.Throw(p, ChargePower);
            }
            else if (button == MouseButton.Left && action == MouseAction.Pressed) {
                // Launch player upward
                if (p.Supports(CpeExt.VelocityControl)) {
                    p.Send(Packet.VelocityControl(0, 5f, 0, 0, 1, 0));
                } else {
                    p.Message("&cCannot launch: client lacks VelocityControl.");
                }
            }
        }

        #region Charge Projectile
        public class Charge {
            public void Throw(Player player, float power) {
                Vec3F32 dir = DirUtils.GetDirVector(player.Rot.RotY, player.Rot.HeadX);
                ChargeData data = new ChargeData {
                    player = player,
                    block  = Block.FromRaw(95),
                    drag   = new Vec3F32(0.99f, 0.99f, 0.99f),
                    gravity= WindChargePlugin.ChargeGravity,
                    pos    = player.Pos.BlockCoords,
                    last   = Round(player.Pos.BlockCoords),
                    next   = Round(player.Pos.BlockCoords),
                    vel    = new Vec3F32(dir.X * power, dir.Y * power, dir.Z * power)
                };
                SchedulerTask task = new SchedulerTask(Callback, data, TimeSpan.FromMilliseconds(85), true);
                player.CriticalTasks.Add(task);
            }

            private static Vec3U16 Round(Vec3F32 v) {
                unchecked { return new Vec3U16((ushort)Math.Round(v.X), (ushort)Math.Round(v.Y), (ushort)Math.Round(v.Z)); }
            }

            void Callback(SchedulerTask task) {
                ChargeData data = (ChargeData)task.State;
                if (Tick(data)) return;
                Revert(data.player, data);
                task.Repeating = false;
            }

            bool Tick(ChargeData data) {
                Player player = data.player;
                Vec3U16 pos = data.next;
                BlockID cur = player.Level.GetBlock(pos.X, pos.Y, pos.Z);
                if (cur == Block.Invalid) return false;
                if (cur != Block.Air) {
                    Revert(player, data);
                    return false;
                }
                Player hit = PlayerAt(player, pos);
                if (hit != null) {
                    OnHit(data, hit);
                    return false;
                }
                data.pos += data.vel;
                data.vel.X *= data.drag.X; data.vel.Y *= data.drag.Y; data.vel.Z *= data.drag.Z;
                data.vel.Y -= data.gravity;
                data.next = Round(data.pos);
                if (!data.next.Equals(data.last)) {
                    Revert(player, data);
                    Update(player, data);
                    data.last = data.next;
                }
                return true;
            }

            void Revert(Player player, ChargeData data) {
                player.Level.BroadcastRevert(data.last.X, data.last.Y, data.last.Z);
            }
            void Update(Player player, ChargeData data) {
                player.Level.BroadcastChange(data.next.X, data.next.Y, data.next.Z, data.block);
            }

void OnHit(ChargeData data, Player target) {
    Player attacker = data.player;

    // Check if the target player has the model "shieldb3"
    if (target.Model == "shieldb3") {
        // Do nothing if the player has the "shieldb3" model
        return;
    }

    // Normal knockback logic if the target doesn't have the "shieldb3" model
    int dx = attacker.Pos.X - target.Pos.X;
    int dy = attacker.Pos.Y - target.Pos.Y;
    int dz = attacker.Pos.Z - target.Pos.Z;
    Vec3F32 dir = Vec3F32.Normalise(new Vec3F32(dx, dy, dz));
    float strength = 1.5f;

    if (target.Supports(CpeExt.VelocityControl) && attacker.Supports(CpeExt.VelocityControl)) {
        target.Send(Packet.VelocityControl(
            -dir.X * strength,
            (0.5f * strength) + 3f,
            -dir.Z * strength,
            0, 1, 0));
    } else {
        attacker.Message("&cKnockback failed: client lacks VelocityControl.");
    }
}


            Player PlayerAt(Player player, Vec3U16 pos) {
                foreach (Player pl in PlayerInfo.Online.Items) {
                    if (pl == player) continue;
                    if (pl.Level != player.Level) continue;
                    if (Math.Abs(pl.Pos.BlockX - pos.X) <= 1 && Math.Abs(pl.Pos.BlockY - pos.Y) <= 1 && Math.Abs(pl.Pos.BlockZ - pos.Z) <= 1)
                        return pl;
                }
                return null;
            }
        }

        public class ChargeData {
            public Player player;
            public BlockID block;
            public Vec3F32 pos, vel, drag;
            public Vec3U16 last, next;
            public float gravity;
        }
        #endregion
    }
}
