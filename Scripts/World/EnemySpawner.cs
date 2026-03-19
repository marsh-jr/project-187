using Godot;

namespace Project187
{
	/// Manages the round timer and continuously spawns enemies with increasing frequency.
	/// Add as a child of Main. Call StartRound() after the player and enemy container are ready.
	public partial class EnemySpawner : Node
	{
		[Signal] public delegate void RoundOverEventHandler();

		[Export] public PackedScene EnemyScene         { get; set; }
		[Export] public float RoundDuration            { get; set; } = 90f;   // 1:30
		[Export] public float InitialSpawnInterval     { get; set; } = 2.5f;  // seconds between spawns at start
		[Export] public float FinalSpawnInterval       { get; set; } = 0.4f;  // seconds between spawns at end
		[Export] public float SpawnRadius              { get; set; } = 450f;  // pixels from player

		/// Seconds remaining in the current round. Read by HUD.
		public float TimeRemaining { get; private set; }

		private float  _spawnTimer = 0f; // starts at 0 so first enemy spawns immediately
		private Node   _enemyContainer;
		private Player _player;
		private bool   _active = false;

		public void StartRound(Player player, Node enemyContainer)
		{
			_player         = player;
			_enemyContainer = enemyContainer;
			TimeRemaining   = RoundDuration;
			_active         = true;
		}

		/// Stop the round without emitting RoundOver (used on player death).
		public void Stop() => _active = false;

		public override void _Process(double delta)
		{
			if (!_active) return;

			TimeRemaining -= (float)delta;
			if (TimeRemaining <= 0f)
			{
				TimeRemaining = 0f;
				_active = false;
				EmitSignal(SignalName.RoundOver);
				return;
			}

			_spawnTimer -= (float)delta;
			if (_spawnTimer <= 0f)
			{
				// progress: 0 at round start, 1 at round end
				float progress = 1f - TimeRemaining / RoundDuration;
				_spawnTimer = Mathf.Lerp(InitialSpawnInterval, FinalSpawnInterval, progress);
				SpawnEnemy();
			}
		}

		private void SpawnEnemy()
		{
			if (EnemyScene == null || _enemyContainer == null || _player == null) return;

			float angle = GD.Randf() * Mathf.Tau;
			var spawnPos = _player.GlobalPosition + Vector2.Right.Rotated(angle) * SpawnRadius;

			var enemy = EnemyScene.Instantiate<BasicEnemy>();
			_enemyContainer.AddChild(enemy);
			enemy.GlobalPosition = spawnPos;
		}
	}
}
