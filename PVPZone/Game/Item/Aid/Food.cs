using PVPZone.Game.Player;

namespace PVPZone.Game.Item.Weapon.Ranged
{
    public class Food : PVPZoneItem
    {
        public override bool Use(PVPPlayer player)
        {
            if (!base.Use(player)) return false;
            player.HungerReplenish(1);
            return true;
        }
        public override void OnHit(PVPPlayer attacker, PVPPlayer victim)
        {

        }

        public Food(ushort id, ushort textureId = 0, string Name = "") : base(id, textureId, Name)
        {
            this.RemoveOnUse = true;
            this.XPLevelRequired = 10;
            this.Cooldowntime = 1;
        }
    }
}
