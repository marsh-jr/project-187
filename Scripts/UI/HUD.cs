using Godot;

namespace Project187
{
	public partial class HUD : CanvasLayer
	{
		private Label _hpLabel;
		private Label _timerLabel;
		private Player _player;
		private EnemySpawner _spawner;

		public override void _Ready()
		{
			_hpLabel    = GetNodeOrNull<Label>("MarginContainer/VBox/HpLabel");
			_timerLabel = GetNodeOrNull<Label>("MarginContainer/VBox/TimerLabel");

			var players = GetTree().GetNodesInGroup("Player");
			if (players.Count > 0)
				_player = players[0] as Player;

			_spawner = GetTree().Root.FindChild("EnemySpawner", true, false) as EnemySpawner;
		}

		public override void _Process(double delta)
		{
			if (_player != null && _hpLabel != null)
				_hpLabel.Text = $"HP: {_player.CurrentHp:F0}";

			if (_timerLabel != null && _spawner != null)
			{
				int secs = Mathf.CeilToInt(_spawner.TimeRemaining);
				_timerLabel.Text = $"{secs / 60}:{secs % 60:00}";
			}
		}
	}
}
