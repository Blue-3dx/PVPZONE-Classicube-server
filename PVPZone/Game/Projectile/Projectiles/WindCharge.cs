using PVPZone.Game.Player;

namespace PVPZone.Game.Projectile.Projectiles
{
    public class WindCharge : Projectile
    {
        public override void OnCollide(PVPPlayer player)
        {
            if (player == null) return;
            if (player.MCGalaxyPlayer.Model == "shieldb3")
                return;

            player.Knockback(this.Velocity.X, 2f, this.Velocity.Z, 5f);
        }
        public WindCharge() : base()
        {
            BlockId = 256 + 95;
        }
    }
}
