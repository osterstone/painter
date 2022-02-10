using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Wing.uPainter
{    
    /// <summary>
    /// Paint base canvas
    /// manage multiply paint layers
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class PaintCanvas : MonoBehaviour //SerializedMonoBehaviour
    {
        #region Events
        public delegate void PaintEventHandler(PaintCanvas canvas, BaseBrush brush);
        /// <summary>
        /// event on circle paint start
        /// </summary>
        public event PaintEventHandler OnPaintStart;
        /// <summary>
        /// event on circle paint end
        /// </summary>
        public event PaintEventHandler OnPaintEnd;
        #endregion

        #region Serialize Fields
        /// <summary>
        /// paint brush
        /// </summary>
        [SerializeField]
        public BaseBrush Brush;

        /// <summary>
        /// mask texture, just can drawing on alpha lager than zero in mask
        /// </summary>
        [SerializeField]
        public Texture MaskTexture = null;

        /// <summary>
        /// when use mask texture, you just can drawing on alpha equal zero in mask
        /// </summary>
        [SerializeField]
        //[ShowIf("_hasMask")]
        public bool InvertMask = false;
        private bool _hasMask
        {
            get
            {
                return MaskTexture != null;
            }
        }

        #endregion

        #region Public Fields

        [NonSerialized]
        protected List<PaintCanvasLayer> _layers = new List<PaintCanvasLayer>();
        public List<PaintCanvasLayer> Layers
        {
            get
            {
                return _layers;
            }
        }

        [NonSerialized]
        private bool _drawing = false;
        /// <summary>
        /// if in drawing status
        /// </summary>
        public bool Drawing
        {
            get
            {
                return _drawing;
            }
        }

        private bool _changed = false;
        /// <summary>
        /// marked if current texture has changed in this drawing circle
        /// </summary>
        public bool Changed
        {
            get
            {
                return _changed;
            }
        }

        /// <summary>
        /// current canvas's drawer
        /// </summary>
        public Drawer Drawer
        {
            get;
            internal set;
        }

        #endregion

        #region Protected Fields

        /// <summary>
        /// if initial on awake, then you should inital yourself
        /// </summary>
        protected virtual bool InitialOnAwake
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region Protected Methods

        protected virtual void Awake()
        {
            if (InitialOnAwake)
            {
                OnInitialStart();
                Reset();
                OnInitialEnd();
            }
        }

        /// <summary>
        /// initial canvas, when you changed raw texture, you can invoke this
        /// </summary>
        public void Initial()
        {
            OnInitialStart();
            Reset();
            OnInitialEnd();
        }

        protected virtual void OnInitialStart()
        {

        }

        protected virtual void OnInitialEnd()
        {

        }

        protected virtual void Reset()
        {
            Layers.Clear();
        }

        protected virtual void Update()
        {
            if (!Drawing && Layers != null)
            {
                for (var i = 0; i < Layers.Count; i++)
                {
                    Layers[i].Update();
                }
            }
        }

        protected virtual void FixedUpdate()
        {
            if (!Drawing && Layers != null)
            {
                for (var i = 0; i < Layers.Count; i++)
                {
                    Layers[i].FixedUpdate();
                }
            }
        }

        protected virtual void OnDestroy()
        {
            if (Layers != null)
            {
                for (var i = 0; i < Layers.Count; i++)
                {
                    Layers[i].Dispose();
                }
            }
        }

        /// <summary>
        /// save current canvas textures
        /// </summary>
        private void Store()
        {
            if (Layers.Count > 0)
            {
                PainterOperation.Instance.Store(this);
            }
        }

        /// <summary>
        /// restore textures to this canvas 
        /// </summary>
        private void Restore()
        {
            PainterOperation.Instance.Undo();
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// rawing begin a new circle
        /// </summary>
        public virtual void BeginDraw()
        {
            if (_drawing)
            {
                return;
            }

            if (Brush != null)
            {
                Brush.Start();
            }
            _drawing = true;
            _changed = false;

            PaintCanvasLayer[] layers = null;
            var err = GetPaintLayers(out layers);
            if (err != EPaintStatus.Success)
            {
                return;
            }

            for (var i = 0; i < layers.Length; i++)
            {
                layers[i].BeginDraw(Brush);
            }

            Store();
        }

        /// <summary>
        /// before paint done
        /// </summary>
        public virtual void WillEndDraw()
        {
            if (!_drawing)
            {
                return;
            }

            if (Brush != null)
            {
                Brush.WillStop();
            }
        }

        /// <summary>
        /// stop current drawing circle
        /// </summary>
        public virtual void EndDraw()
        {
            if (!_drawing)
            {
                return;
            }
            _drawing = false;

            PaintCanvasLayer[] layers = null;
            var err = GetPaintLayers(out layers);
            if (err != EPaintStatus.Success)
            {
                return;
            }

            for (var i = 0; i < layers.Length; i++)
            {
                layers[i].EndDraw(Brush);
            }

            if (Brush != null)
            {
                Brush.Stop();

                if (!_changed)
                {
                    Restore();
                }
            }

            _changed = false;
        }

        /// <summary>
        /// get valiad layers
        /// </summary>
        /// <param name="layers">out paint layers</param>
        /// <returns></returns>
        public EPaintStatus GetPaintLayers(out PaintCanvasLayer[] layers)
        {
            layers = null;

            if (Layers == null || Layers.Count == 0)
            {
                return EPaintStatus.NoLayerError;
            }

            layers = Layers.Where(l => !l.Locked).ToArray();
            if (layers.Count() == 0)
            {
                return EPaintStatus.NoLayerError;
            }

            return EPaintStatus.Success;
        }

        /// <summary>
        /// convert world point to uv
        /// </summary>
        /// <param name="worldPos"></param>
        /// <param name="renderCamera"></param>
        /// <returns></returns>
        public abstract Vector2? WorldToPaintUV(Vector3 worldPos, Camera renderCamera = null);
        /// <summary>
        /// convert mouse point to uv
        /// </summary>
        /// <param name="mousePos"></param>
        /// <param name="renderCamera"></param>
        /// <returns></returns>
        public abstract Vector2? MousePointToPaintUV(Vector3 mousePos, Camera renderCamera = null);

        /// <summary>
        /// convert mouse point or world point to uv
        /// </summary>
        /// <param name="mousePos">mouse point</param>
        /// <param name="worldPos">point in world coordinate system</param>
        /// <param name="renderCamera"></param>
        /// <returns></returns>
        public Vector2? ToPaintUV(Vector3 mousePos, Vector3 worldPos, Camera renderCamera = null)
        {
            var pos = WorldToPaintUV(worldPos, renderCamera);
            if(pos == null)
            {
                pos = MousePointToPaintUV(mousePos, renderCamera);
            }

            if (pos != null)
            {
                var p = pos.Value;
                p.x = Mathf.Clamp01(p.x);
                p.y = Mathf.Clamp01(p.y);
                pos = p;
            }

            return pos;
        }

        /// <summary>
        /// Paint processing that use world-space surface position(must initial mesh).
        /// </summary>
        /// <param name="brush">Brush data.</param>
        /// <param name="uvs">paint uv list. vlaue must between 0-1</param>
        /// <param name="bezierControls">points' besizer constrol points</param>
        /// <param name="sizeOffsets">points' size offset, determin point real size</param>
        /// <param name="renderCamera">Camera to use to render the object.</param>
        /// <returns>The success or failure of the paint.</returns>
        public virtual EPaintStatus Paint(Vector2[] uvs, Vector4[] bezierControls = null, float[] sizeOffsets = null, Camera renderCamera = null)
        {
            if (renderCamera == null)
                renderCamera = Camera.main;

            var pos = new List<PaintPoint>();
            for (var i = 0; i < uvs.Length; i++)
            {
                var uv = uvs[i];
                pos.Add(new PaintPoint
                {
                    UV = uv,
                    BezierControl = bezierControls != null ? bezierControls[i] : new Vector4(uv.x, uv.y, uv.x, uv.y),
                    SizeOffset = sizeOffsets != null ? sizeOffsets[i] : 0,
                });
            }

            return PaintUVDirect(pos.ToArray());
        }

        /// <summary>
        /// Onle draw one point to layers, and the brush must support one point mode
        /// </summary>
        /// <param name="uv">paint uv, vlaue must between 0-1</param>
        /// <param name="renderCamera"></param>
        public void DrawPoint(Vector2 uv, Camera renderCamera = null)
        {
            Brush.DrawInScriptMode = true;

            var paintMode = EPaintMode.Dash;
            if(Brush is ScratchBrush)
            {
                var sb = Brush as ScratchBrush;
                paintMode = sb.PaintMode;
                sb.PaintMode = EPaintMode.Dash;
            }

            BeginDraw();
            Paint(new Vector2[] { uv }, renderCamera: renderCamera);
            WillEndDraw();
            EndDraw();

            if (Brush is ScratchBrush)
            {
                (Brush as ScratchBrush).PaintMode = paintMode;
            }

            Brush.DrawInScriptMode = false;
        }

        /// <summary>
        /// Onle draw one line to layers
        /// </summary>
        /// <param name="start">paint start uv, vlaue must between 0-1</param>
        /// <param name="start">paint end uv, vlaue must between 0-1</param>
        /// <param name="renderCamera"></param>
        public void DrawLine(Vector2 start, Vector2 end, Camera renderCamera = null)
        {
            Brush.DrawInScriptMode = true;

            var paintMode = EPaintMode.Dash;
            var interval = 0f;
            if (Brush is ScratchBrush)
            {
                var sb = Brush as ScratchBrush;
                paintMode = sb.PaintMode;
                sb.PaintMode = EPaintMode.Line;
                interval = sb.PointDistanceInterval;
                sb.PointDistanceInterval = 0;
            }

            var pts = new Vector2[] { start, start, end, end };
            BeginDraw();
            Paint(pts, renderCamera: renderCamera);
            WillEndDraw();
            // for round cap when solftness > 0 to draw end cap
            Paint(pts, renderCamera: renderCamera);
            EndDraw();

            if (Brush is ScratchBrush)
            {
                var sb = Brush as ScratchBrush;
                sb.PaintMode = paintMode;
                sb.PointDistanceInterval = interval;
            }

            Brush.DrawInScriptMode = false;
        }

        /// <summary>
		/// Paint by point array
		/// </summary>
		/// <param name="points">paint point array</param>
		/// <returns>The status of 0-success or 1-failure or 2-brush check error of the paint.</returns>
        public virtual EPaintStatus PaintUVDirect(PaintPoint[] points)
        {
            #region Error Check

            if (Brush == null)
            {
                Debug.LogError("must set a brush!");
                return EPaintStatus.NoBrushError;
            }

            PaintCanvasLayer[] layers = null;
            var err = GetPaintLayers(out layers);
            if (err != EPaintStatus.Success)
            {
                return err;
            }

            if (!Brush.InPreview && !Brush.CheckData(points))
            {
                return EPaintStatus.BrushCheckFailed;
            }

            #endregion

            foreach (var layer in layers)
            {
                var brush = Brush;
                if (OnPaintStart != null)
                {
                    brush = brush.Clone() as BaseBrush;
                    OnPaintStart(this, brush);
                }

                layer.Draw(brush, points);

                if (OnPaintEnd != null)
                {
                    OnPaintEnd(this, brush);
                }
            }

            _changed = true;
            return EPaintStatus.Success;
        }

        /// <summary>
        /// Add layer to canvas
        /// </summary>
        /// <param name="settings">Layer's setting, this will determine some default value</param>
        /// <param name="textureChangedHandle">texture vlaue change callback</param>
        /// <param name="data">custom data return by callback</param>
        /// <returns></returns>
        public PaintCanvasLayer AddLayer(LayerSettings settings, TextureChangedEvent textureChangedHandle, object data)
        {
            var layer = new PaintCanvasLayer();
            layer.OnShowTextureChanged += textureChangedHandle;
            layer.Initial(this, settings, data);
            Layers.Add(layer);

            return layer;
        }

        /// <summary>
        /// clear all layers' texture to default
        /// </summary>
        public void ClearAll()
        {
            Store();
            for (var i=0;i<Layers.Count;i++)
            {
                Layers[i].Clear(false);
            }
        }

        #endregion
    }
}
