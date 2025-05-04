using MCGalaxy;
using MCGalaxy.Maths;
using PVPZone.Game.Player;

namespace PVPZone.Game.Projectile.Projectiles
{
    public class Snowball : Projectile
    {
        public override void OnCollide(PVPPlayer player)
        {
            if (player == null) return;
            if (player.MCGalaxyPlayer.Model == "shieldb3")
            {
                // Do nothing if the player has the "shieldb3" model
                return;
            }
            player.Damage(new DamageReason(DamageReason.DamageType.Arrow, 1, player, this.Thrower));

            player.Knockback(this.Velocity.X, 2.5f, this.Velocity.Z, 1f);

            player.DamageEffect();
            //this.Thrower.MCGalaxyPlayer.Pos = new Position((int)Position.X, (int)Position.Y + 1, (int)Position.Z);
        }
        public Snowball() : base()//(Level level, Vec3F32 Position, Vec3F32 Velocity, PVPPlayer Thrower = null) : base(level, Position, Velocity, Thrower)
        {
            BlockId = 488;
        }
    }
}
