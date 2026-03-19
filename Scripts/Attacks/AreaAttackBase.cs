using Godot;

namespace Project187
{
    /// Abstract base for all area-type attacks.
    /// Concrete attacks (e.g. ShockPulse) inherit from this to express their type.
    public abstract partial class AreaAttackBase : AttackInstance
    {
        protected override void ExecuteFire()
        {
            if (Data.ProjectileScene == null)
            {
                GD.PushWarning($"{GetType().Name} '{AttackId}': no ProjectileScene assigned.");
                return;
            }

            var stats = GetComputedStats();
            var owner = GetParent<AttackManager>().GetParent<Player>();

            var area = Data.ProjectileScene.Instantiate<AreaEffectNode>();
            area.Initialize(this, stats);
            area.GlobalPosition = owner.GlobalPosition;

            var container = ProjectileContainer ?? GetTree().Root;
            container.AddChild(area);
        }
    }
}
