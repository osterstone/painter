using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.uPainter
{
    [CreateAssetMenu(menuName = "uPainter Brush/DropperBrush")]
    [System.Serializable]
    public class DropperBrush : BaseBrush
    {
        public event Action<Color> OnDroppedColor;
        public void SetDropColorEventHandler(Action<Color> action)
        {
            OnDroppedColor = action;
        }

        [Range(0,128)]
        public byte Threshold = 0;

        bool _canUse = false;

        protected override bool SupportSize
        {
            get
            {
                return false;
            }
        }

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
            if (pos.Length != 1 || !_canUse)
            {
                return null;
            }
            _canUse = false;

            int width = paintTexture.width;
            int height = paintTexture.height;

            var p0 = pos[0];
            var x = (int)(p0.UV.x * width);
            var y = (int)(p0.UV.y * height);
            var color = TextureTool.GetColor(paintTexture, x, y);

            if(OnDroppedColor != null)
            {
                OnDroppedColor(color);
            }

            return null;
        }
    }
}
