using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace Wing.uPainter
{
    [CreateAssetMenu(menuName = "uPainter Brush/GrapicBrush")]
    [System.Serializable]
    public class GrapicBrush : BaseBrush
    {
        protected override void OnInitial()
        {
            base.OnInitial();

            BrushType = 3;
        }

        public override bool SupportShaderSmooth
        {
            get
            {
                return false;
            }
        }

        protected override void OnLoadPaintMaterial()
        {
            base.OnLoadPaintMaterial();

            if (paintMaterial == null)
            {
                paintMaterial = GameObject.Instantiate<Material>(Resources.Load<Material>("Materials/uPainter.Brush.Graphic"));
            }
        }

        public override RenderTexture Paint(Texture rawTexture, RenderTexture paintTexture, PaintPoint[] pos, int defaultWidth = 1024, int defaultHeight = 1024, RenderTexture target = null)
        {
            int width = defaultWidth, height = defaultHeight;
            if (rawTexture != null)
            {
                width = rawTexture.width;
                height = rawTexture.height;
            }

            var paintTextureBuffer = target != null ? target : RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

            SetData(rawTexture, paintTexture, pos);

            Graphics.SetRenderTarget(paintTextureBuffer);            
            GL.Clear(true, true, new Color(0,0,0,0));
            GL.PushMatrix();
            PaintMaterial.SetPass(0);
            GL.LoadOrtho();
            OnRender(pos);
            GL.PopMatrix();
            Graphics.SetRenderTarget(null);

            PostEffect(rawTexture, paintTextureBuffer);

            return paintTextureBuffer;
        }

        protected virtual void OnRender(PaintPoint[] pos)
        {
            if (pos.Length < 3)
                return;
            GL.Begin(GL.LINES);
            GL.TexCoord2(0.0f, 0.0f);  GL.Vertex3(pos[1].UV.x, pos[1].UV.y, 0f);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(pos[2].UV.x, pos[2].UV.y, 0f);
            GL.End();
        }
    }
}
