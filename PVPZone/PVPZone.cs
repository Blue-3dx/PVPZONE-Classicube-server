using PVPZone;
using PVPZone.Game.Commands;
using PVPZone.Game.Gamemodes;
using PVPZone.Game.Item;
using PVPZone.Game.Map;
using PVPZone.Game.Player;
using PVPZone.Game.Projectile;
using System.IO;

namespace MCGalaxy
{
    public class PVPZone : Plugin
    {
        public override string name { get { return "PVPZone"; } }
        public override string MCGalaxy_Version { get { return "1.9.5.1"; } }
        public override int build { get { return 1; } }
        public override string welcome { get { return "PVPZone loaded!"; } }
        public override string creator { get { return "Blue3dx, Venk, morgana"; } }
        public override bool LoadAtStartup { get { return true; } }

        public static PVPZoneConfig Config { get; set; }

        public static CmdCurse curseCommand = new CmdCurse();
        public static CmdPVPZone pvpZoneCommand = new CmdPVPZone();
        public static Cmdxp xpCommand = new Cmdxp();
        
        public override void Load(bool startup)
        {
            Config = new PVPZoneConfig();

            PlayerManager.Load();
            ProjectileManager.Load();
            ItemManager.Load();
            CurseManager.Load();
            XPSystem.Load();
            LootManager.Load();

            Command.Register(curseCommand);
            Command.Register(pvpZoneCommand);
            Command.Register(xpCommand);

            PVPZoneGame.Instance = new PVPZoneGame();
            PVPZoneGame.Instance.Config.Path = "plugins/PVP/game.properties";
            if (!Directory.Exists("plugins/PVP"))
                Directory.CreateDirectory("plugins/PVP");
            PVPZoneGame.Instance.GetConfig().Load();
            if (!PVPZoneGame.Instance.Running)
                PVPZoneGame.Instance.Start(Player.Console, "", int.MaxValue);
        }

        public override void Unload(bool shutdown)
        {
            PlayerManager.Unload();
            ProjectileManager.Unload();
            ItemManager.Unload();
            CurseManager.Unload();
            XPSystem.Unload();
            LootManager.Unload();

            Command.Unregister(curseCommand);
            Command.Unregister(pvpZoneCommand);
            Command.Unregister(xpCommand);

            if (PVPZoneGame.Instance.Running)
            {
                PVPZoneGame.Instance.End();
                PVPZoneGame.Instance.Running = false;
            }
        }
    }
}
