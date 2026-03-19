namespace Project187
{
    /// Abstract base for energy sources that operate independently —
    /// no dependency on other attacks (timed, tick-based, passive).
    public abstract partial class EnergyGenerator : Godot.Node
    {
        public AttackInstance TargetAttack { get; protected set; }
        public EnergyGeneratorData Config  { get; protected set; }

        public abstract void Initialize(AttackInstance target, EnergyGeneratorData config, AttackManager manager);
    }
}
