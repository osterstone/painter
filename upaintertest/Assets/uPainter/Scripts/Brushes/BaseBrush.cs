using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wing.uPainter
{
    public class PaintPoint
    {
        public Vector2 UV;
        public Vector4 BezierControl;
        public float SizeOffset;
        public float AlphaOffset;

        public PaintPoint()
        {

        }

        public PaintPoint(Vector2 uv, float sizeOffset, float alphaOffset = 0)
        {
            UV = uv;
            SizeOffset = sizeOffset;
            AlphaOffset = alphaOffset;
            BezierControl = new Vector4(uv.x, uv.y, uv.x, uv.y);
        }

        public Vector4 ToVector(float size = 0)
        {
            return new Vector4(UV.x, UV.y, AlphaOffset, SizeOffset + size);
        }
    }

    [System.Serializable]
    public class BaseBrush : ScriptableObject, ICloneable //SerializedScriptableObject
    {
        #region Property IDs

        protected int _brushColorPropertyID;
        protected int _brushTypePropertyID;
        protected int _BrushSizePropertyID;
        protected int _operationStatusPropertyID;
        protected int _inScriptModePropertyID;

        #endregion

        #region Serialize Fields

        //[HideIf("IsComposite")]
        [SerializeField]
        private Color _brushColor = Color.black;
        /// <summary>
        /// Color of brush
        /// </summary>
        public Color BrushColor
        {
            get
            {
                return _brushColor;
            }
            set
            {
                _brushColor = value;
            }
        }

        //[ShowIf("SupportSize")]
        //[HideIf("IsComposite")]
        [SerializeField, Range(0.001f, 1)]
        private float _size = 0.01f;
        /// <summary>
        /// Size of brush
        /// </summary>
        public float Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
            }
        }

        /// <summary>
        /// Minimum between two points
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        protected float _pointDistanceInterval = 0.001f;

        /// <summary>
        /// If true, will overlay paint mark in one paint circle lift time
        /// </summary>
        public bool SelfOverlay = true;

        /// <summary>
        /// Effects after paint
        /// </summary>
        [SerializeField]
        public BaseBrushPost[] PostEffects;
               
        [SerializeField]
        private EBlendMode _blendMode = EBlendMode.Normal;
        /// <summary>
        /// Brush blend mode, determine how to blend with other layer
        /// </summary>
        public EBlendMode BlendMode
        {
            get
            {
                return _blendMode;
            }
            set
            {
                _blendMode = value;
            }
        }
        #endregion

        #region Proctectd Fileds

        [NonSerialized]
        private bool _initialed = false;

        [NonSerialized]
        internal bool ShowPreview = false;

        [NonSerialized]
        internal FilterMode TextureFilterMode = FilterMode.Bilinear;

        [NonSerialized]
        internal bool DrawInScriptMode = false;

        /// <summary>
        /// preview status, when in preview , brush only draw in temp texture
        /// </summary>
        public bool InPreview
        {
            get
            {
                return !painting;
            }
        }

        /// <summary>
        /// Brush's unique type id
        /// </summary>
        [NonSerialized]        
        protected int BrushType = 0;

        /// <summary>
        /// When open this, will use shader bezier mode
        /// </summary>
        [NonSerialized]
        internal bool UseShaderSmooth = true;

        /// <summary>
        /// Brush's paint metarial,which real paint the texture, can assign by custom
        /// </summary>
        [NonSerialized]
        protected Material paintMaterial;
        public Material PaintMaterial
        {
            get
            {
                return paintMaterial;
            }
            set
            {
                paintMaterial = value;
            }
        }

        protected bool painting = false;
        protected bool willstop = false;
        protected bool justBegin = true;
        protected PaintPoint[] lastPaintPos;
        protected RenderTexture oneTimeTempTexture;
        protected bool outputUVMap = false;

        public bool Painting
        {
            get
            {
                return painting;
            }
        }

        protected bool FinalOutputUVMap
        {
            get
            {
                return outputUVMap && !InPreview;
            }
        }

        /// <summary>
        /// Two points's distance must large then this value
        /// </summary>
        [NonSerialized]
        internal float? TempPointDistanceInterval = null;

        /// <summary>
        /// have value only when this brush in composite brush
        /// </summary>
        public BaseBrush Parent
        {
            get;
            internal set;
        }

        #endregion

        #region Public Fileds

        /// <summary>
        /// this brush is a composite brush
        /// </summary>
        public virtual bool IsComposite
        {
            get
            {
                return false;
            }
        }

        // 笔刷需要的点数量
        /// <summary>
        /// only paint to texture when points count large than this value
        /// </summary>
        public virtual int NeedPointNumber
        {
            get
            {
                return 3;
            }
        }

        /// <summary>
        /// means this brush support shader smooth
        /// </summary>
        /// <see cref="UseShaderSmooth"/>
        public virtual bool SupportShaderSmooth
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// means this brush support Size property
        /// </summary>
        /// <see cref="Size"/>
        protected virtual bool SupportSize
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region Protected Methods

        private void initial()
        {
            if (!_initialed)
            {              
                OnLoadPaintMaterial();
                OnInitial();
                _initialed = true;
            }
        }

        internal float PointDistanceInterval
        {
            get
            {
                if(TempPointDistanceInterval != null)
                {
                    return TempPointDistanceInterval.Value;
                }
                return _pointDistanceInterval;
            }
            set
            {
                _pointDistanceInterval = value;
            }
        }

        protected virtual Vector4[] RelayoutUVs(PaintPoint[] points, float size)
        {
            return points.Select(p => p.ToVector(size)).ToArray();
        }

        protected bool IsEqualWidthLine(PaintPoint[] points)
        {
            if(points != null)
            {
                if(points.Length > 1)
                {
                    float offset = points[0].SizeOffset;
                    for(int i=1;i< points.Length; i++)
                    {
                        if(offset != points[i].SizeOffset)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        protected virtual void OnLoadPaintMaterial()
        {
        }

        protected virtual void OnInitial()
        {
            _brushColorPropertyID = Shader.PropertyToID("_ControlColor");
            _brushTypePropertyID = Shader.PropertyToID("_BrushType");
            _BrushSizePropertyID = Shader.PropertyToID("_BrushSize");
            _operationStatusPropertyID = Shader.PropertyToID("_OperationStatus");
            _inScriptModePropertyID = Shader.PropertyToID("_InScriptMode");
        }

        protected virtual void PrepareForPaint()
        {
            
        }

        protected virtual void OnDestroy()
        {
            if(paintMaterial)
            {
                Destroy(paintMaterial);
                paintMaterial = null;
            }

            ClearTemp();
        }
        #endregion

        #region Public Methods

        public RenderTexture Paint(PaintCanvasLayer layer, PaintPoint[] pos)
        {
            return Paint(layer.RawTexture, layer.PaintTexture, pos, layer.PaintTexture.width, layer.PaintTexture.height);
        }

        public virtual RenderTexture Paint(Texture rawTexture, RenderTexture paintTexture, PaintPoint[] pos, int defaultWidth = 1024, int defaultHeight = 1024, RenderTexture temp = null)
        {
            if(justBegin && FinalOutputUVMap && !SelfOverlay)
            {
                oneTimeTempTexture = TextureTool.GetTempRenderTexture(null, paintTexture.width, paintTexture.height, false, new Color(0, 0, 0, 0), paintTexture.filterMode);
            }                

            lastPaintPos = pos;

            int width = defaultWidth, height = defaultHeight;
            if (rawTexture != null)
            {
                width = rawTexture.width;
                height = rawTexture.height;
            }

            var paintTextureBuffer = temp != null ? temp : RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            SetData(rawTexture, paintTexture, pos);

            if(FinalOutputUVMap && !SelfOverlay)
            {
                Graphics.Blit(oneTimeTempTexture, paintTextureBuffer, PaintMaterial);
                Graphics.Blit(paintTextureBuffer, oneTimeTempTexture);
            }
            else
            {
                Graphics.Blit(null, paintTextureBuffer, PaintMaterial);
            }

            BeforeEffect(rawTexture, paintTextureBuffer);
            PostEffect(rawTexture, paintTextureBuffer);

            // RenderTexture.ReleaseTemporary(paintTextureBuffer);
            justBegin = false;
            return paintTextureBuffer;
        }

        protected virtual void BeforeEffect(Texture rawTexture, RenderTexture tempTexture)
        {

        }

        protected virtual void PostEffect(Texture rawTexture, RenderTexture tempTexture)
        {
            if (!FinalOutputUVMap && DrawToTempLayer())
            {
                var useTemp = !InPreview &&
                    !IsComposite &&
                    (!SelfOverlay || Parent != null) &&
                    (Parent != null && Parent.DrawToTempLayer() || Parent == null);

                if (useTemp)
                {
                    if (oneTimeTempTexture == null)
                    {
                        // must render transport color(0,0,0,0). or will render old frame to _tempTexture
                        oneTimeTempTexture = TextureTool.GetTempRenderTexture(null, tempTexture.width, tempTexture.height, false, new Color(0, 0, 0, 0), tempTexture.filterMode);
                    }

                    var tempTex = RenderTexture.GetTemporary(tempTexture.width, tempTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

                    OverlayBlendMaterial.Instance.Blend(oneTimeTempTexture, tempTexture, tempTex);
                    Graphics.Blit(tempTex, oneTimeTempTexture);
                    Graphics.Blit(tempTex, tempTexture);

                    RenderTexture.ReleaseTemporary(tempTex);
                }
            }

            InternalPostEffect(rawTexture, tempTexture);
        }

        internal virtual void InternalPostEffect(Texture rawTexture, RenderTexture tempTexture)
        {
            if (WithPostProcess)
            {
                foreach (var post in PostEffects)
                {
                    if (post != null)
                    {
                        post.Process(rawTexture, tempTexture, lastPaintPos);
                    }
                }
            }
        }
        
        public bool WithPostProcess
        {
            get
            {
                return PostEffects != null && PostEffects.Select(p=>p!=null).Count() > 0;
            }
        }

        public virtual bool DrawToTempLayer()
        {
            return ShowPreview && InPreview || 
                !SelfOverlay && painting ||
                Parent != null;
        }

        public virtual void Start()
        {
            painting = true;
            willstop = false;
            justBegin = true;

            ClearTemp();
        }

        public virtual void Stop()
        {
            painting = false;
            lastPaintPos = null;
            justBegin = true;
            willstop = false;

            ClearTemp();
        }

        public virtual void WillStop()
        {
            willstop = true;
        }

        public void ClearTemp()
        {
            if(oneTimeTempTexture != null)
            {
                RenderTexture.ReleaseTemporary(oneTimeTempTexture);
                oneTimeTempTexture = null;
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public virtual bool CheckData(Vector2[] pos, float[] sizeOffset)
        {
            return !willstop;
        }

        public virtual bool CheckData(PaintPoint[] pos)
        {
            return !willstop;
        }

        public virtual void SetData(Texture baseTexture, RenderTexture paintTexture, PaintPoint[] pos)
        {
            initial();

            PrepareForPaint();

            if (PaintMaterial != null)
            {
                foreach (var key in PaintMaterial.shaderKeywords)
                {
                    if (key != "UPAINTER_BRUSH_TYPE")
                    {
                        PaintMaterial.DisableKeyword(key);
                    }
                }
                PaintMaterial.SetFloat(_brushTypePropertyID, BrushType);
                PaintMaterial.SetVector(_brushColorPropertyID, BrushColor);
                PaintMaterial.SetFloat(_BrushSizePropertyID, Size);
                PaintMaterial.SetInt(_operationStatusPropertyID, justBegin ? 0 : (willstop ? 2 : 1));
                PaintMaterial.SetInt(_inScriptModePropertyID, DrawInScriptMode ? 1 : 0);
            }
        }

        #endregion
    }

    //[CustomPropertyDrawer(typeof(BaseBrush), false)]
    //public class BrushDrawer : PropertyDrawer
    //{
    //    // Cached scriptable object editor
    //    private Editor editor = null;
    //    private SerializedObject _serializedObject = null;

    //    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //    {
    //        // Draw label
    //        EditorGUI.PropertyField(position, property, label, true);

    //        // Draw foldout arrow
    //        if (property.objectReferenceValue != null)
    //        {
    //            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, GUIContent.none);

    //            if (_serializedObject == null)
    //            {
    //                _serializedObject = new SerializedObject(property.objectReferenceValue);
    //            }
    //        }

    //        // Draw foldout properties
    //        if (property.isExpanded)
    //        {
    //            // Make child fields be indented
    //            var indent = EditorGUI.indentLevel;
    //            EditorGUI.indentLevel++;

    //            //// Draw object properties
    //            if (!editor)
    //            {
    //                Editor.CreateCachedEditor(property.objectReferenceValue, null, ref editor);
    //            }

    //            if (editor != null && !property.displayName.Contains("Element "))
    //            {
    //                editor.OnInspectorGUI();
    //            }

    //            EditorGUI.indentLevel = indent;
    //        }
    //    }
    //}
}
