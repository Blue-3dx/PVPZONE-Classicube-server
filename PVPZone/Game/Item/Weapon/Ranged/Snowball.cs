using PVPZone.Game.Player;

namespace PVPZone.Game.Item.Weapon.Ranged
{
    public class Snowball : PVPZoneItem
    {
        public override bool Use(PVPPlayer player)
        {
            if (!base.Use(player)) return false;

            Projectile.Projectile.Throw(new Projectile.Projectiles.Snowball(), player, 2f);

            return true;
        }
        public override void OnHit(PVPPlayer attacker, PVPPlayer victim)
        {

        }

        public Snowball(ushort id, ushort textureId = 0) : base(id, textureId)
        {
            this.RemoveOnUse = true;
            this.XPLevelRequired = 10;
        }
    }
}
