using Godot;

namespace Project187
{
	/// Abstract base for energy sources that observe a parent attack's events.
	/// The source attack is resolved from the parent node in the scene tree —
	/// no string ID lookup needed. The observer must be AddChild'd to an AttackInstance
	/// before Initialize() is called.
	public abstract partial class EnergyObserver : EnergyGeneratorBase
	{
		/// The attack whose signals this observer watches.
		/// Always the AttackInstance that owns this observer as a child.
		protected AttackInstance SourceAttack { get; private set; }

		public override void Initialize(EnergyGeneratorData config, AttackManager manager)
		{
			Config = config;

			// Source is the parent AttackInstance — set via AddChild before Initialize is called.
			SourceAttack = GetParent<AttackInstance>();
			if (SourceAttack == null)
			{
				GD.PushWarning($"{GetType().Name}: parent is not an AttackInstance. Observer must be added as a child of its source attack.");
				return;
			}

			// Create and own the next attack in the chain.
			ChildAttack  = CreateAndInitChildAttack(config.Attack, manager);
			TargetAttack = ChildAttack;

			// Wire signal subscriptions now that both ends are ready.
			Subscribe(SourceAttack);
		}

		/// Wire signal subscriptions to the source attack.
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
