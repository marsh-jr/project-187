namespace Project187
{
    public partial class OnKillEnergyGenerator : EnergyObserver
    {
        protected override void Subscribe(AttackInstance source)
            => source.EnemyKilled += OnSourceKill;

        protected override void Unsubscribe(AttackInstance source)
            => source.EnemyKilled -= OnSourceKill;

        private void OnSourceKill(AttackInstance source, Godot.Node enemy)
            => TargetAttack?.AddEnergy(Config.EnergyPerEvent);
    }
}
