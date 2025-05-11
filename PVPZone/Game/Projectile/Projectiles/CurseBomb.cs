using MCGalaxy.Maths;
using PVPZone.Game.Player;

namespace PVPZone.Game.Projectile.Projectiles
{
    public class CurseBomb : Projectile
    {
        public override void OnCollide(PVPPlayer player)
        {
            Vec3U16 blockPos = Util.Round(Position);
            Util.Effect(Level, "explosion", blockPos.X, blockPos.Y, blockPos.Z);
            if (player == null)
                return;
            if (player.MCGalaxyPlayer.Model == "shieldb3")
                return;

            player.Curse();
            player.Knockback(this.Velocity.X, 2f, this.Velocity.Z, 1.5f);
            player.DamageEffect();
        }

        public CurseBomb() : base()
        {
            BlockId = 256 + 486;
            Gravity = 0.05f;
        }
    }
}
