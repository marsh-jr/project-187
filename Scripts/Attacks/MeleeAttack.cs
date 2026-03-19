using Godot;

namespace Project187
{
	/// Placeholder — performs an arc sweep around the player.
	/// Full implementation in Phase 7.
	public partial class MeleeAttack : AttackInstance
	{
		protected override void ExecuteFire(float efficiency, Vector2 spawnPosition)
		{
			var stats = GetComputedStats();
			var owner = OwnerPlayer;
			GD.Print($"MeleeAttack '{AttackId}' fired from {owner.GlobalPosition} with range {stats.MeleeRange}");
			// TODO Phase 7: use OverlapCircle / ShapeCast2D
		}
	}
}
