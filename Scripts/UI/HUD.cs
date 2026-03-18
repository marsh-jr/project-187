using Godot;
using System.Collections.Generic;

namespace Project187
{
	public partial class HUD : CanvasLayer
	{
		private Label _hpLabel;
		private VBoxContainer _energyContainer;
		private Player _player;
		private AttackManager _attackManager;

		// Cached label references keyed by attack id
		private readonly Dictionary<string, Label> _energyLabels = new();

		public override void _Ready()
		{
			_hpLabel         = GetNodeOrNull<Label>("MarginContainer/VBox/HpLabel");
			_energyContainer = GetNodeOrNull<VBoxContainer>("MarginContainer/VBox/EnergyContainer");

			// Find player via group
			var players = GetTree().GetNodesInGroup("Player");
			if (players.Count > 0)
			{
				_player         = players[0] as Player;
				_attackManager  = _player?.GetNodeOrNull<AttackManager>("AttackManager");
				BuildEnergyLabels();
			}
		}

		private void BuildEnergyLabels()
		{
			if (_attackManager == null || _energyContainer == null) return;

			foreach (var attack in _attackManager.GetChildren())
			{
				if (attack is not AttackInstance ai) continue;

				var label = new Label { Text = $"{ai.AttackId}: 0 / {ai.Data.EnergyThreshold}" };
				_energyContainer.AddChild(label);
				_energyLabels[ai.AttackId] = label;
			}
		}

		public override void _Process(double delta)
		{
			if (_player != null && _hpLabel != null)
				_hpLabel.Text = $"HP: {_player.CurrentHp:F0}";

			if (_attackManager == null) return;

			// Lazily build labels once attacks have been initialized
			if (_energyLabels.Count == 0)
				BuildEnergyLabels();

			foreach (var node in _attackManager.GetChildren())
			{
				if (node is not AttackInstance ai) continue;
				if (_energyLabels.TryGetValue(ai.AttackId, out var label))
					label.Text = $"{ai.AttackId}: {ai.CurrentEnergy:F0} / {ai.Data.EnergyThreshold:F0}";
			}
		}
	}
}
