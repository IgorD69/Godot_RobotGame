using Godot;
using System;

public partial class FPSPlayer : CharacterBody3D
{
    [Export] public Camera3D camera;

    [Export] public CollisionShape3D LHandStaticBodyCol;
    [Export] public CollisionShape3D RHandStaticBodyCol;
    [Export] public AnimationPlayer PlayerAnim; public const float MouseSensitivity = 0.001f;


    public const float Speed = 10.0f;
    public const float JumpVelocity = 4.5f;
    public const float SprintSpeed = 15f;
    private float _rotationX;
    public float CurentSpeed;

    private MeshInstance3D Head;
    private MeshInstance3D Torso;

    public Marker3D RFoot;
    public Marker3D RKnee;
    public Marker3D LFoot;
    public Marker3D LKnee;


    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;

        RFoot.GetNode<Marker3D>("LegMarkers/RFoot");
        RKnee.GetNode<Marker3D>("LegMarkers/RKnee");
        LFoot.GetNode<Marker3D>("LegMarkers/LFoot");
        LKnee.GetNode<Marker3D>("LegMarkers/LKnee");

        LHandStaticBodyCol?.SetDeferred(CollisionShape3D.PropertyName.Disabled, true);
        RHandStaticBodyCol?.SetDeferred(CollisionShape3D.PropertyName.Disabled, true);

        Head.CastShadow = GeometryInstance3D.ShadowCastingSetting.ShadowsOnly;
        Torso.CastShadow = GeometryInstance3D.ShadowCastingSetting.ShadowsOnly;

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

        if (PlayerAnim.IsPlaying() && (PlayerAnim.CurrentAnimation == "RPunch" || PlayerAnim.CurrentAnimation == "LPunch"))
        {
            return;
        }

        if (LHandStaticBodyCol.Disabled == false || RHandStaticBodyCol.Disabled == false)
        {
            LHandStaticBodyCol.SetDeferred(CollisionShape3D.PropertyName.Disabled, true);
            RHandStaticBodyCol.SetDeferred(CollisionShape3D.PropertyName.Disabled, true);
        }

        if (Input.IsActionJustPressed("RClick"))
        {
            RHandStaticBodyCol.SetDeferred(CollisionShape3D.PropertyName.Disabled, false);
            PlayerAnim.Play("RPunch");
            return;
        }

        if (Input.IsActionJustPressed("LClick"))
        {
            LHandStaticBodyCol.SetDeferred(CollisionShape3D.PropertyName.Disabled, false);
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
        if (@event is InputEventMouseMotion mouseMotion)
        {
            RotateY(-mouseMotion.Relative.X * MouseSensitivity);
            _rotationX -= mouseMotion.Relative.Y * MouseSensitivity;
            _rotationX = Mathf.Clamp(_rotationX, Mathf.DegToRad(-89f), Mathf.DegToRad(89f));
            if (camera != null)
                camera.Rotation = new Vector3(_rotationX, camera.Rotation.Y, camera.Rotation.Z);
        }

        if (@event.IsActionPressed("esc")) GetTree().Quit();
    }
}