using Godot;

namespace Project187
{
    /// Abstract base for all beam-type attacks. Full implementation in Phase 7.
    public abstract partial class BeamAttackBase : AttackInstance
    {
        protected override void ExecuteFire()
        {
            var stats = GetComputedStats();
            var owner = OwnerPlayer;
            GD.Print($"{GetType().Name} '{AttackId}' fired from {owner.GlobalPosition} with length {stats.BeamLength}");
            // TODO Phase 7: instantiate beam scene
        }
    }
}
