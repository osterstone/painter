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
    [RequireComponent(typeof(RawImage))]
    public class RawImagePaintCanvas : PaintCanvas
    {
        [SerializeField]
        LayerSettings _layerSetting = new LayerSettings();

        RawImage _image;
        Canvas _canvas;
        PaintCanvasLayer _canvasLayer;

        public PaintCanvasLayer CanvasLayer
        {
            get
            {
                return _canvasLayer;
            }
        }

        protected override void OnInitialStart()
        {
            base.OnInitialStart();

            _image = GetComponent<RawImage>();
            _canvas = GetComponentInParent<Canvas>();
        }

        protected override void OnInitialEnd()
        {
            base.OnInitialEnd();

            if (_image.texture != null)
            {
                _layerSetting.RawTexture = _image.texture;
            }

            _canvasLayer = AddLayer(_layerSetting, 
                (layer, textureName, texture, data)=>{
                _image.texture = texture;
            }, null);
        }

        public override Vector2? WorldToPaintUV(Vector3 worldPos, Camera renderCamera = null)
        {
            return null;
        }

        public override Vector2? MousePointToPaintUV(Vector3 mousePos, Camera renderCamera = null)
        {
            if(_image == null)
            {
                return Vector2.zero;
            }

            Vector2 localpoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_image.rectTransform, mousePos, _canvas.worldCamera, out localpoint);

            return Rect.PointToNormalized(_image.rectTransform.rect, localpoint);
        }
    }
}
