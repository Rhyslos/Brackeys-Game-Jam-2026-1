using Godot;

public partial class Player : RigidBody3D
{
    [ExportCategory("Movement")]
    [Export] public float Speed = 5.0f;
    [Export] public float SprintSpeed = 10.0f;
    [Export] public float JumpVelocity = 4.5f;

    [ExportCategory("Camera")]
    [Export(PropertyHint.Range, "60, 120")] public float BaseFov = 75.0f;
    [Export] public float MouseSensitivity = 0.003f;
    [Export] public Node3D CameraNode;

    [ExportCategory("Physics")]
    [Export] public RayCast3D FloorCheck;

    private float _targetYRotation;
    private bool _jumpRequested;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;

        AxisLockAngularX = true;
        AxisLockAngularY = true;
        AxisLockAngularZ = true;
        CanSleep = false;

        _targetYRotation = Rotation.Y;

        if (CameraNode == null)
        {
            CameraNode = GetNode<Camera3D>("Camera");
        }

        if (CameraNode is Camera3D cam)
        {
            cam.Fov = BaseFov;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            _targetYRotation -= mouseMotion.Relative.X * MouseSensitivity;

            if (CameraNode != null)
            {
                CameraNode.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);
                
                Vector3 camRot = CameraNode.Rotation;
                camRot.X = Mathf.Clamp(camRot.X, Mathf.DegToRad(-89f), Mathf.DegToRad(89f));
                CameraNode.Rotation = camRot;
            }
        }
        
        if (@event.IsActionPressed("Jump"))
        {
            _jumpRequested = true;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionPressed("Pause"))
        {
            QuitGame();
        }
    }

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        Transform3D newTransform = state.Transform;
        newTransform.Basis = Basis.FromEuler(new Vector3(0, _targetYRotation, 0));
        state.Transform = newTransform;

        Vector3 currentVelocity = state.LinearVelocity;
        
        bool isOnFloor = FloorCheck != null && FloorCheck.IsColliding();

        if (_jumpRequested)
        {
            GD.Print($"Jump Button Pressed! IsOnFloor: {isOnFloor}");
            
            if (isOnFloor)
            {
                GodotObject hitObject = FloorCheck.GetCollider();
                GD.Print($"RayCast is hitting: {((Node)hitObject).Name}");
                
                currentVelocity.Y = JumpVelocity; 
            }
            _jumpRequested = false;
        }

        float currentSpeed = Input.IsActionPressed("Sprint") ? SprintSpeed : Speed;
        Vector2 inputDir = Input.GetVector("Left", "Right", "Forward", "Back");
        Vector3 direction = (state.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        if (direction != Vector3.Zero)
        {
            currentVelocity.X = direction.X * currentSpeed;
            currentVelocity.Z = direction.Z * currentSpeed;
        }
        else
        {
            currentVelocity.X = 0;
            currentVelocity.Z = 0;
        }

        state.LinearVelocity = currentVelocity;
    }

    public void QuitGame()
    {
        GetTree().Quit();
    }    
}