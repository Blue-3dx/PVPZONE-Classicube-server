using PVPZone.Game.Player;
using System;
using System.Collections.Generic;

namespace PVPZone.Game.Item
{
    public class ItemManager
    {
        public enum PVPZoneItems
        {
            None = 0,
            Enderpearl = 128,
            Firework = 483,
            FireworkMissile = 501,
            IceBomb = 489, 
            Arrow = 156,
            WindCharge = 95,
            CurseBomb = 486,
            Snowball = 488,
            GoldenApple = 512,
            Food1 = 116,
            Food2 = 117,
            Food3 = 118,
            Food4 = 119
        }

        public static Dictionary<PVPPlayer, Dictionary<ushort, DateTime>> Cooldowns = new Dictionary<PVPPlayer, Dictionary<ushort, DateTime>>();

        public static Dictionary<ushort, PVPZoneItem> Items = new Dictionary<ushort, PVPZoneItem>()
        {
            [(ushort)PVPZoneItems.Enderpearl] = new PVPZone.Game.Item.Weapon.Ranged.EnderPearl((ushort)PVPZoneItems.Enderpearl),

            [(ushort)PVPZoneItems.CurseBomb] = new PVPZone.Game.Item.Weapon.Ranged.CurseBomb((ushort)PVPZoneItems.CurseBomb),

            [(ushort)PVPZoneItems.Snowball] = new PVPZone.Game.Item.Weapon.Ranged.Snowball((ushort)PVPZoneItems.Snowball),

            [(ushort)PVPZoneItems.Firework] = new PVPZone.Game.Item.Weapon.Ranged.Firework((ushort)PVPZoneItems.Firework),

            [(ushort)PVPZoneItems.FireworkMissile] = new PVPZone.Game.Item.Weapon.Ranged.FireworkMissile((ushort)PVPZoneItems.FireworkMissile),

            [(ushort)PVPZoneItems.IceBomb] = new PVPZone.Game.Item.Weapon.Ranged.Icebomb((ushort)PVPZoneItems.IceBomb),

            [(ushort)PVPZoneItems.WindCharge] = new PVPZone.Game.Item.Weapon.Ranged.Windcharge((ushort)PVPZoneItems.WindCharge),

            [(ushort)PVPZoneItems.Arrow] = new PVPZone.Game.Item.Weapon.Ranged.Arrow((ushort)PVPZoneItems.Arrow),

            [(ushort)PVPZoneItems.GoldenApple] = new PVPZone.Game.Item.Weapon.Ranged.GoldenApple((ushort)PVPZoneItems.GoldenApple),
            [(ushort)PVPZoneItems.Food1] = new PVPZone.Game.Item.Weapon.Ranged.Food((ushort)PVPZoneItems.Food1),
            [(ushort)PVPZoneItems.Food2] = new PVPZone.Game.Item.Weapon.Ranged.Food((ushort)PVPZoneItems.Food2),
            [(ushort)PVPZoneItems.Food3] = new PVPZone.Game.Item.Weapon.Ranged.Food((ushort)PVPZoneItems.Food3),
            [(ushort)PVPZoneItems.Food4] = new PVPZone.Game.Item.Weapon.Ranged.Food((ushort)PVPZoneItems.Food4),
        };
        
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
