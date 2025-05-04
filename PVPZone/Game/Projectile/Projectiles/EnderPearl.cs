using MCGalaxy;
using MCGalaxy.Maths;
using PVPZone.Game.Player;

namespace PVPZone.Game.Projectile.Projectiles
{
    public class EnderPearl : Projectile
    {
        public override void OnCollide(PVPPlayer player)
        {
            this.Thrower.MCGalaxyPlayer.Pos = new Position((int)Position.X, (int)Position.Y+1, (int)Position.Z);
        }
        public EnderPearl() : base()//(Level level, Vec3F32 Position, Vec3F32 Velocity, PVPPlayer Thrower = null) : base (level, Position, Velocity, Thrower)
        {
            BlockId = 128;
        }
    }
}
