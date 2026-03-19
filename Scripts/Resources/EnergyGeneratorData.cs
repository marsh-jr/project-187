using Godot;

namespace Project187
{
	/// Base resource for describing how an attack's energy pool is filled.
	[GlobalClass]
	public partial class EnergyGeneratorData : Resource
	{
		/// Energy delivered per event (meaning depends on subclass).
		[Export] public float EnergyPerEvent { get; set; } = 10f;

		/// Fully-qualified C# class name of the concrete EnergyGeneratorBase subclass.
		/// E.g. "Project187.TimedEnergyGenerator". Drives instantiation in AttackManager.
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

	/// Delivers EnergyPerSecond continuously via _Process.
	[GlobalClass]
	public partial class TimedGeneratorData : EnergyGeneratorData
	{
		[Export] public float EnergyPerSecond { get; set; } = 10f;
	}

	/// Abstract base for observer data — no SourceAttackId needed;
	/// the observer resolves its source from its parent AttackInstance in the scene tree.
	public abstract partial class ObserverData : EnergyGeneratorData { }

	/// Delivers EnergyPerEvent each time the parent attack hits an enemy.
	[GlobalClass]
	public partial class OnHitGeneratorData : ObserverData { }

	/// Delivers EnergyPerEvent each time the parent attack kills an enemy.
	[GlobalClass]
	public partial class OnKillGeneratorData : ObserverData { }
}
