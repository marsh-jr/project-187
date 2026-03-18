using Godot;

namespace Project187
{
    /// Reduces pulse interval — area hits enemies more frequently.
    public class PulseFrequencyAdaptation : IAdaptation
    {
        public AdaptationCategory Category => AdaptationCategory.Area;
        public float Multiplier { get; set; } = 0.6f; // shorter interval = faster pulses

        public void OnFire(AttackInstance attack, ref AttackFireParams p) { }
        public void OnHitEnemy(AttackInstance attack, Node enemy, ref HitResult r) { }
        public void ModifyStats(AttackInstance attack, ref AttackRuntimeStats s) => s.PulseInterval *= Multiplier;
    }
}
