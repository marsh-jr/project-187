using Godot;
using Godot.Collections;

namespace Project187
{
	/// Abstract base for melee-type attacks.
	/// Uses an immediate physics shape query so no persistent Area2D node is needed.
	/// Both damage and hit range scale with efficiency.
	public abstract partial class MeleeAttackBase : AttackInstance
	{
		protected override void ExecuteFire(float efficiency, Vector2 spawnPosition)
		{
			var stats = GetComputedStats();
			float effectiveRange  = stats.MeleeRange * efficiency;
			float effectiveDamage = stats.BaseDamage  * efficiency;

			if (effectiveRange <= 0f) return;

			// Shape query — finds all enemy bodies within the radius instantly.
			var shapeParams = new PhysicsShapeQueryParameters2D();
			shapeParams.Shape         = new CircleShape2D { Radius = effectiveRange };
			shapeParams.Transform     = new Transform2D(0, spawnPosition);
			shapeParams.CollisionMask = 8; // layer 4 = enemy bodies

			var hits = GetWorld2D().DirectSpaceState.IntersectShape(shapeParams, maxResults: 64);

			// Arc filter: for attacks with AreaAngle < 360° only hit enemies in the cone.
			float halfAngle = Mathf.DegToRad(Data.AreaAngle * 0.5f);
			Vector2 aimDir  = GetNearestEnemyDirection(spawnPosition);

			foreach (Dictionary hit in hits)
			{
				if (hit["collider"].AsGodotObject() is not BasicEnemy enemy) continue;

				if (Data.AreaAngle < 360f)
				{
					Vector2 toEnemy = (enemy.GlobalPosition - spawnPosition).Normalized();
					float   angle   = Mathf.Abs(toEnemy.AngleTo(aimDir));
					if (angle > halfAngle) continue;
				}

				RegisterHit(enemy, effectiveDamage);
			}

			// Brief visual flash at the hit zone.
			SpawnMeleeVisual(spawnPosition, aimDir.Angle(), effectiveRange, Data.AreaAngle);
		}

		private void SpawnMeleeVisual(Vector2 position, float directionAngle, float range, float angleDeg)
		{
			var effect = new MeleeEffectNode();
			effect.Setup(range, angleDeg, directionAngle);
			effect.GlobalPosition = position;
			GetTree().CurrentScene.AddChild(effect);
		}
	}
}
