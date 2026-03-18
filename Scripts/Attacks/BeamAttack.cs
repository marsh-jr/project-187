using Godot;

namespace Project187
{
    /// Placeholder — fires a RayCast2D-style beam.
    /// Full implementation in Phase 7.
    public partial class BeamAttack : AttackInstance
    {
        protected override void ExecuteFire()
        {
            var stats = GetComputedStats();
            var owner = GetParent<AttackManager>().GetParent<Player>();
            GD.Print($"BeamAttack '{AttackId}' fired from {owner.GlobalPosition} with length {stats.BeamLength}");
            // TODO Phase 7: instantiate beam scene
        }
    }
}
