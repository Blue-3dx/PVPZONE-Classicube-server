using MCGalaxy.Commands.Fun;
using MCGalaxy.Games;
using MCGalaxy.Maths;
using MCGalaxy;
using System.IO;
using PVPZone.Game.Gamemodes;
using MCGalaxy.Commands;
namespace PVPZone.Game.Commands
{
    public sealed class CmdPVPZone : RoundsGameCmd
    {
        public override string name { get { return "PVPZone"; } }
        public override string shortcut { get { return "PVP"; } }
        protected override RoundsGame Game { get { return (RoundsGame)PVPZoneGame.Instance; } }
        public override CommandPerm[] ExtraPerms
        {
            get { return new[] { new CommandPerm(LevelPermission.Operator, "can manage pvpzone") }; }
        }
        protected override void HandleStart(MCGalaxy.Player p, RoundsGame game, string[] args)
        {
            if (game.Running) 
            { 
                p.Message("{0} is already running", game.GameName);
                return;
            }

            game.Start(p, "", int.MaxValue);
        }

        protected override void HandleSet(MCGalaxy.Player p, RoundsGame game, string[] args)
        {
            if (args.Length < 2) { Help(p, "set"); return; }
            string prop = args[1];

            if (prop.CaselessEq("spawn"))
            {
                PVPMapConfig cfg = RetrieveConfig(p);
                cfg.Spawn = (Vec3U16)p.Pos.FeetBlockCoords;
                p.Message("Set spawn pos to: &b{0}", cfg.Spawn);
                UpdateConfig(p, cfg);
                return;
            }

            if (args.Length < 3) { Help(p, "set"); }
        }

        static PVPMapConfig RetrieveConfig(MCGalaxy.Player p)
        {
            PVPMapConfig cfg = new PVPMapConfig();
            cfg.SetDefaults(p.level);
            cfg.Load(p.level.name);
            return cfg;
        }

        static void UpdateConfig(MCGalaxy.Player p, PVPMapConfig cfg)
        {
            if (!Directory.Exists("PVP")) Directory.CreateDirectory("PVP");
            cfg.Save(p.level.name);

            if (p.level == PVPZoneGame.Instance.Map)
                PVPZoneGame.Instance.UpdateMapConfig();
        }

        public override void Help(MCGalaxy.Player p, string message)
        {
            if (message.CaselessEq("h2p"))
            {
                p.Message("%H2-16 players will spawn. You will have 10 seconds grace");
                p.Message("%Hperiod in which you cannot be killed. After these");
                p.Message("%H10 seconds it's anyone's game. Click on chests to gain");
                p.Message("%Hloot and click on people to attack them.");
                p.Message("%HLast person standing wins the game.");
            }
            else
            {
                base.Help(p, message);
            }
        }

        public override void Help(MCGalaxy.Player p)
        {
            p.Message("%T/PVP start %H- Starts a game of PVP");
            p.Message("%T/PVP stop %H- Immediately stops PVP");
            p.Message("%T/PVP end %H- Ends current round of PVP");
            p.Message("%T/PVP add/remove %H- Adds/removes current map from the map list");
            p.Message("%T/PVP status %H- Outputs current status of PVP");
            p.Message("%T/PVP go %H- Moves you to the current PVP map.");
        }
    }
}
