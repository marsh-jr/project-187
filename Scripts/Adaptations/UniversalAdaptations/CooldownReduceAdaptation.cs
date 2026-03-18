using Godot;

namespace Project187
{
    /// Reduces the effective energy threshold — attack fires more frequently.
    public class CooldownReduceAdaptation : IAdaptation
    {
        public AdaptationCategory Category => AdaptationCategory.Universal;
        public float ThresholdMultiplier { get; set; } = 0.75f;

        public void OnFire(AttackInstance attack, ref AttackFireParams p) { }
        public void OnHitEnemy(AttackInstance attack, Node enemy, ref HitResult r) { }

        // Note: threshold reduction is baked into AttackData at init time for simplicity.
        // For runtime application, AttackInstance would need to expose a computed threshold.
        public void ModifyStats(AttackInstance attack, ref AttackRuntimeStats s) { }
    }
}
