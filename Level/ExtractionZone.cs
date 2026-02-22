using Godot;

public partial class ExtractionZone : Area3D
{
    // state variables
    private Player _playerInZone;

    // initialization functions
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    // signal functions
    private void OnBodyEntered(Node3D body)
    {
        if (body is Player player)
        {
            _playerInZone = player;
        }
    }

    private void OnBodyExited(Node3D body)
    {
        if (body is Player player && _playerInZone == player)
        {
            _playerInZone = null;
        }
    }

    // loop functions
    public override void _Process(double delta)
    {
        if (_playerInZone != null && Input.IsActionJustPressed("Interact"))
        {
            if (_playerInZone.InventoryUI != null)
            {
                _playerInZone.InventoryUI.TryEscape();
            }
        }
    }
}