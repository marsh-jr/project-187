using Godot;

namespace Project187
{
	/// Triggers its attack on a fixed time interval.
	/// Interval and efficiency are rolled from the data's min/max ranges once at Initialize().
	public partial class EnergyCore : EnergyGenerator
	{
		private float _interval;
		private float _efficiency;
		private float _timer = 0f;

		public override void Initialize(EnergyGeneratorData config, AttackManager manager)
		{
			Config = config;

			if (config is EnergyCoreData core)
			{
				_interval   = Mathf.Lerp(core.TriggerIntervalMin, core.TriggerIntervalMax, GD.Randf());
				_efficiency = Mathf.Lerp(core.EfficiencyMin,      core.EfficiencyMax,      GD.Randf());
			}
			else
			{
				_interval   = 1.0f;
				_efficiency = Mathf.Lerp(config.EfficiencyMin, config.EfficiencyMax, GD.Randf());
			}

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

	/// Fast timed core. 0.7–0.9 s interval, 70–85% efficiency.
	public partial class TurboCore    : EnergyCore { }

	/// Balanced timed core. 1.1–1.3 s interval, 105–120% efficiency.
	public partial class MilitaryCore : EnergyCore { }

	/// Heavy timed core. 1.8–2.1 s interval, 210–230% efficiency.
	public partial class HeavyCore    : EnergyCore { }
}
