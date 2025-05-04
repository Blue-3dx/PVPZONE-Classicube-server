using System;
using System.IO;
using System.Collections.Generic;
using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using BlockID = System.UInt16;

public class XPPlugin : Plugin
{
    public override string name
    {
        get { return "XPPlugin"; }
    }
    public override string MCGalaxy_Version
    {
        get { return "1.9.3.0"; }
    }
    public static XPPlugin instance;

    // In-memory storage: player name -> XP and level.
    static Dictionary<string, int> playerXP = new Dictionary<string, int>();
    static Dictionary<string, int> playerLevel = new Dictionary<string, int>();
    // Store each player's original prefix to preserve their team title and other settings.
    static Dictionary<string, string> originalPrefix = new Dictionary<string, string>();

    // Delegates for events.
    private OnPlayerChat chatDelegate;
    private OnBlockChanging blockChangingDelegate;
    private OnPlayerDied diedDelegate;

    private Cmdxp xpCmd;

    // Promotion thresholds – if player's level equals any of these, they are promoted.
    static readonly int[] promotionThresholds = new int[] { 10, 50, 100, 150, 200 };

    public override void Load(bool startup)
    {
        instance = this;
        LoadGlobalData();

        // Register events.
        chatDelegate = new OnPlayerChat(OnPlayerChatHandler);
        OnPlayerChatEvent.Register(chatDelegate, Priority.Low, false);

        blockChangingDelegate = new OnBlockChanging(OnBlockChangingHandler);
        OnBlockChangingEvent.Register(blockChangingDelegate, Priority.Low, false);

        diedDelegate = new OnPlayerDied(OnPlayerDiedHandler);
        OnPlayerDiedEvent.Register(diedDelegate, Priority.Low, false);

        // Register /xp command.
        xpCmd = new Cmdxp();
        Command.Register(xpCmd);
    }

    public override void Unload(bool shutdown)
    {
        OnPlayerChatEvent.Unregister(chatDelegate);
        OnBlockChangingEvent.Unregister(blockChangingDelegate);
        OnPlayerDiedEvent.Unregister(diedDelegate);
        Command.Unregister(xpCmd);
        SaveGlobalData();
    }

    // -------------------------
    // Chat Prefix Helper
    // -------------------------
    // Updates the player's prefix to display current level and XP before their original prefix.
    void UpdateChatPrefix(Player p)
    {
        // Store the player's original prefix the first time.
        if (!originalPrefix.ContainsKey(p.name))
        {
            originalPrefix[p.name] = p.prefix;  // Save current prefix that contains team title, etc.
        }
        p.prefix = "&2Level " + GetLevel(p) + " |%f " + originalPrefix[p.name];
    }

