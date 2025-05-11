using MCGalaxy.Network;
using MCGalaxy;
using System.Collections.Generic;
using PVPZone.Game.Item;
using MCGalaxy.DB;
namespace PVPZone.Game.Player
{
    public class PVPPlayerInventory
    {

        public PVPPlayer pl;

        public Dictionary<ushort, int> Inventory = new Dictionary<ushort, int>();

        public PVPPlayerInventory(PVPPlayer pl)
        {
            this.pl = pl;
        }

        public void SendInventoryOrder()
        {
            ushort x = 1;
            for (ushort i = 0; i <= 767; i++)
            {
                if (!Has(i) && !pl.MCGalaxyPlayer.Game.Referee)
                {
                    pl.MCGalaxyPlayer.Send(Packet.SetInventoryOrder(Block.Air, i, pl.MCGalaxyPlayer.Session.hasExtBlocks));
                    continue;
                }
                pl.MCGalaxyPlayer.Send(Packet.SetInventoryOrder(i, x, pl.MCGalaxyPlayer.Session.hasExtBlocks));
                x++;
            }
        }
        public void SendCanPlaceBreak()
        {
            var p = this.pl.MCGalaxyPlayer;

            if (!Util.IsPVPLevel(p.level)) return;

            bool extBlocks = p.Session.hasExtBlocks;
            int count = p.Session.MaxRawBlock + 1;
            int size = extBlocks ? 5 : 4;
            byte[] bulk = new byte[count * size];
            Level level = p.level;
            for (ushort i = 0; i < count; i++)
            {
                bool canPlaceItem = (Has(i) && (ItemManager.Items.ContainsKey(i) ? ItemManager.Items[i].Placeable : true));
                bool canBreakItem = (ItemManager.Items.ContainsKey(i) ? ItemManager.Items[i].Breakable : false);

                bool canPlace = p.Game.Referee || canPlaceItem;
                bool canBreak = p.Game.Referee || canBreakItem;

                if (i == 0)
                {
                    canPlace = true;//Util.CanBreakBlocks(pl.MCGalaxyPlayer.level);
                    canBreak = true;// Util.CanBreakBlocks(pl.MCGalaxyPlayer.level);
                }

                Packet.WriteBlockPermission((ushort)i, canPlace, canBreak, p.Session.hasExtBlocks, bulk, i * size);
            }
            p.Send(bulk);
        }
        public void Remove(ushort blockId, int amount = 1)
        {
            if (!Inventory.ContainsKey(blockId))
                return;

            if (amount >= Inventory[blockId])
            {
                Inventory.Remove(blockId);
                SendInventoryOrder();
                SendCanPlaceBreak();
                pl.GuiHeldBlock();
                return;
            }

            Inventory[blockId] -= amount;
            pl.GuiHeldBlock();
        }
        public void Add(ItemManager.PVPZoneItems item, int amount = 1)
        {
            Add((ushort)item, amount);
        }
        public void Add(ushort blockId, int amount = 1)
        {
            if (amount < 0)
            {
                Remove(blockId, -amount);
                return;
            }

            if (!Inventory.ContainsKey(blockId))
            {
                Inventory.Add(blockId, amount);
                SendInventoryOrder();
                SendCanPlaceBreak();
                pl.GuiHeldBlock();
                return;
            }

            Inventory[blockId] += amount;
            pl.GuiHeldBlock();
        }

        public void Clear()
        {
            if (Inventory.Keys.Count == 0)
                return;
            Inventory.Clear();
            SendInventoryOrder();
            SendCanPlaceBreak();
        }

        public int Get(ushort blockId)
        {
            return Inventory.ContainsKey(blockId) ? Inventory[blockId] : 0;
        }

        public bool Has(ushort blockId, int amount = 1)
        {
            if (Util.IsNoInventoryLevel(pl.MCGalaxyPlayer.level)) return ItemManager.Items.ContainsKey(blockId);
            if (!Inventory.ContainsKey(blockId)) return false;

            return Inventory[blockId] >= amount;
        }
       
    }
}