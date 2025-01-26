using System;
using UnityEngine;

namespace HuskyUnity.UI.Elements.RoundedRect
{
    [Serializable]
    public struct RoundedRectStyleCornerAxis
    {
        [SerializeField] private float radius;
        [SerializeField] private RectUnit radiusUnits;

        public readonly float Radius => !IsRounded ? 0.0f : radius;
        public readonly bool IsRounded => radius > 0.01f;
        public readonly RectUnit RadiusUnits => radiusUnits;

        public readonly float CalculateSize(Vector2 maxSize)
        {
            return radiusUnits switch
            {
                RectUnit.PercentOfWidth => Math.Min(maxSize.x, maxSize.x * radius),
                RectUnit.PercentOfHeight => Math.Min(maxSize.y, maxSize.y * radius),
                _ => Radius,
            };
        }
    }
}
