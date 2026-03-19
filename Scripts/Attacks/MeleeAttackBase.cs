using Godot;

namespace Project187
{
	/// Abstract base for all melee-type attacks. Full implementation in Phase 7.
	public abstract partial class MeleeAttackBase : AttackInstance
	{
		protected override void ExecuteFire()
		{
			var stats = GetComputedStats();
			var owner = OwnerPlayer;
			GD.Print($"{GetType().Name} '{AttackId}' fired from {owner.GlobalPosition} with range {stats.MeleeRange}");
			// TODO Phase 7: use OverlapCircle / ShapeCast2D
		}
	}
}
