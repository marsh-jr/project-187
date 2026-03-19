using Godot;
using Godot.Collections;
using System.Collections.Generic;

namespace Project187
{
	public partial class AttackManager : Node
	{
		private readonly List<AttackInstance> _activeAttacks = new();
		private readonly System.Collections.Generic.Dictionary<string, AttackInstance> _attackById = new();

		/// Initialize from a list of root generators.
		/// Each generator creates and owns its attack chain as child nodes — no two-pass needed.
		public void Initialize(Array<EnergyGeneratorData> startingGenerators)
		{
			foreach (var genConfig in startingGenerators)
			{
				if (genConfig == null) continue;

				var node = genConfig.CreateRuntimeInstance();
				if (node == null)
				{
					GD.PushWarning($"AttackManager: could not instantiate root generator — ImplementingClass not set or invalid.");
					continue;
				}

				AddChild(node);

				if (node is EnergyGeneratorBase gen)
					gen.Initialize(genConfig, this);
				else
					GD.PushWarning($"AttackManager: root entry '{genConfig.ImplementingClass}' is not an EnergyGeneratorBase.");
			}
		}

		/// Register an attack for adaptation equipping and UI queries.
		/// Called by EnergyGeneratorBase.CreateAndInitChildAttack() for every attack in the chain.
		public void RegisterAttack(AttackInstance attack)
		{
			if (attack == null) return;
			_activeAttacks.Add(attack);
			if (!string.IsNullOrEmpty(attack.AttackId))
				_attackById[attack.AttackId] = attack;
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
