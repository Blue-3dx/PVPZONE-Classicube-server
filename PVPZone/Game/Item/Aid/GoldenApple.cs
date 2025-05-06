using PVPZone.Game.Player;

namespace PVPZone.Game.Item.Weapon.Ranged
{
    public class GoldenApple : PVPZoneItem
    {
        public override bool Use(PVPPlayer player)
        {
            if (!base.Use(player)) return false;

            player.HealGolden(MCGalaxy.PVPZone.Config.Player.MaxHealthGolden);
            return true;
        }
        public override bool CanUse(PVPPlayer player)
        {
            if (player.HealthGolden >= MCGalaxy.PVPZone.Config.Player.MaxHealthGolden) return false;
            return base.CanUse(player);
        }

        public GoldenApple(ushort id, ushort textureId = 0, string Name = "") : base(id, textureId, Name)
        {
            this.RemoveOnUse = true;
            this.XPLevelRequired = 10;
            this.Cooldowntime = 1;
        }
    }
}
