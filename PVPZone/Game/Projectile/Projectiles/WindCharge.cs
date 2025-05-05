using PVPZone.Game.Player;

namespace PVPZone.Game.Projectile.Projectiles
{
    public class WindCharge : Projectile
    {
        public override void OnCollide(PVPPlayer player)
        {
            if (player == null) return;
            if (player.MCGalaxyPlayer.Model == "shieldb3")
            {
                // Do nothing if the player has the "shieldb3" model
                return;
            }
            player.Knockback(this.Velocity.X, 2f, this.Velocity.Z, 5f);
        }
        public WindCharge() : base()//(Level level, Vec3F32 Position, Vec3F32 Velocity, PVPPlayer Thrower = null) : base(level, Position, Velocity, Thrower)
        {
            BlockId = 95;
        }
    }
}
