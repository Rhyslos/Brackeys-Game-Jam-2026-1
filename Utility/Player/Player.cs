using Godot;

public partial class Player : RigidBody3D
{
    // exported variables
    [ExportCategory("Movement")]
    [Export] public float Speed = 5.0f;
    [Export] public float SprintSpeed = 10.0f;
    [Export] public float JumpVelocity = 4.5f;
    [Export] public float MaxSlopeAngle = 45.0f;
    [Export] public float AirControl = 2.5f;
    [Export] public float CoyoteTime = 0.15f;

    [ExportCategory("Visuals")]
    [Export] public Node3D GunNode;
    [Export] public float WeaponBobFrequency = 2.0f;
    [Export] public float WeaponBobAmplitude = 0.04f;
    [Export] public float HeadBobFrequency = 2.0f;
    [Export] public float HeadBobAmplitude = 0.08f;
    [Export] public float LandingDipAmount = 0.2f;
    [Export] public float LandingRecoverySpeed = 8.0f;

    [ExportCategory("Camera")]
    [Export(PropertyHint.Range, "60, 120")] public float BaseFov = 75.0f;
    [Export] public float MouseSensitivity = 0.003f;
    [Export] public Node3D CameraNode;

    [ExportCategory("Physics")]
    [Export] public RayCast3D FloorCheck;

    // state variables
    private float _targetYRotation;
    private bool _jumpRequested;
    private bool _isJumping;
    private float _bobTime;
    private Vector3 _gunDefaultPosition;
    private Vector3 _cameraDefaultPosition;
    private float _currentLandingDip;
    private bool _wasOnFloor;
    private float _landingTimer = -1f;
    private float _coyoteTimer;

    // initialization functions
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
            _cameraDefaultPosition = cam.Position;
        }

        if (GunNode != null)
        {
            _gunDefaultPosition = GunNode.Position;
        }
    }

    // input functions
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

    // visual functions
    public override void _Process(double delta)
    {
        Vector3 velocity = LinearVelocity;
        Vector2 horizontalVelocity = new Vector2(velocity.X, velocity.Z);
        
        float currentSpeed = horizontalVelocity.Length();
        bool isMoving = currentSpeed > 0.5f;
        bool isOnFloor = FloorCheck != null && FloorCheck.IsColliding();

        Vector3 targetCamPos = _cameraDefaultPosition;
        Vector3 targetGunPos = _gunDefaultPosition;

        if (isMoving && isOnFloor)
        {
            _bobTime += (float)delta * currentSpeed * 1.5f;
            
            float headBobY = Mathf.Sin(_bobTime * HeadBobFrequency);
            float headBobX = Mathf.Cos(_bobTime * HeadBobFrequency * 0.5f);
            targetCamPos += new Vector3(headBobX * HeadBobAmplitude, headBobY * HeadBobAmplitude, 0);

            if (GunNode != null)
            {
                float gunBobY = Mathf.Sin(_bobTime * WeaponBobFrequency);
                float gunBobX = Mathf.Cos(_bobTime * WeaponBobFrequency * 0.5f);
                targetGunPos += new Vector3(gunBobX * WeaponBobAmplitude, gunBobY * WeaponBobAmplitude, 0);
            }
        }
        else
        {
            _bobTime = 0;
        }

        _currentLandingDip = Mathf.Lerp(_currentLandingDip, 0f, (float)delta * LandingRecoverySpeed);
        
        targetCamPos.Y -= _currentLandingDip;
        if (GunNode != null)
        {
            targetGunPos.Y -= _currentLandingDip * 0.5f;
        }

        if (CameraNode != null)
        {
            CameraNode.Position = CameraNode.Position.Lerp(targetCamPos, (float)delta * 15f);
        }
        
        if (GunNode != null)
        {
            GunNode.Position = GunNode.Position.Lerp(targetGunPos, (float)delta * 15f);
        }
    }

    // physics functions
    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionPressed("Pause"))
        {
            QuitGame();
        }
    }

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        float delta = state.Step;
        
        Transform3D newTransform = state.Transform;
        newTransform.Basis = Basis.FromEuler(new Vector3(0, _targetYRotation, 0));
        state.Transform = newTransform;

        Vector3 currentVelocity = state.LinearVelocity;
        Vector2 inputDir = Input.GetVector("Left", "Right", "Forward", "Back");
        Vector3 direction = (state.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        
        if (currentVelocity.Y <= 0.1f)
        {
            _isJumping = false;
        }

        bool isOnFloor = FloorCheck != null && FloorCheck.IsColliding();
        Vector3 floorNormal = isOnFloor ? FloorCheck.GetCollisionNormal() : Vector3.Up;
        float slopeAngle = Mathf.RadToDeg(Mathf.Acos(floorNormal.Dot(Vector3.Up)));
        bool isWalkableSlope = isOnFloor && slopeAngle <= MaxSlopeAngle && !_isJumping;

        if (isWalkableSlope)
        {
            _coyoteTimer = CoyoteTime;
        }
        else
        {
            _coyoteTimer -= delta;
        }

        if (!_wasOnFloor && isOnFloor && !_isJumping)
        {
            _landingTimer = 0.03f;
        }

        if (_landingTimer > 0f)
        {
            _landingTimer -= delta;
            if (_landingTimer <= 0f)
            {
                _currentLandingDip = LandingDipAmount;
            }
        }

        _wasOnFloor = isOnFloor;

        if (_jumpRequested && (_coyoteTimer > 0f || isWalkableSlope))
        {
            currentVelocity.Y = JumpVelocity; 
            _isJumping = true;
            isWalkableSlope = false; 
            _landingTimer = -1f; 
            _coyoteTimer = 0f;
        }

        if (isWalkableSlope)
        {
            if (direction == Vector3.Zero && !_jumpRequested)
            {
                currentVelocity = Vector3.Zero;
            }
            else
            {
                float currentSpeed = Input.IsActionPressed("Sprint") ? SprintSpeed : Speed;
                Vector3 slopeDirection = (direction - floorNormal * direction.Dot(floorNormal)).Normalized();
                currentVelocity.X = slopeDirection.X * currentSpeed;
                currentVelocity.Y = slopeDirection.Y * currentSpeed; 
                currentVelocity.Z = slopeDirection.Z * currentSpeed;
            }
        }
        else 
        {
            float currentSpeed = Input.IsActionPressed("Sprint") ? SprintSpeed : Speed;
            Vector3 targetVelocity = direction * currentSpeed;
            
            currentVelocity.X = Mathf.Lerp(currentVelocity.X, targetVelocity.X, AirControl * delta);
            currentVelocity.Z = Mathf.Lerp(currentVelocity.Z, targetVelocity.Z, AirControl * delta);

            for (int i = 0; i < state.GetContactCount(); i++)
            {
                Vector3 contactNormal = state.GetContactLocalNormal(i);
                float contactAngle = Mathf.RadToDeg(Mathf.Acos(contactNormal.Dot(Vector3.Up)));
                
                if (contactAngle > MaxSlopeAngle)
                {
                    if (currentVelocity.Dot(contactNormal) < 0)
                    {
                        currentVelocity -= contactNormal * currentVelocity.Dot(contactNormal);
                    }
                }
            }
        }

        _jumpRequested = false;
        state.LinearVelocity = currentVelocity;
    }

    // helper functions
    public void QuitGame()
    {
        GetTree().Quit();
    }    
}