using System.Diagnostics;
using System;
using UnityEngine;

[Serializable]
public class SmoothDampInterpolator : IInterpolatorFloat
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [SerializeField] private float smoothTime = 1.0f;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [SerializeField] private float maxSpeed = 1000.0f;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [SerializeField] private float velocity;

    public float Target { get; set; }

    public float Value { get; set; }

    public float Velocity { get => velocity; set => velocity = value; }

    public SmoothDampInterpolator()
    {
    }

    public SmoothDampInterpolator(float smoothTime, float maxSpeed)
    {
        this.smoothTime = smoothTime;
        this.maxSpeed = maxSpeed;
    }

    public void Update(float deltaTime)
    {
        Value = Mathf.SmoothDamp(Value, Target, ref velocity, smoothTime, maxSpeed, deltaTime);
    }
}
