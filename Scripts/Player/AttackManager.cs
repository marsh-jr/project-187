using Godot;
using Godot.Collections;
using System.Collections.Generic;

namespace Project187
{
	public partial class AttackManager : Node
	{
		private readonly List<AttackInstance> _activeAttacks = new();
		private readonly System.Collections.Generic.Dictionary<string, AttackInstance> _attackById = new();

		public void Initialize(Array<AttackData> startingAttacks)
		{
			// Phase 1: instantiate all AttackInstances and register them
			foreach (var data in startingAttacks)
			{
				if (data == null) continue;

				AttackInstance instance = data.Type switch
				{
					AttackType.Projectile => new ProjectileAttack(),
					AttackType.Area       => new AreaAttack(),
					AttackType.Beam       => new BeamAttack(),
					AttackType.Melee      => new MeleeAttack(),
					_                     => new ProjectileAttack(),
				};

				AddChild(instance);
				instance.Initialize(data, this);
				_activeAttacks.Add(instance);

				if (!string.IsNullOrEmpty(data.AttackId))
					_attackById[data.AttackId] = instance;
			}

			// Phase 2: create generators (after all attacks are registered so cross-refs resolve)
			foreach (var data in startingAttacks)
			{
				if (data == null) continue;
				if (!_attackById.TryGetValue(data.AttackId, out var targetAttack)) continue;

				foreach (var genConfig in data.GeneratorConfigs)
				{
					if (genConfig == null) continue;

					EnergyGeneratorBase gen = genConfig switch
					{
						TimedGeneratorData  => new TimedEnergyGenerator(),
						OnHitGeneratorData  => new OnHitEnergyGenerator(),
						OnKillGeneratorData => new OnKillEnergyGenerator(),
						_                   => new TimedEnergyGenerator(),
					};

					AddChild(gen);
					gen.Initialize(targetAttack, genConfig, this);
				}
			}
		}

		public AttackInstance GetAttackById(string id)
		{
			if (_attackById.TryGetValue(id, out var attack))
				return attack;

			GD.PushWarning($"AttackManager: no attack with id '{id}'");
			return null;
		}

		/// Equip an adaptation to a specific attack (used by upgrade UI and debug).
		public bool TryEquipAdaptation(string attackId, IAdaptation adaptation)
		{
			if (!_attackById.TryGetValue(attackId, out var attack)) return false;
			if (attack.Adaptations.Count >= attack.AdaptationSlotCount) return false;

			bool canEquip = adaptation.Category == AdaptationCategory.Universal
						 || (int)adaptation.Category == (int)attack.Data.Type;

			if (!canEquip) return false;

			attack.Adaptations.Add(adaptation);
			return true;
		}
	}
}
