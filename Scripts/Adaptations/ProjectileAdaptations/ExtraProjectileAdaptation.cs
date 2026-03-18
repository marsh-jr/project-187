using Godot;

namespace Project187
{
    public class ExtraProjectileAdaptation : IAdaptation
    {
        public AdaptationCategory Category => AdaptationCategory.Projectile;
        public int ExtraCount  { get; set; } = 2;
        public float SpreadDeg { get; set; } = 30f;

        public void OnFire(AttackInstance attack, ref AttackFireParams p)
        {
            p.ProjectileCount     += ExtraCount;
            p.SpreadAngleDegrees  =  Mathf.Max(p.SpreadAngleDegrees, SpreadDeg);
        }
        public void OnHitEnemy(AttackInstance attack, Node enemy, ref HitResult r) { }
        public void ModifyStats(AttackInstance attack, ref AttackRuntimeStats s) { }
    }
}
