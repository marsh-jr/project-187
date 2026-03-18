using Godot;

namespace Project187
{
    /// Base resource for describing how an attack's energy pool is filled.
    [GlobalClass]
    public partial class EnergyGeneratorData : Resource
    {
        /// Energy delivered per event (meaning depends on subclass).
        [Export] public float EnergyPerEvent { get; set; } = 10f;
    }

    /// Delivers EnergyPerSecond continuously via _Process.
    [GlobalClass]
    public partial class TimedGeneratorData : EnergyGeneratorData
    {
        [Export] public float EnergyPerSecond { get; set; } = 10f;
    }

    /// Delivers EnergyPerEvent each time the named attack hits an enemy.
    [GlobalClass]
    public partial class OnHitGeneratorData : EnergyGeneratorData
    {
        /// Must match the AttackData.AttackId of the attack to watch.
        [Export] public string SourceAttackId { get; set; } = "";
    }

    /// Delivers EnergyPerEvent each time the named attack kills an enemy.
    [GlobalClass]
    public partial class OnKillGeneratorData : EnergyGeneratorData
    {
        /// Must match the AttackData.AttackId of the attack to watch.
        [Export] public string SourceAttackId { get; set; } = "";
    }
}
