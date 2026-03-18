using Godot;
using Godot.Collections;

namespace Project187
{
    /// Defines an attack archetype. One .tres per attack type.
    /// At runtime, CreateRuntimeInstance() produces the correct AttackInstance subclass.
    [GlobalClass]
    public partial class AttackData : Resource
    {
        // ── Identity ───────────────────────────────────────────────────────────
        [Export] public string AttackId        { get; set; } = "";
        [Export] public string AttackName      { get; set; } = "";
        [Export] public Texture2D Icon         { get; set; }
        [Export] public AttackType Type        { get; set; } = AttackType.Projectile;
        [Export] public DamageType DamageType  { get; set; } = DamageType.Kinetic;

        // ── Energy ─────────────────────────────────────────────────────────────
        /// Attack fires when CurrentEnergy reaches this value.
        [Export] public float EnergyThreshold  { get; set; } = 100f;

        // ── Base Stats ─────────────────────────────────────────────────────────
        [Export] public float BaseDamage       { get; set; } = 10f;
        [Export] public float ProjectileSpeed  { get; set; } = 400f;
        [Export] public float AreaRadius       { get; set; } = 80f;
        [Export] public float AreaDuration     { get; set; } = 1.5f;
        [Export] public float PulseInterval    { get; set; } = 0.3f;
        [Export] public float BeamLength       { get; set; } = 300f;
        [Export] public float MeleeRange       { get; set; } = 80f;

        // ── Adaptation Slots ───────────────────────────────────────────────────
        /// Maximum number of adaptation slots this attack can roll.
        [Export] public int MaxAdaptationSlots { get; set; } = 3;

        // ── Spawn ──────────────────────────────────────────────────────────────
        /// The scene to instantiate when this attack fires (ProjectileNode or AreaEffectNode).
        [Export] public PackedScene ProjectileScene { get; set; }

        // ── Generators ─────────────────────────────────────────────────────────
        [Export] public Array<EnergyGeneratorData> GeneratorConfigs { get; set; } = new();

        // ── Default Adaptations ────────────────────────────────────────────────
        [Export] public Array<AdaptationData> DefaultAdaptations { get; set; } = new();
    }
}
