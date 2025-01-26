using UnityEngine;
using UnityEngine.UI;

namespace HuskyUnity.UI.Elements.RoundedRect
{
    /// <summary>
    /// A rounded rectangle graphic.
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("UI/Shapes/Rounded Rect Border", 30)]
    public sealed class RoundedRectBorderGraphic : MaskableGraphic
    {
        [SerializeField] private RoundedRectStyle roundedRectStyle;
        [SerializeField] public RectBorderStyle rectBorderStyle;

        private Material cachedMaterial;

        /// <inheritdoc/>
        public override Material materialForRendering
        {
            get
            {
                if (cachedMaterial == null)
                {
                    var shader = Shader.Find("Shader Graphs/Donut");
                    cachedMaterial = new Material(shader);
                }

                return cachedMaterial;
            }
        }

        /// <inheritdoc/>
        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            ForceMeshUpdate();
        }

        /// <inheritdoc/>
        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            ForceMeshUpdate();
        }

#if UNITY_EDITOR
        /// <inheritdoc/>
        protected override void OnValidate()
        {
            base.OnValidate();
            ForceMeshUpdate();
        }
#endif

        /// <inheritdoc/>
        public override bool Raycast(Vector2 sp, Camera eventCamera)
        {
            if (!base.Raycast(sp, eventCamera))
            {
                return false;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                sp,
                eventCamera,
                out var localPoint);

            var rect = GetPixelAdjustedRect();

            var roundedRect = roundedRectStyle.Build(rect);

            return roundedRect.ContainsPoint(localPoint);
        }

        /// <inheritdoc/>
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            var rect = GetPixelAdjustedRect();

            var roundedRect = roundedRectStyle.Build(rect);
            var roundedRectBorder = roundedRect.InsetRectBorder(rectBorderStyle);

            var quadBuffer = new UIVertex[4];

            var rectSegments = roundedRectBorder.RectSegments.AsSpan();
            for (int i = 0; i < rectSegments.Length; i++)
            {
                var rectSegment = rectSegments[i];

                PushQuad(rectSegment.rect, rectSegment.uvTopLeft, rectSegment.uvBottomRight);
            }

            var cornerSegments = roundedRectBorder.CornerSegments.AsSpan();
            for (int i = 0; i < cornerSegments.Length; i++)
            {
                var cornerSegment = cornerSegments[i];

                PushQuad(cornerSegment.rect, cornerSegment.uvTopLeft, cornerSegment.uvBottomRight);
            }

            void PushQuad(Rect quadRect, Vector4 uvMin, Vector4 uvMax)
            {
                quadBuffer[0] = new UIVertex()
                {
                    position = new Vector3(quadRect.xMin, quadRect.yMin, 0.0f),
                    color = color,
                    uv0 = new Vector4(uvMin.x, uvMax.y, uvMin.z, uvMax.w),
                    uv1 = new Vector4(uvMin.z, uvMax.w, uvMin.z, uvMax.w)
                };
                quadBuffer[1] = new UIVertex()
                {
                    position = new Vector3(quadRect.xMin, quadRect.yMax, 0.0f),
                    color = color,
                    uv0 = new Vector4(uvMin.x, uvMin.y, uvMin.z, uvMin.w),
                    uv1 = new Vector4(uvMin.z, uvMin.w, uvMin.z, uvMin.w)
                };
                quadBuffer[2] = new UIVertex()
                {
                    position = new Vector3(quadRect.xMax, quadRect.yMax, 0.0f),
                    color = color,
                    uv0 = new Vector4(uvMax.x, uvMin.y, uvMax.z, uvMin.w),
                    uv1 = new Vector4(uvMax.z, uvMin.w, uvMax.z, uvMin.w)
                };
                quadBuffer[3] = new UIVertex()
                {
                    position = new Vector3(quadRect.xMax, quadRect.yMin, 0.0f),
                    color = color,
                    uv0 = new Vector4(uvMax.x, uvMax.y, uvMax.z, uvMax.w),
                    uv1 = new Vector4(uvMax.z, uvMax.w, uvMax.z, uvMax.w)
                };

                vh.AddUIVertexQuad(quadBuffer);
            }
        }

        /// <inheritdoc/>
        protected override void UpdateMaterial()
        {
            base.UpdateMaterial();
        }

        private void ForceMeshUpdate()
        {
            SetVerticesDirty();
            SetMaterialDirty();
        }
    }
}
