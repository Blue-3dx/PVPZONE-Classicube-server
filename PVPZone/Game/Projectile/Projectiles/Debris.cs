using PVPZone.Game.Player;

namespace PVPZone.Game.Projectile.Projectiles
{
    public class Debris : Projectile
    {
        public override void OnCollide(PVPPlayer player)
        {
            if (player == null) return;
            player.Knockback(this.Velocity.X, 2f, this.Velocity.Z, 1.5f);
            player.Damage(new DamageReason(DamageReason.DamageType.Debris, 5, player, Thrower));
            player.DamageEffect();
        }
        public override void OnTick()
        {
        
        }
        public Debris() : base()
        {

        }
    }
}
