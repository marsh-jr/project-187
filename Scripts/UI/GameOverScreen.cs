using Godot;

namespace Project187
{
	public partial class GameOverScreen : CanvasLayer
	{
		public override void _Ready()
		{
			Hide();
			GetNode<Button>("Root/Center/VBox/RestartButton").Pressed += Restart;
		}

		public void ShowScreen(string title = "GAME OVER")
		{
			GetNode<Label>("Root/Center/VBox/Title").Text = title;
			Show();
		}

		public override void _UnhandledInput(InputEvent @event)
		{
			if (!Visible) return;
			if (@event is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.O)
				Restart();
		}

		private void Restart()
		{
			GetTree().ReloadCurrentScene();
		}
	}
}
