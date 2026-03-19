using Godot;

namespace Project187
{
    /// Abstract base for all projectile-type attacks.
    /// Concrete attacks (e.g. MachineGun) inherit from this to express their type.
    public abstract partial class ProjectileAttackBase : AttackInstance
    {
        protected override void ExecuteFire()
        {
            if (Data.ProjectileScene == null)
            {
                GD.PushWarning($"{GetType().Name} '{AttackId}': no ProjectileScene assigned.");
                return;
            }

            var fireParams = BuildBaseFireParams();
            var stats      = GetComputedStats();
            var owner      = GetParent<AttackManager>().GetParent<Player>();

            for (int i = 0; i < fireParams.ProjectileCount; i++)
            {
                var proj = Data.ProjectileScene.Instantiate<ProjectileNode>();

                float baseAngle   = owner.Rotation;
                float totalSpread = fireParams.SpreadAngleDegrees;
                float angleOffset = fireParams.ProjectileCount > 1
                    ? Mathf.DegToRad(totalSpread / (fireParams.ProjectileCount - 1) * i - totalSpread / 2f)
                    : 0f;

                Vector2 direction = Vector2.Right.Rotated(baseAngle + angleOffset);

                proj.Initialize(
                    ownerAttack:  this,
                    direction:    direction,
                    speed:        stats.ProjectileSpeed * fireParams.SpeedMultiplier,
                    damage:       stats.BaseDamage * fireParams.DamageMultiplier,
                    isPiercing:   fireParams.IsPiercing,
                    isHoming:     fireParams.IsHoming,
                    bounces:      fireParams.RicochetBounces
                );

                proj.GlobalPosition = owner.GlobalPosition;

                var container = ProjectileContainer ?? GetTree().Root;
                container.AddChild(proj);
            }
        }
    }
}
