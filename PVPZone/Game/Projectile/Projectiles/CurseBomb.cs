using PVPZone.Game.Player;

namespace PVPZone.Game.Projectile.Projectiles
{
    public class CurseBomb : Projectile
    {
        public override void OnCollide(PVPPlayer player)
        {
            if (player == null)
                return;
            if (player.MCGalaxyPlayer.Model == "shieldb3")
                return;

            player.Curse();
            player.Knockback(this.Velocity.X, 2f, this.Velocity.Z, 1.5f);
            player.DamageEffect();
        }

        public CurseBomb() : base()//(Level level, Vec3F32 Position, Vec3F32 Velocity, PVPPlayer Thrower = null) : base (level, Position, Velocity, Thrower)
        {
            BlockId = 256 + 486;
            Gravity = 0.05f;
        }
    }
}
