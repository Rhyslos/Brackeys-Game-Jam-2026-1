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
    [Export] public float PreRiseWarningTime = 2.0f;

    [ExportCategory("Speeds & Ramping")]
    [Export] public float RiseSpeed = 0.6f;
    [Export] public float RiseAcceleration = 0.8f;
    [Export] public float RiseDeceleration = 0.8f;
    [Export] public float LowerSpeed = 0.7f;
    [Export] public float LowerDeceleration = 0.8f;
    [Export(PropertyHint.Range, "0.0, 1.0")] public float EventRampPercentage = 0.15f;

    [ExportCategory("Effects")]
    [Export] public AudioStreamPlayer RumbleAudioPlayer;
    [Export] public float BaseShakeIntensity = 0.02f;

    // state variables
    private enum LakeState { High, Lowering, Low, PreRising, Rising }
    private LakeState _currentState = LakeState.High;
    private float _timer;
    private float _currentRiseSpeed;
    private float _currentLowerSpeed;

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
                _currentRiseSpeed = 0.0f;
            }
            else if (keyEvent.Keycode == Key.L)
            {
                _currentState = LakeState.Lowering;
                _currentLowerSpeed = LowerSpeed;
            }
        }
    }

    // loop functions
    public override void _PhysicsProcess(double delta)
    {
        float targetShake = 0f;
        float totalLakeDepth = Mathf.Abs(MaxWaterLevel - MinWaterLevel);
        float rampDistance = totalLakeDepth * EventRampPercentage;

        switch (_currentState)
        {
            case LakeState.High:
                _timer -= (float)delta;
                if (_timer <= 0)
                {
                    _currentState = LakeState.Lowering;
                    _currentLowerSpeed = LowerSpeed;
                }
                break;

            case LakeState.Lowering:
                float distanceToBottom = Mathf.Abs(GlobalPosition.Y - MinWaterLevel);
                
                if (distanceToBottom < rampDistance)
                {
                    _currentLowerSpeed = Mathf.Max(0.1f, _currentLowerSpeed - LowerDeceleration * (float)delta);
                    targetShake = BaseShakeIntensity * (distanceToBottom / rampDistance);
                }
                else
                {
                    targetShake = BaseShakeIntensity;
                }
                
                MoveLake(MinWaterLevel, _currentLowerSpeed, (float)delta);
                if (Mathf.IsEqualApprox(GlobalPosition.Y, MinWaterLevel))
                {
                    StartLowState();
                }
                break;

            case LakeState.Low:
                _timer -= (float)delta;
                if (_timer <= 0)
                {
                    _currentState = LakeState.PreRising;
                    _timer = PreRiseWarningTime;
                }
                break;

            case LakeState.PreRising:
                targetShake = BaseShakeIntensity;
                _timer -= (float)delta;
                if (_timer <= 0)
                {
                    _currentState = LakeState.Rising;
                    _currentRiseSpeed = 0.0f;
                }
                break;

            case LakeState.Rising:
                float distanceToTop = Mathf.Abs(MaxWaterLevel - GlobalPosition.Y);

                if (distanceToTop < rampDistance)
                {
                    _currentRiseSpeed = Mathf.Max(0.1f, _currentRiseSpeed - RiseDeceleration * (float)delta);
                    targetShake = BaseShakeIntensity * (distanceToTop / rampDistance);
                }
                else
                {
                    _currentRiseSpeed = Mathf.Min(RiseSpeed, _currentRiseSpeed + RiseAcceleration * (float)delta);
                    targetShake = BaseShakeIntensity;
                }

                MoveLake(MaxWaterLevel, _currentRiseSpeed, (float)delta);
                CheckForPlayer(); 
                
                if (Mathf.IsEqualApprox(GlobalPosition.Y, MaxWaterLevel))
                {
                    StartHighState();
                }
                break;
        }

        UpdateEventEffects(targetShake);
    }

    // movement functions
    private void MoveLake(float targetY, float speed, float delta)
    {
        Vector3 pos = GlobalPosition;
        pos.Y = Mathf.MoveToward(pos.Y, targetY, speed * delta);
        GlobalPosition = pos;
    }

    // effect functions
    private void UpdateEventEffects(float shakeAmount)
    {
        if (RumbleAudioPlayer != null)
        {
            if (shakeAmount > 0)
            {
                if (!RumbleAudioPlayer.Playing)
                {
                    RumbleAudioPlayer.Play();
                }
                
                float volumeRatio = Mathf.Max(0.001f, shakeAmount / BaseShakeIntensity);
                RumbleAudioPlayer.VolumeDb = Mathf.LinearToDb(volumeRatio);
            }
            else
            {
                if (RumbleAudioPlayer.Playing)
                {
                    RumbleAudioPlayer.Stop();
                }
            }
        }

        var players = GetTree().GetNodesInGroup("player");
        foreach (Node node in players)
        {
            if (node is Player player)
            {
                player.SetCameraShake(shakeAmount);
            }
        }
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
                player.TriggerGameOver(false, "You dissolved in the acid lake.");
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
                    player.TriggerGameOver(false, "You dissolved in the acid lake.");
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