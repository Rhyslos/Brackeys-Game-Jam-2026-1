using Godot;

public partial class DeathZone : Area3D
{
    // exported variables
    [ExportCategory("Lake Levels")]
    [Export] public float MaxWaterLevel = 0.0f;
    [Export] public float MinWaterLevel = -4.26f;

    [ExportCategory("Timings")]
    [Export] public float MinHighTime = 20.0f;
    [Export] public float MaxHighTime = 45.0f;
    [Export] public float MinLowTime = 10.0f;
    [Export] public float MaxLowTime = 20.0f;

    [ExportCategory("Speeds")]
    [Export] public float RiseSpeed = 0.6f;
    [Export] public float LowerSpeed = 0.7f;

    // state variables
    private enum LakeState { High, Lowering, Low, Rising }
    private LakeState _currentState = LakeState.High;
    private float _timer;

    // initialization functions
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        
        Vector3 startPos = GlobalPosition;
        startPos.Y = MaxWaterLevel;
        GlobalPosition = startPos;

        StartHighState();
    }

    // input functions
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            if (keyEvent.Keycode == Key.O)
            {
                _currentState = LakeState.Rising;
            }
            else if (keyEvent.Keycode == Key.L)
            {
                _currentState = LakeState.Lowering;
            }
        }
    }

    // loop functions
    public override void _PhysicsProcess(double delta)
    {
        switch (_currentState)
        {
            case LakeState.High:
                _timer -= (float)delta;
                if (_timer <= 0)
                {
                    _currentState = LakeState.Lowering;
                }
                break;

            case LakeState.Lowering:
                MoveLake(MinWaterLevel, LowerSpeed, (float)delta);
                if (Mathf.IsEqualApprox(GlobalPosition.Y, MinWaterLevel))
                {
                    StartLowState();
                }
                break;

            case LakeState.Low:
                _timer -= (float)delta;
                if (_timer <= 0)
                {
                    _currentState = LakeState.Rising;
                }
                break;

            case LakeState.Rising:
                MoveLake(MaxWaterLevel, RiseSpeed, (float)delta);
                CheckForPlayer(); 
                
                if (Mathf.IsEqualApprox(GlobalPosition.Y, MaxWaterLevel))
                {
                    StartHighState();
                }
                break;
        }
    }

    // movement functions
    private void MoveLake(float targetY, float speed, float delta)
    {
        Vector3 pos = GlobalPosition;
        pos.Y = Mathf.MoveToward(pos.Y, targetY, speed * delta);
        GlobalPosition = pos;
    }

    // state functions
    private void StartHighState()
    {
        _currentState = LakeState.High;
        _timer = (float)GD.RandRange(MinHighTime, MaxHighTime);
    }

    private void StartLowState()
    {
        _currentState = LakeState.Low;
        _timer = (float)GD.RandRange(MinLowTime, MaxLowTime);
    }

    // trigger functions
    private void OnBodyEntered(Node3D body)
    {
        if (body.Name == "Player" || body.IsInGroup("player"))
        {
            if (body is Player player)
            {
                player.QuitGame();
            }
            TriggerGameOver();
        }
    }

    private void CheckForPlayer()
    {
        Godot.Collections.Array<Node3D> overlappingBodies = GetOverlappingBodies();
        foreach (Node3D body in overlappingBodies)
        {
            if (body.Name == "Player" || body.IsInGroup("player"))
            {
                if (body is Player player)
                {
                    player.QuitGame();
                }
                TriggerGameOver();
                break;
            }
        }
    }

    public void TriggerGameOver()
    {
        GD.Print("YOU DIED - MELTED IN ACID");
    }
}