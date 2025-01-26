using UnityEngine;
using UnityEngine.UI;

namespace HuskyUnity.UI.Elements.RoundedRect
{
    /// <summary>
    /// A rounded rectangle graphic.
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("UI/Shapes/Rounded Rect", 30)]
    public sealed class RoundedRectGraphic : MaskableGraphic
    {
        private const string shaderTextureName = "_Texture";

        [SerializeField] private RoundedRectStyle roundedRectStyle;

        [SerializeField] private Texture mainTex;

        private Material cachedMaterial;

        /// <inheritdoc/>
        public override Material materialForRendering
        {
            get
            {
                if (cachedMaterial == null)
                {
                    var shader = Shader.Find("Shader Graphs/Circle");
                    cachedMaterial = new Material(shader);
                    cachedMaterial.SetTexture(shaderTextureName, mainTex);
                }

                return cachedMaterial;
            }
        }

        public new Texture mainTexture
        {
            get => mainTex;
            set
            {
                mainTex = value;
                if (cachedMaterial != null)
                {
                    cachedMaterial.SetTexture(shaderTextureName, mainTex);
                }
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

            var quadBuffer = new UIVertex[4];

            var rectSegments = roundedRect.RectSegments.AsSpan();
            for (int i = 0; i < rectSegments.Length; i++)
            {
                var rectSegment = rectSegments[i];

                PushQuad(rectSegment.rect, rectSegment.uvTopLeft, rectSegment.uvBottomRight);
            }

            var cornerSegments = roundedRect.CornerSegments.AsSpan();
            for (int i = 0; i < cornerSegments.Length; i++)
            {
                var cornerSegment = cornerSegments[i];

                PushQuad(cornerSegment.rect, cornerSegment.uvTopLeft, cornerSegment.uvBottomRight);
            }

            void PushQuad(Rect quadRect, Vector2 uvMin, Vector2 uvMax)
            {
                quadBuffer[0] = new UIVertex()
                {
                    position = new Vector3(quadRect.xMin, quadRect.yMin, 0.0f),
                    color = color,
                    uv0 = new Vector4(uvMin.x, uvMax.y),
                    uv1 = new Vector4(
                        Mathf.InverseLerp(rect.xMin, rect.xMax, quadRect.xMin),
                        Mathf.InverseLerp(rect.yMin, rect.yMax, quadRect.yMin))
                };
                quadBuffer[1] = new UIVertex()
                {
                    position = new Vector3(quadRect.xMin, quadRect.yMax, 0.0f),
                    color = color,
                    uv0 = new Vector4(uvMin.x, uvMin.y),
                    uv1 = new Vector4(
                        Mathf.InverseLerp(rect.xMin, rect.xMax, quadRect.xMin),
                        Mathf.InverseLerp(rect.yMin, rect.yMax, quadRect.yMax))
                };
                quadBuffer[2] = new UIVertex()
                {
                    position = new Vector3(quadRect.xMax, quadRect.yMax, 0.0f),
                    color = color,
                    uv0 = new Vector4(uvMax.x, uvMin.y),
                    uv1 = new Vector4(
                        Mathf.InverseLerp(rect.xMin, rect.xMax, quadRect.xMax),
                        Mathf.InverseLerp(rect.yMin, rect.yMax, quadRect.yMax))
                };
                quadBuffer[3] = new UIVertex()
                {
                    position = new Vector3(quadRect.xMax, quadRect.yMin, 0.0f),
                    color = color,
                    uv0 = new Vector4(uvMax.x, uvMax.y),
                    uv1 = new Vector4(
                        Mathf.InverseLerp(rect.xMin, rect.xMax, quadRect.xMax),
                        Mathf.InverseLerp(rect.yMin, rect.yMax, quadRect.yMin))
                };

                vh.AddUIVertexQuad(quadBuffer);
            }
        }

        /// <inheritdoc/>
        protected override void UpdateMaterial()
        {
            base.UpdateMaterial();
            materialForRendering.SetTexture(shaderTextureName, mainTex);
        }

        private void ForceMeshUpdate()
        {
            SetVerticesDirty();
            SetMaterialDirty();
        }
    }
}
