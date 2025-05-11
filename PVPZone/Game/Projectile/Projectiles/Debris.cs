using PVPZone.Game.Map;
using PVPZone.Game.Player;
using System;

namespace PVPZone.Game.Projectile.Projectiles
{
    public class Debris : Projectile
    {
        public bool PlaceOnDestroy = false;
        public override void OnCollide(PVPPlayer player)
        {
            if (!PlaceOnDestroy)
                this.Expire = DateTime.Now.AddSeconds(1);

            this.Destroy = true;
            if (player == null) return;
            player.Knockback(this.Velocity.X, 2f, this.Velocity.Z, 1.5f);
            player.Damage(new DamageReason(DamageReason.DamageType.Debris, 5, player, Thrower));
            player.DamageEffect();
        }
        public override void OnDestroy()
        {
            var blockpos = BlockPosition;

            if (!PlaceOnDestroy)
                return;
            if (Level.FastGetBlock(blockpos.X, blockpos.Y, blockpos.Z) != 0)
                return;
            Level.UpdateBlock(MCGalaxy.Player.Console,blockpos.X, blockpos.Y, blockpos.Z, this.BlockId);
        }
        public Debris() : base()
        {
            this.DestroyOnContact = false;
        }
    }
}