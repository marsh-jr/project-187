using Godot;

namespace Project187
{
	[GlobalClass]
	public partial class BasicEnemy : CharacterBody2D
	{
		[Export] public EnemyStats Stats { get; set; }

		public float CurrentHp { get; private set; }

		private Player _target;
		private float _contactCooldown = 0f;
		private const float ContactCooldownDuration = 0.5f;

		public override void _Ready()
		{
			CurrentHp = Stats?.MaxHp ?? 30f;
			MotionMode = MotionModeEnum.Floating;
			AddToGroup("Enemies");

			// Find player via group — robust to scene order
			var players = GetTree().GetNodesInGroup("Player");
			if (players.Count > 0)
				_target = players[0] as Player;
		}

		public override void _PhysicsProcess(double delta)
		{
			_contactCooldown -= (float)delta;

			if (_target == null)
			{
				var players = GetTree().GetNodesInGroup("Player");
				if (players.Count > 0) _target = players[0] as Player;
				return;
			}

			Vector2 dir = (_target.GlobalPosition - GlobalPosition).Normalized();
			Velocity = dir * (Stats?.Speed ?? 80f);
			MoveAndSlide();

			if (_contactCooldown <= 0f)
			{
				for (int i = 0; i < GetSlideCollisionCount(); i++)
				{
					if (GetSlideCollision(i).GetCollider() is Player player)
					{
						player.TakeDamage(Stats?.ContactDamage ?? 5f);
						_contactCooldown = ContactCooldownDuration;
						break;
					}
				}
			}
		}

		public void TakeDamage(float amount)
		{
			CurrentHp -= amount;
			if (CurrentHp <= 0f)
				Die();
		}

		private void Die()
		{
			QueueFree();
		}
	}
}
