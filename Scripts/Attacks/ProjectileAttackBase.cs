using Godot;

namespace Project187
{
	/// Abstract base for all projectile-type attacks.
	/// Concrete attacks (e.g. MachineGun) inherit from this to express their type.
	public abstract partial class ProjectileAttackBase : AttackInstance
	{
		protected override void ExecuteFire(float efficiency, Vector2 spawnPosition)
		{
			if (Data.ProjectileScene == null)
			{
				GD.PushWarning($"{GetType().Name} '{AttackId}': no ProjectileScene assigned.");
				return;
			}

			var fireParams = BuildBaseFireParams();
			var stats      = GetComputedStats();

			// Aim toward the nearest enemy; fall back to player rotation if none exist.
			float baseAngle   = GetNearestEnemyDirection(spawnPosition).Angle();
			float totalSpread = fireParams.SpreadAngleDegrees;

			for (int i = 0; i < fireParams.ProjectileCount; i++)
			{
				var proj = Data.ProjectileScene.Instantiate<ProjectileNode>();

				float angleOffset = fireParams.ProjectileCount > 1
					? Mathf.DegToRad(totalSpread / (fireParams.ProjectileCount - 1) * i - totalSpread / 2f)
					: 0f;

				Vector2 direction = Vector2.Right.Rotated(baseAngle + angleOffset);

				proj.Initialize(
					ownerAttack:  this,
					direction:    direction,
					speed:        stats.ProjectileSpeed * fireParams.SpeedMultiplier,
					damage:       stats.BaseDamage * efficiency * fireParams.DamageMultiplier,
					isPiercing:   fireParams.IsPiercing,
					isHoming:     fireParams.IsHoming,
					bounces:      fireParams.RicochetBounces
				);

				proj.GlobalPosition = spawnPosition;

				var container = ProjectileContainer ?? GetTree().Root;
				container.AddChild(proj);
			}
		}

		/// Returns a normalised direction from <paramref name="from"/> toward the nearest enemy.
		/// Falls back to the player's facing direction if no enemies are present.
		private Vector2 GetNearestEnemyDirection(Vector2 from)
		{
			var enemies = GetTree().GetNodesInGroup("Enemies");
			Node2D nearest = null;
			float nearestDist = float.MaxValue;

			foreach (var node in enemies)
			{
				if (node is not Node2D e) continue;
				float d = from.DistanceSquaredTo(e.GlobalPosition);
				if (d < nearestDist) { nearestDist = d; nearest = e; }
			}

			return nearest != null
				? (nearest.GlobalPosition - from).Normalized()
				: Vector2.Right.Rotated(OwnerPlayer.Rotation);
		}
	}
}
