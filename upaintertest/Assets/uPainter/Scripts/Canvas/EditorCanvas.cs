using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wing.uPainter
{
    [ExecuteInEditMode]
    public class EditorCanvas : PaintCanvas
    {
        public event Action ChangeTextured;

        LayerSettings _layerSetting = new LayerSettings();
        PaintCanvasLayer _canvasLayer;

        public Rect CanvasPosition;

        public Texture RawTexture;
        public Texture PaintTexture;

        public PaintCanvasLayer Layer
        {
            get
            {
                return _canvasLayer;
            }
        }

        protected override bool InitialOnAwake
        {
            get
            {
                return false;
            }
        }

        protected override void OnInitialStart()
        {
            base.OnInitialStart();
        }

        protected override void OnInitialEnd()
        {
            base.OnInitialEnd();

            _canvasLayer = AddLayer(_layerSetting, 
                (layer, textureName, texture, data)=>{
                    PaintTexture = texture;

                    if(ChangeTextured != null)
                    {
                        ChangeTextured();
                    }
            }, null);
        }

        public override Vector2? WorldToPaintUV(Vector3 worldPos, Camera renderCamera = null)
        {
            return null;
        }

        public override Vector2? MousePointToPaintUV(Vector3 mousePos, Camera renderCamera = null)
        {
            var pos = mousePos;
            pos.x = (pos.x) / CanvasPosition.width;
            pos.y = 1 - (pos.y) / CanvasPosition.height;

            return pos;
        }

        public void OnUpdate()
        {
            Update();
        }

        public void OnGUI()
        {
            FixedUpdate();
        }
    }
}
