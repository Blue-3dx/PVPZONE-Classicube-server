using System;
using MCGalaxy.Maths;
using PVPZone.Game.Player;

namespace PVPZone.Game.Projectile.Projectiles
{
    public class Loot : Projectile
    {
        static string[] effects = { "bluefirework", "greenfirework", "purplefirework", "rainbowfirework", "redfirework", "yellowfirework" };
        static System.Random rnd = new System.Random();
        ushort lootBlock;
        ushort lootAmount;
        private void pickup(PVPPlayer player)
        {
            if (player == null) return;
            if (this.Destroy) return;
            this.Destroy = true;

            player.Pickup(this.lootBlock, this.lootAmount);

            Vec3U16 blockPos = Util.Round(Position);
            Util.Effect(Level, effects[rnd.Next(0, effects.Length - 1)], blockPos.X, blockPos.Y, blockPos.Z);
            this.Level.BroadcastRevert(blockPos.X, blockPos.Y, blockPos.Z);
        }
        public override void OnCollide(PVPPlayer player)
        {
            pickup(player);
        }
        public override void OnClick(PVPPlayer player)
        {
            pickup(player);
        }
        public override void OnCreation()
        {
        }
        public Loot(ushort lootBlock, ushort lootAmount=1) : base()
        {
            this.BlockId = (ushort)(lootBlock + 256);
            this.lootBlock = lootBlock;
            this.lootAmount = lootAmount;
            this.DestroyOnContact = false;
            this.ExpireTime = MCGalaxy.PVPZone.Config.Item.LootItemExpiryTime;
            this.Expire = DateTime.Now.AddSeconds(ExpireTime);
            this.Gravity = 0.03f;
        }
    }
}
