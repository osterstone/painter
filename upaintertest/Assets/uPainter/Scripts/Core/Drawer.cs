using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Wing.uPainter
{
    /// <summary>
    /// painter delegate, pass in points to draw on layer
    /// points,controls,offsets must have same count
    /// </summary>
    /// <param name="points">uv points</param>
    /// <param name="controls">uv besizer controls</param>
    /// <param name="offsets">width offset, control point size</param>
    /// <returns>paint status</returns>
    public delegate EPaintStatus PaintHandle(List<Vector2> points, List<Vector4> controls, List<float> widthOffsets);
    /// <summary>
    /// generator current point width offset by previous points
    /// </summary>
    /// <param name="points">previous uv points</param>
    /// <param name="controls">previous uv besizer controls</param>
    /// <param name="offsets">previous width offsets</param>
    /// <returns>current width offset</returns>
    public delegate float WidthOffsetHandle(List<Vector2> points, List<Vector4> controls, List<float> widthOffsets);

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class Drawer
    {
        public delegate void DrawEventHandler(Drawer sender, Vector3 mousePos, Vector3 worldPos, Vector2? uv = null);
        /// <summary>
        /// event of add point
        /// </summary>
        public event DrawEventHandler AddPointEvent;
        /// <summary>
        /// event of drawing start
        /// </summary>
        public event Action<Drawer> StartEvent;
        /// <summary>
        /// event of drawing end
        /// </summary>
        public event Action<Drawer> EndEvent;

        /// <summary>
        /// when not set brush to canvas, will use brush to replace
        /// </summary>
        [SerializeField]
        public BaseBrush ReplaceBrush;

        /// <summary>
        /// if true, will show preview effect on texture
        /// </summary>
        [SerializeField]
        public bool ShowPreview = false;

        /// <summary>
        /// simulate pressure by point width offset, when you move fast get a smaller width, then you get a larger width
        /// </summary>
        [SerializeField]
        public bool SimulatePressure = false;

        /// <summary>
        /// strength of width offset when you enable SimulatePressure
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        //[ShowIf("SimulatePressure")]
        public float BrushPressureStrength = 0.5f;

        /// <summary>
        /// Bezier smooth strength, when value is zero, will close smooth
        /// </summary>
        [SerializeField]
        [Range(0, 0.5f)]
        public float CornerSmooth = 0.1f;
        private bool _withSmooth
        {
            get
            {
                return CornerSmooth > 0.0001;
            }
        }

        /// <summary>
        /// if you want use shader besizer smooth, or use CPU to calculate smooth points
        /// </summary>
        [SerializeField]
        //[ShowIf("_shaderSmooth")]
        public bool UseShaderSmooth = true;
        private bool _shaderSmooth
        {
            get
            {
                return _withSmooth && (CanDraw() && _currentCanvas.Brush.SupportShaderSmooth);
            }
        }

        /// <summary>
        /// bezier interpolation points count when CornerSmooth larger than zero 
        /// </summary>
        [SerializeField]
        [Range(0, 5)]
        //[HideIf("UseShaderSmooth")]
        public int CurveInterpolation = 3;

        private List<Vector2> _points = new List<Vector2>();
        private List<Vector4> _controls = new List<Vector4>();
        private List<float> _widthOffsets = new List<float>();

        private PaintCanvas _currentCanvas;
        /// <summary>
        /// current drawing canvas
        /// </summary>
        public PaintCanvas CurrentCanvas
        {
            get
            {
                return _currentCanvas;
            }
        }

        /// <summary>
        /// if in preview status
        /// </summary>
        public bool InPreview
        {
            get
            {
                return CanDraw() && CurrentCanvas.Brush.InPreview;
            }
        }

        bool _lastTouchDown = false;

        public Drawer()
        {
            _widthOffsetHandle = WidthOffsetHandle;
            _paintHandle = PaintHandle;
        }

        PaintHandle _paintHandle;
        /// <summary>
        /// replace default paint handle by yourself
        /// </summary>
        /// <param name="handle">paint handle</param>
        public void SetPaintHandle(PaintHandle handle)
        {
            _paintHandle = handle;
        }

        WidthOffsetHandle _widthOffsetHandle;
        /// <summary>
        /// replace default width offset handle by yourself
        /// </summary>
        /// <param name="handle"></param>
        public void SetWidthOffsetHandle(WidthOffsetHandle handle)
        {
            _widthOffsetHandle = handle;
        }

        /// <summary>
        /// set current drawing canvas
        /// </summary>
        /// <param name="canvas">paint canvs</param>
        public void Catch(PaintCanvas canvas)
        {
            if (canvas != CurrentCanvas)
            {                
                End();

                _currentCanvas = canvas;
                _currentCanvas.Drawer = this;
            }
        }

        /// <summary>
        /// begin a circle drawing
        /// </summary>
        public void Begin()
        {
            if (_currentCanvas != null && !_lastTouchDown)
            {
                _currentCanvas.BeginDraw();
                _lastTouchDown = true;
            }

            if (!CanDraw())
            {
                Debug.LogError("You must set a canvas to drawer and set a brush to canvas!");
            }

            if(StartEvent != null)
            {
                StartEvent(this);
            }
        }

        /// <summary>
        /// end a circle drawing
        /// </summary>
        public void End()
        {
            if (CanDraw() && _lastTouchDown)
            {
                if(CurrentCanvas.Brush.NeedPointNumber > 1 && _points.Count > 1)
                {
                    CurrentCanvas.WillEndDraw();

                    if (CurrentCanvas.Brush.NeedPointNumber == _points.Count)
                    {
                        _paintHandle(_points, _controls, _widthOffsets);
                    }
                }

                if (EndEvent != null)
                {
                    EndEvent(this);
                }

                _points.Clear();
                _controls.Clear();
                _widthOffsets.Clear();
                _currentCanvas.EndDraw();

                _lastTouchDown = false;
            }
        }

        /// <summary>
        /// check if can drawing on canvas
        /// </summary>
        /// <returns></returns>
        bool CanDraw()
        {
            if(CurrentCanvas != null && CurrentCanvas.Brush == null)
            {
                CurrentCanvas.Brush = ReplaceBrush;
            }

            return CurrentCanvas != null && CurrentCanvas.Brush != null;
        }

        /// <summary>
        /// set some runtime paramters for canvs or bursh
        /// </summary>
        void SetPaintData()
        {
            CurrentCanvas.Brush.UseShaderSmooth = UseShaderSmooth && CurveInterpolation > 0;
            CurrentCanvas.Brush.ShowPreview = ShowPreview;
        }

        // 生成倒数第二个点的控制点
        /// <summary>
        /// generate second to last bezier control point
        /// </summary>
        /// <param name="points">previous points</param>
        /// <param name="smooth">smooth strength</param>
        /// <returns></returns>
        protected virtual Vector4 GenerateCurveControl(List<Vector2> points, float smooth)
        {
            var MIN_DIST = 0.001;
            var ret = Vector4.zero;
            if (points.Count == 0)
            {
            }
            else
            {
                if (points.Count <= 2)
                {
                    var last = points.Last();
                    ret.x = last.x;
                    ret.y = last.y;
                    ret.z = last.x;
                    ret.w = last.y;
                }
                else
                {
                    var last = points[2];
                    ret.x = last.x;
                    ret.y = last.y;
                    ret.z = last.x;
                    ret.w = last.y;

                    var size = points.Count;
                    var p0 = points[0];
                    var p1 = points[1];
                    var p2 = points[2];
                    var v10 = (p1 - p0);
                    var v12 = (p1 - p2);
                    var nv10 = v10.normalized;
                    var nv12 = v12.normalized;
                    var angle012 = Vector2.Angle(nv10, nv12);
                    var minDist012 = Mathf.Min(v10.magnitude, v12.magnitude) * smooth;

                    var c01 = p1 + nv12 * minDist012;
                    var c12 = p1 + nv10 * minDist012;

                    if (points.Count == 3)
                    {
                        if (minDist012 > MIN_DIST && Mathf.Abs(angle012 - 180) > 0.01f)
                        {
                            ret.x = c01.x;
                            ret.y = c01.y;
                            ret.z = c12.x;
                            ret.w = c12.y;
                        }
                    }
                    else
                    {
                        var p3 = points[3];
                        var v21 = p2 - p1;
                        var v23 = p2 - p3;
                        var nv21 = v21.normalized;
                        var nv23 = v23.normalized;
                        var angle123 = Vector2.Angle(nv21, nv23);
                        var minDist123 = Mathf.Min(v21.magnitude, v23.magnitude) * smooth;

                        if (minDist123 > MIN_DIST && Mathf.Abs(angle123 - 180) > 0.01f)
                        {
                            var c21 = p2 + nv23 * minDist123;
                            var c23 = p2 + nv21 * minDist123;

                            ret.x = c21.x;
                            ret.y = c21.y;
                            ret.z = c23.x;
                            ret.w = c23.y;
                        }
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// generate current width offset by previous points
        /// </summary>
        /// <param name="points"></param>
        /// <param name="controls"></param>
        /// <param name="offsets"></param>
        /// <returns></returns>
        protected virtual float WidthOffsetHandle(List<Vector2> points, List<Vector4> controls, List<float> offsets)
        {
            if (BrushPressureStrength == 0 
                || CanDraw() && _currentCanvas.Brush.NeedPointNumber <= 2) // do not support dash mode
            {
                return 0;
            }

            if (points.Count < 2)
            {
                return -CurrentCanvas.Brush.Size * BrushPressureStrength;
            }

            var size = points.Count;
            var len = (points[size - 1] - points[size - 2]).magnitude;
            var dw = Mathf.Pow(len, 2) * BrushPressureStrength;
            var offset = Mathf.Min(Mathf.Abs(dw), CurrentCanvas.Brush.Size);
            offset = -offset;

            return (offsets[offsets.Count - 1] + offset) * 0.5f;
        }

        /// <summary>
        /// draw to canvas
        /// </summary>
        /// <param name="points"></param>
        /// <param name="controls"></param>
        /// <param name="offsets"></param>
        /// <returns></returns>
        protected virtual EPaintStatus PaintHandle(List<Vector2> points, List<Vector4> controls, List<float> offsets)
        {
            return CurrentCanvas.Paint(points.ToArray(),
                controls == null ? null : controls.ToArray(),
                offsets == null ? null : offsets.ToArray());
        }

        /// <summary>
        /// add point when touch down
        /// </summary>
        /// <param name="mousePos"></param>
        /// <param name="worldPos"></param>
        /// <param name="directUV"></param>
        public void TouchMove(Vector3 mousePos, Vector3 worldPos, Vector2? directUV = null)
        {
            if (CanDraw())
            {
                if (!InPreview)
                {
                    Vector2? vUV = null;
                    if (directUV != null)
                    {
                        vUV = directUV;
                    }
                    else
                    {
                        vUV = CurrentCanvas.ToPaintUV(mousePos, worldPos);
                    }
                    if(vUV == null)
                    {
                        return;
                    }
                    var uv = vUV.Value;
                    uv.x = Mathf.Clamp01(uv.x);
                    uv.y = Mathf.Clamp01(uv.y);

                    if (_paintHandle != null)
                    {
                        if (_points.Count > 0 && _points.Last() == uv)
                        {
                            return;
                        }

                        SetPaintData();

                        if (!UseShaderSmooth &&
                            CornerSmooth != 0 && 
                            CurveInterpolation != 0 && 
                            CurrentCanvas.Brush.NeedPointNumber > 3)
                        {
                            drawBezier(uv);
                        }
                        else
                        {
                            draw(uv);
                        }
                    }

                    if (AddPointEvent != null)
                    {
                        AddPointEvent(this, mousePos, worldPos, uv);
                    }
                }
            }
        }

        /// <summary>
        /// use cpu to drawing bezier
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        bool drawBezier(Vector2 position)
        {
            // add end point
            _points.Add(position);
            if (_points.Count > CurrentCanvas.Brush.NeedPointNumber)
            {
                _points.RemoveAt(0);
            }

            var control = GenerateCurveControl(_points, CornerSmooth);
            // 为上倒数第二个控制点点赋值
            if (_controls.Count >= 2)
            {
                _controls[_controls.Count - 1] = control;
                // 控制点占位
                _controls.Add(new Vector4(position.x, position.y, position.x, position.y));
            }
            else
            {
                _controls.Add(control);
            }

            if (_controls.Count > CurrentCanvas.Brush.NeedPointNumber)
            {
                _controls.RemoveAt(0);
            }

            var offset = 0f;
            if (SimulatePressure && _widthOffsetHandle != null)
            {
                offset = _widthOffsetHandle(_points, _controls, _widthOffsets);
            }
            _widthOffsets.Add(offset);
            if (_widthOffsets.Count > CurrentCanvas.Brush.NeedPointNumber)
            {
                _widthOffsets.RemoveAt(0);
            }

            if(!CurrentCanvas.Brush.CheckData(_points.ToArray(), _widthOffsets.ToArray()))
            {
                _points.RemoveAt(_points.Count - 1);
                _controls.RemoveAt(_controls.Count - 1);
                _widthOffsets.RemoveAt(_widthOffsets.Count - 1);
            }

            if (_points.Count < CurrentCanvas.Brush.NeedPointNumber)
            {
                return false;
            }

            // calculate bezier points in point1 && point2
            var p1 = _points[1];
            var p2 = _points[2];

            var c1 = _controls[1];
            var c2 = _controls[2];

            var offset1 = _widthOffsets[1];
            var offset2 = _widthOffsets[2];

            var bezier = new CubicBezierCurve(new[] { new Vector3(p1.x, p1.y, 0),
                                                        new Vector3(c1.z, c1.w, 0),
                                                        new Vector3(c2.x, c2.y, 0),
                                                        new Vector3(p2.x, p2.y, 0) });

            var pts = new List<Vector2>();
            var ctrls = new List<Vector4>();
            var offsets = new List<float>();

            var bezierDelta = 1.0f / (CurveInterpolation + 1);
            for (var i = 1; i <= CurveInterpolation; i++)
            {
                var t = i * bezierDelta;
                var p = bezier.GetPoint(t);
                pts.Add(new Vector2(p.x, p.y));
                ctrls.Add(Vector4.Lerp(c1, c2, t));
                offsets.Add(Mathf.Lerp(offset1, offset2, t));
            }
            // add last 2 points
            pts.Add(p2);
            ctrls.Add(c2);
            offsets.Add(offset2);

            pts.Add(_points[_points.Count - 1]);
            ctrls.Add(_controls[_controls.Count - 1]);
            offsets.Add(_widthOffsets[_widthOffsets.Count - 1]);

            for (int i = 0; i < 2; i++)
            {
                _points.RemoveAt(_points.Count - 1);
                _controls.RemoveAt(_controls.Count - 1);
                _widthOffsets.RemoveAt(_widthOffsets.Count - 1);
            }

            // draw bezier points
            CurrentCanvas.Brush.TempPointDistanceInterval = 0;
            for (var i=0;i<pts.Count;i++)
            {
                draw(pts[i], ctrls[i], offsets[i]);
            }
            CurrentCanvas.Brush.TempPointDistanceInterval = null;

            return true;
        }

        /// <summary>
        /// add one point for drawing, will calucate control point and width offset
        /// </summary>
        /// <param name="position">current point</param>
        /// <returns></returns>
        bool draw(Vector2 position)
        {
            _points.Add(position);

            Vector2 oldPos = Vector2.zero;
            Vector4 oldCtrl = Vector4.zero;
            float oldOffset = 0;

            if (_points.Count > CurrentCanvas.Brush.NeedPointNumber)
            {
                oldPos = _points[0];
                _points.RemoveAt(0);
            }

            var control = GenerateCurveControl(_points, CornerSmooth);
            // 为上倒数第二个控制点点赋值
            if (_controls.Count >= 2)
            {
                _controls[_controls.Count - 1] = control;
                // 控制点占位
                _controls.Add(new Vector4(position.x, position.y, position.x, position.y));
            }
            else
            {
                _controls.Add(control);
            }

            if (_controls.Count > CurrentCanvas.Brush.NeedPointNumber)
            {
                oldCtrl = _controls[0];
                _controls.RemoveAt(0);
            }

            var offset = 0f;
            if (SimulatePressure && _widthOffsetHandle != null)
            {
                offset = _widthOffsetHandle(_points, _controls, _widthOffsets);
            }
            _widthOffsets.Add(offset);
            if (_widthOffsets.Count > CurrentCanvas.Brush.NeedPointNumber)
            {
                oldOffset = _widthOffsets[0];
                _widthOffsets.RemoveAt(0);
            }

            var need = CurrentCanvas.Brush.NeedPointNumber - _points.Count;
            if (CurrentCanvas.Brush.NeedPointNumber == 4 && need > 0 && (_points.Count == 1 || _points.Count == 3))
            {
                _points.Add(_points.Last());
                _controls.Add(_controls.Last());
                _widthOffsets.Add(_widthOffsets.Last());

                if(_points.Count == 4)
                {
                    _widthOffsets[0] = _widthOffsets[1] = _widthOffsets[2];
                }
            }

            if (_points.Count == CurrentCanvas.Brush.NeedPointNumber)
            {
                EPaintStatus status = _paintHandle(_points, _controls, _widthOffsets);
                if (status != EPaintStatus.Success)
                {
                    _points.RemoveAt(_points.Count - 1);
                    _controls.RemoveAt(_controls.Count - 1);
                    _widthOffsets.RemoveAt(_widthOffsets.Count - 1);

                    _points.Insert(0, oldPos);
                    _controls.Insert(0, oldCtrl);
                    _widthOffsets.Insert(0, oldOffset);

                    if (status != EPaintStatus.BrushCheckFailed)
                    {
                        End();

                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// add one point for drawing
        /// </summary>
        /// <param name="position">current point</param>
        /// <returns></returns>
        bool draw(Vector2 position, Vector4 ctrl, float offset)
        {
            Vector2 oldPos = Vector2.zero;
            Vector4 oldCtrl = Vector4.zero;
            float oldOffset = 0;

            _points.Add(position);
            if (_points.Count > CurrentCanvas.Brush.NeedPointNumber)
            {
                oldPos = _points[0];
                _points.RemoveAt(0);
            }

            _controls.Add(ctrl);
            if (_controls.Count > CurrentCanvas.Brush.NeedPointNumber)
            {
                oldCtrl = _controls[0];
                _controls.RemoveAt(0);
            }
           
            _widthOffsets.Add(offset);
            if (_widthOffsets.Count > CurrentCanvas.Brush.NeedPointNumber)
            {
                oldOffset = _widthOffsets[0];
                _widthOffsets.RemoveAt(0);
            }

            var need = CurrentCanvas.Brush.NeedPointNumber - _points.Count;
            if (CurrentCanvas.Brush.NeedPointNumber == 4 && need > 0 && (_points.Count == 1 || _points.Count == 3))
            {
                _points.Add(_points.Last());
                _controls.Add(_controls.Last());
                _widthOffsets.Add(_widthOffsets.Last());

                if (_points.Count == 4)
                {
                    _widthOffsets[0] = _widthOffsets[1] = _widthOffsets[2];
                }
            }

            if (_points.Count == CurrentCanvas.Brush.NeedPointNumber)
            {
                EPaintStatus status = _paintHandle(_points, _controls, _widthOffsets);
                if (status != EPaintStatus.Success)
                {
                    _points.RemoveAt(_points.Count - 1);
                    _controls.RemoveAt(_controls.Count - 1);
                    _widthOffsets.RemoveAt(_widthOffsets.Count - 1);

                    _points.Insert(0, oldPos);
                    _controls.Insert(0, oldCtrl);
                    _widthOffsets.Insert(0, oldOffset);

                    if (status != EPaintStatus.BrushCheckFailed)
                    {
                        End();

                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// add point when mouse pointer hover on
        /// </summary>
        /// <param name="mousePos"></param>
        /// <param name="worldPos"></param>
        /// <param name="directUV"></param>
        public void HoverMove(Vector3 mousePos, Vector3 worldPos, Vector2? directUV = null)
        {
            if (CanDraw())
            {
                if (InPreview && ShowPreview)
                {
                    SetPaintData();

                    if (_paintHandle != null)
                    {
                        Vector2? vUV = null;
                        if (directUV != null)
                        {
                            vUV = directUV;
                        }
                        else
                        {
                            vUV = CurrentCanvas.ToPaintUV(mousePos, worldPos);
                        }
                        if(vUV == null)
                        {
                            return;
                        }

                        var uv = vUV.Value;

                        var pts = new List<Vector2>(){ uv };
                        var offsets = new List<float>(){ 0f };
                        var ctrols = new List<Vector4>() { new Vector4(uv.x, uv.y, uv.x, uv.y) };

                        _paintHandle(pts, ctrols, offsets);
                    }
                }
            }
        }
    }
}
