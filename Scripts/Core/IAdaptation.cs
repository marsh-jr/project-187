namespace Project187
{
    /// <summary>
    /// Implemented by plain C# classes (not Nodes).
    /// Each hook is called at a specific point in the attack lifecycle.
    /// All structs are passed by ref so the adaptation pipeline mutates a single copy.
    /// </summary>
    public interface IAdaptation
    {
        AdaptationCategory Category { get; }

        /// Called before projectiles/effects are spawned — mutate spawn parameters.
        void OnFire(AttackInstance attack, ref AttackFireParams fireParams);

        /// Called when a hit is confirmed — mutate the hit result (damage, split, ricochet).
        void OnHitEnemy(AttackInstance attack, Godot.Node enemy, ref HitResult result);

        /// Called via GetComputedStats() — pure stat multipliers, no side effects.
        void ModifyStats(AttackInstance attack, ref AttackRuntimeStats stats);
    }

    /// Spawn-time parameters; mutated by OnFire adaptations before any projectile is created.
    public struct AttackFireParams
    {
        public int   ProjectileCount;
        public float SpeedMultiplier;
        public float DamageMultiplier;
        public bool  IsPiercing;
        public bool  IsHoming;
        public int   RicochetBounces;
        public float SpreadAngleDegrees;
    }

    /// Hit-time result; mutated by OnHitEnemy adaptations after a collision is detected.
    public struct HitResult
    {
        public float DamageDealt;
        public bool  ShouldSplit;
        public bool  ShouldRicochet;
        public int   BouncesRemaining;
        public float SlowFactor;
        public float SlowDuration;
    }

    /// Stat snapshot used by AreaAttack and HUD display; mutated by ModifyStats adaptations.
    public struct AttackRuntimeStats
    {
        public float BaseDamage;
        public float Radius;
        public float Duration;
        public float PulseInterval;
        public float ProjectileSpeed;
        public float BeamLength;
        public float MeleeRange;
    }
}
