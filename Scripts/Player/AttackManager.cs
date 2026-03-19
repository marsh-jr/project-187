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
			// Phase 1: instantiate all AttackInstances and register them by ID
			foreach (var data in startingAttacks)
			{
				if (data == null) continue;

				var instance = data.CreateRuntimeInstance();
				if (instance == null)
				{
					GD.PushWarning($"AttackManager: could not instantiate attack '{data.AttackId}' — ImplementingClass not set or invalid.");
					continue;
				}

				AddChild(instance);
				instance.Initialize(data, this);
				_activeAttacks.Add(instance);

				if (!string.IsNullOrEmpty(data.AttackId))
					_attackById[data.AttackId] = instance;
			}

			// Phase 2: create generators (after all attacks registered so cross-refs resolve)
			foreach (var data in startingAttacks)
			{
				if (data == null) continue;
				if (!_attackById.TryGetValue(data.AttackId, out var targetAttack)) continue;

				foreach (var genConfig in data.GeneratorConfigs)
				{
					if (genConfig == null) continue;

					var node = genConfig.CreateRuntimeInstance();
					if (node == null)
					{
						GD.PushWarning($"AttackManager: could not instantiate generator for '{data.AttackId}' — ImplementingClass not set or invalid.");
						continue;
					}

					AddChild(node);

					if (node is EnergyGenerator gen)
						gen.Initialize(targetAttack, genConfig, this);
					else if (node is EnergyObserver obs)
						obs.Initialize(targetAttack, genConfig, this);
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
						 || IsCompatible(attack, adaptation.Category);

			if (!canEquip) return false;

			attack.Adaptations.Add(adaptation);
			return true;
		}

		private static bool IsCompatible(AttackInstance attack, AdaptationCategory category) => category switch
		{
			AdaptationCategory.Projectile => attack is ProjectileAttackBase,
			AdaptationCategory.Area       => attack is AreaAttackBase,
			AdaptationCategory.Beam       => attack is BeamAttackBase,
			AdaptationCategory.Melee      => attack is MeleeAttackBase,
			_                             => false,
		};
	}
}
