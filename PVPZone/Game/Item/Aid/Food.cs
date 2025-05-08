using PVPZone.Game.Player;

namespace PVPZone.Game.Item.Weapon.Ranged
{
    public class Food : PVPZoneItem
    {
        public override bool Use(PVPPlayer player)
        {
            if (!base.Use(player)) return false;
            //player.HungerReplenish(1);
            player.Heal(2);
            return true;
        }
        public override bool CanUse(PVPPlayer player)
        {
            //if (player.Hunger >= MCGalaxy.PVPZone.Config.Player.MaxHunger) return false;
            if (player.Health >= MCGalaxy.PVPZone.Config.Player.MaxHealth) return false;
            return base.CanUse(player);
        }
        public override void OnHit(PVPPlayer attacker, PVPPlayer victim)
        {

        }

        public Food(ushort id, ushort textureId = 0, string Name = "") : base(id, textureId, Name)
        {
            this.RemoveOnUse = true;
            this.XPLevelRequired = 1;
            this.Cooldowntime = 1;
        }
    }
}
