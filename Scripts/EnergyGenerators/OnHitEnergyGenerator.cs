namespace Project187
{
	public partial class OnHitEnergyGenerator : EnergyObserver
	{
		protected override void Subscribe(AttackInstance source)
			=> source.EnemyHit += OnSourceHit;

		protected override void Unsubscribe(AttackInstance source)
			=> source.EnemyHit -= OnSourceHit;

		private void OnSourceHit(AttackInstance source, Godot.Node enemy, float damage)
		{
			var pos = (enemy as Godot.Node2D)?.GlobalPosition;
			TargetAttack?.Trigger(Efficiency, pos);
		}
	}

	/// On-hit processor. Efficiency 30–40%.
	public partial class AggressiveCombatProcessor : OnHitEnergyGenerator { }
}