    // -------------------------
    // XP and Level Persistence
    // -------------------------
    void LoadGlobalData()
    {
        string folder = "xp";
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        string file = Path.Combine(folder, "globalxp.txt");
        if (File.Exists(file))
        {
            try
            {
                string[] lines = File.ReadAllLines(file);
                foreach (string line in lines)
                {
                    if (!String.IsNullOrWhiteSpace(line))
                    {
                        // File format: playername xp level
                        string[] parts = line.Split(' ');
                        if (parts.Length >= 3)
                        {
                            string name = parts[0];
                            int xpVal, levelVal;
                            int.TryParse(parts[1], out xpVal);
                            int.TryParse(parts[2], out levelVal);
                            playerXP[name] = xpVal;
                            playerLevel[name] = levelVal;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }

    void SaveGlobalData()
    {
        string folder = "xp";
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        string file = Path.Combine(folder, "globalxp.txt");
        try
        {
            List<string> lines = new List<string>();
            foreach (KeyValuePair<string, int> kvp in playerXP)
            {
                string name = kvp.Key;
                int xpVal = kvp.Value;
                int levelVal = playerLevel.ContainsKey(name) ? playerLevel[name] : 1;
                lines.Add(name + " " + xpVal + " " + levelVal);
            }
            File.WriteAllLines(file, lines.ToArray());
        }
        catch (Exception e)
        {
            Logger.LogError(e);
        }
    }

    // -------------------------
    // XP & Level Helpers
    // -------------------------
    // Cumulative XP thresholds (cumulative system):
    // Level 1 starts at 0 XP.
    // Level 2 requires 5 XP total.
    // Level 3 requires 15 XP total.
    // Level 4 requires 35 XP total.
    // Level 5 requires 65 XP total, etc.
    public int CumulativxpForLevel(int level)
    {
        if (level == 1) return 0;
        if (level == 2) return 5;
        if (level == 3) return 15;
        int cumulative = 15;
        for (int L = 4; L <= level; L++)
        {
            cumulative += 10 * (L - 2);
        }
        return cumulative;
    }

    public static int GetXP(Player p)
    {
        if (!playerXP.ContainsKey(p.name))
            playerXP[p.name] = 0;
        return playerXP[p.name];
    }

    public static void SetXP(Player p, int xp)
    {
        playerXP[p.name] = xp;
    }

    public static int GetLevel(Player p)
    {
        if (!playerLevel.ContainsKey(p.name))
            playerLevel[p.name] = 1;
        return playerLevel[p.name];
    }

    public static void SetLevel(Player p, int level)
    {
        playerLevel[p.name] = level;
    }

    // -------------------------
    // Level Up Checking & Promotion
    // -------------------------
    public void CheckLevelUp(Player p)
    {
        int xp = GetXP(p);
        int level = GetLevel(p);
        bool leveled = false;
        // Loop to allow multiple level-ups if sufficient XP is accrued.
        while (xp >= CumulativxpForLevel(level + 1))
        {
            level++;
            leveled = true;
            SetLevel(p, level);
            p.Message("&aYou leveled up to level " + level + "!");
            // Automatically promote if level matches any threshold.
            foreach (int thresh in promotionThresholds)
            {
                if (level == thresh)
                {
                    Command.Find("promote").Use(Player.Console, p.name);
                    p.Message("&eCongratulations! You have been promoted!");
                    break;
                }
            }
        }
        if (leveled)
        {
            SaveGlobalData();
            UpdateChatPrefix(p);
        }
    }

    // -------------------------
    // Event Handlers for XP Gains
    // -------------------------
    // XP Gains: Chat: +3 XP, Block placing: +5 XP, Block breaking: +1 XP, Dying: +1 XP.
    void OnPlayerChatHandler(Player p, string message)
    {
        int xp = GetXP(p);
        SetXP(p, xp + 3);
        CheckLevelUp(p);
        UpdateChatPrefix(p);
    }

    void OnBlockChangingHandler(Player p, ushort x, ushort y, ushort z, BlockID block, bool placing, ref bool cancel)
    {
        int xp = GetXP(p);
        if (placing)
            SetXP(p, xp + 5);
        else
            SetXP(p, xp + 1);
        CheckLevelUp(p);
        UpdateChatPrefix(p);
    }

    void OnPlayerDiedHandler(Player p, BlockID cause, ref TimeSpan cooldown)
    {
        int xp = GetXP(p);
        SetXP(p, xp + 1);
        CheckLevelUp(p);
        UpdateChatPrefix(p);
    }
}

// -------------------------
// Command: /xp
// -------------------------
public class Cmdxp : Command
{
    public override string name { get { return "xp"; } }
    public override string type { get { return CommandTypes.Information; } }

    // /xp shows your XP, level, and how much XP is needed for the next level.
    // /xp playername shows the same info for that player.
    // /xp give password player amount lets you give XP (only for players level 10+)
    public override void Use(Player p, string message)
    {
        string trimmed = message.Trim();
        if (trimmed == "")
        {
            DisplayPlayerInfo(p, p);
            return;
        }
        string[] parts = trimmed.Split(' ');
        if (parts.Length >= 1 && parts[0].ToLower() == "give")
        {
            // xpected format: /xp give password player amount
            if (parts.Length != 4)
            {
                Help(p);
                return;
            }
            if (XPPlugin.GetLevel(p) < 10)
            {
                p.Message("&cYou need to be at least level 10 to give XP.");
                return;
            }
            if (parts[1] != "password")
            {
                p.Message("&cIncorrect password!");
                return;
            }
            string targetName = parts[2];
            int amount;
            try
            {
                amount = Convert.ToInt32(parts[3]);
            }
            catch (Exception)
            {
                p.Message("&cInvalid amount.");
                return;
            }
            Player target = PlayerInfo.FindMatches(p, targetName);
            if (target == null)
            {
                p.Message("&cPlayer not found.");
                return;
            }
            int targetXP = XPPlugin.GetXP(target);
            XPPlugin.SetXP(target, targetXP + amount);
            p.Message("&aYou have given " + target.name + " " + amount + " XP.");
            target.Message("&aYou received " + amount + " XP from " + p.name + ".");
            XPPlugin.instance.CheckLevelUp(target);
            return;
        }
        // Otherwise, assume it's a player name lookup.
        Player targetPlayer = PlayerInfo.FindMatches(p, trimmed);
        if (targetPlayer == null)
        {
            p.Message("&cPlayer not found.");
            return;
        }
        DisplayPlayerInfo(p, targetPlayer);
    }

    // Displays player's XP, level, and the XP required for the next level.
    void DisplayPlayerInfo(Player viewer, Player target)
    {
        int xp = XPPlugin.GetXP(target);
        int level = XPPlugin.GetLevel(target);
        viewer.Message("&a" + target.name + "'s XP: &e" + xp);
        viewer.Message("&a" + target.name + "'s Level: &e" + level);
        int nextLevelXP = XPPlugin.instance.CumulativxpForLevel(level + 1);
        int xpNeeded = nextLevelXP - xp;
        viewer.Message("&aXP needed for next level: &e" + xpNeeded);
    }

    public override void Help(Player p)
    {
        p.Message("&T/xp");
        p.Message("&HShows your current XP, level, and how much XP is needed for the next level.");
        p.Message("&T/xp playername");
        p.Message("&HShows the XP info of the specified player.");
        p.Message("&T/xp give password player amount");
        p.Message("&HOnly players of level 10+ can use this to give XP to others.");
    }
}