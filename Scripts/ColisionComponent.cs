using Godot;
using System;

public partial class HealthComponent : Node
{
    [Export] public float MaxHealth = 100f;
    private float _currentHealth;
    private Node3D _parent;

    public override void _Ready()
    {
        _currentHealth = MaxHealth;
        _parent = GetParent<Node3D>();
    }

    public void Damage(float amount, Vector3 sourcePosition, float knockbackForce)
    {
        _currentHealth -= amount;
        GD.Print($"{_parent.Name} a luat {amount} damage. HP rămas: {_currentHealth}");

        ApplyKnockback(sourcePosition, knockbackForce);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void ApplyKnockback(Vector3 sourcePosition, float force)
    {
        if (_parent is RigidBody3D rigid)
        {
            Vector3 dir = (rigid.GlobalPosition - sourcePosition).Normalized();
            dir.Y = 0.4f; // Să sară puțin
            rigid.LinearVelocity = Vector3.Zero;
            rigid.ApplyCentralImpulse(dir * force);
        }
        else if (_parent is CharacterBody3D charBody)
        {
            Vector3 dir = (charBody.GlobalPosition - sourcePosition).Normalized();
            charBody.Velocity += dir * force;
        }
    }

    private void Die()
    {
        GD.Print($"{_parent.Name} a murit!");

        if (_parent.HasMethod("TweenBurstAnimation"))
        {
            _parent.Call("TweenBurstAnimation");
        }
        else
        {
            _parent.QueueFree();
        }
    }
}