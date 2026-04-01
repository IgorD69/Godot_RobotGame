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
        _currentRock = null;

        var spaceState = GetViewport().World3D.DirectSpaceState;

        Node3D overlappingRock = null;
        var bodies = IntersectionArea.GetOverlappingBodies();

        GD.Print($"Obiecte detectate în zonă: {bodies.Count}");

        foreach (var body in bodies)
        {
            GD.Print($"Am atins: {body.Name}");

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
            GD.Print("--- RECUPERARE PIATRĂ EXISTENTĂ ---");
        }
        else
        {
            _currentRock = (Node3D)RockScene.Instantiate();
            GetTree().Root.AddChild(_currentRock);
            _yOffset = -2.0f;
            GD.Print("--- INSTANȚIERE PIATRĂ NOUĂ ---");
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

    public void ReleaseRockWithDirection(Vector3 direction)
    {
        if (!_isFollowing || _currentRock == null || !IsInstanceValid(_currentRock)) return;

        _isFollowing = false;

        if (_currentRock is RigidBody3D rb)
        {
            rb.Freeze = false;
            rb.LinearVelocity = Vector3.Zero;
            rb.ApplyCentralImpulse(direction.Normalized() * ThrowForce);
            rb.Sleeping = false;
        }

        _currentRock = null;
    }
}