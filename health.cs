//pluginref GoodlyEffects.dll
using System;
using System.Collections.Generic;
using System.Threading;
using MCGalaxy;	
using MCGalaxy.Commands;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Maths;
using MCGalaxy.Network;

namespace MCGalaxy {
    public class PvpKnockbackAlwaysHealth : Plugin {
        public override string name { get { return "Health"; } }
        public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
        public override bool LoadAtStartup { get { return true; } }
        public override string creator { get { return "Blue3dx"; } }

        public static bool KnockbackEnabled    = true;
        public static bool GlobalHealthEnabled = true;

        static Dictionary<string, DateTime> lastHitTime          = new Dictionary<string, DateTime>();
        public static Dictionary<string, int> playerHealth       = new Dictionary<string, int>();
        public static Dictionary<string, int> extraHearts        = new Dictionary<string, int>();
        public static Dictionary<string, int> playerHunger        = new Dictionary<string, int>();

        static Dictionary<string, DateTime> goldenAppleCooldown  = new Dictionary<string, DateTime>();
        static Dictionary<string, DateTime> regenCooldown         = new Dictionary<string, DateTime>();

        static Thread hungerThread;
        static bool   hungerLoopRunning;

        public override void Load(bool startup) {
            OnPlayerClickEvent .Register(HandleClick,   Priority.High);
            OnPlayerConnectEvent.Register(OnConnect,    Priority.High);

            hungerLoopRunning = true;
            hungerThread = new Thread(HungerDecayLoop) { IsBackground = true };
            hungerThread.Start();
        }

        public override void Unload(bool shutdown) {
            OnPlayerClickEvent .Unregister(HandleClick);
            OnPlayerConnectEvent.Unregister(OnConnect);

            hungerLoopRunning = false;
            hungerThread.Join();
        }

        void OnConnect(Player p) {
            playerHealth       [p.name] = 9;
            extraHearts        [p.name] = 0;
            playerHunger       [p.name] = 9;
            regenCooldown      [p.name] = DateTime.MinValue;
            goldenAppleCooldown[p.name] = DateTime.MinValue;

            DisplayHearts      (p);
            DisplayExtraHearts (p);
            DisplayHunger      (p);
        }

        void HungerDecayLoop() {
            while (hungerLoopRunning) {
                Thread.Sleep(60000);
                foreach (Player p in PlayerInfo.Online.Items) {
                    int h;
                    if (!playerHunger.TryGetValue(p.name, out h)) h = 9;
                    h--;
                    if (h <= 0) {
                        ResetAllBars(p);
                        Command.Find("kill").Use(Player.Console, p.name);
                    } else {
                        playerHunger[p.name] = h;
                        DisplayHunger(p);
                    }
                }
            }
        }

        static void ResetAllBars(Player p) {
            playerHealth[p.name] = 9;
            extraHearts [p.name] = 0;
            playerHunger[p.name] = 9;
            DisplayHearts      (p);
            DisplayExtraHearts (p);
            DisplayHunger      (p);
        }

        public static void DisplayHearts(Player p) {
            if (!GlobalHealthEnabled) {
                p.SendCpeMessage(CpeMessageType.BottomRight1, "");
                return;
            }
            int hp;
            if (!playerHealth.TryGetValue(p.name, out hp)) hp = 9;
            string bar = "";
            for (int i = 0; i < 9; i++) bar += (i < hp) ? "♥" : "%0♥";
            p.SendCpeMessage(CpeMessageType.BottomRight1, bar);
        }

        public static void DisplayExtraHearts(Player p) {
            int extra;
            if (!extraHearts.TryGetValue(p.name, out extra)) extra = 0;
            string bar = "";
            for (int i = 0; i < extra; i++) bar += "↨";
            for (int i = extra; i < 9; i++) bar += "%0↨";
            p.SendCpeMessage(CpeMessageType.BottomRight2, extra > 0 ? bar : "");
        }

        public static void DisplayHunger(Player p) {
            int h;
            if (!playerHunger.TryGetValue(p.name, out h)) h = 9;
            string bar = "";
            for (int i = 0; i < 9; i++) bar += (i < h) ? "←" : "%0←";
            p.SendCpeMessage(CpeMessageType.BottomRight3, bar);
        }

