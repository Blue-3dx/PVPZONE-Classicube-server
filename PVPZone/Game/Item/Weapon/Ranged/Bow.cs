using PVPZone.Game.Player;

namespace PVPZone.Game.Item.Weapon.Ranged
{
    public class Bow : PVPZoneItem
    {
        public override bool Use(PVPPlayer player)
        {
            if (!base.Use(player)) return false;
            if (!player.Inventory.Has((ushort)ItemManager.PVPZoneItems.Arrow)) return false;

            Projectile.Projectile.Throw(new Projectile.Projectiles.Arrow(), player, 2f);
            player.Inventory.Remove((ushort)ItemManager.PVPZoneItems.Arrow);
            return true;
        }
        public override bool CanUse(PVPPlayer player)
        {
            if (!player.Inventory.Has((ushort)ItemManager.PVPZoneItems.Arrow)) return false;
            return base.CanUse(player);
        }
        public Bow(ushort id, ushort textureId = 0, string Name="") : base(id, textureId, Name)
        {
            this.RemoveOnUse = false;
            this.XPLevelRequired = 2;
            this.Cooldowntime = 0.3f;
        }
    }
}
