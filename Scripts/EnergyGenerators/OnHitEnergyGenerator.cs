namespace Project187
{
    public partial class OnHitEnergyGenerator : EnergyObserver
    {
        protected override void Subscribe(AttackInstance source)
            => source.EnemyHit += OnSourceHit;

        protected override void Unsubscribe(AttackInstance source)
            => source.EnemyHit -= OnSourceHit;

        private void OnSourceHit(AttackInstance source, Godot.Node enemy, float damage)
            => TargetAttack?.AddEnergy(Config.EnergyPerEvent);
    }
}
