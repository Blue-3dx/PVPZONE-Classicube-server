using MCGalaxy;
using System;
using System.Threading;
using PVPZone.Game.Player;
namespace PVPZone.Game.Commands
{
    public class CmdCurse : Command
    {
        public override string name { get { return "Curse"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return CommandTypes.Other; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public override bool museumUsable { get { return false; } }

        static readonly string[] Curses = { "Slowness", "Backwards", "LowJumps", "Harming" };
        static readonly Random rand = new Random();

        public override void Use(MCGalaxy.Player p, string message)
        {
            MCGalaxy.Player target = p;
            if (message.Length > 0)
            {
                target = PlayerInfo.FindExact(message);
                if (target == null)
                {
                    p.Message("%cPlayer not found.");
                    return;
                }
            }

            string curse = Curses[rand.Next(Curses.Length)];
            p.Message("%cYou have cursed %f" + target.name + "%c with %f" + curse + "%c!");

            if (curse == "Harming")
            {
                PVPPlayer pvpp = PVPPlayer.Get(p);
                if (pvpp != null) pvpp.Damage(new DamageReason(DamageReason.DamageType.None, 10, pvpp));
                //Command.Find("kill").Use(Player.Console, target.name);
                return;
            }

            target.Extras["curse"] = curse;
            target.SendMapMotd();

            // Start a trail thread for the cursed player
            new Thread(() =>
            {
                int time = 0;
                while (time < 10000 && target.Extras.Contains("curse"))
                {
                    Thread.Sleep(200); time += 200;

                    foreach (MCGalaxy.Player pl in PlayerInfo.Online.Items)
                    {
                        if (pl.Level != target.Level) continue;

                        // Trail: you can change "red" to anything like "purple", "curseTrail", etc.
                        GoodlyEffects.SpawnEffectFor(pl, "purpleeffect",
                            target.Pos.X / 32f, target.Pos.Y / 32f, target.Pos.Z / 32f,
                            0f, 0f, 0f);
                    }
                }

                // Curse wears off
                if (target.Extras.Contains("curse"))
                {
                    target.Extras.Remove("curse");
                    target.SendMapMotd();
                    target.Message("%7Your curse has worn off.");
                }

            }).Start();
        }

        public override void Help(MCGalaxy.Player p)
        {
            p.Message("&T/Curse [player] &H- Curse yourself or someone else with a random effect for 10s.");
        }
    }
}
