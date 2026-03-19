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

		private EnemySpawner _spawner;

		public override void _Ready()
		{
			_gameOverScreen = GetNode<GameOverScreen>("GameOverScreen");
			_spawner        = GetNode<EnemySpawner>("EnemySpawner");

			var players = GetTree().GetNodesInGroup("Player");
			if (players.Count == 0) return;

			var player = players[0] as Player;
			if (player == null) return;

			// Wire round and game-over signals
			player.Died       += () => { _spawner.Stop(); _gameOverScreen.ShowScreen("GAME OVER"); };
			_spawner.RoundOver += () => _gameOverScreen.ShowScreen("ROUND OVER");

			// Start the round
			_spawner.StartRound(player, GetNode("Enemies"));

			// Only bootstrap if player has no starting generators configured
			if (player.Stats?.StartingGenerators?.Count > 0) return;

			BootstrapTestAttacks(player);
		}

		private void BootstrapTestAttacks(Player player)
		{
			var projectileScene = GD.Load<PackedScene>("res://Scenes/Attacks/Projectile.tscn");
			var meleeScene      = GD.Load<PackedScene>("res://Scenes/Attacks/AreaEffect.tscn"); // placeholder visual

			// ── Skewer — terminal melee lance (no chain) ───────────────────────────
			var skewerData = new AttackData
			{
				ImplementingClass  = "Project187.Skewer",
				AttackId           = "Skewer",
				AttackName         = "Skewer",
				Type               = AttackType.Melee,
				BaseDamage         = 20f,
				CritChance         = 0.06f,
				CritMultiplier     = 2.0f,
				MeleeRange         = 250f,
				AreaAngle          = 20f,
				MaxAdaptationSlots = 2,
				ChainSlots         = new Array<EnergyGeneratorData>()
			};

			// ── Aggressive Combat Processor — on-hit, fires Skewer ─────────────────
			var aggressiveCPData = new AggressiveCombatProcessorData
			{
				ImplementingClass = "Project187.AggressiveCombatProcessor",
				Attack            = skewerData
				// EfficiencyMin/Max = 0.30/0.40 set by constructor
			};

			// ── Slam — 360° melee burst, chains into Aggressive CP → Skewer ────────
			var slamData = new AttackData
			{
				ImplementingClass  = "Project187.Slam",
				AttackId           = "Slam",
				AttackName         = "Slam",
				Type               = AttackType.Melee,
				BaseDamage         = 25f,
				CritChance         = 0.06f,
				CritMultiplier     = 2.0f,
				MeleeRange         = 100f,
				AreaAngle          = 360f,
				MaxAdaptationSlots = 2,
				ChainSlots         = new Array<EnergyGeneratorData> { aggressiveCPData }
			};

			// ── Precise Combat Processor — on-crit, fires Slam ─────────────────────
			var preciseCPData = new PreciseCombatProcessorData
			{
				ImplementingClass = "Project187.PreciseCombatProcessor",
				Attack            = slamData
				// EfficiencyMin/Max = 1.00/1.15 set by constructor
			};

			// ── Machine Gun — 5-round burst, chains into Precise CP → Slam ─────────
			var machineGunData = new AttackData
			{
				ImplementingClass  = "Project187.MachineGun",
				AttackId           = "MachineGun",
				AttackName         = "Machine Gun",
				Type               = AttackType.Projectile,
				BaseDamage         = 12f,
				CritChance         = 0.06f,
				CritMultiplier     = 2.0f,
				ProjectileCount    = 5,
				PierceCount        = 1,
				SpreadAngleDegrees = 15f,
				ProjectileSpeed    = 500f,
				ProjectileDuration = 1.0f,
				MaxAdaptationSlots = 3,
				ProjectileScene    = projectileScene,
				ChainSlots         = new Array<EnergyGeneratorData> { preciseCPData }
			};

			// ── Turbo Core — fast timed trigger, fires Machine Gun ──────────────────
			var turboCoreData = new TurboCoreData
			{
				ImplementingClass = "Project187.TurboCore",
				Attack            = machineGunData
				// TriggerIntervalMin/Max = 0.7/0.9, EfficiencyMin/Max = 0.70/0.85 set by constructor
			};

			player.AttackManager.Initialize(new Array<EnergyGeneratorData> { turboCoreData });

			GD.Print("[Main] Bootstrap: TurboCore → MachineGun → PreciseCP → Slam → AggressiveCP → Skewer");
		}
	}
}
