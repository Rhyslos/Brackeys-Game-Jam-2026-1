using Godot;
using System;

[GlobalClass]
public partial class HealthComponent : Node
{
	[Export] public int HealthAmount = 10;
    private int CurrentHealth = 10;

    public override void _Ready()
    {
        
    }
    
    public void _CalculateNewHealth()
    {
        
    }
	
}
