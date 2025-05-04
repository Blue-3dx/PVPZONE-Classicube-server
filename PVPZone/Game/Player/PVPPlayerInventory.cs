using MCGalaxy.Network;
using MCGalaxy;
using System.Collections.Generic;
using PVPZone.Game.Item;
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
            for (ushort i = 0; i < 767; i++)
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
        public void Remove(ushort blockId, int amount = 1)
        {
            if (!Inventory.ContainsKey(blockId))
                return;

            if (amount >= Inventory[blockId])
            {
                Inventory.Remove(blockId);
                SendInventoryOrder();
            }

            Inventory[blockId] -= amount;
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
                return;
            }

            Inventory[blockId] += amount;
        }

        public void Clear()
        {
            if (Inventory.Keys.Count == 0)
                return;
            Inventory.Clear();
            SendInventoryOrder();
        }

        public int Get(ushort blockId)
        {
            return Inventory.ContainsKey(blockId) ? Inventory[blockId] : 0;
        }

        public bool Has(ushort blockId, int amount = 1)
        {
            if (!Inventory.ContainsKey(blockId)) return false;

            return Inventory[blockId] >= amount;
        }
       
    }
}