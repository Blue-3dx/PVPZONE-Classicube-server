using PVPZone;
using PVPZone.Game;

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

        public override void Load(bool startup)
        {
            Config = new PVPZoneConfig();
            PVPZoneGame.Load();
        }

        public override void Unload(bool shutdown)
        {
            PVPZoneGame.Unload();
        }
    }
}
