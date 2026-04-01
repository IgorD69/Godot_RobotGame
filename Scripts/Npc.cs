using Godot;
using System;
using System.Numerics;
using Vector3 = Godot.Vector3;
public partial class Npc : RigidBody3D
{
    [Export] public float Speed = 3.0f;
    [Export] public float KnockbackForce = 15.0f;
    public CharacterBody3D _player;
    private bool _isStunned = false;

    private Marker3D _lTarget;
    public Vector3 BoxPosition = new(0, 0, -2f);
    public float LTarget = -0.4f;

    public int HP = 10;

    private float _interval = 1f;
    private float _timePassed = 0.0f;

    public override void _Ready()
    {

        _player = GetTree().GetFirstNodeInGroup("Player") as CharacterBody3D;

        if (_player == null)
            GD.PrintErr("Player Not Found");

        ContactMonitor = true;
        MaxContactsReported = 5;

        BodyEntered += OnBodyEntered;

        _lTarget = GetNode<Marker3D>("Boxer/HandsMarkers/LTarget");

    }

    public override void _PhysicsProcess(double delta)
    {

        if (_player == null || _isStunned) return;

        _timePassed += (float)delta;

        if (_timePassed >= _interval)
        {
            _timePassed = 0.0f;
            BoxAnimation();

            // GD.Print("Interval reached!");
        }
        Vector3 playerPos = _player.GlobalPosition;
        Vector3 currentPos = GlobalPosition;

        Vector3 direction = (playerPos - currentPos).Normalized();
        direction.Y = 0;


        LinearVelocity = new Vector3(
            direction.X * Speed,
            LinearVelocity.Y,
            direction.Z * Speed
        );

        AngularVelocity = Vector3.Zero;

        if (direction.Length() > 0.1f)
        {
            if (currentPos.DistanceTo(playerPos) > 0.1f)
            {
                LookAt(new Vector3(playerPos.X, GlobalPosition.Y, playerPos.Z), Vector3.Up);
            }

        }
    }

    public void TweenBurstAnimation()
    {
        Tween tween = GetTree().CreateTween();

        tween.TweenProperty(this, "scale", new Vector3(1.2f, 1.2f, 1.2f), 0.1f)
             .SetTrans(Tween.TransitionType.Back)
             .SetEase(Tween.EaseType.Out);

        tween.TweenProperty(this, "scale", Vector3.Zero, 0.2f)
             .SetTrans(Tween.TransitionType.Expo)
             .SetEase(Tween.EaseType.In);

        tween.Finished += () => QueueFree();
    }

    public async void BoxAnimation()
    {
        Vector3 newPosition = _lTarget.Position;
        newPosition.Z = BoxPosition.Z;
        Vector3 ResetPosition = new(-0.56f, 1.46f, 0f);

        _lTarget.Position = newPosition;

        await ToSignal(GetTree().CreateTimer(0.1f), Timer.SignalName.Timeout);
        _lTarget.Position = ResetPosition;

    }


    public void ApplyKnockback(Vector3 sourcePosition)
    {
        _isStunned = true;

        Vector3 pushDirection = (GlobalPosition - sourcePosition).Normalized();
        LinearVelocity = Vector3.Zero;

        pushDirection.Y = !_player.IsOnFloor() ? 0.4f : 0.1f;

        ApplyCentralImpulse(pushDirection * KnockbackForce);

        GetTree().CreateTimer(1.3f).Timeout += () =>
        {
            HP--;
            _isStunned = false;
            Die();
        };
    }

    private void Die()
    {
        if (HP <= 0)
        {
            TweenBurstAnimation();
        }
    }

    public void OnBodyEntered(Node body)
    {
        if (body.IsInGroup("Hands"))
        {
            if (body is Node3D body3D)
            {
                ApplyKnockback(body3D.GlobalPosition);
            }
        }
    }
}