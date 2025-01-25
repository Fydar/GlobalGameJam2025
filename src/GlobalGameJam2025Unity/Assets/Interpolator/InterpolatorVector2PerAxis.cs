using System.Diagnostics;
using System;
using UnityEngine;
using System.ComponentModel;

[Serializable]
[DisplayName("Per-Axis")]
public class InterpolatorVector2PerAxis : IInterpolatorVector2
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [SerializeReference] private IInterpolatorFloat x;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [SerializeReference] private IInterpolatorFloat y;

    public IInterpolatorFloat X { get => x; set => x = value; }

    public IInterpolatorFloat Y { get => y; set => y = value; }

    public Vector2 Value
    {
        get => new(x.Value, y.Value);
        set
        {
            x.Value = value.x;
            y.Value = value.y;
        }
    }

    public Vector2 Target
    {
        get => new(x.Target, y.Target);
        set
        {
            x.Target = value.x;
            y.Target = value.y;
        }
    }

    public InterpolatorVector2PerAxis()
    {

    }

    public InterpolatorVector2PerAxis(
        IInterpolatorFloat x,
        IInterpolatorFloat y)
    {
        this.x = x;
        this.y = y;
    }

    public void Update(float deltaTime)
    {
        x.Update(deltaTime);
        y.Update(deltaTime);
    }
}
