using Godot;
using Godot.Collections;

namespace Project187
{
	/// Root scene script.
	/// Bootstraps a test attack configuration when no attacks are configured via PlayerStats,
	/// so the game is immediately playable without needing .tres assets set up in the editor.
	public partial class Main : Node2D
	{
		private GameOverScreen _gameOverScreen;

		public override void _Ready()
		{
			_gameOverScreen = GetNode<GameOverScreen>("GameOverScreen");

			var players = GetTree().GetNodesInGroup("Player");
			if (players.Count == 0) return;

			var player = players[0] as Player;
			if (player == null) return;

			player.Died += _gameOverScreen.ShowScreen;

			// Only bootstrap if player has no starting attacks configured
			if (player.Stats?.StartingAttacks?.Count > 0) return;

			BootstrapTestAttacks(player);
		}

		private void BootstrapTestAttacks(Player player)
		{
			var projectileScene = GD.Load<PackedScene>("res://Scenes/Attacks/Projectile.tscn");
			var areaScene       = GD.Load<PackedScene>("res://Scenes/Attacks/AreaEffect.tscn");

			// ── Machine Gun (Timed → fires every 1 second) ──────────────────────
			var machineGun = new AttackData
			{
				AttackId        = "MachineGun",
				AttackName      = "Machine Gun",
				Type            = AttackType.Projectile,
				BaseDamage      = 15f,
				EnergyThreshold = 100f,
				ProjectileSpeed = 500f,
				MaxAdaptationSlots = 3,
				ProjectileScene = projectileScene,
				GeneratorConfigs = new Array<EnergyGeneratorData>
				{
					new TimedGeneratorData { EnergyPerSecond = 100f } // fires ~every 1s
				}
			};

			// ── Shock Pulse (OnHit → fires after 3 machine gun hits) ────────────
			var shockPulse = new AttackData
			{
				AttackId        = "ShockPulse",
				AttackName      = "Shock Pulse",
				Type            = AttackType.Area,
				BaseDamage      = 25f,
				EnergyThreshold = 100f,
				AreaRadius      = 100f,
				AreaDuration    = 1.2f,
				PulseInterval   = 0.3f,
				MaxAdaptationSlots = 2,
				ProjectileScene = areaScene,
				GeneratorConfigs = new Array<EnergyGeneratorData>
				{
					new OnHitGeneratorData
					{
						SourceAttackId = "MachineGun",
						EnergyPerEvent = 34f  // 3 hits = ~100 energy = fires
					}
				}
			};

			var startingAttacks = new Array<AttackData> { machineGun, shockPulse };
			player.AttackManager.Initialize(startingAttacks);

			GD.Print("[Main] Bootstrapped test attacks: MachineGun → ShockPulse chain");
		}

		/// Debug: press Tab to equip a PiercingAdaptation on the MachineGun.
		public override void _Input(InputEvent @event)
		{
			if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.Tab)
			{
				var players = GetTree().GetNodesInGroup("Player");
				if (players.Count == 0) return;

				var player = (players[0] as Player)?.AttackManager;
				if (player == null) return;

				bool equipped = player.TryEquipAdaptation("MachineGun", new PiercingAdaptation());
				GD.Print(equipped
					? "[Debug] Piercing adaptation equipped on MachineGun."
					: "[Debug] Could not equip Piercing (slots full or type mismatch).");
			}
		}
	}
}
