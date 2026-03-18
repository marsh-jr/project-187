using Godot;

namespace Project187
{
    public partial class OnKillEnergyGenerator : EnergyGeneratorBase
    {
        private AttackInstance _watchedAttack;

        public override void Initialize(AttackInstance target, EnergyGeneratorData config, AttackManager manager)
        {
            TargetAttack = target;
            Config = config;

            if (config is OnKillGeneratorData onKillData)
            {
                _watchedAttack = manager.GetAttackById(onKillData.SourceAttackId);
                if (_watchedAttack != null)
                    _watchedAttack.EnemyKilled += OnSourceKill;
                else
                    GD.PushWarning($"OnKillEnergyGenerator: attack '{onKillData.SourceAttackId}' not found.");
            }
        }

        public override void _ExitTree()
        {
            if (_watchedAttack != null)
                _watchedAttack.EnemyKilled -= OnSourceKill;
        }

        private void OnSourceKill(AttackInstance source, Node enemy)
        {
            TargetAttack?.AddEnergy(Config.EnergyPerEvent);
        }
    }
}
