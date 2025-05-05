using MCGalaxy.Maths;
using PVPZone.Game.Player;

namespace PVPZone.Game.Projectile.Projectiles
{
    public class BlastBall : Projectile
    {
        static string[] effects = { "bluefirework", "greenfirework", "purplefirework", "rainbowfirework", "redfirework", "yellowfirework" };
        static System.Random rnd = new System.Random();
        public override void OnCollide(PVPPlayer player)
        {
            Vec3U16 blockPos = Util.Round(Position);
            Util.Effect(Level, effects[rnd.Next(0, effects.Length - 1)], blockPos.X, blockPos.Y, blockPos.Z);
            player.Knockback(this.Velocity.X, 2f, this.Velocity.Z, 1.5f);
            player.Damage(new DamageReason(DamageReason.DamageType.Explosion, 15, player, Thrower));
            player.DamageEffect();
        }
        public override void OnTick()
        {
            Vec3U16 blockPos = Util.Round(Position);
            Util.Effect(Level, effects[rnd.Next(0, effects.Length - 1)], blockPos.X, blockPos.Y, blockPos.Z);
        }
        public BlastBall() : base()
        {
            BlockId = 256 + 501;
        }
    }
}
