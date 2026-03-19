using Godot;

namespace Project187
{
	/// Abstract base for all energy generators and observers.
	/// Subclasses push energy into TargetAttack (their ChildAttack).
	/// The generator is responsible for creating and owning its ChildAttack as a scene-tree child.
	public abstract partial class EnergyGeneratorBase : Node
	{
		/// The attack this generator is currently charging.
		public AttackInstance TargetAttack { get; protected set; }

		/// The attack node owned by this generator/observer as a direct child.
		public AttackInstance ChildAttack  { get; protected set; }

		public EnergyGeneratorData Config  { get; protected set; }

		/// Initialize from config. The generator creates and wires its own ChildAttack.
		public abstract void Initialize(EnergyGeneratorData config, AttackManager manager);

		/// Shared helper: instantiate the attack described by attackData, add it as a child,
		/// call its Initialize, and register it with the AttackManager for UI/adaptation queries.
		protected AttackInstance CreateAndInitChildAttack(AttackData attackData, AttackManager manager)
		{
			if (attackData == null) return null;

			var instance = attackData.CreateRuntimeInstance();
			if (instance == null)
			{
				GD.PushWarning($"{GetType().Name}: could not instantiate attack '{attackData.AttackId}' — ImplementingClass not set or invalid.");
				return null;
			}

			AddChild(instance);
			instance.Initialize(attackData, manager);
			manager.RegisterAttack(instance);
			return instance;
		}
	}
}
