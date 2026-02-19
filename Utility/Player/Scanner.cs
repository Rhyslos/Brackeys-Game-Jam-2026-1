using Godot;

public partial class Scanner : Node3D
{
    [Export] public float Damage = 10.0f;
    
    private RayCast3D _rayCast;

    public override void _Ready()
    {
        _rayCast = GetNode<RayCast3D>("RayCast3D"); 
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionPressed("Fire"))
        {
            if (_rayCast.IsColliding())
            {
                var collider = _rayCast.GetCollider();

                if (collider is OreDeposit deposit)
                {
                    deposit.Mine(Damage);
                }
            }
        }
    }
}