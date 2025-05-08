using MCGalaxy;
using MCGalaxy.Config;
using MCGalaxy.DB;
using MCGalaxy.Events.EntityEvents;
using MCGalaxy.Events.GameEvents;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Games;
using MCGalaxy.Maths;
using MCGalaxy.Network;
using PVPZone.Game.Map;
using PVPZone.Game.Player;
using PVPZone.Game.Projectile;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PVPZone.Game.Gamemodes
{
    public class PVPMapConfig
    {
        [ConfigVec3("PVP-spawn", null)]
        public Vec3U16 Spawn;

        static string Path(string map) { return "./plugins/PVP/maps" + map + ".config"; }
        static ConfigElement[] cfg;

        public void SetDefaults(Level lvl)
        {
            Spawn.X = (ushort)(lvl.Width / 2);
            Spawn.Y = (ushort)(lvl.Height / 2 + 1);
            Spawn.Z = (ushort)(lvl.Length / 2);
        }

        public void Load(string map)
        {
            if (cfg == null) cfg = ConfigElement.GetAll(typeof(PVPMapConfig));
            ConfigElement.ParseFile(cfg, Path(map), this);
        }

        public void Save(string map)
        {
            if (cfg == null) cfg = ConfigElement.GetAll(typeof(PVPMapConfig));
            ConfigElement.SerialiseSimple(cfg, Path(map), this);
        }
    }
    public class PVPZoneGameConfig : RoundsGameConfig
    {
        override protected string GameName { get { return "PVPZone"; } }
        public override bool AllowAutoload { get { return true; } }
    }

    public class PVPZoneGame : RoundsGame
    {
        public ConcurrentDictionary<MCGalaxy.Player, int> KillLeaderboard  = new ConcurrentDictionary<MCGalaxy.Player, int>();
        public ConcurrentDictionary<MCGalaxy.Player, int> DeathLeaderboard = new ConcurrentDictionary<MCGalaxy.Player, int>();

        public VolatileArray<MCGalaxy.Player> AlivePlayers = new VolatileArray<MCGalaxy.Player>();

        public static PVPZoneGame Instance = new PVPZoneGame();

        public PVPMapConfig MapConfig = new PVPMapConfig();

        public PVPZoneGameConfig Config = new PVPZoneGameConfig() { StartImmediately = true };

        static string customMotdAddition = "-hax +speed maxspeed=1.47";
        public PVPZoneGame() { 
            Picker = new SimpleLevelPicker();
        }
        public DateTime nextLoot = DateTime.Now;
        public override string GameName { get { return "PVP"; } }
        protected override string WelcomeMessage
        {
            get { return ""; } // Message shown to players when connecting
        }

        public override void Start(MCGalaxy.Player p, string map, int rounds)
        {
            // Starts on current map by default
            if (!p.IsSuper && map.Length == 0) map = p.level.name;
            base.Start(p, map, rounds);
        }
        protected override void DoRound()
        {
            if (!Running) return;

            UpdateMapConfig();

            ProjectileManager.ClearMap(Map);

            KillLeaderboard.Clear();

            foreach (var player in Map.players)
            {
                Util.UnsetSpectator(player);
                PlayerActions.Respawn(player);
            }
            DoRoundCountdown(MCGalaxy.PVPZone.Config.Round.Countdown);

            if (Map == null) return;
            if (!Running) return;

            foreach (var player in Map.players)
            {
                PVPPlayer pvppl = PVPPlayer.Get(player);
                if (pvppl == null) continue;
                AlivePlayers.Add(player);

                pvppl.Spawn();
            }

            RoundInProgress = true;
            Instance.UpdateAllStatus();

            Thread.Sleep(100);

            if (!Running) return;
            if (Map == null) return;
            MessageMap(CpeMessageType.Announcement, "%aBegin!!!!!");
            while (Running && RoundInProgress && AlivePlayers.Count > 0 && Map != null)
            {
                Thread.Sleep(500);
                if (DateTime.Now > nextLoot)
                {
                    for (int i=0;i<4;i++)
                        LootManager.SpawnLoot(Map);
                   
                    nextLoot = DateTime.Now.AddSeconds(MCGalaxy.PVPZone.Config.Item.LootItemSpawnInteveral);
                }
            }
            Thread.Sleep(2000);
            if (!Running) return;
            if (Map == null) return;
            Map.Message("Starting new round...");
            Thread.Sleep(5000);
        }
        protected override string FormatStatus1(MCGalaxy.Player p)
        {
            if (RoundInProgress)
            {
                int kills = 0;
                KillLeaderboard.TryGetValue(p, out kills);
                return $"%bAlive %e{AlivePlayers.Count}%7 | %cKills %e{kills}";
            }
            return "";
        }
        protected override void StartGame()
        {
            Config.Load();
        }
        protected override void EndGame()
        {
            if (!RoundInProgress) return;
            AlivePlayers.Clear();
        }
        public override RoundsGameConfig GetConfig()
        {
            return Config;
        }

        public override void UpdateMapConfig()
        {
            MapConfig = new PVPMapConfig();
            MapConfig.SetDefaults(Map);
            MapConfig.Load(Map.name);
        }
        protected override List<MCGalaxy.Player> GetPlayers()
        {
            return Map.getPlayers();
        }
        private static SortedDictionary<MCGalaxy.Player, int> sortLeaderboard(ConcurrentDictionary<MCGalaxy.Player, int> lb)
        {
            SortedDictionary<MCGalaxy.Player, int> Sorted = new SortedDictionary<MCGalaxy.Player, int>();

            foreach (var pair in lb)
                Sorted.Add(pair.Key, pair.Value);

            return Sorted;
        }
        public override void OutputStatus(MCGalaxy.Player p)
        {
            p.Message("%bAlive players: " + AlivePlayers.Items.Join(pl => pl.ColoredName, "%f, "));

            if (KillLeaderboard.Count == 0) return;

            p.Message("%bLeaderboard:");
            var sortedKills = sortLeaderboard(KillLeaderboard);

            for (int i=0; i<sortedKills.Count; i++)
            {
                var pl = sortedKills.Keys.ElementAt(i);
                p.Message($"%e{i}. {pl.ColoredName} %f|%c {sortedKills[pl]}");
            }
        }
        public override void EndRound() {
            foreach(var player in GetPlayers())
                 Util.UnsetSpectator(player);
            
            RoundInProgress = false;
            KillLeaderboard.Clear();

            ProjectileManager.ClearMap(Map);
        }
        protected override void HookEventHandlers()
        {
            OnPlayerChatEvent.Register(HandlePlayerChat, Priority.High);
            OnPlayerSpawningEvent.Register(HandlePlayerSpawn, Priority.High);
            OnGettingCanSeeEntityEvent.Register(HandlePlayerCanSee, Priority.High);
            base.HookEventHandlers();
        }

        protected override void UnhookEventHandlers()
        {
            OnPlayerChatEvent.Unregister(HandlePlayerChat);
            OnPlayerSpawningEvent.Unregister(HandlePlayerSpawn);
            OnGettingCanSeeEntityEvent.Unregister(HandlePlayerCanSee);

            base.UnhookEventHandlers();
        }
   
        public void Gameover(MCGalaxy.Player winner=null)
        {
            string message = $"%c No winners!";
            if (winner != null)
            {
                message = $"%a{winner.DisplayName}%e wins!";
                XPSystem.ExpUp(winner, 100);
            }

            MessageMap(CpeMessageType.Normal, message);
            MessageMap(CpeMessageType.Announcement, message);

            EndRound();
        }
        public void CheckWin()
        {
            if (Instance.AlivePlayers.Count > 1)
                return;
            Gameover(AlivePlayers.Count > 0 ? AlivePlayers.Items.ElementAt(0) : null);
        }
        public static void OnKill(DamageReason damagedata)
        {
            if (!PVPZoneGame.Instance.RoundInProgress)
                return;

            Instance.AddDeath(damagedata.Victim.MCGalaxyPlayer);

            if (damagedata.Attacker != null)
            {
                Instance.AddKill(damagedata.Attacker.MCGalaxyPlayer);
            }

            if (!Instance.AlivePlayers.Contains(damagedata.Victim.MCGalaxyPlayer))
                return;

            Instance.AlivePlayers.Remove(damagedata.Victim.MCGalaxyPlayer);
            Instance.CheckWin();

            Util.SetSpectator(damagedata.Victim.MCGalaxyPlayer);

            Instance.UpdateAllStatus();
        }
        public void AddKill(MCGalaxy.Player p)
        {
            if (p.level != Map)
                return;

            if (!KillLeaderboard.ContainsKey(p))
            {
                KillLeaderboard.TryAdd(p, 1);
                return;
            }
            KillLeaderboard[p] += 1;
        }
        public void AddDeath(MCGalaxy.Player p)
        {
            if (p.level != Map)
                return;

            if (!DeathLeaderboard.ContainsKey(p))
            {
                DeathLeaderboard.TryAdd(p, 1);
                return;
            }
            DeathLeaderboard[p] += 1;
        }
        void HandlePlayerChat(MCGalaxy.Player p, string message)
        {
            if (p.level != Map) return;
            if (Picker.HandlesMessage(p, message)) { p.cancelchat = true; return; }
        }
        void HandlePlayerSpawn(MCGalaxy.Player p, ref Position pos, ref byte yaw, ref byte pitch, bool respawning)
        {
            if (!Running) return;
            if (p.level != Map)
            {
                if (AlivePlayers.Contains(p))
                    PlayerLeftGame(p);
                return;
            }

                pos = Position.FromFeetBlockCoords(MapConfig.Spawn.X, MapConfig.Spawn.Y, MapConfig.Spawn.Z);

            if (RoundInProgress)
            {
                Util.SetSpectator(p);
                return;
            }
            PVPPlayer pvp = PVPPlayer.Get(p);
            if (pvp == null) return;
            pvp.Inventory.SendInventoryOrder();
            if (!pvp.Dead)
                p.Send(Packet.HackControl(false, false, false, false, true, -1));
        }
        void HandlePlayerCanSee(MCGalaxy.Player p, ref bool canSee, Entity target)
        {
            MCGalaxy.Player targetPlayer = (MCGalaxy.Player)(target);

            if (targetPlayer == null) return;

            if (targetPlayer.Level != p.Level) return;

            if (!Util.IsPVPLevel(p.level)) return;

            PVPPlayer pvptarget = PVPPlayer.Get(targetPlayer);
            if (pvptarget == null) return;

            PVPPlayer pvp = PVPPlayer.Get(p);
            if (pvp == null) return;

            if (!pvp.Spectator && pvptarget.Spectator) canSee = false;
        }
        public override void PlayerLeftGame(MCGalaxy.Player p)
        {
            if (KillLeaderboard.ContainsKey(p))
                KillLeaderboard.TryRemove(p, out _);

            if (AlivePlayers.Contains(p))
                AlivePlayers.Remove(p);

            UpdateAllStatus1();
            CheckWin();
         
        }
        public override void PlayerJoinedGame(MCGalaxy.Player p)
        {
            if (RoundInProgress)
            {
                Util.SetSpectator(p);
                return;
            }
            Util.UnsetSpectator(p);
            PVPPlayer pvppl = PVPPlayer.Get(p);
            if (pvppl == null) return;

            AlivePlayers.Add(p);
            pvppl.Spawn();

            UpdateStatus1(p);
        }

       
    }
}
