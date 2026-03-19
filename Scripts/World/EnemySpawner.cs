using Godot;

namespace Project187
{
	/// Manages the round timer and continuously spawns enemy groups with increasing frequency.
	/// Add as a child of Main. Call StartRound() after the player and enemy container are ready.
	public partial class EnemySpawner : Node
	{
		[Signal] public delegate void RoundOverEventHandler();

		[Export] public PackedScene EnemyScene             { get; set; }
		[Export] public float RoundDuration                { get; set; } = 90f;   // 1:30
		[Export] public float InitialSpawnInterval         { get; set; } = 2.5f;  // seconds between groups at start
		[Export] public float FinalSpawnInterval           { get; set; } = 0.4f;  // seconds between groups at end
		/// Fraction of the viewport half-diagonal used as spawn ring radius.
		/// 0.65 places the anchor just off-screen at any resolution.
		[Export] public float SpawnRadiusFraction          { get; set; } = 0.65f;
		[Export] public int   GroupSizeMin                 { get; set; } = 6;
		[Export] public int   GroupSizeMax                 { get; set; } = 10;
		/// Max offset radius (units) for each enemy within its group anchor.
		[Export] public float GroupSpread                  { get; set; } = 180f;

		/// Seconds remaining in the current round. Read by HUD.
		public float TimeRemaining { get; private set; }

		private float  _spawnTimer = 0f; // starts at 0 so first group spawns immediately
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
				SpawnGroup();
			}
		}

		private void SpawnGroup()
		{
			if (EnemyScene == null || _enemyContainer == null || _player == null) return;

			// Derive spawn ring radius from the current viewport size so it scales with resolution.
			// Length() = diagonal of the viewport rect; half of that is roughly the max visible distance.
			var viewSize = GetViewport().GetVisibleRect().Size;
			float radius = viewSize.Length() * 0.5f * SpawnRadiusFraction;

			// Pick one anchor point on the ring, then scatter enemies around it.
			float anchorAngle = GD.Randf() * Mathf.Tau;
			Vector2 anchor = _player.GlobalPosition + Vector2.Right.Rotated(anchorAngle) * radius;

			int count = (int)GD.RandRange(GroupSizeMin, GroupSizeMax + 1); // RandRange is inclusive on floats; +1 to include max
			for (int i = 0; i < count; i++)
			{
				float offsetAngle = GD.Randf() * Mathf.Tau;
				float offsetDist  = GD.Randf() * GroupSpread;
				Vector2 pos = anchor + Vector2.Right.Rotated(offsetAngle) * offsetDist;

				var enemy = EnemyScene.Instantiate<BasicEnemy>();
				_enemyContainer.AddChild(enemy);
				enemy.GlobalPosition = pos;
			}
		}
	}
}
