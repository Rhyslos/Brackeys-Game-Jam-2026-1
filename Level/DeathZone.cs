using Godot;

public partial class DeathZone : Area3D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body.Name == "Player" || body.IsInGroup("player"))
        {
            TriggerGameOver();
        }
    }

    public void TriggerGameOver()
    {
        GD.Print("YOU DIED");
    }
}