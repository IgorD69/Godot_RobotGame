using Godot;
using System;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Serialization;

public partial class Player : CharacterBody3D
{

    [Export] public float LeanAmount = 0.2f;
    [Export] public float LeanSpeed = 10f;

    [Export] public float MaxSteerAngle = 30f;
    [Export] public float SteerSpeed = 10f;

    [Export] Camera3D camera;

    public float WheelsRotateAnount = 30f;
    public float WheelsRotateSpeed = 10f;

    public const float MouseSensitivity = 0.001f;
    public const float Speed = 15.0f;
    public const float JumpVelocity = 4.5f;

    public const float SprintSpeed = 30f;
    private float _rotationX;
    private float _rotationY;
    public float CurentSpeed;
    public Vector3 currentRot;
    public Node3D visualModel;
    public Node3D RobotBody;
    private float _currentLean = 0f;
    public AnimationPlayer PlayerAnim;
    bool isAnimPlaying = false;
    public Node3D L_Wheel;
    public Node3D R_Wheel;
    public float _wheelRotationX;



    private float _currentSteerAngle = 0f;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;

        visualModel = GetNode<Node3D>("RobotMesh");

        RobotBody = GetNode<Node3D>("RobotMesh/Hands");

        // PlayerAnim = GetNode<AnimationPlayer>("AnimationPlayer");

        L_Wheel = GetNode<Node3D>("RobotMesh/Wheels/LWheel");
        R_Wheel = GetNode<Node3D>("RobotMesh/Wheels/RWheel");


    }
    public override void _PhysicsProcess(double delta)
    {

        Vector3 velocity = Velocity;

        CurentSpeed = Input.IsActionPressed("sprint") ? SprintSpeed : Speed;

        if (!IsOnFloor())
            velocity += GetGravity() * (float)delta;

        if (Input.IsActionJustPressed("jump") && IsOnFloor())
            velocity.Y = JumpVelocity;

        Vector2 inputDir = Input.GetVector("left", "right", "up", "down");

        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        LeanForward(inputDir, delta);
        RotateWheels(inputDir, delta);

        if (direction != Vector3.Zero)
        {
            // if (inputDir.Y < 0)
            // {
            //     if (PlayerAnim.CurrentAnimation != "Roll")
            //     {
            //         PlayerAnim.Play("Roll");
            //     }
            // }
            // else
            // {
            //     {
            //         PlayerAnim.PlayBackwards("Roll");
            //     }
            // }

            velocity.X = direction.X * CurentSpeed;
            velocity.Z = direction.Z * CurentSpeed;


        }
        else
        {
            // PlayerAnim.Play("RESET");
            velocity.X = Mathf.MoveToward(Velocity.X, 0, CurentSpeed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, CurentSpeed);
        }

        Velocity = velocity;

        MoveAndSlide();
    }

    public void LeanForward(Vector2 inputDir, double delta)
    {
        if (visualModel == null) return;


        float targetLeanForward = inputDir.Y * LeanAmount;
        float targetLeanSide = -inputDir.X * LeanAmount;

        float leanX = Mathf.Lerp(visualModel.Rotation.X, targetLeanForward, (float)delta * LeanSpeed);
        float leanZ = Mathf.Lerp(visualModel.Rotation.Z, targetLeanSide, (float)delta * LeanSpeed);

        visualModel.Rotation = new Vector3(leanX, visualModel.Rotation.Y, leanZ);
    }
    public void RotateWheels(Vector2 inputDir, double delta)
    {
        if (L_Wheel == null || R_Wheel == null) return;

        float forwardSign = Mathf.Sign(inputDir.Y);
        if (inputDir.Y == 0) forwardSign = 1f;

        float targetAngle = inputDir.X * Mathf.DegToRad(MaxSteerAngle) * forwardSign;

        _currentSteerAngle = Mathf.Lerp(_currentSteerAngle, targetAngle, (float)delta * SteerSpeed);

        float leftAngle = _currentSteerAngle;
        float rightAngle = _currentSteerAngle;

        if (inputDir.X > 0)
        {
            rightAngle *= 1.5f;
            leftAngle *= 0.8f;
        }
        else if (inputDir.X < 0)
        {
            leftAngle *= 1.5f;
            rightAngle *= 0.8f;
        }

        L_Wheel.Rotation = new Vector3(L_Wheel.Rotation.X, leftAngle, L_Wheel.Rotation.Z);
        R_Wheel.Rotation = new Vector3(R_Wheel.Rotation.X, rightAngle, R_Wheel.Rotation.Z);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            RotateY(-mouseMotion.Relative.X * MouseSensitivity);

            _rotationX -= mouseMotion.Relative.Y * MouseSensitivity;
            _rotationX = Mathf.Clamp(_rotationX, Mathf.DegToRad(-45f), Mathf.DegToRad(45f));

            if (camera != null)
                camera.Rotation = new Vector3(_rotationX, camera.Rotation.Y, camera.Rotation.Z);

            if (RobotBody != null)
                RobotBody.Rotation = new Vector3(_rotationX, RobotBody.Rotation.Y, RobotBody.Rotation.Z);
        }

        if (@event.IsActionPressed("esc"))
        {
            GetTree().Quit();
        }
    }


}
