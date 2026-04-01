using Godot;
using System;

public partial class EarthComponent : Node
{
    [Export] public Godot.Collections.Array<PackedScene> Rocks { get; set; } = new();
    [Export] public Marker3D SpawnPlace;

    private Node3D _currentRock;
    private bool _isFollowing = false;
    private float _yOffset = -2.0f;

    [Export] public float MinZ = -5.0f;
    [Export] public float MaxZ = -1.5f;
    [Export] public float ScrollSpeed = 0.5f;

    [Export] public float ThrowForce = 1000.0f;

    public Area3D IntersectionArea;

    public override void _Ready()
    {
        IntersectionArea = SpawnPlace.GetChild<Area3D>(0);
    }


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
        if (!_isFollowing || _currentRock == null) return;

        if (@event is InputEventMouseButton btn && btn.Pressed)
        {
            Vector3 p = SpawnPlace.Position;
            if (btn.ButtonIndex == MouseButton.WheelUp) p.Z -= ScrollSpeed;
            if (btn.ButtonIndex == MouseButton.WheelDown) p.Z += ScrollSpeed;
            p.Z = Mathf.Clamp(p.Z, MinZ, MaxZ);
            SpawnPlace.Position = p;

            if (btn.ButtonIndex == MouseButton.Left || btn.ButtonIndex == MouseButton.Right)
            {
                Vector3 lookDirection = -SpawnPlace.GlobalTransform.Basis.Z;

                float punchForce = ThrowForce * 1.5f;

                ReleaseWithCustomForce(lookDirection, punchForce);

            }
        }
    }

    public void InstantiateRock()
    {
        _currentRock = null;

        Node3D overlappingRock = null;
        var bodies = IntersectionArea.GetOverlappingBodies();

        foreach (var body in bodies)
        {
            if (body.IsInGroup("Rocks") && body is Node3D rockNode)
            {
                overlappingRock = rockNode;
                break;
            }
        }


        if (overlappingRock != null)
        {
            _currentRock = overlappingRock;

            _yOffset = _currentRock.GlobalPosition.Y - SpawnPlace.GlobalPosition.Y;
        }
        else
        {
            if (Rocks == null || Rocks.Count == 0)
            {
                return;
            }

            int randomIndex = (int)(GD.Randi() % (uint)Rocks.Count);
            _currentRock = (Node3D)Rocks[randomIndex].Instantiate();

            GetTree().Root.AddChild(_currentRock);

            _yOffset = -2.0f;
        }

        _isFollowing = true;

        if (_currentRock is RigidBody3D rb)
        {
            rb.Freeze = true;
            rb.LinearVelocity = Vector3.Zero;
            rb.AngularVelocity = Vector3.Zero;
        }

        var tween = GetTree().CreateTween();
        tween.TweenProperty(this, nameof(_yOffset), 0.0f, 0.4f)
             .SetTrans(Tween.TransitionType.Back)
             .SetEase(Tween.EaseType.Out);
    }




    private void ReleaseWithCustomForce(Vector3 direction, float force)
    {
        _isFollowing = false;

        if (_currentRock is RigidBody3D rb)
        {
            rb.Freeze = false;
            rb.LinearVelocity = Vector3.Zero;
            rb.AngularVelocity = Vector3.Zero;
            rb.ApplyCentralImpulse(direction.Normalized() * force);
            rb.Sleeping = false;
        }

        _currentRock = null;
    }
    public void ReleaseRock()
    {
        if (!_isFollowing || _currentRock == null || !IsInstanceValid(_currentRock)) return;

        _isFollowing = false;

        if (_currentRock is RigidBody3D rb)
        {
            rb.Freeze = false;
            Vector3 throwDirection = -SpawnPlace.GlobalTransform.Basis.Z;
            throwDirection.Y += 0.2f;

            rb.LinearVelocity = Vector3.Zero;
            rb.ApplyCentralImpulse(throwDirection.Normalized() * ThrowForce);
            rb.Sleeping = false;
        }

        _currentRock = null;
    }


    public void DropRock()
    {
        if (!_isFollowing || _currentRock == null || !IsInstanceValid(_currentRock)) return;

        _isFollowing = false;

        if (_currentRock is RigidBody3D rb)
        {
            rb.Freeze = false;
            rb.Sleeping = false;
        }

        _currentRock = null;
    }

    public void ReleaseRockWithDirection(Vector3 direction)
    {
        if (!_isFollowing || _currentRock == null || !IsInstanceValid(_currentRock)) return;

        _isFollowing = false;

        if (_currentRock is RigidBody3D rb)
        {
            rb.Freeze = false;
            rb.LinearVelocity = Vector3.Zero;
            rb.AngularVelocity = Vector3.Zero;

            rb.ApplyCentralImpulse(direction.Normalized() * ThrowForce);

            rb.Sleeping = false;
        }

        _currentRock = null;
    }


    public bool IsHoldingRock()
    {
        return _isFollowing && IsInstanceValid(_currentRock);
    }
}