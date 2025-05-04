using PVPZone.Game.Player;

namespace PVPZone.Game.Item.Weapon.Ranged
{
    public class Arrow : PVPZoneItem
    {
        public override bool Use(PVPPlayer player)
        {
            return false;
 /*               if (!base.Use(player)) return false;


                return true;*/
        }
        public override void OnHit(PVPPlayer attacker, PVPPlayer victim)
        {

        }

        public Arrow(ushort id, ushort textureId = 0, string Name="") : base(id, textureId, Name)
        {
            this.RemoveOnUse = false;
            this.XPLevelRequired = 0;
        }
    }
}
