using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wing.uPainter
{
    using Object = UnityEngine.Object;
    public delegate void TextureChangedEvent(PaintCanvasLayer layer, string textureName, RenderTexture texture, object data);
    public delegate void BeforeBlendEvent(PaintCanvasLayer layer,RenderTexture texture, out RenderTexture uvMap);

    /// <summary>
    /// layer is the real paint on, manage brush draw on texture
    /// </summary>
    [System.Serializable]
    public class PaintCanvasLayer
    {
        /// <summary>
        /// event of active texture changed
        /// </summary>
        public event TextureChangedEvent OnShowTextureChanged;
        /// <summary>
        /// event of before this circle drawing blend with main drawing texture
        /// </summary>
        public event BeforeBlendEvent OnBeforeBlend;

        #region Protect Fields
        private PaintCanvas _paintCanvas;
        private bool _lastEnableTempTexture = false;
        private Vector2 _textureSize = new Vector2(512, 512);

        private string _rawTextureName = "_RawTex";
        private int _rawTexturePropertyID;

        private string _overlayTexName = "_OverlayTex";
        private int _overlayTexPropertyID;

        private string _uvMapTexName = "_UVRemapTex";
        private int _uvMapTexPropertyID;

        private int _blendTypePropertyID;
        private object _customData;

        private bool _needClearTemp = false;
        #endregion

        #region Serialized Fields
        /// <summary>
        /// Set layer support alpha channel
        /// </summary>
        [SerializeField]
        public bool EnableAlpha = false;

        [SerializeField]
        private string _textureName = "";
        /// <summary>
        /// texture name in material
        /// </summary>
        public string TextureName
        {
            get
            {
                return _textureName;
            }
        }

        [SerializeField]
        private Color _backgroundColor = Color.white;
        /// <summary>
        /// when didnot set raw texture, will fill this color to created texture
        /// </summary>
        public Color BackgroundColor
        {
            get
            {
                return _backgroundColor;
            }
            set
            {
                _backgroundColor = value;
            }
        }

        [SerializeField]
        private Material _blendMaterial;
        /// <summary>
        /// blend material for circle drawing texture and main drawing texture
        /// </summary>
        public Material BlendMaterial
        {
            get
            {
                if(_blendMaterial == null)
                {
                    _blendMaterial = GameObject.Instantiate<Material>(Resources.Load<Material>("Materials/uPainter.Blend.Blender"));
                }

                return _blendMaterial;
            }
            set
            {
                _blendMaterial = value;
            }
        }

        #endregion

        #region Public Fields

        /// <summary>
        /// filter mode of texture
        /// when you want use pixel mode, you should change this value to FilterMode.Point
        /// </summary>
        [SerializeField]
        public FilterMode TextureFilterMode = FilterMode.Bilinear;

        /// <summary>
        /// when locked, will can not paint to this layer
        /// </summary>
        [SerializeField]
        public bool Locked = false;

        [HideInInspector]
        [NonSerialized]
        private Texture _rawTexture;
        /// <summary>
        /// the initial texture, when not none, painting will base on it
        /// </summary>
        public Texture RawTexture
        {
            get
            {
                return _rawTexture;
            }
        }

        [HideInInspector]
        [NonSerialized]
        private RenderTexture _paintTempTexture;
        /// <summary>
        /// temp texture to paint
        /// </summary>
        protected RenderTexture PaintTempTexture
        {
            get
            {
                if (_paintTempTexture == null)
                {
                    int width = (int)_textureSize.x;
                    int height = (int)_textureSize.y;

                    _paintTempTexture = TextureTool.CreateRenderTexture(PaintTexture, width, height, false, filterMode: TextureFilterMode);
                    _paintTempTexture.name = "PaintTempTexture";
                }

                return _paintTempTexture;
            }
        }
        /// <summary>
        /// copy from raw texture for paint
        /// </summary>
        [HideInInspector]
        [NonSerialized]
        private RenderTexture _paintTexture;
        /// <summary>
        /// Paint target texture
        /// </summary>
        public RenderTexture PaintTexture
        {
            get
            {
                if(_paintTexture == null)
                {
                    _paintTexture = SetupRenderTexture(RawTexture);
                    _paintTexture.name = "PaintTexture";
                }

                return _paintTexture;
            }
        }

        public RenderTexture GetShowTexture(bool temp)
        {
            if (temp)
            {
                return PaintTempTexture;
            }

            return PaintTexture;
        }

        private LayerSettings _settings = null;
        /// <summary>
        /// layer settings
        /// </summary>
        public LayerSettings Settings
        {
            get
            {
                return _settings;
            }
            private set
            {
                if (_settings != value)
                {
                    _settings = value;
                    UpdateSettings();
                }
            }
        }

        #endregion

        #region Protected Methods

        private RenderTexture SetupRenderTexture(Texture baseTex)
        {
            _rawTexture = baseTex;
            int width = (int)_textureSize.x;
            int height = (int)_textureSize.y;
            return TextureTool.CreateRenderTexture(baseTex, width, height, background: BackgroundColor, filterMode: TextureFilterMode);
        }

        internal void CheckShowTexture(BaseBrush brush)
        {
            if(brush == null)
            {
                return;
            }

            var needTemp = brush.DrawToTempLayer();
            if (_lastEnableTempTexture != needTemp)
            {
                _lastEnableTempTexture = needTemp;

                var showTexture = GetShowTexture(needTemp);
                if(_lastEnableTempTexture)
                {
                    Graphics.Blit(PaintTexture, PaintTempTexture);
                }
                //else
                //{
                //    Object.Destroy(PaintTempTexture);
                //    _paintTempTexture = null;
                //}

                if (OnShowTextureChanged != null)
                {
                    OnShowTextureChanged(this, TextureName, showTexture, _customData);
                }
            }
        }

        /// <summary>
        /// before drawing, set data to material
        /// </summary>
        /// <param name="tempTexture"></param>
        /// <param name="blendMode"></param>
        private void SetData(RenderTexture tempTexture, EBlendMode blendMode)
        {
            foreach (var key in BlendMaterial.shaderKeywords)
                BlendMaterial.DisableKeyword(key);

            BlendMaterial.SetTexture(_overlayTexPropertyID, tempTexture);
            BlendMaterial.SetTexture(_rawTexturePropertyID, RawTexture);
            BlendMaterial.EnableKeyword("UPAINTER_LAYER_BLEND_" + blendMode.ToString().ToUpper());

            RenderTexture uvMap = null;
            if (OnBeforeBlend != null)
            {
                OnBeforeBlend(this, tempTexture, out uvMap);
            }
            if (uvMap != null)
            {
                BlendMaterial.EnableKeyword(Constants.UPAINTER_ENABLE_REMAP_UV);
                BlendMaterial.SetTexture(_uvMapTexPropertyID, uvMap);
            }

            BlendMaterial.SetFloat(_blendTypePropertyID, (int)(EnableAlpha ? EBlendType.WithAlpha : EBlendType.NoAlpha));
        }

        protected void ProcessMask(RenderTexture target)
        {
            if (_paintCanvas.MaskTexture != null)
            {
                var temp = TextureTool.GetTempRenderTexture(null, target.width, target.height, false, filterMode: target.filterMode);
                Graphics.Blit(target, temp);
                MaskBlendMaterial.Instance.Blend(temp, _paintCanvas.MaskTexture, target, _paintCanvas.InvertMask);
                RenderTexture.ReleaseTemporary(temp);
            }
        }

        private void Blend(BaseBrush brush, RenderTexture tempTexture, EBlendMode blendMode)
        {
            if (PaintTexture == null)
            {
                Debug.LogError("Paint texteure can not be none!");
                return;
            }

            ProcessMask(tempTexture);
            SetData(tempTexture, blendMode);

            var drawTemp = brush.DrawToTempLayer();
            if (!drawTemp)
            {
                Graphics.Blit(PaintTexture, PaintTempTexture, BlendMaterial);
                Graphics.Blit(PaintTempTexture, PaintTexture);
            }
            else
            {
                Graphics.Blit(PaintTexture, PaintTempTexture, BlendMaterial);

                if (brush.InPreview)
                {
                    _needClearTemp = true;
                }
            }
        }

        protected virtual void UpdateSettings()
        {
            if(Settings != null)
            {
                Locked = Settings.Locked;
                TextureFilterMode = Settings.TextureFilterMode;
                BackgroundColor = Settings.BackgroundColor;
                EnableAlpha = Settings.EnableAlpha;
                //BlendMode = Settings.BlendMode;
                if (Settings.BlendMaterial != null)
                {
                    BlendMaterial = Settings.BlendMaterial;
                }
            }
        }

        protected virtual void SetPaintData()
        {
            if(_paintCanvas != null && _paintCanvas.Brush != null)
            {
                _paintCanvas.Brush.TextureFilterMode = TextureFilterMode;
            }
        }

        #endregion

        #region Public Methods

        public virtual void Update()
        {
            UpdateSettings();

            if (_paintCanvas != null && _paintCanvas.Brush != null)
            {
                CheckShowTexture(_paintCanvas.Brush);
            }
        }

        public virtual void FixedUpdate()
        {
            if(_needClearTemp)
            {
                Graphics.Blit(PaintTexture, PaintTempTexture);

                _needClearTemp = false;
            }
        }

        internal void CopyToTemp()
        {
            if (_paintTempTexture != null)
            {
                Graphics.Blit(PaintTexture, PaintTempTexture);
            }
        }

        /// <summary>
        /// initial this layer
        /// </summary>
        /// <param name="canvas">paint canvas</param>
        /// <param name="settings">layer settings</param>
        /// <param name="data">custom data</param>
        public void Initial(PaintCanvas canvas, LayerSettings settings, object data)
        {
            _paintCanvas = canvas;
            _textureName = settings.TextureName;
            _customData = data;
            _textureSize = settings.DefaultSize;

            _rawTexturePropertyID = Shader.PropertyToID(_rawTextureName);
            _blendTypePropertyID = Shader.PropertyToID("_BlendType");
            _rawTexture = settings.RawTexture;

            if(_rawTexture != null)
            {
                _textureSize.Set(_rawTexture.width, _rawTexture.height);
            }

            _overlayTexPropertyID = Shader.PropertyToID(_overlayTexName);
            Settings = settings;

            _uvMapTexPropertyID = Shader.PropertyToID(_uvMapTexName);

            UpdateSettings();
            SetPaintData();

            if (OnShowTextureChanged != null)
            {
                OnShowTextureChanged(this, TextureName, PaintTexture, _customData);
            }
        }

        public void SetRawTexture(Texture baseTex)
        {
            _rawTexture = baseTex;
            Clear();
        }

        public void SetPaintTexture(RenderTexture texture)
        {
            _paintTexture = texture;
            if (_paintTempTexture != null)
            {
                Graphics.Blit(PaintTexture, PaintTempTexture);
            }
        }

        /// <summary>
        /// get current drawing texture
        /// </summary>
        /// <returns>active texture you can show on</returns>
        public RenderTexture GetActiveTexture()
        {
            return this.GetShowTexture(_paintCanvas.Brush.DrawToTempLayer());
        }

        public void Dispose()
        {
            RenderTexture.active = null;

            if (_paintTexture != null)
            {
                Object.DestroyImmediate(_paintTexture);
                _paintTexture = null;
            }

            if (_paintTempTexture != null)
            {
                Object.DestroyImmediate(_paintTempTexture);
                _paintTempTexture = null;
            }
        }

        /// <summary>
        /// Begin a draw circle
        /// </summary>
        /// <param name="brush">brush instance</param>
        public void BeginDraw(BaseBrush brush)
        {
            CheckShowTexture(brush);
            if (brush != null && brush.DrawToTempLayer())
            {
                Graphics.Blit(PaintTexture, PaintTempTexture);
            }
        }

        /// <summary>
        /// end a draw circle
        /// </summary>
        /// <param name="brush">brush instance</param>
        public void EndDraw(BaseBrush brush)
        {
            if (brush != null && brush.DrawToTempLayer())
            {
                if (brush.IsComposite || !brush.SelfOverlay)
                {
                    Graphics.Blit(PaintTempTexture, PaintTexture);
                }

                //TextureTool.SaveToFile(PaintTexture, "./temp.jpg", 512, 512, EPictureType.JPG);
            }
            CheckShowTexture(brush);
        }

        /// <summary>
        /// draw uvs to texture
        /// </summary>
        /// <param name="brush">brush instance</param>
        /// <param name="pos">points of uv, some brush need more than one point</param>
        public virtual void Draw(BaseBrush brush, PaintPoint[] pos)
        {
            var rt = brush.Paint(this, pos);
            if (rt != null)
            {
                SetPaintData();
                Blend(brush, rt, brush.BlendMode);
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        /// <summary>
        /// save this layer's texture to file
        /// </summary>
        /// <param name="filename">image file name</param>
        /// <param name="width">image width</param>
        /// <param name="height">image height</param>
        /// <param name="type">image file type, support jpeg/png</param>
        public void Save(string filename, int width, int height, EPictureType type)
        {
            TextureTool.SaveToFile(PaintTexture, filename, width, height, type);
        }

        /// <summary>
        /// clear texture, and restore to raw texture you had set
        /// </summary>
        /// <param name="store">True will store old texture, then you can use undo</param>
        public void Clear(bool store = true)
        {
            if (store && _paintCanvas != null)
            {
                PainterOperation.Instance.Store(_paintCanvas, this);
            }

            if (_paintCanvas != null)
            {
                TextureTool.ClearTexture(PaintTexture, BackgroundColor, RawTexture);
                if (_paintTempTexture != null)
                {
                    Graphics.Blit(PaintTexture, PaintTempTexture);
                }
            }
        }
        #endregion
    }
}