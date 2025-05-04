using MCGalaxy.Maths;
using PVPZone.Game.Player;

namespace PVPZone.Game.Projectile.Projectiles
{
    public class Firework : Projectile
    {
        static string[] effects = { "bluefirework", "greenfirework", "purplefirework", "rainbowfirework", "redfirework", "yellowfirework" };
        static System.Random rnd = new System.Random();
        public override void OnCollide(PVPPlayer player)
        {
            Vec3U16 blockPos = Util.Round(Position);
            Util.Effect(Level, effects[rnd.Next(0, effects.Length - 1)], blockPos.X, blockPos.Y, blockPos.Z);
        }
        public override void OnTick()
        {
            Vec3U16 blockPos = Util.Round(Position);
            Util.Effect(Level, effects[rnd.Next(0, effects.Length - 1)], blockPos.X, blockPos.Y, blockPos.Z);
        }
        public override void OnCreation()
        {
            this.Velocity = new Vec3F32(0, 2f, 0);
        }
        public Firework() : base()//(Level level, Vec3F32 Position, Vec3F32 Velocity, PVPPlayer Thrower = null) : base (level, Position, Velocity, Thrower)
        {
            BlockId = 483;
            Gravity = 0f;
        }
    }
}