        void HandleClick(Player p, MouseButton button, MouseAction action,
                         ushort yaw, ushort pitch, byte entity,
                         ushort x, ushort y, ushort z, TargetBlockFace face) {

            if (button == MouseButton.Right && action == MouseAction.Pressed) {
                ushort held = p.GetHeldBlock();

                // Totem-of-Undying golden apple
                if (held == Block.FromRaw(512)) {
                    DateTime lastUse;
                    goldenAppleCooldown.TryGetValue(p.name, out lastUse);
                    double since = (DateTime.UtcNow - lastUse).TotalSeconds;
                    if (since < 180) {
                        p.Message("%cWait {0}s before Golden Apple.", (int)(180 - since));
                    } else {
                        extraHearts[p.name] = 9;
                        DisplayExtraHearts(p);
                        goldenAppleCooldown[p.name] = DateTime.UtcNow;
                    }
                    return;
                }

                // Hunger AND Heart regeneration with blocks 116–119
                if (held >= Block.FromRaw(116) && held <= Block.FromRaw(119)) {
                    DateTime lastRegen;
                    regenCooldown.TryGetValue(p.name, out lastRegen);
                    double since = (DateTime.UtcNow - lastRegen).TotalSeconds;
                    if (since < 60) {
                        p.Message("%cRegen cooldown: {0}s.", (int)(60 - since));
                    } else {
                        regenCooldown[p.name] = DateTime.UtcNow;
                        new Thread(() => {
                            while (playerHunger[p.name] < 9) {
                                playerHunger[p.name]++;
                                DisplayHunger(p);
                                Thread.Sleep(1000);
                            }
                        }).Start();
                        new Thread(() => {
                            while (playerHealth[p.name] < 9) {
                                playerHealth[p.name]++;
                                DisplayHearts(p);
                                Thread.Sleep(1000);
                            }
                        }).Start();
                    }
                    return;
                }
            }

            // Knockback on left-click
            if (!KnockbackEnabled || button != MouseButton.Left || action != MouseAction.Pressed) return;

            Player victim = null;
            foreach (Player pl in PlayerInfo.Online.Items) {
                if (pl.EntityID == entity) { victim = pl; break; }
            }
            if (victim == null || victim == p || victim.Model == "shieldb3") return;

            double dist = Math.Sqrt(
                Math.Pow(p.Pos.X - victim.Pos.X, 2) +
                Math.Pow(p.Pos.Y - victim.Pos.Y, 2) +
                Math.Pow(p.Pos.Z - victim.Pos.Z, 2)
            ) / 32.0;
            if (dist > 4.0) return;

            DateTime lastHit;
            if (lastHitTime.TryGetValue(p.name, out lastHit) &&
                (DateTime.UtcNow - lastHit).TotalSeconds < 1.0) return;
            lastHitTime[p.name] = DateTime.UtcNow;

            var dir = new Vec3F32(
                p.Pos.X - victim.Pos.X,
                p.Pos.Y - victim.Pos.Y,
                p.Pos.Z - victim.Pos.Z
            );
            if (dir.Length > 0) dir = Vec3F32.Normalise(dir);
            if (victim.Supports(CpeExt.VelocityControl)) {
                victim.Send(Packet.VelocityControl(-dir.X * 1.5f, 0.75f, -dir.Z * 1.5f, 0, 1, 0));
            }

            // == DAMAGE & PARTICLES ==
            bool isCrit = (p.Pos.Y > victim.Pos.Y + 32);
            int dmg = isCrit ? 2 : 1;

            // determine block coordinates for effect
// determine block coordinates for effect
int ex = (int)(victim.Pos.X / 32);
int ey = (int)(victim.Pos.Y / 32);
int ez = (int)(victim.Pos.Z / 32);

if (isCrit) {
    // critical particles for everyone
    foreach (Player pl in PlayerInfo.Online.Items) {
        GoodlyEffects.SpawnEffectFor(pl, "pvp", ex, ey, ez, 0, 0, 0);
    }
} else {
    // heart particles for everyone
    foreach (Player pl in PlayerInfo.Online.Items) {
        GoodlyEffects.SpawnEffectFor(pl, "crit", ex, ey, ez, 0, 0, 0);
    }
}


            for (int i = 0; i < dmg; i++) {
                if (extraHearts.ContainsKey(victim.name) && extraHearts[victim.name] > 0) {
                    extraHearts[victim.name]--;
                    DisplayExtraHearts(victim);
                } else {
                    int hp;
                    playerHealth.TryGetValue(victim.name, out hp);
                    hp--;
                    if (hp <= 0) {
                        ResetAllBars(victim);
                        Command.Find("kill").Use(Player.Console, victim.name);
                        break;
                    }
                    playerHealth[victim.name] = hp;
                    DisplayHearts(victim);
                }
            }
        }
    }
}
