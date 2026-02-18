using Godot;
using System;

public partial class DamageComponent : Node
{

    [Export] public int DamageAmount;
    public void _OnTakingDamage(int set)
    {
    }
}
