using Godot;

namespace Project187
{
	[GlobalClass]
	public partial class EnemyStats : Resource
	{
		[Export] public float MaxHp    { get; set; } = 30f;
		[Export] public float Speed    { get; set; } = 80f;
		[Export] public float ContactDamage { get; set; } = 5f;
	}
}
