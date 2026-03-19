using Godot;
using Godot.Collections;

namespace Project187
{
	[GlobalClass]
	public partial class PlayerStats : Resource
	{
		[Export] public float MaxHp  { get; set; } = 100f;
		[Export] public float Speed  { get; set; } = 200f;

		/// Root generators — each owns the first attack in its energy chain.
		/// Assign TimedGeneratorData (or other root generator) entries here.
		[Export] public Array<EnergyGeneratorData> StartingGenerators { get; set; } = new();
	}
}
