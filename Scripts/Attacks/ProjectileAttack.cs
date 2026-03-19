using Godot;

namespace Project187
{
	public partial class ProjectileAttack : AttackInstance
	{
		protected override void ExecuteFire(float efficiency, Vector2 spawnPosition)
		{
			if (Data.ProjectileScene == null)
			{
				GD.PushWarning($"ProjectileAttack '{AttackId}': no ProjectileScene assigned.");
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
