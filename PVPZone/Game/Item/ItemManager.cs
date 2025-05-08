using MCGalaxy;
using PVPZone.Game.Player;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PVPZone.Game.Item
{
    public class ItemManager
    {
        public enum PVPZoneItems
        {
            None = 0,
            Enderpearl = 128,
            Firework = 483,
            BlastBall = 501,
            IceBomb = 489, 
            Bow = 156,
            Arrow = 147,
            WindCharge = 95,
            CurseBomb = 486,
            Snowball = 488,
            GoldenApple = 512,
            Food1 = 116,
            Food2 = 117,
            Food3 = 118,
            Food4 = 119,
            TNT = Block.TNT,
            Mace = 243,
            Sword = 91
        }

        public static Dictionary<PVPPlayer, Dictionary<ushort, DateTime>> Cooldowns = new Dictionary<PVPPlayer, Dictionary<ushort, DateTime>>();

        public static Dictionary<ushort, PVPZoneItem> Items = new Dictionary<ushort, PVPZoneItem>()
        {
            [(ushort)PVPZoneItems.Enderpearl] = new PVPZone.Game.Item.Weapon.Ranged.EnderPearl((ushort)PVPZoneItems.Enderpearl, textureId:216, "Ender Pearl"),

            [(ushort)PVPZoneItems.CurseBomb] = new PVPZone.Game.Item.Weapon.Ranged.CurseBomb((ushort)PVPZoneItems.CurseBomb, textureId: 284, "Curse Potion"),

            [(ushort)PVPZoneItems.Snowball] = new PVPZone.Game.Item.Weapon.Ranged.Snowball((ushort)PVPZoneItems.Snowball, textureId: 233, "Snowball"),

            [(ushort)PVPZoneItems.Firework] = new PVPZone.Game.Item.Weapon.Ranged.Firework((ushort)PVPZoneItems.Firework, textureId: 286, "Firework"),

            [(ushort)PVPZoneItems.BlastBall] = new PVPZone.Game.Item.Weapon.Ranged.BlastBall((ushort)PVPZoneItems.BlastBall, textureId: 299, "Blast Ball"),

            [(ushort)PVPZoneItems.IceBomb] = new PVPZone.Game.Item.Weapon.Ranged.Icebomb((ushort)PVPZoneItems.IceBomb, textureId: 285, "Ice Bomb"),

            [(ushort)PVPZoneItems.WindCharge] = new PVPZone.Game.Item.Weapon.Ranged.Windcharge((ushort)PVPZoneItems.WindCharge, textureId: 438, "Wind Charge"),

            [(ushort)PVPZoneItems.Bow] = new PVPZone.Game.Item.Weapon.Ranged.Bow((ushort)PVPZoneItems.Bow, textureId: 223, "Bow"),
            [(ushort)PVPZoneItems.Arrow] = new PVPZone.Game.Item.Weapon.Ranged.Arrow((ushort)PVPZoneItems.Arrow, textureId: 239, "Arrow"),

            [(ushort)PVPZoneItems.GoldenApple] = new PVPZone.Game.Item.Weapon.Ranged.GoldenApple((ushort)PVPZoneItems.GoldenApple, textureId: 366, "Golden Apple"),
            [(ushort)PVPZoneItems.Food1] = new PVPZone.Game.Item.Weapon.Ranged.Food((ushort)PVPZoneItems.Food1, textureId: 201, "Cookie"),
            [(ushort)PVPZoneItems.Food2] = new PVPZone.Game.Item.Weapon.Ranged.Food((ushort)PVPZoneItems.Food2, textureId: 214, "Raw Beef"),
            [(ushort)PVPZoneItems.Food3] = new PVPZone.Game.Item.Weapon.Ranged.Food((ushort)PVPZoneItems.Food3, textureId: 215, "Steak"),
            [(ushort)PVPZoneItems.Food4] = new PVPZone.Game.Item.Weapon.Ranged.Food((ushort)PVPZoneItems.Food4, textureId: 229, "Apple"),

            [(ushort)PVPZoneItems.TNT] = new PVPZone.Game.Item.Weapon.Ranged.TNT((ushort)PVPZoneItems.TNT),

            [(ushort)PVPZoneItems.Mace] = new PVPZone.Game.Item.Weapon.Melee.Mace((ushort)PVPZoneItems.Mace, textureId: 453, "Mace"),

            [(ushort)PVPZoneItems.Sword] = new PVPZone.Game.Item.Weapon.Melee.Sword((ushort)PVPZoneItems.Sword, textureId: 204, "Iron Sword"),
        };
        public static PVPZoneItems[] GetItemArray()
        {
            return Enum.GetValues(typeof(PVPZoneItems)).Cast<PVPZoneItems>().ToArray();
        }
        static System.Random rnd = new System.Random();
        public static PVPZoneItems GetRandomItem()
        {
            var itemArray = GetItemArray();
            var item = itemArray[rnd.Next(0, itemArray.Count())];
            if (item == PVPZoneItems.GoldenApple)
                return itemArray[rnd.Next(0, itemArray.Count())];
            return item;
        }
        
        public static void Cooldown(PVPPlayer player, ushort BlockId, float duration)
        {
            if (!Cooldowns.ContainsKey(player))
                Cooldowns.Add(player, new Dictionary<ushort, DateTime>());

            DateTime newCooldown = DateTime.Now.AddSeconds(duration);

            if (!Cooldowns[player].ContainsKey(BlockId))
                Cooldowns[player].Add(BlockId, newCooldown);
            else
                Cooldowns[player][BlockId] = newCooldown;
        }
        public static bool IsCooldown(PVPPlayer player, ushort blockid)
        {
            if (!Cooldowns.ContainsKey(player))
                return false;
            if (!Cooldowns[player].ContainsKey(blockid))
                return false;
            return Cooldowns[player][blockid] > DateTime.Now;
        }
        public static DateTime GetCooldown(PVPPlayer player, ushort blockid)
        {
            if (!Cooldowns.ContainsKey(player))
                return DateTime.Now;
            if (!Cooldowns[player].ContainsKey(blockid))
                return DateTime.Now;
            return Cooldowns[player][blockid];
        }
        public static void PlayerDisconnect(PVPPlayer pl) // Called by Playermanager for now
        {
            if (Cooldowns.ContainsKey(pl))
                Cooldowns.Remove(pl);
        }
        public static void Load()
        {
          
            Cooldowns.Clear();
        }
        public static void Unload()
        {
            Cooldowns.Clear();
        }
    }
}
