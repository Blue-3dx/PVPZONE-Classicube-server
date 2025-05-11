using System;
using MCGalaxy;
using MCGalaxy.Maths;
using PVPZone.Game.Player;

namespace PVPZone.Game.Projectile.Projectiles
{
    public class TNT : Projectile
    {

        public override void OnCollide(PVPPlayer player)
        {
            this.Expire = DateTime.Now.AddSeconds(2);
        }
        public override void OnDestroy()
        {
            Explode();
        }
        public override void OnTick()
        {
          
        }
        public void Explode()
        {
            Vec3U16 blockPos = Util.Round(Position);
            Util.SpawnExplosion(this.Level, (int)blockPos.X, (int)blockPos.Y, (int)blockPos.Z, radius: 2, thrower: Thrower);
            this.Destroy = true;
        }

        public TNT() : base()
        {
            BlockId = Block.TNT;
            this.DestroyOnContact = false;
        }
    }
}
