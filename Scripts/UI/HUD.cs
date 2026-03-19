using Godot;

namespace Project187
{
	public partial class HUD : CanvasLayer
	{
		private Label _hpLabel;
		private Player _player;

		public override void _Ready()
		{
			_hpLabel = GetNodeOrNull<Label>("MarginContainer/VBox/HpLabel");

			var players = GetTree().GetNodesInGroup("Player");
			if (players.Count > 0)
				_player = players[0] as Player;
		}

		public override void _Process(double delta)
		{
			if (_player != null && _hpLabel != null)
				_hpLabel.Text = $"HP: {_player.CurrentHp:F0}";
		}
	}
}
