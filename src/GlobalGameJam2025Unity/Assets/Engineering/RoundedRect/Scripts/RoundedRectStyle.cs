using System;
using UnityEngine;

namespace HuskyUnity.UI.Elements.RoundedRect
{
    [Serializable]
    public struct RoundedRectStyle
    {

        [SerializeField] private RoundedRectStyleCorner topLeft;
        [SerializeField] private RoundedRectStyleCorner topRight;
        [SerializeField] private RoundedRectStyleCorner bottomLeft;
        [SerializeField] private RoundedRectStyleCorner bottomRight;

        public readonly RoundedRect Build(Rect area)
        {
            var size = area.size;
            var topLeftSize = topLeft.CalculateCornerSizes(size);
            var topRightSize = topRight.CalculateCornerSizes(size);
            var bottomLeftSize = bottomLeft.CalculateCornerSizes(size);
            var bottomRightSize = bottomRight.CalculateCornerSizes(size);

            // Top horizontal overflow.
            RescaleToFitSize(
                size,
                ref topLeftSize, topLeft.OverflowPreserveAspect,
                ref topRightSize, topRight.OverflowPreserveAspect,
                true);

            // Bottom horizontal overflow.
            RescaleToFitSize(
                size,
                ref bottomLeftSize, bottomLeft.OverflowPreserveAspect,
                ref bottomRightSize, bottomRight.OverflowPreserveAspect,
                true);

            // Left vertical overflow.
            RescaleToFitSize(
                size,
                ref topLeftSize, topLeft.OverflowPreserveAspect,
                ref bottomLeftSize, bottomLeft.OverflowPreserveAspect,
                false);

            // Right vertical overflow.
            RescaleToFitSize(
                size,
                ref topRightSize, topRight.OverflowPreserveAspect,
                ref bottomRightSize, bottomRight.OverflowPreserveAspect,
                false);

            return new RoundedRect(
                area,
                topLeftSize,
                topRightSize,
                bottomLeftSize,
                bottomRightSize);
        }

        private readonly void RescaleToFitSize(
            Vector2 size,
            ref Vector2 corner1,
            bool shouldPreserveCorner1Aspect,
            ref Vector2 corner2,
            bool shouldPreserveCorner2Aspect,
            bool scalingHorizontally)
        {
            float rawBottomX = scalingHorizontally ?
                corner1.x + corner2.x : corner1.y + corner2.y;
            float sizeToFit = scalingHorizontally ? size.x : size.y;

            if (rawBottomX > sizeToFit)
            {
                float percentOverflow = rawBottomX / sizeToFit;
                if (shouldPreserveCorner1Aspect)
                {
                    corner1 /= percentOverflow;
                }
                else
                {
                    if (scalingHorizontally)
                    {
                        corner1.x /= percentOverflow;
                    }
                    else
                    {
                        corner1.y /= percentOverflow;
                    }
                }

                if (shouldPreserveCorner2Aspect)
                {
                    corner2 /= percentOverflow;
                }
                else
                {
                    if (scalingHorizontally)
                    {
                        corner2.x /= percentOverflow;
                    }
                    else
                    {
                        corner2.y /= percentOverflow;
                    }
                }
            }
        }
    }
}
