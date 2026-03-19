using Godot;

namespace Project187
{
    /// Abstract base for energy sources that observe another attack's events.
    /// Holds the SourceAttackId and owns the resolve-by-ID logic so subclasses
    /// only need to implement Subscribe / Unsubscribe.
    public abstract partial class EnergyObserver : Node
    {
        public AttackInstance TargetAttack    { get; protected set; }
        public EnergyGeneratorData Config     { get; protected set; }
        protected AttackInstance SourceAttack { get; private set; }

        public void Initialize(AttackInstance target, EnergyGeneratorData config, AttackManager manager)
        {
            TargetAttack = target;
            Config       = config;

            string sourceId = (config as ObserverData)?.SourceAttackId ?? "";
            if (string.IsNullOrEmpty(sourceId))
            {
                GD.PushWarning($"{GetType().Name}: no SourceAttackId set.");
                return;
            }

            SourceAttack = manager.GetAttackById(sourceId);
            if (SourceAttack != null)
                Subscribe(SourceAttack);
        }

        /// Wire signal subscriptions to the resolved source attack.
        protected abstract void Subscribe(AttackInstance source);

        /// Remove signal subscriptions from the source attack.
        protected abstract void Unsubscribe(AttackInstance source);

        public override void _ExitTree()
        {
            if (SourceAttack != null)
                Unsubscribe(SourceAttack);
        }
    }
}
