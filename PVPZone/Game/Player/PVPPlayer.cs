using MCGalaxy;
using MCGalaxy.Maths;
using MCGalaxy.Network;
using PVPZone.Game.Item;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PVPZone.Game.Player
{
 
    public class PVPPlayer
    {
        public static List<PVPPlayer> Players = new List<PVPPlayer>();

        public static PVPPlayer Get(MCGalaxy.Player player)
        {
            return Players.First((x) => x.MCGalaxyPlayer == player);
        }

        public MCGalaxy.Player MCGalaxyPlayer;

        public PVPPlayerInventory Inventory;


        public int Health = MCGalaxy.PVPZone.Config.Player.DefaultHealth;
        public int HealthGolden = 0;
        public int Hunger = MCGalaxy.PVPZone.Config.Player.MaxHunger;

        public bool Dead { get { return Health <= 0; } }
        public bool Exhausted { get { return Hunger < MCGalaxy.PVPZone.Config.Player.HungerExhausted; } }
        public bool Starving { get { return Health <= MCGalaxy.PVPZone.Config.Player.HungerStarving; } }

        public uint XP { get { return XPSystem.GetXP(MCGalaxyPlayer); } }
        public uint XPLevel { get { return XPSystem.GetXP(MCGalaxyPlayer); } }

        DateTime nextHunger = DateTime.Now.AddSeconds(MCGalaxy.PVPZone.Config.Player.HungerDecayInterval);
        DateTime nextStarve = DateTime.Now.AddSeconds(MCGalaxy.PVPZone.Config.Player.HungerStarveInterval);
        DateTime nextHeal = DateTime.Now.AddSeconds(MCGalaxy.PVPZone.Config.Player.HealInterval);

        public ushort HeldBlock { get {

                if (Dead)
                    return 0;

                ushort heldBlock = MCGalaxyPlayer.GetHeldBlock();
                if (heldBlock > 256)
                    heldBlock = (ushort)(heldBlock - 256);
                if (!Inventory.Has(heldBlock))
                    return 0;

                return heldBlock;
            } }
        public PVPZoneItem HeldItem { get
            {
                ushort block = HeldBlock;

                if (block == 0) return null;

                if (!ItemManager.Items.ContainsKey(block))
                    return null;

                return ItemManager.Items[block];
            } }

        public DateTime nextAttack = DateTime.Now;

        public PVPPlayer (MCGalaxy.Player player)
        {
            MCGalaxyPlayer = player;
            Inventory = new PVPPlayerInventory(this);
            Players.Add(this);
        }

        public void Spawn()
        {
            Inventory.SendInventoryOrder();
            Inventory.Clear();
            Util.ClearHotbar(MCGalaxyPlayer);


            Health = MCGalaxy.PVPZone.Config.Player.DefaultHealth;
            HealthGolden = 0;
            Hunger = MCGalaxy.PVPZone.Config.Player.MaxHunger;

            nextHunger = DateTime.Now.AddSeconds(MCGalaxy.PVPZone.Config.Player.HungerDecayInterval);
            nextStarve = DateTime.Now.AddSeconds(MCGalaxy.PVPZone.Config.Player.HungerStarveInterval);

            SendGui();

            //Testing
            Inventory.Add(ItemManager.PVPZoneItems.BlastBall, 50);
            Inventory.Add(ItemManager.PVPZoneItems.Firework, 50);
            Inventory.Add(ItemManager.PVPZoneItems.Food1, 50);
            Inventory.Add(ItemManager.PVPZoneItems.IceBomb,50);
            Inventory.Add(ItemManager.PVPZoneItems.Bow, 50);
            Inventory.Add(ItemManager.PVPZoneItems.Arrow, 50);
            Inventory.Add(ItemManager.PVPZoneItems.CurseBomb, 50);
            Inventory.Add(ItemManager.PVPZoneItems.WindCharge, 50);
            Inventory.Add(ItemManager.PVPZoneItems.Snowball, 50);
            Inventory.Add(ItemManager.PVPZoneItems.Enderpearl, 50);
            Inventory.Add(ItemManager.PVPZoneItems.GoldenApple, 50);
        }

        public void Die(DamageReason damageHandler=null, string deathMessage = "")
        {
            Health = 0;
            HealthGolden = 0;
            Hunger = 0;
            Inventory.Clear();
            Util.ClearHotbar(MCGalaxyPlayer);

            if (deathMessage == "" && damageHandler != null)
                deathMessage = DamageReason.GetDeathString(damageHandler);

            MCGalaxyPlayer.HandleDeath(4, immediate: true, customMsg: deathMessage);

            if (damageHandler != null && damageHandler.Attacker != null)
                XPSystem.ExpUp(damageHandler.Attacker.MCGalaxyPlayer, MCGalaxy.PVPZone.Config.XP.XPReward_Kill);

            XPSystem.ExpUp(MCGalaxyPlayer, MCGalaxy.PVPZone.Config.XP.XPReward_Die);
        }
        public void OnDeath()
        {
            Health = 0;
            HealthGolden = 0;
            Hunger = 0;
            Inventory.Clear();

            SendGui();
        }
        public void Curse()//(string curse="Slowness")
        {
            //MCGalaxyPlayer.Extras["curse"] = curse;
            Command curseCmd = Command.Find("curse");
            if (curseCmd != null)
            {
                curseCmd.Use(MCGalaxy.Player.Console, MCGalaxyPlayer.name);
            }
        }
        public void DamageEffect(bool crit=false)
        {
            int ex = MCGalaxyPlayer.Pos.BlockX;
            int ey = MCGalaxyPlayer.Pos.BlockY;
            int ez = MCGalaxyPlayer.Pos.BlockZ;

            string effect = crit ? "crit" : "pvp";

            Util.Effect(MCGalaxyPlayer.level, effect, ex, ey, ez);
        }
        public void UseItem()
        {
            if (HeldItem == null)
                return;
            HeldItem.Use(this);
            GuiHeldBlock();
        }

        public void Punch(byte entityId)
        {
            if (Dead) return;

            if (DateTime.Now < nextAttack) return;

            MCGalaxy.Player victim = Util.PlayerFrom(entityId);

            if (victim == null || victim == MCGalaxyPlayer)
                return;

            PVPPlayer pvpVictim = PVPPlayer.Get(victim);

            if (pvpVictim == null) return;
            
            if (victim.Model == "shieldb3") return;

            double dist = Math.Sqrt(
                Math.Pow(MCGalaxyPlayer.Pos.X - victim.Pos.X, 2) +
                Math.Pow(MCGalaxyPlayer.Pos.Y - victim.Pos.Y, 2) +
                Math.Pow(MCGalaxyPlayer.Pos.Z - victim.Pos.Z, 2)
            ) / 32.0;

            if (dist > 4.0)
            {
                nextAttack = DateTime.Now.AddSeconds(0.05f);
                return;
            }

            nextAttack = DateTime.Now.AddSeconds(0.5f);

            bool isCrit = (MCGalaxyPlayer.Pos.Y > victim.Pos.Y + 32);

            int damage = (isCrit ? 2 : 1);

            if (HeldItem != null)
                damage += HeldItem.Damage;

            pvpVictim.Damage(new DamageReason(DamageReason.DamageType.Punch, damage, pvpVictim, this));

            if (HeldItem != null)
                HeldItem.OnHit(this, pvpVictim);

            if (pvpVictim.Dead)
                return;

            var dir = new Vec3F32(
                MCGalaxyPlayer.Pos.X - victim.Pos.X,
                MCGalaxyPlayer.Pos.Y - victim.Pos.Y,
                MCGalaxyPlayer.Pos.Z - victim.Pos.Z
            );

            float power = isCrit ? 2f : 1f;
            if (HeldItem != null)
                power *= HeldItem.Knockback;

            pvpVictim.Knockback( dir, isCrit ? 2f : 1f);
            pvpVictim.DamageEffect(isCrit);
        }

        // Returns if player died because of this damage
        public void Damage(int amount)
        {
            Damage(new DamageReason(DamageReason.DamageType.None, amount, this));
        }

        public void Damage(DamageReason damageHandler)
        {
            if (Dead)
                return;

            if (MCGalaxyPlayer.Game.Referee)
                return;

            if (MCGalaxyPlayer.invincible)
                return;

            if (damageHandler.Attacker != null && damageHandler.Attacker.MCGalaxyPlayer.Game.Referee)
                return;

            if (HealthGolden > 0)
            {
                HealthGolden -= damageHandler.Amount;
                GuiHealthExtra();
                return;
            }

            Health -= damageHandler.Amount;

            if (Health <= 0)
            {
                Die(damageHandler);
                return;
            }

            GuiHealth();
        }
        public void Heal(int amount)
        {
            if (Health == MCGalaxy.PVPZone.Config.Player.MaxHealth) return;

            Health += amount;
            if (Health >= MCGalaxy.PVPZone.Config.Player.MaxHealth)
                Health = MCGalaxy.PVPZone.Config.Player.MaxHealth;

            GuiHealth();
        }
        public void HealGolden(int amount)
        {
            HealthGolden += amount;
            if (HealthGolden >= MCGalaxy.PVPZone.Config.Player.MaxHealthGolden)
                HealthGolden = MCGalaxy.PVPZone.Config.Player.MaxHealthGolden;
            GuiHealthExtra();
        }

        public void HungerReplenish(int amount)
        {
            Hunger += amount;
            if (Hunger >= MCGalaxy.PVPZone.Config.Player.MaxHunger)
                Hunger = MCGalaxy.PVPZone.Config.Player.MaxHunger;
            GuiHunger();
        }
        public void Knockback(Vec3F32 dir, float power=1)
        {
            if (dir.Length <= 0) return;

            if (!MCGalaxyPlayer.Supports(CpeExt.VelocityControl))
                return;

            Vec3F32 normalDir = Vec3F32.Normalise(dir);
            Knockback(-normalDir.X, -normalDir.Y, -normalDir.Z,power);
        }
        public void Knockback(float dx, float dy, float dz, float power = 1)
        {
            if (Dead)
                return;

            if (!MCGalaxyPlayer.Supports(CpeExt.VelocityControl))
                return;

            MCGalaxyPlayer.Send(Packet.VelocityControl(dx * power, dy * power, dz*power, 0, 1, 0));
        }

        public void SendGui()
        {
            GuiHealth();
            GuiHealthExtra();
            GuiHunger();
            GuiHeldBlock();
        }
        public void GuiHealth()
        {
            MCGalaxyPlayer.SendCpeMessage(CpeMessageType.BottomRight1, Util.HealthBar("♥", this.Health, MCGalaxy.PVPZone.Config.Player.MaxHealth));
        }
        public void GuiHunger()
        {
            MCGalaxyPlayer.SendCpeMessage(CpeMessageType.BottomRight2, Util.HealthBar("←", this.Health, MCGalaxy.PVPZone.Config.Player.MaxHunger));
        }
        public void GuiHealthExtra()
        {
            MCGalaxyPlayer.SendCpeMessage(CpeMessageType.BottomRight3, this.HealthGolden != 0 ? Util.HealthBar("↨", this.Health, MCGalaxy.PVPZone.Config.Player.MaxHealthGolden) : "");
        }
        public void GuiHeldBlock()
        {
            int blockAmount = Inventory.Get(HeldBlock);
            MCGalaxyPlayer.SendCpeMessage(CpeMessageType.Status1, blockAmount == 0 ? "" : blockAmount.ToString());
        }
        public void GuiHint(string message)
        {
            MCGalaxyPlayer.SendCpeMessage(CpeMessageType.SmallAnnouncement, message);
        }
        ushort lastHeldBlock = 0;
        public void Think()
        {
            if (lastHeldBlock != HeldBlock)
            {
                lastHeldBlock = HeldBlock;
                GuiHeldBlock();
            }
            if (DateTime.Now > nextHunger)
            {
                if (Hunger > 0)
                    Hunger--;
                nextHunger = DateTime.Now.AddSeconds(MCGalaxy.PVPZone.Config.Player.HungerDecayInterval);
            }
            if (!Exhausted && !Starving && DateTime.Now > nextHeal)
            {
                nextHeal = DateTime.Now.AddSeconds(MCGalaxy.PVPZone.Config.Player.HealInterval);
                Heal(1);
            }
            if (Starving && DateTime.Now > nextStarve)
            {
                nextStarve = DateTime.Now.AddSeconds(MCGalaxy.PVPZone.Config.Player.HungerStarveInterval);
                Damage(new DamageReason(DamageReason.DamageType.Starve, 1, this));
            }
        }
    }
}
