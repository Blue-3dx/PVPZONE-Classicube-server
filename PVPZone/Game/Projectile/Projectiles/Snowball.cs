using PVPZone.Game.Player;

namespace PVPZone.Game.Projectile.Projectiles
{
    public class Snowball : Projectile
    {
        public override void OnCollide(PVPPlayer player)
        {
            if (player == null) return;
            if (player.MCGalaxyPlayer.Model == "shieldb3")
                return;

            player.Damage(new DamageReason(DamageReason.DamageType.Snowball, 1, player, this.Thrower));
            player.Knockback(this.Velocity.X, 2.5f, this.Velocity.Z, 1f);
            player.DamageEffect();
        }
        public Snowball() : base()
        {
            BlockId = 256 + 488;
        }
    }
}
