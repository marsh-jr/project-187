using Godot;
using Godot.Collections;

namespace Project187
{
	/// Root scene script.
	/// Shows the ChainBuilderScreen before each round; wires player and spawner signals.
	public partial class Main : Node2D
	{
		private GameOverScreen    _gameOverScreen;
		private EnemySpawner      _spawner;
		private ChainBuilderScreen _chainBuilder;
		private Player            _player;
		private bool              _firstRound = true;

		public override void _Ready()
		{
			_gameOverScreen = GetNode<GameOverScreen>("GameOverScreen");
			_spawner        = GetNode<EnemySpawner>("EnemySpawner");

			var players = GetTree().GetNodesInGroup("Player");
			if (players.Count == 0) return;
			_player = players[0] as Player;
			if (_player == null) return;

			// Player death: stop spawner, show game-over overlay
			_player.Died += () => { _spawner.Stop(); _gameOverScreen.ShowScreen("GAME OVER"); };

			// Round over: clear enemies and show the chain builder again
			_spawner.RoundOver += OnRoundOver;

			// Chain builder (constructed in code -- no scene file needed)
			_chainBuilder = new ChainBuilderScreen();
			AddChild(_chainBuilder);
			_chainBuilder.ChainConfirmed += OnChainConfirmed;
			_chainBuilder.ShowForRound();
		}

		private void OnChainConfirmed(Array<EnergyGeneratorData> generators)
		{
			_player.AttackManager.Clear();
			_player.AttackManager.Initialize(generators);

			if (_firstRound)
			{
				_firstRound = false;
				_spawner.StartRound(_player, GetNode("Enemies"));
			}
			else
			{
				_spawner.RestartRound();
			}
		}

		private void OnRoundOver()
		{
			// Remove any enemies still alive in the field
			var enemyContainer = GetNode("Enemies");
			foreach (var child in enemyContainer.GetChildren())
				child.QueueFree();

			_chainBuilder.ShowForRound();
		}
	}
}
