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
			float speed       = stats.ProjectileSpeed * fireParams.SpeedMultiplier;
			float damage      = stats.BaseDamage * efficiency * fireParams.DamageMultiplier;

			for (int i = 0; i < fireParams.ProjectileCount; i++)
			{
				float angleOffset;
				if (radial)
				{
					// Flak / radial ring: keep evenly spaced — that's the intended geometry
					angleOffset = Mathf.DegToRad(360f / fireParams.ProjectileCount * i);
				}
				else
				{
					// FanAim (MachineGun, Shotgun, …): randomise within the spread arc
					angleOffset = totalSpread > 0f
						? Mathf.DegToRad((GD.Randf() - 0.5f) * totalSpread)
						: 0f;
				}

				// Capture loop variables so the lambda closure is correct
				int    shotIndex       = i;
				float  capturedOffset  = angleOffset;
				int    pierceCount     = fireParams.PierceCount;
				bool   isHoming        = fireParams.IsHoming;
				int    bounces         = fireParams.RicochetBounces;

				if (Data.BurstInterval <= 0f || i == 0)
				{
					SpawnProjectile(baseAngle, capturedOffset, radial, speed, damage,
						pierceCount, isHoming, bounces, lifetime, spawnPosition);
				}
				else
				{
					float delay = Data.BurstInterval * shotIndex;
					var timer = GetTree().CreateTimer(delay, false);
					timer.Timeout += () =>
					{
						if (!IsInstanceValid(this)) return;
						SpawnProjectile(baseAngle, capturedOffset, radial, speed, damage,
							pierceCount, isHoming, bounces, lifetime, spawnPosition);
					};
				}
			}
		}

		private void SpawnProjectile(
			float baseAngle, float angleOffset, bool radial,
			float speed, float damage, int pierceCount,
			bool isHoming, int bounces, float lifetime,
			Vector2 spawnPosition)
		{
			if (Data.ProjectileScene == null || !IsInstanceValid(this)) return;

			Vector2 direction = radial
				? Vector2.Right.Rotated(angleOffset)
				: Vector2.Right.Rotated(baseAngle + angleOffset);

			var proj = Data.ProjectileScene.Instantiate<ProjectileNode>();
			proj.Initialize(
				ownerAttack:  this,
				direction:    direction,
				speed:        speed,
				damage:       damage,
				pierceCount:  pierceCount,
				isHoming:     isHoming,
				bounces:      bounces,
				maxLifetime:  lifetime
			);
			proj.GlobalPosition = spawnPosition;

			var container = ProjectileContainer ?? GetTree().Root;
			if (IsInstanceValid(container))
				container.AddChild(proj);
		}
	}
}
