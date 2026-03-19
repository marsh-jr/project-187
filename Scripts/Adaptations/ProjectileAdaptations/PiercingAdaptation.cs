using Godot;

namespace Project187
{
    public class PiercingAdaptation : IAdaptation
    {
        public AdaptationCategory Category => AdaptationCategory.Projectile;

        public void OnFire(AttackInstance attack, ref AttackFireParams p) => p.PierceCount = 999;
        public void OnHitEnemy(AttackInstance attack, Node enemy, ref HitResult r) { }
        public void ModifyStats(AttackInstance attack, ref AttackRuntimeStats s) { }
    }
}
