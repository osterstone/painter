using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Wing.uPainter
{
    [CreateAssetMenu(menuName = "uPainter Brush/BlurBrushPost")]
    [System.Serializable]

    public class BlurBrushPost : BaseBrushPost
    {
        [SerializeField]
        public Color BlurColor = Color.yellow;
        [SerializeField]
        [Range(0, 1)]
        public float BlurWidth = 0.1f;
        [SerializeField]
        [Range(0, 5)]
        public int BlurIteration = 1;
        [SerializeField]
        [Range(0, 10)]
        public int BlurRadius = 2;
        [SerializeField]
        [Range(0, 4)]
        public int DownSample = 1;
        [SerializeField]
        public bool SharpMode = false;
        [SerializeField]
        public bool UseColor = false;

        protected override void Initial()
        {
            if(mMaterial == null)
            {
                mMaterial = GameObject.Instantiate<Material>(Resources.Load<Material>("Materials/uPainter.BrushPost.Blur"));
            }
        }

        public override void Process(Texture rawTexture, RenderTexture tempTexture, PaintPoint[] pos)
        {
            base.Process(rawTexture, tempTexture, pos);

            if(mMaterial != null && tempTexture != null)
            {
                mMaterial.mainTexture = rawTexture;

                mMaterial.SetInt("_SharpMode", SharpMode ? 1 : 0);
                mMaterial.SetTexture("_Overlay", tempTexture);

                var r1 = RenderTexture.GetTemporary(tempTexture.width >> DownSample, tempTexture.height >> DownSample, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                if (UseColor)
                {
                    mMaterial.SetColor("_BlurColor", BlurColor);
                    mMaterial.SetFloat("_BlurWidth", Mathf.Max(tempTexture.width, tempTexture.height) * BlurWidth);
                    Graphics.Blit(tempTexture, r1, mMaterial, 0);
                }
                else
                {
                    Graphics.Blit(tempTexture, r1);
                }
                
                if (BlurRadius > 0)
                {
                    var r2 = RenderTexture.GetTemporary(tempTexture.width >> DownSample, tempTexture.height >> DownSample, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                    for (var i = 0; i < BlurIteration; i++)
                    {
                        mMaterial.SetVector("_Offsets", new Vector4(0, BlurRadius, 0, 0));
                        Graphics.Blit(r1, r2, mMaterial, 1);

                        mMaterial.SetVector("_Offsets", new Vector4(BlurRadius, 0, 0, 0));
                        Graphics.Blit(r2, r1, mMaterial, 1);
                    }
                    RenderTexture.ReleaseTemporary(r2);
                }

                Graphics.Blit(r1, tempTexture);
                RenderTexture.ReleaseTemporary(r1);
            }
        }
    }
}
