using Godot;
using Godot.Collections;

namespace Project187
{
	/// Root scene script.
	/// Bootstraps a test attack configuration when no generators are configured via PlayerStats,
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

			// Only bootstrap if player has no starting generators configured
			if (player.Stats?.StartingGenerators?.Count > 0) return;

			BootstrapTestAttacks(player);
		}

		private void BootstrapTestAttacks(Player player)
		{
			var projectileScene = GD.Load<PackedScene>("res://Scenes/Attacks/Projectile.tscn");
			var areaScene       = GD.Load<PackedScene>("res://Scenes/Attacks/AreaEffect.tscn");

			// Shock Pulse — terminal attack (no chain slots of its own)
			// Triggered by OnHit observer at 30% efficiency → 25 × 0.3 = 7.5 dmg per pulse tick
			var shockPulseData = new AttackData
			{
				ImplementingClass  = "Project187.ShockPulse",
				AttackId           = "ShockPulse",
				AttackName         = "Shock Pulse",
				Type               = AttackType.Area,
				BaseDamage         = 25f,
				AreaRadius         = 100f,
				AreaDuration       = 1.2f,
				PulseInterval      = 0.3f,
				MaxAdaptationSlots = 2,
				ProjectileScene    = areaScene,
				ChainSlots         = new Array<EnergyGeneratorData>()
			};

			// Machine Gun — triggered by EnergyCore at 90% efficiency → 15 × 0.9 = 13.5 dmg
			// Chains into ShockPulse: every hit triggers ShockPulse at 30% efficiency
			var machineGunData = new AttackData
			{
				ImplementingClass  = "Project187.MachineGun",
				AttackId           = "MachineGun",
				AttackName         = "Machine Gun",
				Type               = AttackType.Projectile,
				BaseDamage         = 15f,
				ProjectileSpeed    = 500f,
				MaxAdaptationSlots = 3,
				ProjectileScene    = projectileScene,
				ChainSlots         = new Array<EnergyGeneratorData>
				{
					new OnHitGeneratorData
					{
						ImplementingClass = "Project187.OnHitEnergyGenerator",
						Efficiency        = 0.30f,
						Attack            = shockPulseData
					}
				}
			};

			// EnergyCore: triggers MachineGun every 0.9s at 90% damage efficiency
			var energyCore = new EnergyCoreData
			{
				ImplementingClass = "Project187.EnergyCore",
				TriggerInterval   = 0.9f,
				Efficiency        = 0.90f,
				Attack            = machineGunData
			};

			player.AttackManager.Initialize(new Array<EnergyGeneratorData> { energyCore });

			GD.Print("[Main] Bootstrapped: EnergyCore (0.9s) → MachineGun (90%) → OnHit → ShockPulse (30%)");
		}

		/// Debug: press Tab to equip a PiercingAdaptation on the MachineGun.
		public override void _Input(InputEvent @event)
		{
			if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.Tab)
			{
				var players = GetTree().GetNodesInGroup("Player");
				if (players.Count == 0) return;

				var manager = (players[0] as Player)?.AttackManager;
				if (manager == null) return;

				bool equipped = manager.TryEquipAdaptation("MachineGun", new PiercingAdaptation());
				GD.Print(equipped
					? "[Debug] Piercing adaptation equipped on MachineGun."
					: "[Debug] Could not equip Piercing (slots full or type mismatch).");
			}
		}
	}
}
