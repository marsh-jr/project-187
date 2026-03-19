using Godot;

namespace Project187
{
	/// Base resource describing how an energy source is configured.
	[GlobalClass]
	public partial class EnergyGeneratorData : Resource
	{
		/// Roll range for the efficiency value passed to the triggered attack.
		/// The runtime node rolls once at Initialize() — the resource stays immutable.
		[Export] public float EfficiencyMin { get; set; } = 1.0f;
		[Export] public float EfficiencyMax { get; set; } = 1.0f;

		/// Fully-qualified C# class name of the concrete EnergyGeneratorBase subclass.
		[Export] public string ImplementingClass { get; set; } = "";

		/// The attack this generator or observer powers.
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

	// ── Timed Cores ────────────────────────────────────────────────────────────

	/// Triggers its attack on a fixed time interval (rolled from range at init).
	[GlobalClass]
	public partial class EnergyCoreData : EnergyGeneratorData
	{
		[Export] public float TriggerIntervalMin { get; set; } = 1.0f;
		[Export] public float TriggerIntervalMax { get; set; } = 1.0f;
	}

	/// Fast timed core. Interval 0.7–0.9 s, efficiency 70–85%.
	[GlobalClass]
	public partial class TurboCoreData : EnergyCoreData
	{
		public TurboCoreData()
		{
			TriggerIntervalMin = 0.7f; TriggerIntervalMax = 0.9f;
			EfficiencyMin      = 0.70f; EfficiencyMax     = 0.85f;
		}
	}

	/// Slow but powerful timed core. Interval 1.1–1.3 s, efficiency 105–120%.
	[GlobalClass]
	public partial class MilitaryCoreData : EnergyCoreData
	{
		public MilitaryCoreData()
		{
			TriggerIntervalMin = 1.1f; TriggerIntervalMax = 1.3f;
			EfficiencyMin      = 1.05f; EfficiencyMax     = 1.20f;
		}
	}

	/// Very slow, very high efficiency. Interval 1.8–2.1 s, efficiency 210–230%.
	[GlobalClass]
	public partial class HeavyCoreData : EnergyCoreData
	{
		public HeavyCoreData()
		{
			TriggerIntervalMin = 1.8f; TriggerIntervalMax = 2.1f;
			EfficiencyMin      = 2.10f; EfficiencyMax     = 2.30f;
		}
	}

	// ── Observers (chain-slot reactive triggers) ────────────────────────────────

	/// Abstract base for observer data. Source is resolved from the parent
	/// AttackInstance in the scene tree — no string ID needed.
	public abstract partial class ObserverData : EnergyGeneratorData { }

	/// Triggers on each hit of the parent attack.
	[GlobalClass]
	public partial class OnHitGeneratorData : ObserverData { }

	/// Triggers on each kill of the parent attack.
	[GlobalClass]
	public partial class OnKillGeneratorData : ObserverData { }

	/// Triggers on each critical hit of the parent attack.
	[GlobalClass]
	public partial class OnCritGeneratorData : ObserverData { }

	// ── Named Combat Processors ────────────────────────────────────────────────

	/// On-hit processor. Efficiency 30–40%.
	[GlobalClass]
	public partial class AggressiveCombatProcessorData : OnHitGeneratorData
	{
		public AggressiveCombatProcessorData()
		{
			EfficiencyMin = 0.30f; EfficiencyMax = 0.40f;
		}
	}

	/// On-crit processor. Efficiency 100–115%.
	[GlobalClass]
	public partial class PreciseCombatProcessorData : OnCritGeneratorData
	{
		public PreciseCombatProcessorData()
		{
			EfficiencyMin = 1.00f; EfficiencyMax = 1.15f;
		}
	}
}
