using UnityEngine;

namespace Wing.uPainter
{
    [CreateAssetMenu(menuName = "uPainter Brush/SolidBrush")]
    [System.Serializable]
    public class SolidBrush : ScratchBrush
    {
        #region Property IDs
        #endregion

        #region Serialize Fields     

        #endregion

        #region Protected Metholds
        private Material postMaterial;

        private void OnEnable()
        {
            outputUVMap = true;
        }

        protected override void OnLoadPaintMaterial()
        {
            base.OnLoadPaintMaterial();

            if (paintMaterial == null)
            {
                paintMaterial = GameObject.Instantiate<Material>(Resources.Load<Material>("Materials/uPainter.Brush.Solid"));
            }

            if(postMaterial == null)
            {
                postMaterial = GameObject.Instantiate<Material>(Resources.Load<Material>("Materials/uPainter.BrushPost.Solid"));
            }
        }

        protected override void OnInitial()
        {
            base.OnInitial();
        }

        #endregion

        #region Public Methods

        public override void SetData(Texture baseTexture, RenderTexture paintTexture, PaintPoint[] pos)
        {
            base.SetData(baseTexture, paintTexture, pos);

            if (!SelfOverlay && !InPreview)
            {
                postMaterial.SetFloat(_brushTypePropertyID, BrushType);
                postMaterial.SetVector(_brushColorPropertyID, BrushColor);
                postMaterial.SetFloat(_BrushSizePropertyID, Size);
                postMaterial.SetInt(_operationStatusPropertyID, justBegin ? 0 : (willstop ? 2 : 1));
                postMaterial.SetFloat(_noisePropertyID, NoiseRatio);
                postMaterial.SetFloat(_noiseSizePropertyID, NoiseSize);
                postMaterial.SetFloat(_softnessPropertyID, Softness);
                postMaterial.SetFloat(_totalLineDistancePropertyID, _totalLineLength);
                if (PaintMode == EPaintMode.Line)
                {
                    postMaterial.SetInt(_brushStatusPropertyID, pos.Length + 2);
                }
                else
                {
                    postMaterial.SetInt(_brushStatusPropertyID, InPreview ? 1 : 2);
                }
            }
        }

        protected override void BeforeEffect(Texture rawTexture, RenderTexture tempTexture)
        {
            base.BeforeEffect(rawTexture, tempTexture);

            if (!SelfOverlay && !InPreview)
            {
                Graphics.Blit(base.oneTimeTempTexture, tempTexture, postMaterial);
            }
        }

        #endregion

    }
}
