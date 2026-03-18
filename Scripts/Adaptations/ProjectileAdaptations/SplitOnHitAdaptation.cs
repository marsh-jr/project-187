using Godot;

namespace Project187
{
    public class SplitOnHitAdaptation : IAdaptation
    {
        public AdaptationCategory Category => AdaptationCategory.Projectile;

        public void OnFire(AttackInstance attack, ref AttackFireParams p) { }
        public void OnHitEnemy(AttackInstance attack, Node enemy, ref HitResult r) => r.ShouldSplit = true;
        public void ModifyStats(AttackInstance attack, ref AttackRuntimeStats s) { }
    }
}
