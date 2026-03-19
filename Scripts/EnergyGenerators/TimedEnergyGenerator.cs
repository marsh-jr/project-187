namespace Project187
{
	public partial class TimedEnergyGenerator : EnergyGenerator
	{
		private float _energyPerSecond;

		public override void Initialize(EnergyGeneratorData config, AttackManager manager)
		{
			Config           = config;
			_energyPerSecond = config is TimedGeneratorData timed ? timed.EnergyPerSecond : config.EnergyPerEvent;

			// Create and own the attack this generator charges as a child node.
			ChildAttack  = CreateAndInitChildAttack(config.Attack, manager);
			TargetAttack = ChildAttack;
		}

		public override void _Process(double delta)
		{
			if (TargetAttack == null) return;
			TargetAttack.AddEnergy(_energyPerSecond * (float)delta);
		}
	}
}
