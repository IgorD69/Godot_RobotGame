using Godot;
using System;

public partial class Box : RigidBody3D
{

    public float KnockbackForce = 50f;

    public void ApplyKnockback(Vector3 sourcePosition)
    {

        Vector3 pushDirection = (GlobalPosition - sourcePosition).Normalized();

        ApplyCentralImpulse(pushDirection * KnockbackForce);

    }
    public void OnBodyEntered(Node body)
    {
        if (body.IsInGroup("Hands"))
        {
            if (body is Node3D body3D)
            {
                GD.Print("Coliziune detectata!");
                ApplyKnockback(body3D.GlobalPosition);
            }
        }
    }

}

