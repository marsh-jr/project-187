using Godot;
using System.Collections.Generic;

namespace Project187
{
	/// Runtime node representing one active attack on the player.
	/// Fired directly by its parent generator/observer with a damage efficiency value.
	public abstract partial class AttackInstance : Node2D
	{
		// ── Signals ────────────────────────────────────────────────────────────
		[Signal] public delegate void EnemyHitEventHandler(AttackInstance source, Node enemy, float damage);
		[Signal] public delegate void EnemyKilledEventHandler(AttackInstance source, Node enemy);
		[Signal] public delegate void AttackFiredEventHandler(AttackInstance source);

		// ── State ──────────────────────────────────────────────────────────────
		public AttackData Data             { get; private set; }
		public string AttackId             { get; private set; }
		public int AdaptationSlotCount     { get; private set; }
		public List<IAdaptation> Adaptations { get; } = new();

		protected AttackManager Manager    { get; private set; }
		protected Node ProjectileContainer { get; private set; }

		/// Cached Player reference. Resolved via the AttackManager (which is a direct child of
		/// Player) so the parent-chain navigation is safe regardless of how deep this attack
		/// sits in the generator/observer hierarchy.
		protected Player OwnerPlayer       { get; private set; }

		// ── Init ───────────────────────────────────────────────────────────────
		public void Initialize(AttackData data, AttackManager manager)
		{
			Data    = data;
			AttackId = data.AttackId;
			Manager = manager;

			// AttackManager is always a direct child of Player — safe path regardless of nesting.
			OwnerPlayer = manager.GetParent<Player>();

			// Roll slot count: 0..MaxAdaptationSlots
			AdaptationSlotCount = GD.RandRange(0, data.MaxAdaptationSlots);

			// Apply any default adaptations defined on the resource
			foreach (var adaptData in data.DefaultAdaptations)
			{
				var instance = adaptData?.CreateInstance();
				if (instance != null)
					Adaptations.Add(instance);
			}

			// Cache the projectile container node from Main scene
			ProjectileContainer = GetTree().Root.FindChild("Projectiles", true, false) as Node;

			// Spawn chain-slot observers as children of this attack.
			// Each observer watches this attack's signals and powers the next attack in the chain.
			foreach (var slotConfig in data.ChainSlots)
			{
				if (slotConfig == null) continue;

				var node = slotConfig.CreateRuntimeInstance();
				if (node is EnergyObserver observer)
				{
					AddChild(observer);               // parent = this attack — read by observer.Initialize()
					observer.Initialize(slotConfig, manager);
				}
				else
				{
					GD.PushWarning($"AttackInstance '{AttackId}': ChainSlot entry is not an EnergyObserver. Only observer types are valid chain slots.");
					node?.QueueFree();
				}
			}
		}

		// ── Trigger ────────────────────────────────────────────────────────────
		/// Called by a generator or observer to fire this attack.
		/// efficiency (0–1) scales the resulting damage.
		public void Trigger(float efficiency)
		{
			EmitSignal(SignalName.AttackFired, this);
			ExecuteFire(efficiency);
		}

		// ── Hooks ──────────────────────────────────────────────────────────────
		/// Subclasses implement the actual spawn logic.
		/// efficiency is applied to BaseDamage before spawning projectiles/areas.
		protected abstract void ExecuteFire(float efficiency);

		/// Called by ProjectileNode / AreaEffectNode when a hit is detected.
		public HitResult RegisterHit(Node enemy, float rawDamage)
		{
			var result = new HitResult
			{
				DamageDealt      = rawDamage,
				ShouldSplit      = false,
				ShouldRicochet   = false,
				BouncesRemaining = 0,
				SlowFactor       = 0f,
				SlowDuration     = 0f,
			};

			foreach (var adaptation in Adaptations)
				adaptation.OnHitEnemy(this, enemy, ref result);

			if (enemy is BasicEnemy basicEnemy)
			{
				basicEnemy.TakeDamage(result.DamageDealt);

				bool died = basicEnemy.CurrentHp <= 0f;
				EmitSignal(SignalName.EnemyHit, this, enemy, result.DamageDealt);
				if (died)
					EmitSignal(SignalName.EnemyKilled, this, enemy);
			}

			return result;
		}

		// ── Stat Computation ───────────────────────────────────────────────────
		public AttackRuntimeStats GetComputedStats()
		{
			var stats = new AttackRuntimeStats
			{
				BaseDamage      = Data.BaseDamage,
				Radius          = Data.AreaRadius,
				Duration        = Data.AreaDuration,
				PulseInterval   = Data.PulseInterval,
				ProjectileSpeed = Data.ProjectileSpeed,
				BeamLength      = Data.BeamLength,
				MeleeRange      = Data.MeleeRange,
			};

			foreach (var adaptation in Adaptations)
				adaptation.ModifyStats(this, ref stats);

			return stats;
		}

		// ── Adaptation Helpers ─────────────────────────────────────────────────
		protected AttackFireParams BuildBaseFireParams()
		{
			var p = new AttackFireParams
			{
				ProjectileCount     = 1,
				SpeedMultiplier     = 1f,
				DamageMultiplier    = 1f,
				IsPiercing          = false,
				IsHoming            = false,
				RicochetBounces     = 0,
				SpreadAngleDegrees  = 0f,
			};

			foreach (var adaptation in Adaptations)
				adaptation.OnFire(this, ref p);

			return p;
		}
	}
}
