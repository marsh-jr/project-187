using Godot;

namespace Project187
{
    public class HomingAdaptation : IAdaptation
    {
        public AdaptationCategory Category => AdaptationCategory.Projectile;

        public void OnFire(AttackInstance attack, ref AttackFireParams p) => p.IsHoming = true;
        public void OnHitEnemy(AttackInstance attack, Node enemy, ref HitResult r) { }
        public void ModifyStats(AttackInstance attack, ref AttackRuntimeStats s) { }
    }
}
