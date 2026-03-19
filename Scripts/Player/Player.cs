using Godot;

namespace Project187
{
	public partial class Player : CharacterBody2D
	{
		[Export] public PlayerStats Stats { get; set; }

		public float CurrentHp { get; private set; }
		public AttackManager AttackManager { get; private set; }

		public override void _Ready()
		{
			AddToGroup("Player");
			MotionMode = MotionModeEnum.Floating;
			CurrentHp = Stats?.MaxHp ?? 100f;
			AttackManager = GetNode<AttackManager>("AttackManager");

			if (Stats?.StartingGenerators?.Count > 0)
				AttackManager.Initialize(Stats.StartingGenerators);
		}

		public override void _PhysicsProcess(double delta)
		{
			Vector2 input = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
			Velocity = input * (Stats?.Speed ?? 200f);
			MoveAndSlide();
		}

		[Signal] public delegate void DiedEventHandler();

		public void TakeDamage(float amount)
		{
			if (!IsPhysicsProcessing()) return; // already dead
			CurrentHp -= amount;
			if (CurrentHp <= 0f)
				Die();
		}

		private void Die()
		{
			CurrentHp = 0f;
			SetPhysicsProcess(false);
			EmitSignal(SignalName.Died);
		}
	}
}
