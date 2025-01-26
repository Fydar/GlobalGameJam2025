using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace HuskyUnity.UI.Elements.RoundedRect
{
    public struct RoundedRect
    {
        // Replace with an InlineArray (also known as a fixed-size buffer) if/when Unity supports them.
        public struct RoundedRectUVRectBuffer
        {
            public RoundedRectUVRect index0;
            public RoundedRectUVRect index1;
            public RoundedRectUVRect index2;
            public RoundedRectUVRect index3;
            public RoundedRectUVRect index4;

            public Span<RoundedRectUVRect> AsSpan()
            {
                ref var firstElement = ref index0;
                return MemoryMarshal.CreateSpan(ref firstElement, 5);
            }
        }

        // Replace with an InlineArray (also known as a fixed-size buffer) if/when Unity supports them.
        public struct RoundedRectUVCornerBuffer
        {
            public RoundedRectUVCorner index0;
            public RoundedRectUVCorner index1;
            public RoundedRectUVCorner index2;
            public RoundedRectUVCorner index3;

            public Span<RoundedRectUVCorner> AsSpan()
            {
                ref var firstElement = ref index0;
                return MemoryMarshal.CreateSpan(ref firstElement, 4);
            }
        }

        public struct RoundedRectUVRect
        {
            public Rect rect;
            public Vector2 uvTopLeft;
            public Vector2 uvBottomRight;

            public RoundedRectUVRect(
                Rect rect,
                Vector2 uvTopLeft,
                Vector2 uvBottomRight)
            {
                this.rect = rect;
                this.uvTopLeft = uvTopLeft;
                this.uvBottomRight = uvBottomRight;
            }
        }

        public struct RoundedRectUVCorner
        {
            public Rect rect;
            public Vector2 uvTopLeft;
            public Vector2 uvBottomRight;
            public Vector2 circleCenter;
            public Vector2 radiusSquared;

            public RoundedRectUVCorner(
                Rect rect,
                Vector2 uvTopLeft,
                Vector2 uvBottomRight,
                Vector2 circleCenter,
                Vector2 radius)
            {
                this.rect = rect;
                this.uvTopLeft = uvTopLeft;
                this.uvBottomRight = uvBottomRight;
                this.circleCenter = circleCenter;
                radiusSquared = radius * radius;
            }
        }

        [SerializeField] internal Rect boundingRect;
        [SerializeField] internal Vector2 radiusTopLeft;
        [SerializeField] internal Vector2 radiusTopRight;
        [SerializeField] internal Vector2 radiusBottomLeft;
        [SerializeField] internal Vector2 radiusBottomRight;

        public RoundedRectUVRectBuffer RectSegments => new()
        {
            index0 = new(new(
                boundingRect.xMin + radiusBottomLeft.x,
                boundingRect.yMin + radiusBottomRight.y,
                boundingRect.width - radiusTopRight.x - radiusBottomLeft.x,
                boundingRect.height - radiusTopLeft.y - radiusBottomRight.y),
                new(0.5f, 0.5f), new(0.5f, 0.5f)),
            index1 = new(new(
                boundingRect.xMin + radiusTopLeft.x,
                boundingRect.yMax - radiusTopLeft.y,
                boundingRect.width - radiusTopLeft.x - radiusTopRight.x,
                radiusTopLeft.y), new(0.5f, 0.0f), new(0.5f, 0.5f)),
            index2 = new(new(
                boundingRect.xMax - radiusTopRight.x,
                boundingRect.yMin + radiusBottomRight.y,
                radiusTopRight.x,
                boundingRect.height - radiusTopRight.y - radiusBottomRight.y),
                new(0.5f, 0.5f), new(0.0f, 0.5f)),
            index3 = new(new(
                boundingRect.xMin + radiusBottomLeft.x,
                boundingRect.yMin,
                boundingRect.width - radiusBottomLeft.x - radiusBottomRight.x, radiusBottomRight.y),
                new(0.5f, 0.5f), new(0.5f, 0.0f)),
            index4 = new(new(
                boundingRect.xMin,
                boundingRect.yMin + radiusBottomLeft.y,
                radiusBottomLeft.x,
                boundingRect.height - radiusTopLeft.y - radiusBottomLeft.y),
                new(0.0f, 0.5f), new(0.5f, 0.5f)),
        };

        public RoundedRectUVCornerBuffer CornerSegments => new()
        {
            // Top Left
            index0 = new(
                new(boundingRect.xMin, boundingRect.yMax - radiusTopLeft.y, radiusTopLeft.x, radiusTopLeft.y),
                new(0.0f, 0.0f),
                new(0.5f, 0.5f),
                new(boundingRect.xMin + radiusTopLeft.x, boundingRect.yMax - radiusTopLeft.y),
                radiusTopLeft),
            // Top Right
            index1 = new(
                new(boundingRect.xMax - radiusTopRight.x, boundingRect.yMax - radiusTopRight.y, radiusTopRight.x, radiusTopRight.y),
                new(0.5f, 0.0f),
                new(1.0f, 0.5f),
                new(boundingRect.xMax - radiusTopRight.x, boundingRect.yMax - radiusTopRight.y), radiusTopRight),
            // Bottom Left
            index2 = new(
                new(boundingRect.xMin, boundingRect.yMin, radiusBottomLeft.x, radiusBottomLeft.y),
                new(0.0f, 0.5f),
                new(0.5f, 1.0f),
                new(boundingRect.xMin + radiusBottomLeft.x, boundingRect.yMin + radiusBottomLeft.y), radiusBottomLeft),
            // Bottom Right
            index3 = new(
                new(boundingRect.xMax - radiusBottomRight.x, boundingRect.yMin, radiusBottomRight.x, radiusBottomRight.y),
                new(0.5f, 0.5f),
                new(1.0f, 1.0f),
                new(boundingRect.xMax - radiusBottomRight.x, boundingRect.yMin + radiusBottomRight.y), radiusBottomRight),
        };

        public RoundedRect(
            Rect boundingRect,
            Vector2 radiusTopLeft,
            Vector2 radiusTopRight,
            Vector2 radiusBottomLeft,
            Vector2 radiusBottomRight)
        {
            this.boundingRect = boundingRect;
            this.radiusTopLeft = radiusTopLeft;
            this.radiusTopRight = radiusTopRight;
            this.radiusBottomLeft = radiusBottomLeft;
            this.radiusBottomRight = radiusBottomRight;
        }

        public bool ContainsPoint(Vector2 point)
        {
            // Check if the point is within the bounding rectangle
            if (!boundingRect.Contains(point))
            {
                return false;
            }

            var rectSegments = RectSegments.AsSpan();
            for (int i = 0; i < rectSegments.Length; i++)
            {
                var rectSegment = rectSegments[i];

                if (rectSegment.rect.Contains(point))
                {
                    return true;
                }
            }

            var cornerSegments = CornerSegments.AsSpan();
            for (int i = 0; i < cornerSegments.Length; i++)
            {
                var cornerSegment = cornerSegments[i];

                if (cornerSegment.rect.Contains(point))
                {
                    var localPoint = point - cornerSegment.circleCenter;
                    localPoint *= localPoint;
                    localPoint /= cornerSegment.radiusSquared;
                    if (localPoint.x + localPoint.y <= 1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public readonly RoundedRectBorder InsetRectBorder(RectBorderStyle rectBorderStyle)
        {
            return new RoundedRectBorder(
                boundingRect,
                radiusTopLeft,
                radiusTopRight,
                radiusBottomLeft,
                radiusBottomRight,
                rectBorderStyle.borderWidth);
        }
    }
}
