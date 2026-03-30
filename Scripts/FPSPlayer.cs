using Godot;
using System;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Serialization;

public partial class FPSPlayer : CharacterBody3D
{

    public Camera3D camera;

    public const float MouseSensitivity = 0.001f;
    public const float Speed = 10.0f;
    public const float JumpVelocity = 4.5f;

    public const float SprintSpeed = 15f;
    private float _rotationX;
    private float _rotationY;
    public float CurentSpeed;

    [Export] public AnimationPlayer PlayerAnim;



    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        camera = GetNode<Camera3D>("Camera");


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



        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * CurentSpeed;
            velocity.Z = direction.Z * CurentSpeed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, CurentSpeed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, CurentSpeed);
        }



        HandleAnimations();

        Velocity = velocity;

        MoveAndSlide();
    }



    private void HandleAnimations()
    {
        if (Input.IsActionPressed("Block"))
        {
            if (PlayerAnim.CurrentAnimation != "Block")
            {
                PlayerAnim.Play("Block");
            }
            return;
        }

        if (PlayerAnim.IsPlaying() &&
           (PlayerAnim.CurrentAnimation == "RPunch" || PlayerAnim.CurrentAnimation == "LPunch"))
        {
            return;
        }
        if (Input.IsActionJustPressed("RClick"))
        {
            PlayerAnim.Play("RPunch");
            return;
        }

        if (Input.IsActionJustPressed("LClick"))
        {
            PlayerAnim.Play("LPunch");
            return;
        }

        if (PlayerAnim.CurrentAnimation != "Deff")
        {
            PlayerAnim.Play("Deff");
        }
    }
    public override void _Input(InputEvent @event)
    {

        // if (@event.IsActionPressed("RClick"))
        // {
        //     PlayerAnim.Play("RPunch");

        // }
        // if (Input.IsActionJustPressed("LClick"))
        // {
        //     PlayerAnim.Play("Block");
        // }


        if (@event is InputEventMouseMotion mouseMotion)
        {
            RotateY(-mouseMotion.Relative.X * MouseSensitivity);

            _rotationX -= mouseMotion.Relative.Y * MouseSensitivity;
            _rotationX = Mathf.Clamp(_rotationX, Mathf.DegToRad(-89f), Mathf.DegToRad(89f));

            if (camera != null)
                camera.Rotation = new Vector3(_rotationX, camera.Rotation.Y, camera.Rotation.Z);

        }




        if (@event.IsActionPressed("esc"))
        {
            GetTree().Quit();
        }
    }


}
