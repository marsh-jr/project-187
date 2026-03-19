using Godot;

namespace Project187
{
	/// Observer that triggers its target attack whenever the parent attack critically hits.
	/// Subscribes to the parent AttackInstance's EnemyCritHit signal.
	public partial class OnCritEnergyGenerator : EnergyObserver
	{
		protected override void Subscribe(AttackInstance source)
			=> source.EnemyCritHit += OnSourceCrit;

		protected override void Unsubscribe(AttackInstance source)
			=> source.EnemyCritHit -= OnSourceCrit;

		private void OnSourceCrit(AttackInstance _, Node enemy, float __)
		{
			var pos = (enemy as Node2D)?.GlobalPosition;
			TargetAttack?.Trigger(Efficiency, pos);
		}
	}

	/// On-crit processor. Efficiency 100–115%.
	public partial class PreciseCombatProcessor : OnCritEnergyGenerator { }
}
