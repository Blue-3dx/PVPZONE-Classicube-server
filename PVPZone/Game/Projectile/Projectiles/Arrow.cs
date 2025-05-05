using PVPZone.Game.Player;

namespace PVPZone.Game.Projectile.Projectiles
{
    public class Arrow : Projectile
    {
        public override void OnCollide( PVPPlayer player)
        {
            if (player == null) return;

            if (player.MCGalaxyPlayer.Model == "shieldb3")
                return;

            player.Damage(new DamageReason(DamageReason.DamageType.Arrow, 2, player, this.Thrower));
            player.Knockback(this.Velocity.X, 2f, this.Velocity.Z, 1.5f);
            player.DamageEffect();
        }
        public Arrow() : base()
        {
            BlockId = 147 + 256;
        }
    }
}
