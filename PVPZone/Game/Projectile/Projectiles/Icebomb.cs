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
            SpawnIceCube(Level, BlockPosition.X, BlockPosition.Y-1, BlockPosition.Z);

            if (player == null)
                return;

           // player.Knockback(-this.Velocity.X, 2f, -this.Velocity.Z, 1.5f);
            player.Damage(new DamageReason(DamageReason.DamageType.Frozen, 20, player, Thrower));
        }
        private void SpawnIceCube(Level lvl, int cx, int cy, int cz)
        {
            for (int x = -1; x <= 1; x++)
                for (int y = 0; y <= 3; y++) // cy to cy + 2, effectively cy-1 to cy+1
                    for (int z = -1; z <= 1; z++)
                    {
                        int px = cx + x, py = cy + y, pz = cz + z;
                        if (!lvl.IsValidPos(px, py, pz)) continue;
                        if (lvl.GetBlock((ushort)px, (ushort)py, (ushort)pz) != Block.Air)
                            continue;
             
                        lvl.Blockchange((ushort)px, (ushort)py, (ushort)pz, Block.Ice);

                        MCGalaxy.Player pl = Util.PlayerAt(null, px, py, pz);
                        if (pl == null)
                            continue;
                        PVPPlayer pvpPl = PVPPlayer.Get(pl);
                        if (pvpPl == null)
                            continue;
                        pvpPl.Damage(new DamageReason(DamageReason.DamageType.Frozen, 20, pvpPl, this.Thrower));
                    }
        }
        public Icebomb (): base()//(Level level, Vec3F32 Position, Vec3F32 Velocity, PVPPlayer Thrower = null) : base (level, Position, Velocity, Thrower)
        {
            BlockId = 489;
        }
    }
}
