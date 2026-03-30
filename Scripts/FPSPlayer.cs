using Godot;
using System;

public partial class FPSPlayer : CharacterBody3D
{
    [ExportGroup("Components")]
    [Export] public Camera3D camera;
    [Export] public AnimationPlayer PlayerAnim;
    [Export] public CollisionShape3D LHandStaticBodyCol;
    [Export] public CollisionShape3D RHandStaticBodyCol;

    [ExportGroup("Movement")]
    public const float MouseSensitivity = 0.002f;
    public const float Speed = 7.0f;
    public const float SprintSpeed = 11f;
    public const float JumpVelocity = 4.5f;

    [ExportGroup("Procedural Steps")]
    [Export] public float StepDistance = 1.0f;
    [Export] public float StepLength = 1.0f;
    [Export] public float StepHeight = 0.2f;

    private float _rotationX;
    private float _distanceTraveled = 0f;
    private bool _isRightFootNext = true;

    private MeshInstance3D _head, _torso;
    private Marker3D _lookAtNode, _rFoot, _lFoot;
    private float _footBaselineY;

    public Vector3 ForwardVec = new();

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;

        _lookAtNode = GetNode<Marker3D>("LookAt");
        _rFoot = GetNode<Marker3D>("LegMarkers/RFoot");
        _lFoot = GetNode<Marker3D>("LegMarkers/LFoot");
        _head = GetNodeOrNull<MeshInstance3D>("Head");
        _torso = GetNodeOrNull<MeshInstance3D>("Torso");

        if (_rFoot != null) _footBaselineY = _rFoot.Position.Y;
        if (_lFoot != null) _footBaselineY = _lFoot.Position.Y;

        LHandStaticBodyCol?.SetDeferred(CollisionShape3D.PropertyName.Disabled, true);
        RHandStaticBodyCol?.SetDeferred(CollisionShape3D.PropertyName.Disabled, true);

        if (_head != null) _head.CastShadow = GeometryInstance3D.ShadowCastingSetting.ShadowsOnly;
        if (_torso != null) _torso.CastShadow = GeometryInstance3D.ShadowCastingSetting.ShadowsOnly;
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;
        float currentSpeed = Input.IsActionPressed("sprint") ? SprintSpeed : Speed;

        if (!IsOnFloor())
            velocity += GetGravity() * (float)delta;

        if (Input.IsActionJustPressed("jump") && IsOnFloor())
            velocity.Y = JumpVelocity;

        Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * currentSpeed;
            velocity.Z = direction.Z * currentSpeed;

            if (IsOnFloor())
                MoveLegs(delta);
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, currentSpeed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, currentSpeed);
            _distanceTraveled = 0;
        }

        Velocity = velocity;
        MoveAndSlide();
        HandleAnimations();
    }

    private void MoveLegs(double delta)
    {
        Vector2 horizontalVel = new Vector2(Velocity.X, Velocity.Z);
        _distanceTraveled += horizontalVel.Length() * (float)delta;

        if (_distanceTraveled > StepDistance)
        {
            ExecuteStep();
            _distanceTraveled = 0f;
        }
    }

    private void ExecuteStep()
    {
        _isRightFootNext = !_isRightFootNext;

        Marker3D targetFoot = _isRightFootNext ? _rFoot : _lFoot;
        Marker3D backFoot = _isRightFootNext ? _lFoot : _rFoot;

        Tween stepTween = GetTree().CreateTween().SetParallel(true);

        stepTween.TweenProperty(targetFoot, "position:z", -StepLength, 0.15f);
        stepTween.TweenProperty(backFoot, "position:z", 0f, 0.15f);

        Tween liftTween = GetTree().CreateTween();
        liftTween.TweenProperty(targetFoot, "position:y", _footBaselineY + StepHeight, 0.07f)
                 .SetTrans(Tween.TransitionType.Cubic);
        liftTween.TweenProperty(targetFoot, "position:y", _footBaselineY, 0.07f)
                 .SetTrans(Tween.TransitionType.Cubic);
    }

    private void HandleAnimations()
    {
        if (Input.IsActionPressed("Block"))
        {
            if (PlayerAnim.CurrentAnimation != "Block") PlayerAnim.Play("Block");
            return;
        }

        if (PlayerAnim.IsPlaying() && (PlayerAnim.CurrentAnimation == "RPunch" || PlayerAnim.CurrentAnimation == "LPunch"))
            return;

        if (LHandStaticBodyCol.Disabled == false || RHandStaticBodyCol.Disabled == false)
        {
            LHandStaticBodyCol.SetDeferred(CollisionShape3D.PropertyName.Disabled, true);
            RHandStaticBodyCol.SetDeferred(CollisionShape3D.PropertyName.Disabled, true);
        }

        if (Input.IsActionJustPressed("RClick"))
        {
            RHandStaticBodyCol.SetDeferred(CollisionShape3D.PropertyName.Disabled, false);
            PlayerAnim.Play("RPunch");
        }
        else if (Input.IsActionJustPressed("LClick"))
        {
            LHandStaticBodyCol.SetDeferred(CollisionShape3D.PropertyName.Disabled, false);
            PlayerAnim.Play("LPunch");
        }
        else
        {
            if (PlayerAnim.CurrentAnimation != "Deff") PlayerAnim.Play("Deff");
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