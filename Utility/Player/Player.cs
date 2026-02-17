using Godot;

public partial class Player : CharacterBody3D
{
    [ExportCategory("Movement")]
    [Export] public float Speed = 5.0f;
    [Export] public float SprintSpeed = 10.0f;
    [Export] public float JumpVelocity = 4.5f;
    [Export] public float Gravity = 9.8f;

    [ExportCategory("Camera")]
    [Export(PropertyHint.Range, "60, 120")] public float BaseFov = 75.0f;
    [Export] public float MouseSensitivity = 0.003f;
    [Export] public Node3D CameraNode;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;

        if (CameraNode == null)
        {
            CameraNode = GetNode<Camera3D>("Camera");
        }

        // Apply initial FOV
        if (CameraNode is Camera3D cam)
        {
            cam.Fov = BaseFov;
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

        // 1. Gravity
        if (!IsOnFloor())
        {
            velocity.Y -= Gravity * (float)delta;
        }

        // 2. Jump
        if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }

        // 3. Movement (Instant/Snappy)
        float currentSpeed = Input.IsActionPressed("Sprint") ? SprintSpeed : Speed;
        
        Vector2 inputDir = Input.GetVector("Left", "Right", "Forward", "Back");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        if (direction != Vector3.Zero)
        {
            // INSTANT START: Directly assign velocity without acceleration
            velocity.X = direction.X * currentSpeed;
            velocity.Z = direction.Z * currentSpeed;
        }
        else
        {
            // INSTANT STOP: Directly set horizontal velocity to zero
            velocity.X = 0;
            velocity.Z = 0;
        }

        Velocity = velocity;
        MoveAndSlide();

        // Temp quit method call
        if (Input.IsActionPressed("Pause"))
        {
            QuitGame();
        }

    }

    // Method to quit, must be changed later when pause menu is implemented
    public void QuitGame()
    {
        GetTree().Quit();
    }    
    
}