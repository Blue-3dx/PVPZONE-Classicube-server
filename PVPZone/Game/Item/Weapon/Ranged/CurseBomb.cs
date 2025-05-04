using PVPZone.Game.Player;

namespace PVPZone.Game.Item.Weapon.Ranged
{
    public class CurseBomb : PVPZoneItem
    {
        public override bool Use(PVPPlayer player)
        {
            if (!base.Use(player)) return false;

            Projectile.Projectile.Throw(new Projectile.Projectiles.CurseBomb(), player, 2f);

            return true;
        }
        public override void OnHit(PVPPlayer attacker, PVPPlayer victim)
        {

        }

        public CurseBomb(ushort id, ushort textureId = 0) : base(id, textureId)
        {
            this.RemoveOnUse = true;
        }
    }
}
