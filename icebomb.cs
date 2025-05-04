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
        public static float IceBombPower = 1.5f;     // Matches Minecraft IceBomb speed
        public static float IceBombGravity = 0.05f;  // Matches Minecraft IceBomb gravity

        static Dictionary<string, bool> cooldowns = new Dictionary<string, bool>();

        public override string name { get { return "IceBomb"; } }
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
                // Only proceed if holding a snowIceBomb (ID 489)
                if (held == Block.FromRaw(489))
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
                        p.Message("%cYou must wait 3 seconds before throwing another IceBomb!");
                        return;
                    }

                    cooldowns[p.name] = true;
                    new Thread(() =>
                    {
                        Thread.Sleep(3000); // 3 seconds cooldown
                        cooldowns[p.name] = false;
                    }).Start();

                    IceBomb IceBomb = new IceBomb();
                    IceBomb.Throw(p, IceBombPower);
                }
            }
        }

        #region IceBomb

        public class IceBomb
        {
            public void Throw(Player player, float power)
            {
                Vec3F32 dir = DirUtils.GetDirVector(player.Rot.RotY, player.Rot.HeadX);
                IceBombData data = MakeArgs(player, dir, power);

                SchedulerTask task = new SchedulerTask(IceBombCallback, data, TimeSpan.FromMilliseconds(50), true);

                player.CriticalTasks.Add(task);
            }

void SpawnIceCube(Level lvl, int cx, int cy, int cz)
{
    for (int x = -1; x <= 1; x++)
    for (int y = 0; y <= 4; y++) // cy to cy + 2, effectively cy-1 to cy+1
    for (int z = -1; z <= 1; z++)
    {
        int px = cx + x, py = cy + y, pz = cz + z;
        if (lvl.GetBlock((ushort)px, (ushort)py, (ushort)pz) == Block.Air)
            lvl.Blockchange((ushort)px, (ushort)py, (ushort)pz, Block.Ice);
    }
}



            private IceBombData MakeArgs(Player player, Vec3F32 dir, float power)
            {
                return new IceBombData {
                    block   = Block.FromRaw(489),
                    drag    = new Vec3F32(0.99f, 0.99f, 0.99f),
                    gravity = Weapons.IceBombGravity,
                    pos     = player.Pos.BlockCoords,
                    last    = Round(player.Pos.BlockCoords),
                    next    = Round(player.Pos.BlockCoords),
                    vel     = new Vec3F32(dir.X * power, dir.Y * power, dir.Z * power),
                    player  = player
                };
            }

            private void RevertLast(Player player, IceBombData data)
            {
                player.Level.BroadcastRevert(data.last.X, data.last.Y, data.last.Z);
            }

            private void UpdateNext(Player player, IceBombData data)
            {
                player.Level.BroadcastChange(data.next.X, data.next.Y, data.next.Z, data.block);
            }

private void OnHitBlock(IceBombData data, Vec3U16 pos, BlockID block)
{
    int baseY = pos.Y - 1;
    SpawnIceCube(data.player.Level, pos.X, baseY, pos.Z);
}

            private void OnHitPlayer(IceBombData data, Player pl)
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

            private void IceBombCallback(SchedulerTask task)
            {
                IceBombData data = (IceBombData)task.State;
                if (TickIceBomb(data)) return;
                RevertLast(data.player, data);
                task.Repeating = false;
            }

            private static Vec3U16 Round(Vec3F32 v)
            {
                return new Vec3U16((ushort)Math.Round(v.X), (ushort)Math.Round(v.Y), (ushort)Math.Round(v.Z));
            }

            private bool TickIceBomb(IceBombData data)
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

        public class IceBombData
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
