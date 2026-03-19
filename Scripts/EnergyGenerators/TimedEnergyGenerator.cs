namespace Project187
{
	/// Triggers its attack on a fixed time interval at a configured efficiency.
	/// Formerly TimedEnergyGenerator — renamed to EnergyCore.
	public partial class EnergyCore : EnergyGenerator
	{
		private float _interval;
		private float _efficiency;
		private float _timer = 0f;

		public override void Initialize(EnergyGeneratorData config, AttackManager manager)
		{
			Config       = config;
			_interval    = config is EnergyCoreData core ? core.TriggerInterval : 1.0f;
			_efficiency  = config.Efficiency;

			ChildAttack  = CreateAndInitChildAttack(config.Attack, manager);
			TargetAttack = ChildAttack;
		}

		public override void _Process(double delta)
		{
			if (TargetAttack == null) return;
			_timer += (float)delta;
			if (_timer >= _interval)
			{
				_timer -= _interval;
				TargetAttack.Trigger(_efficiency);
			}
		}
	}
}
