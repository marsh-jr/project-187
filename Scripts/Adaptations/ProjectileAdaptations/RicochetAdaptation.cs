using Godot;

namespace Project187
{
    public class RicochetAdaptation : IAdaptation
    {
        public AdaptationCategory Category => AdaptationCategory.Projectile;
        public int Bounces { get; set; } = 2;

        public void OnFire(AttackInstance attack, ref AttackFireParams p)
        {
            p.RicochetBounces += Bounces;
            if (p.PierceCount < 999) p.PierceCount = Mathf.Max(p.PierceCount, Bounces); // must pierce to continue after bounce
        }
        public void OnHitEnemy(AttackInstance attack, Node enemy, ref HitResult r) => r.ShouldRicochet = true;
        public void ModifyStats(AttackInstance attack, ref AttackRuntimeStats s) { }
    }
}
