using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Wing.uPainter
{
    [CreateAssetMenu(menuName = "uPainter Brush/OutlineBrushPost")]
    [System.Serializable]

    public class OutlineBrushPost : BaseBrushPost
    {
        [SerializeField]
        public Color OutlineColor = Color.yellow;
        [SerializeField]
        [Range(0,1)]
        public float OutlineWidth = 0.01f;

        protected override void Initial()
        {
            if(mMaterial == null)
            {
                mMaterial = GameObject.Instantiate<Material>(Resources.Load<Material>("Materials/uPainter.BrushPost.Outline"));
            }
        }

        public override void Process(Texture rawTexture, RenderTexture tempTexture, PaintPoint[] pos)
        {
            base.Process(rawTexture, tempTexture, pos);

            if(mMaterial != null && tempTexture != null)
            {
                mMaterial.SetColor("_OutlineColor", OutlineColor);
                mMaterial.SetFloat("_OutlineWidth", Mathf.Max(tempTexture.width, tempTexture.height) * OutlineWidth);

                var temp = RenderTexture.GetTemporary(tempTexture.width, tempTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                Graphics.Blit(tempTexture, temp, mMaterial);
                Graphics.Blit(temp, tempTexture);

                RenderTexture.ReleaseTemporary(temp);
            }
        }
    }
}
