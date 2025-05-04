using PVPZone.Game.Player;
using System.Collections.Generic;

namespace PVPZone
{
    public class PVPZoneConfig_Player
    {
        public int MaxHealth = 20;

        public int MaxHealthGolden = 20;

        public int MaxHunger = 20;

        public int DefaultHealth = 20;

        public int HungerExhausted = 10;
        public int HungerStarving = 0;

        public int HungerDecayInterval = 60;
        public int HungerStarveInterval = 20;

        public int HealInterval = 10;

    }
    public class PVPZoneConfig_Item {
        public string XPMessage = "%cYou need to be level {xp}+ to use this item!";
        public string Cooldownmessage = "%cCooldown: %e{time} %fseconds!";
    }
    public class PVPZoneConfig_Damage
    {
        public Dictionary<DamageReason.DamageType, string[]> DeathMessages = new Dictionary<DamageReason.DamageType, string[]>()
        {
            [DamageReason.DamageType.None] = new string[] { "{vicColor}{vicName} %fdied a %emysterious %fdeath!" },
            [DamageReason.DamageType.Fall] = new string[] { "{vicColor}{vicName} %efell %fto their death!" },
            [DamageReason.DamageType.Punch] = new string[] { 
                "{vicColor}{vicName} %fwas %ebeaten to a pulp%fby {atkColor}{atkName}%f!", 
                "{vicColor}{vicName} %fstood no chance in a fist fight against {atkColor}{atkName}%f!" 
            },
            [DamageReason.DamageType.Arrow] = new string[] {
                "{vicColor}{vicName} %fwas %eshot %fby {atkColor}{atkName}%f!",
            },
            [DamageReason.DamageType.Snowball] = new string[] {
                "{vicColor}{vicName} %fwas %esnowed %fby {atkColor}{atkName}%f!",
            },
            [DamageReason.DamageType.Explosion] = new string[] {
                "{vicColor}{vicName} %fwas %eblown to pieces %fby {atkColor}{atkName}%f!",
            },
           

        };
    }
    public class PVPZoneConfig
    {
        public PVPZoneConfig_Player Player = new PVPZoneConfig_Player();
        public PVPZoneConfig_Damage Damage = new PVPZoneConfig_Damage();
        public PVPZoneConfig_Item   Item   = new PVPZoneConfig_Item();
    }
}
