using System;
using MCGalaxy.Events.PlayerEvents;

namespace MCGalaxy
{
    public class CurseManager
    {
        public static void Load()
        {
            OnGettingMotdEvent.Register(HandleMotd, Priority.High);
        }

        public static void Unload()
        {
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
}