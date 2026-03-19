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
		/// spawnPosition overrides where the attack spawns; null defaults to the player's position.
		public void Trigger(float efficiency, Vector2? spawnPosition = null)
		{
			EmitSignal(SignalName.AttackFired, this);
			ExecuteFire(efficiency, spawnPosition ?? OwnerPlayer.GlobalPosition);
		}

		// ── Hooks ──────────────────────────────────────────────────────────────
		/// Subclasses implement the actual spawn logic.
		/// efficiency scales BaseDamage; spawnPosition is the resolved world-space origin.
		protected abstract void ExecuteFire(float efficiency, Vector2 spawnPosition);

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
				IsCrit           = false,
			};

			foreach (var adaptation in Adaptations)
				adaptation.OnHitEnemy(this, enemy, ref result);

			// ── Crit roll (after adaptations have had their chance to modify damage) ──
			var stats = GetComputedStats();
			if (GD.Randf() < stats.CritChance)
			{
				result.DamageDealt *= stats.CritMultiplier;
				result.IsCrit = true;
			}

			if (enemy is BasicEnemy basicEnemy)
			{
				basicEnemy.TakeDamage(result.DamageDealt);

				bool died = basicEnemy.CurrentHp <= 0f;
				EmitSignal(SignalName.EnemyHit, this, enemy, result.DamageDealt);
				if (died)
					EmitSignal(SignalName.EnemyKilled, this, enemy);

				SpawnDamageLabel(basicEnemy.GlobalPosition, result.DamageDealt, result.IsCrit);
			}

			return result;
		}

		private void SpawnDamageLabel(Vector2 worldPosition, float damage, bool isCrit)
		{
			var label = new Label();
			label.Text = isCrit
				? $"{Mathf.RoundToInt(damage)}!"
				: $"{Mathf.RoundToInt(damage)}";

			label.AddThemeColorOverride("font_color",
				isCrit ? new Color(1f, 0.85f, 0f) : Colors.White);
			label.AddThemeFontSizeOverride("font_size", isCrit ? 20 : 14);
			label.ZIndex = 100;
			label.GlobalPosition = worldPosition + new Vector2(-20f, -30f);

			GetTree().CurrentScene.AddChild(label);

			float duration = isCrit ? 1.1f : 0.75f;
			var tween = label.CreateTween();
			tween.SetParallel(true);
			tween.TweenProperty(label, "position:y", label.Position.Y - 40f, duration)
			     .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
			tween.TweenProperty(label, "modulate:a", 0f, duration)
			     .SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Linear);
			tween.TweenCallback(Callable.From(label.QueueFree)).SetDelay(duration);
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
				CritChance      = Data.CritChance,
				CritMultiplier  = Data.CritMultiplier,
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
