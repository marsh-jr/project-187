using Godot;

namespace Project187
{
    public partial class AreaAttack : AttackInstance
    {
        protected override void ExecuteFire()
        {
            if (Data.ProjectileScene == null)
            {
                GD.PushWarning($"AreaAttack '{AttackId}': no ProjectileScene assigned.");
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
