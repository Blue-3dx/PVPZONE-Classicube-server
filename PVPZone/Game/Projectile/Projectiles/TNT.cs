using MCGalaxy;
using MCGalaxy.Maths;
using PVPZone.Game.Player;

namespace PVPZone.Game.Projectile.Projectiles
{
    public class TNT : Projectile
    {

        public override void OnCollide(PVPPlayer player)
        {
            Vec3U16 blockPos = Util.Round(Position);
            SpawnExplosion((int)blockPos.X, (int)blockPos.Y, (int)blockPos.Z);
        }
        private void SpawnExplosion(int cx, int cy, int cz)
        {
            Util.Effect(Level, "explosion", cx, cy, cz);
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                    for (int z = -1; z <= 1; z++)
                    {
                        int px = cx + x, py = cy + y, pz = cz + z;
                        MCGalaxy.Player pl = Util.PlayerAt(this.Level, px, py, pz);
                        if (pl == null)
                            continue;
                        PVPPlayer pvpPl = PVPPlayer.Get(pl);
                        if (pvpPl == null)
                            continue;
                        pvpPl.Damage(new DamageReason(DamageReason.DamageType.Explosion, 20, pvpPl, this.Thrower));
                    }
            Util.FakeExplosionEffect(Level, cx, cy, cz, 2);
        }
        public override void OnTick()
        {
          
        }
        public TNT() : base()
        {
            BlockId = Block.TNT;
        }
    }
}
