using System.Collections.Generic;

namespace PVPZone.Game.Player
{
    public class DamageReason
    {

        public PVPPlayer Victim;
        public PVPPlayer Attacker;
        public DamageType Type;
        public int Amount;

        public DamageReason(DamageType Type, int Amount, PVPPlayer Victim, PVPPlayer Attacker=null)
        {
            this.Victim = Victim;
            this.Attacker = Attacker;
            this.Type = Type;
            this.Amount = Amount;
        }
        public enum DamageType {
            None,
            Punch,
            Sword,
            Fire,
            Explosion,
            Poison,
            Fall,
            Starve,
            Frozen,
            Arrow,
            Snowball,
            Debris
        }

        public Dictionary<DamageType, string> DeathSymbols = new Dictionary<DamageType, string>()
        {
            [DamageType.None] = "%f?",
            [DamageType.Punch] = "%f?",
            [DamageType.Explosion] = "%c☼",
            [DamageType.Snowball] = "%c☼",
            [DamageType.Frozen] = "%b☼",
            [DamageType.Arrow] = "%f→",
            [DamageType.Starve] = "%f♦",
            [DamageType.Fire] = "%c♠",
        };


        static System.Random rnd = new System.Random();
        public static string GetDeathString(DamageType type, PVPPlayer victim, PVPPlayer attacker=null)
        {
            if (!MCGalaxy.PVPZone.Config.Damage.DeathMessages.ContainsKey(type))
                return GetDeathString(DamageType.None, victim);

            string[] messagepool = MCGalaxy.PVPZone.Config.Damage.DeathMessages[type];

            string msg = messagepool[rnd.Next(0, messagepool.Length - 1)];

            msg = msg.Replace("{vicColor}", victim.MCGalaxyPlayer.color).Replace("{vicName}", victim.MCGalaxyPlayer.DisplayName);

            if (attacker != null)
                msg = msg.Replace("{atkColor}", attacker.MCGalaxyPlayer.color).Replace("{atkName}", attacker.MCGalaxyPlayer.DisplayName);

            return msg;
        }
        public static string GetDeathString(DamageReason handler)
        {
            return GetDeathString(handler.Type, handler.Victim, handler.Attacker);
        }


    }
}
