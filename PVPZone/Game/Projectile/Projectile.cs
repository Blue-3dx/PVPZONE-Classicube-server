using System;
using MCGalaxy;
using MCGalaxy.Maths;
using PVPZone.Game.Player;

namespace PVPZone.Game.Projectile
{
    public class Projectile
    {
        public ushort BlockId = 1;

        public float Gravity = 0.03f;

        public float Drag = 0.99f;

        public bool Destroy = false;

        public Vec3F32 Position;
        public Vec3U16 PositionLast;

        public Vec3F32 Velocity;

        public Level Level;

        public DateTime Expire;

        public float ExpireTime = 120;

        public bool DestroyOnContact = true;

        public bool Moving = true;

        public bool UpdatePos = false;

        public PVPPlayer Thrower = null;

        public virtual void OnCollide(PVPPlayer victim)
        {
            if (victim != null)
                victim.Knockback(this.Velocity, 1);
        }
        public virtual void OnTick()
        {

        }
        public virtual void OnDestroy()
        {

        }
        public virtual void OnClick(PVPPlayer clicker)
        {

        }
        public bool Tick()
        {
            if (DateTime.Now > Expire)
                return false;

            if (!Moving)
                return true;

            Vec3U16 blockPos = Util.Round(Position);
            Vec3U16 nextBlockPos = Util.Round(Position + Velocity);
            ushort nextBlock = Level.GetBlock(nextBlockPos.X, nextBlockPos.Y, nextBlockPos.Z);
            ushort currentBlock = Level.GetBlock(blockPos.X, blockPos.Y, blockPos.Z);

            if (nextBlock == Block.Invalid || currentBlock == Block.Invalid)
                return false;

            MCGalaxy.Player hit = Thrower != null ? Util.PlayerAt(Thrower.MCGalaxyPlayer, blockPos) : Util.PlayerAt(Level, blockPos);
            if (hit != null)
            {
                OnCollide(PVPPlayer.Get(hit));
                Moving = false;
                return !DestroyOnContact;
            }

            if (nextBlock != Block.Air)
            {
                OnCollide(null);
                Moving = false;
                //Velocity = new Vec3F32(0, 0, 0);
                return !DestroyOnContact;
            }
     
            PositionLast = blockPos;

            Position += Velocity;

            Velocity.X *= Drag;
            Velocity.Y *= Drag;
            Velocity.Z *= Drag;

            Velocity.Y -= Gravity;

            UpdatePos = true;

            return true;

        }
        public virtual void OnCreation()
        {

        }
        public Projectile()
        {
        }
        public static void Throw(Projectile projectile, Level level, Vec3F32 pos, Vec3F32 dir, float power = 1, PVPPlayer thrower=null)
        {
            projectile.Level = level;
            projectile.Velocity = dir;
            projectile.Position = pos;
            projectile.PositionLast = Util.Round(projectile.Position);
            projectile.Thrower = thrower;
            ProjectileManager.ProjectileAdd(projectile);
            projectile.Expire = DateTime.Now.AddSeconds(projectile.ExpireTime);
            projectile.OnCreation();

        }
        public static void Throw(Projectile projectile, PVPPlayer thrower, float power=1)
        {
            Level level = thrower.MCGalaxyPlayer.Level;
            Vec3F32 pos = thrower.MCGalaxyPlayer.Pos.BlockCoords;

            Vec3F32 dir = DirUtils.GetDirVector(thrower.MCGalaxyPlayer.Rot.RotY, thrower.MCGalaxyPlayer.Rot.HeadX);

            Throw(projectile, level, pos, dir, power, thrower);
        }
        public static void Drop(Projectile projectile, Level level, ushort x, ushort y, ushort z)
        {
            Throw(projectile, level, new Vec3F32((float)x,(float)y,(float)z), new Vec3S32(0,0,0));
        }

    }
}
