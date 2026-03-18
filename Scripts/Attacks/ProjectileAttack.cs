using Godot;

namespace Project187
{
    public partial class ProjectileAttack : AttackInstance
    {
        protected override void ExecuteFire()
        {
            if (Data.ProjectileScene == null)
            {
                GD.PushWarning($"ProjectileAttack '{AttackId}': no ProjectileScene assigned.");
                return;
            }

            var fireParams = BuildBaseFireParams();
            var stats      = GetComputedStats();
            var owner      = GetParent<AttackManager>().GetParent<Player>();

            for (int i = 0; i < fireParams.ProjectileCount; i++)
            {
                var proj = Data.ProjectileScene.Instantiate<ProjectileNode>();

                // Calculate spread direction
                float baseAngle   = owner.Rotation; // face direction (0 = right)
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
