using Godot;

public partial class Player : CharacterBody3D
{
    [ExportCategory("Movement")]
    [Export] public float Speed = 5.0f;
    [Export] public float SprintSpeed = 10.0f;
    [Export] public float JumpVelocity = 4.5f;
    [Export] public float Acceleration = 20.0f;
    [Export] public float Deceleration = 20.0f;
    [Export] public float Gravity = 9.8f;

    [ExportCategory("Camera")]
    [Export] public float MouseSensitivity = 0.003f;
    [Export] public Node3D CameraNode;

    private float _currentSpeed;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;

        if (CameraNode == null)
        {
            CameraNode = GetNode<Camera3D>("Camera");
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            RotateY(-mouseMotion.Relative.X * MouseSensitivity);

            if (CameraNode != null)
            {
                CameraNode.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);
                
                Vector3 camRot = CameraNode.Rotation;
                camRot.X = Mathf.Clamp(camRot.X, Mathf.DegToRad(-89f), Mathf.DegToRad(89f));
                CameraNode.Rotation = camRot;
            }
        }
        
        if (@event.IsActionPressed("ui_cancel"))
        {
             Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured ? 
                               Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        if (!IsOnFloor())
        {
            velocity.Y -= Gravity * (float)delta;
        }

        if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }

        _currentSpeed = Input.IsActionPressed("Sprint") ? SprintSpeed : Speed;

        Vector2 inputDir = Input.GetVector("Left", "Right", "Forward", "Back");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        if (direction != Vector3.Zero)
        {
            velocity.X = Mathf.MoveToward(velocity.X, direction.X * _currentSpeed, Acceleration * (float)delta);
            velocity.Z = Mathf.MoveToward(velocity.Z, direction.Z * _currentSpeed, Acceleration * (float)delta);
        }
        else
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0, Deceleration * (float)delta);
            velocity.Z = Mathf.MoveToward(velocity.Z, 0, Deceleration * (float)delta);
        }

        Velocity = velocity;
        MoveAndSlide();
    }
}