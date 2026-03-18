using Godot;

namespace Project187
{
    public partial class OnHitEnergyGenerator : EnergyGeneratorBase
    {
        private AttackInstance _watchedAttack;

        public override void Initialize(AttackInstance target, EnergyGeneratorData config, AttackManager manager)
        {
            TargetAttack = target;
            Config = config;

            if (config is OnHitGeneratorData onHitData)
            {
                _watchedAttack = manager.GetAttackById(onHitData.SourceAttackId);
                if (_watchedAttack != null)
                    _watchedAttack.EnemyHit += OnSourceHit;
                else
                    GD.PushWarning($"OnHitEnergyGenerator: attack '{onHitData.SourceAttackId}' not found.");
            }
        }

        public override void _ExitTree()
        {
            if (_watchedAttack != null)
                _watchedAttack.EnemyHit -= OnSourceHit;
        }

        private void OnSourceHit(AttackInstance source, Node enemy, float damage)
        {
            TargetAttack?.AddEnergy(Config.EnergyPerEvent);
        }
    }
}
