using PVPZone.Game.Player;

namespace PVPZone.Game.Item.Weapon.Melee
{
    public class Mace : PVPZoneItem
    {
        public override void OnHit(PVPPlayer attacker, PVPPlayer victim)
        {
            base.OnHit(attacker, victim);
            victim.DamageEffect(effect:"explosion");
        }
        public Mace(ushort id, ushort textureId = 0, string Name = "") : base(id, textureId, Name)
        {
            this.RemoveOnUse = false;
            this.XPLevelRequired = 5;
            this.Cooldowntime = 1f;
            this.Knockback = 1.5f;
            this.Damage = 3;
            this.DamageType = DamageReason.DamageType.Mace;
        }
    }
}
