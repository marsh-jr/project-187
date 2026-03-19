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

			float baseAngle   = GetNearestEnemyDirection(spawnPosition).Angle();
			float totalSpread = fireParams.SpreadAngleDegrees;
			bool  radial      = Data.SpreadMode == ProjectileSpreadMode.Radial;
			float lifetime    = Data.ProjectileDuration > 0f ? Data.ProjectileDuration : ProjectileNode.DefaultMaxLifetime;

			for (int i = 0; i < fireParams.ProjectileCount; i++)
			{
				var proj = Data.ProjectileScene.Instantiate<ProjectileNode>();

				float angleOffset;
				if (radial)
				{
					// Evenly spaced ring — ignore nearest-enemy direction
					angleOffset = Mathf.DegToRad(360f / fireParams.ProjectileCount * i);
				}
				else
				{
					angleOffset = fireParams.ProjectileCount > 1
						? Mathf.DegToRad(totalSpread / (fireParams.ProjectileCount - 1) * i - totalSpread / 2f)
						: 0f;
				}

				Vector2 direction = radial
					? Vector2.Right.Rotated(angleOffset)
					: Vector2.Right.Rotated(baseAngle + angleOffset);

				proj.Initialize(
					ownerAttack:  this,
					direction:    direction,
					speed:        stats.ProjectileSpeed * fireParams.SpeedMultiplier,
					damage:       stats.BaseDamage * efficiency * fireParams.DamageMultiplier,
					pierceCount:  fireParams.PierceCount,
					isHoming:     fireParams.IsHoming,
					bounces:      fireParams.RicochetBounces,
					maxLifetime:  lifetime
				);

				proj.GlobalPosition = spawnPosition;

				var container = ProjectileContainer ?? GetTree().Root;
				container.AddChild(proj);
			}
		}
	}
}
