using PVPZone.Game.Player;

namespace PVPZone.Game.Projectile.Projectiles
{
    public class EnderPearl : Projectile
    {
        public override void OnCollide(PVPPlayer player)
        {
            this.Thrower.MCGalaxyPlayer.SendPosition(  MCGalaxy.Position.FromFeetBlockCoords((int)Position.X, (int)(Position.Y+1), (int)Position.Z), this.Thrower.MCGalaxyPlayer.Rot);
        }
        public EnderPearl() : base()
        {
            BlockId = 256 + 128;
        }
    }
}
