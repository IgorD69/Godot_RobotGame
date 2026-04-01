using Godot;
using System;

public partial class EarthComponent : Node
{
    [Export] private PackedScene RockScene;
    [Export] public Marker3D SpawnPlace;

    private Node3D _currentRock;
    private bool _isFollowing = false;
    private float _yOffset = -2.0f;

    [Export] public float MinZ = -5.0f;
    [Export] public float MaxZ = -1.5f;
    [Export] public float ScrollSpeed = 0.5f;

    [Export] public float ThrowForce = 20.0f;

    public override void _PhysicsProcess(double delta)
    {
        if (_isFollowing && IsInstanceValid(_currentRock))
        {
            Vector3 targetPos = SpawnPlace.GlobalPosition;
            targetPos.Y += _yOffset;
            _currentRock.GlobalPosition = targetPos;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (_isFollowing && @event is InputEventMouseButton btn && btn.Pressed)
        {
            Vector3 p = SpawnPlace.Position;
            if (btn.ButtonIndex == MouseButton.WheelUp) p.Z -= ScrollSpeed;
            if (btn.ButtonIndex == MouseButton.WheelDown) p.Z += ScrollSpeed;

            p.Z = Mathf.Clamp(p.Z, MinZ, MaxZ);
            SpawnPlace.Position = p;
        }
    }

    public void InstantiateRock()
    {
        _currentRock = (Node3D)RockScene.Instantiate();
        GetTree().Root.AddChild(_currentRock);

        _yOffset = -2.0f;
        _isFollowing = true;

        var tween = GetTree().CreateTween();
        tween.TweenProperty(this, nameof(_yOffset), 0.0f, 0.4f)
             .SetTrans(Tween.TransitionType.Back)
             .SetEase(Tween.EaseType.Out);
    }

    public void ReleaseRock()
    {
        if (!_isFollowing || _currentRock == null || !IsInstanceValid(_currentRock)) return;

        _isFollowing = false;

        if (_currentRock is RigidBody3D rb)
        {
            Vector3 throwDirection = -SpawnPlace.GlobalTransform.Basis.Z;

            throwDirection.Y += 0.2f;
            throwDirection = throwDirection.Normalized();

            rb.LinearVelocity = Vector3.Zero;
            rb.ApplyCentralImpulse(throwDirection * ThrowForce);

            rb.Sleeping = false;
        }
    }


    public void ReleaseRockWithDirection(Vector3 direction)
    {
        _isFollowing = false;
        if (_currentRock is RigidBody3D rb)
        {
            rb.ApplyCentralImpulse(direction.Normalized() * ThrowForce);
        }
    }
}