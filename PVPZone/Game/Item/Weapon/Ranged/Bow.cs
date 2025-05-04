using PVPZone.Game.Player;

namespace PVPZone.Game.Item.Weapon.Ranged
{
    public class Bow : PVPZoneItem
    {
        public override bool Use(PVPPlayer player)
        {
            if (!base.Use(player)) return false;

            Projectile.Projectile.Throw(new Projectile.Projectiles.Arrow(), player, 2f);

            return true;
        }
        public override void OnHit(PVPPlayer attacker, PVPPlayer victim)
        {

        }

        public Bow(ushort id, ushort textureId = 0, string Name="") : base(id, textureId, Name)
        {
            this.RemoveOnUse = true;
            this.XPLevelRequired = 10;
        }
    }
}
