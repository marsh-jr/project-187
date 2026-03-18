using Godot;

namespace Project187
{
    /// Abstract base for all energy generators. Subclasses push energy into TargetAttack.
    public abstract partial class EnergyGeneratorBase : Node
    {
        public AttackInstance TargetAttack { get; protected set; }
        public EnergyGeneratorData Config  { get; protected set; }

        public abstract void Initialize(AttackInstance target, EnergyGeneratorData config, AttackManager manager);
    }
}
