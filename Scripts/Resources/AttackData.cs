using Godot;
using Godot.Collections;

namespace Project187
{
	/// Defines an attack archetype. One .tres per attack type.
	/// At runtime, CreateRuntimeInstance() produces the correct AttackInstance subclass.
	[GlobalClass]
	public partial class AttackData : Resource
	{
		// ── Identity ───────────────────────────────────────────────────────────
		[Export] public string AttackId        { get; set; } = "";
		[Export] public string AttackName      { get; set; } = "";
		[Export] public Texture2D Icon         { get; set; }
		[Export] public AttackType Type        { get; set; } = AttackType.Projectile;
		[Export] public DamageType DamageType  { get; set; } = DamageType.Kinetic;

		// ── Base Stats ─────────────────────────────────────────────────────────
		[Export] public float BaseDamage       { get; set; } = 10f;
		[Export] public float CritChance       { get; set; } = 0.05f;  // 5 % default
		[Export] public float CritMultiplier   { get; set; } = 2.0f;   // 2x default
		[Export] public float ProjectileSpeed  { get; set; } = 400f;
		[Export] public float AreaRadius       { get; set; } = 80f;
		[Export] public float AreaDuration     { get; set; } = 1.5f;
		[Export] public float PulseInterval    { get; set; } = 0.3f;
		[Export] public float BeamLength       { get; set; } = 300f;
		[Export] public float MeleeRange       { get; set; } = 80f;

		// ── Adaptation Slots ───────────────────────────────────────────────────
		/// Maximum number of adaptation slots this attack can roll.
		[Export] public int MaxAdaptationSlots { get; set; } = 3;

		// ── Spawn ──────────────────────────────────────────────────────────────
		/// The scene to instantiate when this attack fires (ProjectileNode or AreaEffectNode).
		[Export] public PackedScene ProjectileScene { get; set; }

		// ── Chain Slots ────────────────────────────────────────────────────────
		/// Observers chained off this attack (must be ObserverData entries).
		/// Each observer is created as a child of this AttackInstance and watches
		/// this attack's signals to power the next attack in the chain.
		[Export] public Array<EnergyGeneratorData> ChainSlots { get; set; } = new();

		// ── Default Adaptations ────────────────────────────────────────────────
		[Export] public Array<AdaptationData> DefaultAdaptations { get; set; } = new();

		// ── Runtime instantiation ───────────────────────────────────────────────
		/// Fully-qualified C# class name of the concrete AttackInstance subclass.
		/// E.g. "Project187.MachineGun". Drives instantiation in AttackManager.
		/// Type field above is kept as display metadata only.
		[Export] public string ImplementingClass { get; set; } = "";

		public AttackInstance CreateRuntimeInstance()
		{
			if (string.IsNullOrEmpty(ImplementingClass)) return null;
			var type = System.Type.GetType(ImplementingClass);
			if (type == null)
			{
				GD.PushWarning($"AttackData '{AttackId}': type '{ImplementingClass}' not found.");
				return null;
			}
			return System.Activator.CreateInstance(type) as AttackInstance;
		}
	}
}
