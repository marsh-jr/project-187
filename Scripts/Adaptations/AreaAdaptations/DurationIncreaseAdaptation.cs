using Godot;

namespace Project187
{
    public class DurationIncreaseAdaptation : IAdaptation
    {
        public AdaptationCategory Category => AdaptationCategory.Area;
        public float Multiplier { get; set; } = 1.5f;

        public void OnFire(AttackInstance attack, ref AttackFireParams p) { }
        public void OnHitEnemy(AttackInstance attack, Node enemy, ref HitResult r) { }
        public void ModifyStats(AttackInstance attack, ref AttackRuntimeStats s) => s.Duration *= Multiplier;
    }
}
