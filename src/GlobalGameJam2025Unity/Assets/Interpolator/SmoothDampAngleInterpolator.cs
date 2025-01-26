using System.Diagnostics;
using System;
using UnityEngine;

[Serializable]
public class SmoothDampAngleInterpolator
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

    public SmoothDampAngleInterpolator()
    {
    }

    public SmoothDampAngleInterpolator(float smoothTime, float maxSpeed)
    {
        this.smoothTime = smoothTime;
        this.maxSpeed = maxSpeed;
    }

    public void Update(float deltaTime)
    {
        Value = Mathf.SmoothDampAngle(Value, Target, ref velocity, smoothTime, maxSpeed, deltaTime);
        if (float.IsNaN(Value))
        {
            Value = 0.0f;
        }
    }
}
