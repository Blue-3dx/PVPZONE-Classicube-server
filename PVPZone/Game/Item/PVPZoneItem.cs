using MCGalaxy;
using PVPZone.Game.Player;
using System;

namespace PVPZone.Game.Item
{
    public class PVPZoneItem
    {
        public bool RemoveOnUse = false;

        public int  XPLevelRequired = 0;

        public int Damage = 0;

        public float Knockback = 1f;

        public ushort Block_TextureId = 0;

        public ushort Block_BlockId = 0;

        public float Cooldowntime = 1;

        public int PickupAmount = 1; // Amount given when picked up off ground

        public virtual bool Use(PVPPlayer player)
        {
            if (!CanUse(player)) return false;

            if (RemoveOnUse)
                player.Inventory.Remove(Block_BlockId);

            ItemManager.Cooldown(player, Block_BlockId, Cooldowntime);
            return true;
        }
        public virtual bool CanUse(PVPPlayer player)
        {
            if (!player.Inventory.Has(Block_BlockId)) return false;

            if (XPLevelRequired > 0 && player.XPLevel < XPLevelRequired)
            {
                player.MCGalaxyPlayer.Message(MCGalaxy.PVPZone.Config.Item.XPMessage.Replace("{xp}", XPLevelRequired.ToString()));
                return false;
            }

            if (ItemManager.IsCooldown(player, this.Block_BlockId))
            {
                DateTime cooldownend = ItemManager.GetCooldown(player, this.Block_BlockId);
                int seconds = (int)Math.Ceiling((cooldownend - DateTime.Now).TotalSeconds);
                player.MCGalaxyPlayer.Message(MCGalaxy.PVPZone.Config.Item.Cooldownmessage.Replace("{time}", seconds.ToString()));
                return false;
            }
            return true;
        }

        public virtual void OnHit(PVPPlayer attacker, PVPPlayer victim)
        {

        }

        public PVPZoneItem(ushort id, ushort textureId = 0, string Name = "")
        {
            Block_BlockId = id;
            Block_TextureId = textureId;

            if (textureId == 0 || Name == "")
                return;

            BlockDefinition def = new BlockDefinition();
            def.RawID = Block_BlockId; def.Name = Name;
            def.Speed = 1; def.CollideType = 0;
            def.TopTex = textureId; def.BottomTex = textureId;

            def.BlocksLight = false; def.WalkSound = 1;
            def.FullBright = false; def.Shape = 0;
            def.BlockDraw = 2; def.FallBack = 5;

            def.FogDensity = 0;
            def.FogR = 0; def.FogG = 0; def.FogB = 0;
            def.MinX = 0; def.MinY = 0; def.MinZ = 0;
            def.MaxX = 0; def.MaxY = 0; def.MaxZ = 0;

            def.LeftTex = textureId; def.RightTex = textureId;
            def.FrontTex = textureId; def.BackTex = textureId;
            def.InventoryOrder = -1;
            BlockDefinition.Add(def, BlockDefinition.GlobalDefs, null);
            
        }

    }
}
