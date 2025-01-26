using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace HuskyUnity.UI.Elements.RoundedRect
{
    public struct RoundedRectBorder
    {
        // Replace with an InlineArray (also known as a fixed-size buffer) if/when Unity supports them.
        public struct RoundedRectBorderUVRectBuffer
        {
            public RoundedRectBorderUVRect index0;
            public RoundedRectBorderUVRect index1;
            public RoundedRectBorderUVRect index2;
            public RoundedRectBorderUVRect index3;

            public Span<RoundedRectBorderUVRect> AsSpan()
            {
                ref var firstElement = ref index0;
                return MemoryMarshal.CreateSpan(ref firstElement, 4);
            }
        }

        // Replace with an InlineArray (also known as a fixed-size buffer) if/when Unity supports them.
        public struct RoundedRectBorderUVCornerBuffer
        {
            public RoundedRectBorderUVCorner index0;
            public RoundedRectBorderUVCorner index1;
            public RoundedRectBorderUVCorner index2;
            public RoundedRectBorderUVCorner index3;

            public Span<RoundedRectBorderUVCorner> AsSpan()
            {
                ref var firstElement = ref index0;
                return MemoryMarshal.CreateSpan(ref firstElement, 4);
            }
        }

        public struct RoundedRectBorderUVRect
        {
            public Rect rect;
            public Vector4 uvTopLeft;
            public Vector4 uvBottomRight;

            public RoundedRectBorderUVRect(
                Rect rect,
                Vector4 uvTopLeft,
                Vector4 uvBottomRight)
            {
                this.rect = rect;
                this.uvTopLeft = uvTopLeft;
                this.uvBottomRight = uvBottomRight;
            }
        }

        public struct RoundedRectBorderUVCorner
        {
            public Rect rect;
            public Vector4 uvTopLeft;
            public Vector4 uvBottomRight;
            public Vector2 circleCenter;
            public Vector2 radiusSquared;

            public RoundedRectBorderUVCorner(
                Rect rect,
                Vector4 uvTopLeft,
                Vector4 uvBottomRight,
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
        [SerializeField] internal float borderWidth;

        public RoundedRectBorderUVRectBuffer RectSegments => new()
        {
            index0 = new(new(
                boundingRect.xMin + radiusTopLeft.x,
                boundingRect.yMax - borderWidth,
                boundingRect.width - radiusTopLeft.x - radiusTopRight.x,
                borderWidth),
                new(0.5f, 0.0f, 1.0f), new(0.5f, 0.25f, 1.0f)),
            index1 = new(new(
                boundingRect.xMax - borderWidth,
                boundingRect.yMin + radiusBottomRight.y,
                borderWidth,
                boundingRect.height - radiusTopRight.y - radiusBottomRight.y),
                new(0.75f, 0.5f, 1.0f), new(1.0f, 0.5f, 1.0f)),
            index2 = new(new(
                boundingRect.xMin + radiusBottomLeft.x,
                boundingRect.yMin,
                boundingRect.width - radiusBottomLeft.x - radiusBottomRight.x,
                borderWidth),
                new(0.5f, 0.75f, 1.0f), new(0.5f, 1.0f, 1.0f)),
            index3 = new(new(
                boundingRect.xMin,
                boundingRect.yMin + radiusBottomLeft.y,
                borderWidth,
                boundingRect.height - radiusTopLeft.y - radiusBottomLeft.y),
                new(0.0f, 0.5f, 1.0f), new(0.25f, 0.5f, 1.0f)),
        };

        public RoundedRectBorderUVCornerBuffer CornerSegments => new()
        {
            // Top Left
            index0 = new(
                new(boundingRect.xMin, boundingRect.yMax - radiusTopLeft.y, radiusTopLeft.x, radiusTopLeft.y),
                new(0.0f, 0.0f, 0.5f + (borderWidth / radiusTopLeft.x / 2), 0.0f),
                new(0.5f, 0.5f, 0.5f + (borderWidth / radiusTopLeft.x / 2), 0.75f),
                new(boundingRect.xMin + radiusTopLeft.x, boundingRect.yMax - radiusTopLeft.y), radiusTopLeft),
            // Top Right
            index1 = new(
                new(boundingRect.xMax - radiusTopRight.x, boundingRect.yMax - radiusTopRight.y, radiusTopRight.x, radiusTopRight.y),
                new(0.5f, 0.0f, 0.5f + (borderWidth / radiusTopLeft.x / 2), 0.0f),
                new(1.0f, 0.5f, 0.5f + (borderWidth / radiusTopLeft.x / 2), 0.5f),
                new(boundingRect.xMax - radiusTopRight.x, boundingRect.yMax - radiusTopRight.y), radiusTopRight),
            // Bottom Left
            index2 = new(
                new(boundingRect.xMin, boundingRect.yMin, radiusBottomLeft.x, radiusBottomLeft.y),
                new(0.0f, 0.5f, 0.5f + (borderWidth / radiusTopLeft.x / 2), 0.5f),
                new(0.5f, 1.0f, 0.5f + (borderWidth / radiusTopLeft.x / 2), 1.0f),
                new(boundingRect.xMin + radiusBottomLeft.x, boundingRect.yMin + radiusBottomLeft.y), radiusBottomLeft),
            // Bottom Right
            index3 = new(
                new(boundingRect.xMax - radiusBottomRight.x, boundingRect.yMin, radiusBottomRight.x, radiusBottomRight.y),
                new(0.5f, 0.5f, 0.5f + (borderWidth / radiusTopLeft.x / 2), 0.5f),
                new(1.0f, 1.0f, 0.5f + (borderWidth / radiusTopLeft.x / 2), 1.0f),
                new(boundingRect.xMax - radiusBottomRight.x, boundingRect.yMin + radiusBottomRight.y), radiusBottomRight),
        };

        public RoundedRectBorder(
            Rect boundingRect,
            Vector2 radiusTopLeft,
            Vector2 radiusTopRight,
            Vector2 radiusBottomLeft,
            Vector2 radiusBottomRight,
            float borderWidth)
        {
            this.boundingRect = boundingRect;
            this.radiusTopLeft = radiusTopLeft;
            this.radiusTopRight = radiusTopRight;
            this.radiusBottomLeft = radiusBottomLeft;
            this.radiusBottomRight = radiusBottomRight;
            this.borderWidth = borderWidth;
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
    }
}
