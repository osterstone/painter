using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
//using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wing.uPainter
{
    public abstract class ScratchBrush : BaseBrush
    {
        #region Property IDs
        protected int _paintUVsPropertyID;
        protected int _softnessPropertyID;
        protected int _noisePropertyID;
        protected int _noiseSizePropertyID;
        protected int _brushStatusPropertyID;
        protected int _totalLineDistancePropertyID;
        protected int _controlsPropertyID;
        protected int _enableOverlayPropertyID;
        #endregion

        #region Serialize Fields

        [SerializeField]
        public bool PixelMode = false;

        [SerializeField, Range(0.0f, 2)]
        private float _softness = 0.0f;
        public float Softness
        {
            get
            {
                if(_softness > 0)
                {
                    return _softness;
                }

                return _softness + 0.0001f;
            }
            set
            {
                _softness = value;
            }
        }

        [SerializeField]
        [Range(0, 1)]
        public float NoiseRatio = 0;

        [SerializeField]
        [Range(0.01f, 10)]
        //[ShowIf("_withNoise")]
        public float NoiseSize = 0;
        private bool _withNoise
        {
            get
            {
                return NoiseRatio >= 0.0001f;
            }
        }

        [SerializeField]
        public EPaintMode PaintMode = EPaintMode.Line;

        [SerializeField]
        public EBrushCapStyle CapStyle = EBrushCapStyle.Round;

        [SerializeField]
        //[ShowIf("PaintMode", EPaintMode.Line)]
        //[HideIf("CapStyle", EBrushCapStyle.Round)]
        public EBrushJointStyle LineJointStyle = EBrushJointStyle.Flat;

        #endregion

        #region Protected Fileds

        protected float _totalLineLength;

        #endregion

        #region Protected Metholds

        public override int NeedPointNumber
        {
            get
            {
                return PaintMode == EPaintMode.Line ? 4 : 1;
            }
        }

        protected virtual bool EnableLineOptimize(PaintPoint[] pos)
        {
            return pos.Length >= 3;
        }

        protected override void OnInitial()
        {
            base.OnInitial();

            BrushType = 1;

            _paintUVsPropertyID = Shader.PropertyToID("_PaintUVs");
            _softnessPropertyID = Shader.PropertyToID("_Softness");
            _noisePropertyID = Shader.PropertyToID("_Noise");
            _noiseSizePropertyID = Shader.PropertyToID("_NoiseSize");
            _brushStatusPropertyID = Shader.PropertyToID("_BrushStatus");
            _totalLineDistancePropertyID = Shader.PropertyToID("_TotalLineDistance");
            _controlsPropertyID = Shader.PropertyToID("_Controls");
            _enableOverlayPropertyID = Shader.PropertyToID("_EnableOverlay");

            if (PaintMaterial != null)
            {
                PaintMaterial.SetVectorArray(_paintUVsPropertyID, new List<Vector4>() { Vector4.zero, Vector4.zero, Vector4.zero, Vector4.zero });
                PaintMaterial.SetVectorArray(_controlsPropertyID, new List<Vector4>() { Vector4.zero, Vector4.zero, Vector4.zero, Vector4.zero });
            }
        }

        #endregion

        #region Public Methods

        public override void Start()
        {
            base.Start();

            _totalLineLength = 0;
        }

        public override void Stop()
        {
            base.Stop();
        }

        public override bool CheckData(Vector2[] pos, float[] sizeOffset)
        {
            if (willstop &&
                (PaintMode == EPaintMode.Dash || Softness == 0 || PaintMode == EPaintMode.Line && CapStyle == EBrushCapStyle.Flat))
            {
                return false;
            }

            if (pos == null || pos.Length == 0)
                return false;

            if (PaintMode == EPaintMode.Line && !willstop)
            {
                if (pos.Length == 2)
                {
                    return (pos[0] - pos[1]).magnitude >= PointDistanceInterval;
                }
                else if (pos.Length == 3)
                {
                    return (pos[1] - pos[2]).magnitude >= PointDistanceInterval;
                }
                else if (pos.Length >= 4)
                {
                    return (pos[2] - pos[3]).magnitude >= PointDistanceInterval;
                }
            }

            return true;
        }

        public override bool CheckData(PaintPoint[] pos)
        {
            //if(willstop && 
            //    (PaintMode == EPaintMode.Dash || Softness == 0 || PaintMode == EPaintMode.Line && CapStyle == EBrushCapStyle.Flat))
            //{
            //    return false;
            //}

            if (pos == null || pos.Length == 0)
            {
                return false;
            }

            if (PaintMode == EPaintMode.Line && !willstop)
            {
                if (pos.Length == 2)
                {
                    return (pos[0].UV - pos[1].UV).magnitude >= PointDistanceInterval;
                }
                else if (pos.Length == 3)
                {
                    return (pos[1].UV - pos[2].UV).magnitude >= PointDistanceInterval;
                }
                else if (pos.Length >= 4)
                {
                    return (pos[2].UV - pos[3].UV).magnitude >= PointDistanceInterval;
                }
            }

            return true;
        }

        public override void SetData(Texture baseTexture, RenderTexture paintTexture, PaintPoint[] pos)
        {
            base.SetData(baseTexture, paintTexture, pos);

            PaintMaterial.SetFloat(_totalLineDistancePropertyID, _totalLineLength);
            if (PaintMode == EPaintMode.Line)
            {
                PaintMaterial.SetInt(_brushStatusPropertyID, pos.Length + 2);

                switch (pos.Length)
                {
                    case 1:
                        PaintMaterial.EnableKeyword(Constants.UPAINTER_POINT_MODE);
                        break;
                    default:
                        PaintMaterial.EnableKeyword(Constants.UPAINTER_LINE_MODE);

                        if (UseShaderSmooth)
                        {
                            PaintMaterial.EnableKeyword(Constants.UPAINTER_ENABLE_BEZIER);
                        }

                        if (!IsEqualWidthLine(pos))
                        {
                            PaintMaterial.EnableKeyword(Constants.UPAINTER_NEQ_WIDTH_LINE);
                        }
                        break;
                }
            }
            else
            {
                PaintMaterial.SetInt(_brushStatusPropertyID, InPreview ? 1 : 2);

                PaintMaterial.EnableKeyword(Constants.UPAINTER_POINT_MODE);
            }

            switch (CapStyle)
            {
                case EBrushCapStyle.Flat:
                    PaintMaterial.EnableKeyword(Constants.UPAINTER_CAP_FLAT);
                    break;
                case EBrushCapStyle.Round:
                    PaintMaterial.EnableKeyword(Constants.UPAINTER_CAP_ROUND);
                    break;
            }

            if (Softness == 0 || CapStyle == EBrushCapStyle.Flat)
            {
                switch (LineJointStyle)
                {
                    case EBrushJointStyle.Flat:
                        PaintMaterial.EnableKeyword(Constants.UPAINTER_CORNER_FLAT);
                        break;
                    case EBrushJointStyle.Round:
                        PaintMaterial.EnableKeyword(Constants.UPAINTER_CORNER_ROUND);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                PaintMaterial.EnableKeyword(Constants.UPAINTER_CORNER_ROUND);
            }

            PaintMaterial.SetFloat(_softnessPropertyID, Softness);
            var uvs = RelayoutUVs(pos, Size);

            if(PixelMode)
            {
                var pw = 1f / paintTexture.width;
                var ph = 1f / paintTexture.height;

                for (var i=0;i<uvs.Length;i++)
                {
                    uvs[i].x = (int)(uvs[i].x / pw) / (float)paintTexture.width + pw * 0.25f;
                    uvs[i].y = (int)(uvs[i].y / ph) / (float)paintTexture.height + ph * 0.25f;
                }
            }
            PaintMaterial.SetVectorArray(_paintUVsPropertyID, uvs);

//#if DEBUG
//            string info = "";
//            for(var i=0;i<uvs.Length;i++)
//            {
//                info += uvs[i].ToString() + "\n";
//            }
//            Debug.Log(info);
//#endif

            PaintMaterial.SetFloat(_noisePropertyID, NoiseRatio);
            PaintMaterial.SetFloat(_noiseSizePropertyID, NoiseSize);
            PaintMaterial.SetVectorArray(_controlsPropertyID, pos.Select(p => p.BezierControl).ToArray());
            PaintMaterial.SetInt(_enableOverlayPropertyID, (SelfOverlay || InPreview) ? 1 : 0);

            if (uvs.Length == 2)
            {
                _totalLineLength += (uvs[0] - uvs[1]).magnitude / Size;
            }
            else if (uvs.Length >= 3)
            {
                _totalLineLength += (uvs[1] - uvs[2]).magnitude / Size;
            }

        }

        public override RenderTexture Paint(Texture rawTexture, RenderTexture paintTexture, PaintPoint[] pos, int defaultWidth = 1024, int defaultHeight = 1024, RenderTexture target = null)
        {
            if (!InPreview && pos.Length < 4 && PaintMode == EPaintMode.Line)
            {
                return null;
            }
            
            return base.Paint(rawTexture, paintTexture, pos, defaultWidth, defaultHeight, target);
        }

        #endregion
    }
}
