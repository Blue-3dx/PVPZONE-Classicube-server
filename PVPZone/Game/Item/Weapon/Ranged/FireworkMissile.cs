using PVPZone.Game.Player;

namespace PVPZone.Game.Item.Weapon.Ranged
{
    public class FireworkMissile : PVPZoneItem
    {
        public override bool Use(PVPPlayer player)
        {
            if (!base.Use(player)) return false;

            Projectile.Projectile.Throw(new Projectile.Projectiles.FireworkMissile(), player, 2f);

            return true;
        }
        public override void OnHit(PVPPlayer attacker, PVPPlayer victim)
        {

        }

        public FireworkMissile(ushort id, ushort textureId = 0) : base(id, textureId)
        {
            this.RemoveOnUse = true;
            this.XPLevelRequired = 10;
        }
    }
}
