using Godot;

namespace Project187
{
    public partial class TimedEnergyGenerator : EnergyGenerator
    {
        private float _energyPerSecond;

        public override void Initialize(AttackInstance target, EnergyGeneratorData config, AttackManager manager)
        {
            TargetAttack = target;
            Config = config;
            _energyPerSecond = config is TimedGeneratorData timed ? timed.EnergyPerSecond : config.EnergyPerEvent;
        }

        public override void _Process(double delta)
        {
            if (TargetAttack == null) return;
            TargetAttack.AddEnergy(_energyPerSecond * (float)delta);
        }
    }
}
