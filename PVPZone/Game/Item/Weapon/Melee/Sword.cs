namespace PVPZone.Game.Item.Weapon.Melee
{
    public class Sword : PVPZoneItem
    {
        public Sword(ushort id, ushort textureId = 0, string Name = "") : base(id, textureId, Name)
        {
            this.RemoveOnUse = false;
            this.XPLevelRequired = 5;
            this.Cooldowntime = 0.15f;
            this.Knockback = 1.5f;
            this.Damage = 2;
            this.DamageType = Player.DamageReason.DamageType.Sword;
        }
    }
}
