using Godot;

namespace Project187
{
    /// Abstract base for all area-type attacks.
    /// Concrete attacks (e.g. ShockPulse) inherit from this to express their type.
    public abstract partial class AreaAttackBase : AttackInstance
    {
        protected override void ExecuteFire(float efficiency, Vector2 spawnPosition)
        {
            if (Data.ProjectileScene == null)
            {
                GD.PushWarning($"{GetType().Name} '{AttackId}': no ProjectileScene assigned.");
                return;
            }

            var stats = GetComputedStats();

            // Scale damage by efficiency before the area effect node picks it up.
            stats.BaseDamage *= efficiency;

            var area = Data.ProjectileScene.Instantiate<AreaEffectNode>();
            area.Initialize(this, stats);
            area.GlobalPosition = spawnPosition;

            var container = ProjectileContainer ?? GetTree().Root;
            container.AddChild(area);
        }
    }
}
