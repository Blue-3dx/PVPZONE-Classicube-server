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

        public Vec3F32 Position;
        public Vec3U16 PositionLast;

        public Vec3F32 Velocity;

        public Level Level;


        public PVPPlayer Thrower = null;

        public virtual void OnCollide(PVPPlayer victim)
        {
            if (victim != null)
                victim.Knockback(this.Velocity, 1);
        }
        public virtual void OnTick()
        {

        }
        public bool Tick()
        {

            Vec3U16 blockPos = Util.Round(Position);

            ushort nextBlock = Level.GetBlock(blockPos.X, blockPos.Y, blockPos.Z);

            if (nextBlock == Block.Invalid || nextBlock != Block.Air)
            {
                Level.BroadcastRevert(PositionLast.X, PositionLast.Y, PositionLast.Z);
                OnCollide(null);
                return false;
            }
            Level.BroadcastRevert(PositionLast.X, PositionLast.Y, PositionLast.Z);

            MCGalaxy.Player hit = Thrower!=null? Util.PlayerAt(Thrower.MCGalaxyPlayer, blockPos) : Util.PlayerAt(Level, blockPos);
            if (hit != null)
            {
                OnCollide(PVPPlayer.Get(hit));
                return false;
            }

            Level.BroadcastChange(blockPos.X, blockPos.Y, blockPos.Z, BlockId);

            PositionLast = blockPos;

            Position += Velocity;

            Velocity.X *= Drag;
            Velocity.Y *= Drag;
            Velocity.Z *= Drag;

            Velocity.Y -= Gravity;

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
            projectile.OnCreation();
        }
        public static void Throw(Projectile projectile, PVPPlayer thrower, float power=1)
        {
            Level level = thrower.MCGalaxyPlayer.Level;
            Vec3F32 pos = thrower.MCGalaxyPlayer.Pos.BlockCoords;

            Vec3F32 dir = DirUtils.GetDirVector(thrower.MCGalaxyPlayer.Rot.RotY, thrower.MCGalaxyPlayer.Rot.HeadX);

            Throw(projectile, level, pos, dir, power, thrower);
        }

    }
}
