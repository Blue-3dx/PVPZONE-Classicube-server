using PVPZone.Game.Player;

namespace PVPZone.Game.Item.Weapon.Melee
{
    public class Mace : PVPZoneItem
    {
        public override void OnHit(PVPPlayer attacker, PVPPlayer victim)
        {
            base.OnHit(attacker, victim);

            if (attacker != null && attacker.MCGalaxyPlayer.Pos.BlockY >= victim.MCGalaxyPlayer.Pos.BlockY + 2)
            {
                attacker.Knockback(0, 6.5f, 0);
                victim.DamageEffect(effect: "explosion");
            }

        }
        public Mace(ushort id, ushort textureId = 0, string Name = "") : base(id, textureId, Name)
        {
            this.RemoveOnUse = false;
            this.XPLevelRequired = 5;
            this.Cooldowntime = 0f;
            this.Knockback = 1.5f;
            this.Damage = 3;
            this.DamageType = DamageReason.DamageType.Mace;
        }
    }
}
