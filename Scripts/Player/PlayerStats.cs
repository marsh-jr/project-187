using Godot;
using Godot.Collections;

namespace Project187
{
    [GlobalClass]
    public partial class PlayerStats : Resource
    {
        [Export] public float MaxHp           { get; set; } = 100f;
        [Export] public float Speed           { get; set; } = 200f;
        [Export] public Array<AttackData> StartingAttacks { get; set; } = new();
    }
}
