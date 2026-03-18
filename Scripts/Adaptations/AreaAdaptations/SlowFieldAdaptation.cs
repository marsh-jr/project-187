using Godot;

namespace Project187
{
    /// Enemies hit by this area attack are slowed.
    public class SlowFieldAdaptation : IAdaptation
    {
        public AdaptationCategory Category => AdaptationCategory.Area;
        public float SlowFactor   { get; set; } = 0.5f; // enemies move at 50% speed
        public float SlowDuration { get; set; } = 2f;

        public void OnFire(AttackInstance attack, ref AttackFireParams p) { }

        public void OnHitEnemy(AttackInstance attack, Node enemy, ref HitResult r)
        {
            r.SlowFactor   = SlowFactor;
            r.SlowDuration = SlowDuration;
            // Actual slow application requires a StatusEffect system (future work).
            // For now the values are recorded in HitResult for future use.
        }

        public void ModifyStats(AttackInstance attack, ref AttackRuntimeStats s) { }
    }
}
