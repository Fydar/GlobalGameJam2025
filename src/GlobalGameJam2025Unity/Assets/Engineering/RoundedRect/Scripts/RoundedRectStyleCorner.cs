using System;
using UnityEngine;

namespace HuskyUnity.UI.Elements.RoundedRect
{
    [Serializable]
    public struct RoundedRectStyleCorner
    {
        [SerializeField] private RoundedRectStyleCornerAxis horizontal;
        [SerializeField] private RoundedRectStyleCornerAxis vertical;
        [SerializeField] public bool OverflowPreserveAspect;

        public readonly bool IsRounded => horizontal.IsRounded && vertical.IsRounded;
        public readonly RoundedRectStyleCornerAxis Horizontal => horizontal;
        public readonly RoundedRectStyleCornerAxis Vertical => vertical;

        public readonly Vector2 CalculateCornerSizes(Vector2 maxSize)
        {
            return new Vector2(horizontal.CalculateSize(maxSize), vertical.CalculateSize(maxSize));
        }
    }
}
