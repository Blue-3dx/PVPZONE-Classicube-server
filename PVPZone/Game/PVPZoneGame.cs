using MCGalaxy;
using PVPZone.Game.Item;
using PVPZone.Game.Player;
using PVPZone.Game.Projectile;

namespace PVPZone.Game
{
    public class PVPZoneGame
    {
        public static void Load()
        {
            PlayerManager.Load();
            ProjectileManager.Load();
            ItemManager.Load();
            CurseManager.Load();
        }
        public static void Unload()
        {
            PlayerManager.Unload();
            ProjectileManager.Unload();
            ItemManager.Unload();
            CurseManager.Unload();
        }
    }
}
