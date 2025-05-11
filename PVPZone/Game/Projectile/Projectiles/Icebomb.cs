using MCGalaxy;
using MCGalaxy.Maths;
using PVPZone.Game.Player;

namespace PVPZone.Game.Projectile.Projectiles
{
    public class Icebomb : Projectile
    {
        public override void OnCollide( PVPPlayer player)
        {
            Vec3U16 BlockPosition = Util.Round(Position);
            SpawnIceCube(Level, BlockPosition.X, BlockPosition.Y+1, BlockPosition.Z);
        }
        static System.Random rnd = new System.Random();

        static int rndDirection { get { return rnd.Next(0, 2) ==1 ? -1 : 1; } }
        private void SpawnIceCube(Level lvl, int cx, int cy, int cz)
        {
            for (int x = -1; x <= 1; x++)
                for (int y = 0; y <= 3; y++) // cy to cy + 2, effectively cy-1 to cy+1
                    for (int z = -1; z <= 1; z++)
                    {
                        int px = cx + x, py = cy + y, pz = cz + z;
                        MCGalaxy.Player pl = Util.PlayerAt(this.Level, px, py, pz);
                        if (pl == null)
                            continue;
                        PVPPlayer pvpPl = PVPPlayer.Get(pl);
                        if (pvpPl == null)
                            continue;
                        pvpPl.Damage(new DamageReason(DamageReason.DamageType.Frozen, 20, pvpPl, this.Thrower));
                    }
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++) // cy to cy + 2, effectively cy-1 to cy+1
                    for (int z = -1; z <= 1; z++)
                    {
                        int px = cx + x, py = cy + y, pz = cz + z;
                        if (!lvl.IsValidPos(px, py, pz)) continue;
                        if (lvl.GetBlock((ushort)px, (ushort)py, (ushort)pz) != Block.Air)
                            continue;
                        Projectile.Throw(new Debris() { BlockId = Block.Ice }, lvl, new Vec3F32((float)px, (float)py, (float)pz), new Vec3F32((float)(rnd.NextDouble()* rndDirection* 0.5f), 0.5f, (float)(rnd.NextDouble()*rndDirection * 0.5f)), 0.5f);
                    }

          
        }
        public Icebomb (): base()
        {
            BlockId = 256 + 489;
        }
    }
}
