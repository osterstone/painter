using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wing.uPainter
{
    [CreateAssetMenu(menuName = "uPainter Brush/TextureBrush")]
    [System.Serializable]
    public class TextureBrush : ScratchBrush
    {
        #region Property IDs
        private int _brushRotatePropertyID;
        protected int _brushTexturePropertyID;
        protected int _brushTextureSTPropertyID;
        protected int _enableGlobalUVPropertyID;
        protected int _enableGlobalRepeatCountPropertyID;
        #endregion

        #region Serialize Fields
        [SerializeField]
        private Texture _brushTexture;
        public Texture BrushTexture
        {
            get
            {
                return _brushTexture;
            }
            set
            {
                _brushTexture = value;
            }
        }

        [SerializeField, Range(0, 360)]
        private float _rotateAngle = 0;
        /// <summary>
        /// Rotate angle of the brush.
        /// </summary>
        public float RotateAngle
        {
            get { return _rotateAngle; }
            set { _rotateAngle = value; }
        }

        [SerializeField]
        public Vector2 Tilling = Vector2.one;

        [SerializeField]
        public Vector2 Offset = Vector2.zero;

        [SerializeField]
        //[ShowIf("PaintMode", EPaintMode.Dash)]
        //[HideIf("EnableGlobalUV")]
        public bool RotateFllowDirection = false;

        [SerializeField]
        public bool EnableGlobalUV = false;

        [SerializeField]
        //[ShowIf("EnableGlobalUV")]
        public bool EnableGlobalRepeatCount = false;

        #endregion

        Vector2? _lastPos;

        #region Protected Metholds

        protected override void OnInitial()
        {
            base.OnInitial();

            BrushType = 2;

            _brushRotatePropertyID = Shader.PropertyToID("_BrushRotate");
            _brushTexturePropertyID = Shader.PropertyToID("_Brush");
            _brushTextureSTPropertyID = Shader.PropertyToID("_Brush_ST");
            _enableGlobalUVPropertyID = Shader.PropertyToID("_EnableGlobalUV");
            _enableGlobalRepeatCountPropertyID = Shader.PropertyToID("_EnableGlobalRepeatCount");
        }

        protected override void OnLoadPaintMaterial()
        {
            base.OnLoadPaintMaterial();

            if (paintMaterial == null)
            {
                paintMaterial = GameObject.Instantiate<Material>(Resources.Load<Material>("Materials/uPainter.Brush.Texture"));                
            }
        }

        public override int NeedPointNumber
        {
            get
            {
                if (PaintMode == EPaintMode.Dash)
                {
                    return RotateFllowDirection ? 2 : 1;
                }
                else
                {
                    return base.NeedPointNumber;
                }
            }
        }

        public override bool CheckData(PaintPoint[] pos)
        {
            if (pos == null || pos.Length < 1)
                return false;

            if(RotateFllowDirection)
            {
                if(pos.Length < 2)
                {
                    return false;
                }
            }

            if (PaintMode == EPaintMode.Dash)
            {
                var ret = _lastPos == null ? true : Vector2.Distance(pos[0].UV, _lastPos.Value) >= PointDistanceInterval;
                if (ret)
                {
                    _lastPos = pos[0].UV;
                }
                return ret;
            }

            return base.CheckData(pos);
        }

        public override void Start()
        {
            base.Start();

            _lastPos = null;
        }

        #endregion

        #region Public Methods

        public override void SetData(Texture baseTexture, RenderTexture paintTexture, PaintPoint[] pos)
        {
            base.SetData(baseTexture, paintTexture, pos);

            var rotoffset = 0f;
            if(RotateFllowDirection && !EnableGlobalUV && PaintMode == EPaintMode.Dash)
            {
                if (pos.Length > 1)
                {
                    rotoffset = Vector2.Angle(pos[1].UV - pos[0].UV, Vector2.right);
                }
            }
            PaintMaterial.SetFloat(_brushRotatePropertyID, Mathf.Deg2Rad * (RotateAngle + rotoffset));
            PaintMaterial.SetTexture(_brushTexturePropertyID, BrushTexture);
            PaintMaterial.SetVector(_brushTextureSTPropertyID, new Vector4(Tilling.x, Tilling.y, Offset.x, Offset.y));
            PaintMaterial.SetInt(_enableGlobalUVPropertyID, EnableGlobalUV ? 1 : 0);
            PaintMaterial.SetInt(_enableGlobalRepeatCountPropertyID, EnableGlobalRepeatCount ? 1 : 0);

            if (!UtilsHelper.IsAndroidGLVersionLGT3())
            {
                PaintMaterial.SetInt(_enableOverlayPropertyID, 1);
            }
        }

        #endregion

    }
}
