using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Wing.uPainter
{

    public class BaseBrushPost : ScriptableObject
    {
        protected Material mMaterial;

        protected virtual void Initial()
        {
           
        }

        public virtual void Process(Texture rawTexture, RenderTexture tempTexture, PaintPoint[] pos)
        {
            Initial();
        }
    }
}
