using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wing.uPainter
{
    [CreateAssetMenu(menuName = "uPainter Brush/CompositeBrush")]
    [System.Serializable]
    public class CompositeBrush : BaseBrush
    {
        public List<BaseBrush> Brushes;

        #region Property IDs
        #endregion

        protected override void OnInitial()
        {
            base.OnInitial();

            BrushType = 5;
        }

        public override int NeedPointNumber
        {
            get
            {
                return Brushes.Count > 0 ? Brushes.FindAll(i => i != null).Select(i => i.NeedPointNumber).Max() : 1;
            }
        }

        public override bool IsComposite
        {
            get
            {
                return true;
            }
        }

        public override void Start()
        {
            Brushes.ForEach(item =>
            {
                if (item != null)
                {
                    item.Start();
                    item.Parent = this;
                }
            });
            base.Start();
        }

        public override void Stop()
        {
            Brushes.ForEach(item =>
            {
                if (item != null)
                {
                    item.Stop();
                    item.Parent = null;
                }
            });

            base.Stop();
        }

        public override void WillStop()
        {
            Brushes.ForEach(item =>
            {
                if (item != null)
                {
                    item.WillStop();
                }
            });
            base.WillStop();
        }

        public override bool CheckData(PaintPoint[] pos)
        {
            return Brushes.Count > 0 ? Brushes.TrueForAll(item => item == null || item != null && item.CheckData(pos)) : false;
        }

        public override bool DrawToTempLayer()
        {
            return true;// base.DrawToTempLayer() || !SelfOverlay && Brushes.Any(item => item != null && item.DrawToTempLayer());
        }

        public override RenderTexture Paint(Texture rawTexture, RenderTexture paintTexture, PaintPoint[] pos, int defaultWidth = 1024, int defaultHeight = 1024, RenderTexture target = null)
        {
            lastPaintPos = pos;

            int width = defaultWidth, height = defaultHeight;
            if (rawTexture != null)
            {
                width = rawTexture.width;
                height = rawTexture.height;
            }
            RenderTexture paintTextureBuffer = target != null ? target : TextureTool.GetTempRenderTexture(null, width, height, false, new Color(0, 0, 0, 0), TextureFilterMode);
            SetData(rawTexture, paintTexture, pos);

            for (int i = 0; i < Brushes.Count; i++)
            {
                var item = Brushes[i];
                if (item == null)
                {
                    continue;
                }

                item.ShowPreview = ShowPreview;
                item.UseShaderSmooth = UseShaderSmooth;

                var texture = item.Paint(rawTexture, paintTexture, pos, defaultWidth, defaultHeight);
                if (texture != null)
                {
                    var temp = TextureTool.GetTempRenderTexture(null, width, height, false, new Color(0, 0, 0, 0), filterMode:TextureFilterMode);
                    BlendMaterial.Instance.Blend(paintTextureBuffer, rawTexture, texture, temp, item.BlendMode);
                    Graphics.Blit(temp, paintTextureBuffer);

                    RenderTexture.ReleaseTemporary(texture);
                    RenderTexture.ReleaseTemporary(temp);
                }
            }

            BeforeEffect(rawTexture, paintTextureBuffer);
            PostEffect(rawTexture, paintTextureBuffer);
            return paintTextureBuffer;
        }
    }
}
