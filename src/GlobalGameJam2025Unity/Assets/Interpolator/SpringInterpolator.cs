using System.Diagnostics;
using System;
using UnityEngine;

[Serializable]
public class SpringInterpolator : IInterpolatorFloat
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [SerializeField] private float power;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [SerializeField] private float damper;

    public float Target { get; set; }

    public float Value { get; set; }

    public float Velocity { get; set; }

    public float Damper { get => damper; set => damper = value; }

    public float Power { get => power; set => power = value; }

    public SpringInterpolator()
    {
    }

    public SpringInterpolator(float power, float damper, float value)
    {
        this.power = power;
        this.damper = damper;
        Value = value;
    }

    public void Update(float deltaTime)
    {
        // Calculate spring force
        float force = (Target - Value) * power;

        // Calculate damping force
        float damping = -Velocity * damper;

        // Calculate acceleration
        float acceleration = (force + damping) / 1f;

        // Update velocity
        Velocity += acceleration * deltaTime;

        // Update position
        Value += Velocity * deltaTime;
    }
}
