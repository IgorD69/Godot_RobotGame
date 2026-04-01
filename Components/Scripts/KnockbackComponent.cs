using Godot;
using System;

public partial class KnockbackComponent : Node
{

    [Export] public RigidBody3D Rock;
    [Export] public float KnockbackForce = 15.0f;
    private bool _isStunned = false;


    public void ApplyKnockback(Vector3 sourcePosition)
    {
        if (Rock == null) return;

        _isStunned = true;

        Vector3 pushDirection = (Rock.GlobalPosition - sourcePosition).Normalized();

        Rock.Sleeping = false;

        Rock.LinearVelocity = Vector3.Zero;
        Rock.AngularVelocity = Vector3.Zero;

        pushDirection.Y = 0.5f;

        Rock.ApplyCentralImpulse(pushDirection.Normalized() * KnockbackForce);

        var timer = GetTree().CreateTimer(1.3f);
        timer.Connect("timeout", Callable.From(() => _isStunned = false));
    }


    public void OnBodyEntered(Node body)
    {
        if (body.IsInGroup("Hands"))
        {
            if (body is Node3D body3D)
            {
                ApplyKnockback(body3D.GlobalPosition);
                GD.Print("ROCK KNOCKBECK");
            }
        }
    }
}
