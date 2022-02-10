using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.uPainter
{
    [CreateAssetMenu(menuName = "uPainter Brush/SealBrush")]
    [System.Serializable]
    public class SealBrush : BaseBrush
    {
        public enum ESealClipShape
        {
            Rect,
            Circle,
        }

        public event Action<RenderTexture> OnClipTexture;
        public Texture ClipTexture;
        /// <summary>
        /// when set clip texture, enable will repace target's alpha from clip texture
        /// </summary>
        public bool ReplaceClipTextureAlpha = true;
        public Vector2 TargetTextureSize = new Vector2(300, 300);
        public ESealClipShape ClipShape = ESealClipShape.Rect;
        public Color FilterColor = new Color(1, 1, 1, 1);
        [Range(0,1)]
        public float FilterColorThreshold = 0.1f;

        public void SetClipTextureEventHandler(Action<RenderTexture> action)
        {
            OnClipTexture = action;
        }

        bool _canUse = false;

        public override void Start()
        {
            base.Start();

            _canUse = true;
        }

        public override void Stop()
        {
            base.Stop();

            _canUse = false;
        }

        public override int NeedPointNumber
        {
            get
            {
                return 1;
            }
        }

        public override bool CheckData(Vector2[] pos, float[] sizeOffset)
        {
            if (willstop || InPreview)
            {
                return false;
            }

            return pos.Length == 1;
        }

        public override bool CheckData(PaintPoint[] pos)
        {
            if(willstop || InPreview)
            {
                return false;
            }

            return pos.Length == 1;
        }

        public override RenderTexture Paint(Texture rawTexture, RenderTexture paintTexture, PaintPoint[] pos, int defaultWidth = 1024, int defaultHeight = 1024, RenderTexture temp = null)
        {
            if (pos.Length != 1) // || !_canUse)
            {
                return null;
            }

            int width = paintTexture.width;
            int height = paintTexture.height;

            var p0 = pos[0];
            var target = TextureTool.CreateRenderTexture(null, (int)TargetTextureSize.x, (int)TargetTextureSize.y);
            GrabArea.UseRectShape = ClipShape == ESealClipShape.Rect;
            GrabArea.FilterColor = FilterColor;
            GrabArea.FilterColorThreshold = FilterColorThreshold;
            GrabArea.Clip(ClipTexture, Size, paintTexture, p0.UV, 0, GrabArea.GrabTextureWrapMode.Clip, target, ReplaceClipTextureAlpha);

            if(OnClipTexture != null)
            {
                OnClipTexture(target);
            }

            return null;
        }
    }
}
