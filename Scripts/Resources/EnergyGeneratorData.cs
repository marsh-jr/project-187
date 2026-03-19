using Godot;

namespace Project187
{
	/// Base resource for describing how an attack's energy source is configured.
	[GlobalClass]
	public partial class EnergyGeneratorData : Resource
	{
		/// Fraction of base damage the triggered attack deals (0–1, where 1 = full damage).
		[Export] public float Efficiency { get; set; } = 1.0f;

		/// Fully-qualified C# class name of the concrete EnergyGeneratorBase subclass.
		/// E.g. "Project187.EnergyCore". Drives instantiation in AttackManager.
		[Export] public string ImplementingClass { get; set; } = "";

		/// The attack this generator or observer powers.
		/// Read at init time to create the child AttackInstance — never mutated at runtime.
		[Export] public AttackData Attack { get; set; }

		public Node CreateRuntimeInstance()
		{
			if (string.IsNullOrEmpty(ImplementingClass)) return null;
			var type = System.Type.GetType(ImplementingClass);
			if (type == null)
			{
				GD.PushWarning($"EnergyGeneratorData: type '{ImplementingClass}' not found.");
				return null;
			}
			return System.Activator.CreateInstance(type) as Node;
		}
	}

	/// Triggers its attack on a fixed time interval.
	[GlobalClass]
	public partial class EnergyCoreData : EnergyGeneratorData
	{
		/// Seconds between each trigger.
		[Export] public float TriggerInterval { get; set; } = 1.0f;
	}

	/// Abstract base for observer data.
	/// The observer resolves its source from its parent AttackInstance in the scene tree.
	public abstract partial class ObserverData : EnergyGeneratorData { }

	/// Delivers EnergyPerEvent each time the parent attack hits an enemy.
	[GlobalClass]
	public partial class OnHitGeneratorData : ObserverData { }

	/// Delivers EnergyPerEvent each time the parent attack kills an enemy.
	[GlobalClass]
	public partial class OnKillGeneratorData : ObserverData { }
}
