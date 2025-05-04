using System;
using System.Threading;
using MCGalaxy;
using MCGalaxy.Commands;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Network;

namespace MCGalaxy
{
    public class CurseManager
    {
        public static void Load()
        {
            Command.Register(new CmdCurse());
            OnGettingMotdEvent.Register(HandleMotd, Priority.High);
        }

        public static void Unload()
        {
            Command.Unregister(Command.Find("Curse"));
            OnGettingMotdEvent.Unregister(HandleMotd);
        }

        static void HandleMotd(Player p, ref string motd)
        {
            if (!p.Extras.Contains("curse")) return;
            string curse = (string)p.Extras["curse"];

            string[] parts = motd.Split(' ');
            string newMotd = "";
            for (int i = 0; i < parts.Length; i++)
            {
                string w = parts[i];
                if (!w.StartsWith("horspeed=", StringComparison.OrdinalIgnoreCase) &&
                    !w.StartsWith("jumpheight=", StringComparison.OrdinalIgnoreCase))
                {
                    newMotd += w + " ";
                }
            }

            if (curse == "Slowness") newMotd += "horspeed=0.5";
            if (curse == "Backwards") newMotd += "horspeed=-1";
            if (curse == "LowJumps") newMotd += "jumpheight=0.5";

            motd = newMotd.TrimEnd();
        }
    }

    public class CmdCurse : Command
    {
        public override string name { get { return "Curse"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return CommandTypes.Other; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public override bool museumUsable { get { return false; } }

        static readonly string[] Curses = { "Slowness", "Backwards", "LowJumps", "Harming" };
        static readonly Random rand = new Random();

        public override void Use(Player p, string message)
        {
            Player target = p;
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
                Command.Find("kill").Use(Player.Console, target.name);
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

                    foreach (Player pl in PlayerInfo.Online.Items)
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

        public override void Help(Player p)
        {
            p.Message("&T/Curse [player] &H- Curse yourself or someone else with a random effect for 10s.");
        }
    }
}