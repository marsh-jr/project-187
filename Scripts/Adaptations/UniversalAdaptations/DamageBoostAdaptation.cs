using Godot;

namespace Project187
{
    public class DamageBoostAdaptation : IAdaptation
    {
        public AdaptationCategory Category => AdaptationCategory.Universal;
        public float Multiplier { get; set; } = 1.25f;

        public void OnFire(AttackInstance attack, ref AttackFireParams p) { }
        public void OnHitEnemy(AttackInstance attack, Node enemy, ref HitResult r) { }
        public void ModifyStats(AttackInstance attack, ref AttackRuntimeStats s) => s.BaseDamage *= Multiplier;
    }
}
