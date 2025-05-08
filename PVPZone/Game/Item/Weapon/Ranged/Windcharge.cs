using PVPZone.Game.Player;

namespace PVPZone.Game.Item.Weapon.Ranged
{
    public class Windcharge : PVPZoneItem
    {
        public override bool Use(PVPPlayer player)
        {
            if (!base.Use(player)) return false;

            Projectile.Projectile.Throw(new Projectile.Projectiles.WindCharge(), player, 2f);
            byte rotx = player.MCGalaxyPlayer.Rot.HeadX;
            if (rotx <= 64 && rotx >= 51)
                player.Knockback(0, 8, 0);
            return true;
        }
        public Windcharge(ushort id, ushort textureId = 0, string Name = "") : base(id, textureId, Name)
        {
            this.RemoveOnUse = true;
            this.XPLevelRequired = 0;
            this.PickupAmount = 5;
        }
    }
}
