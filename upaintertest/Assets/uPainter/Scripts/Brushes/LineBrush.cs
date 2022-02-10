using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace Wing.uPainter
{
    [CreateAssetMenu(menuName = "uPainter Brush/LineBrush")]
    [System.Serializable]
    public class LineBrush : GrapicBrush
    {
        [SerializeField]
        [Range(1, 100)]
        public int LineCount = 1;

        Vector2 _lastP0, _lastP1;
        protected override void OnInitial()
        {
            base.OnInitial();

            BrushType = 4;
        }

        public override int NeedPointNumber
        {
            get
            {
                return 4;
            }
        }

        public override void Start()
        {
            base.Start();

            _lastP0 = Vector2.zero;
            _lastP1 = Vector2.zero;
        }

        public override bool CheckData(Vector2[] pos, float[] sizeOffset)
        {
            if (willstop || LineCount <= 0 || pos.Length == 0)
            {
                return false;
            }

            var ret = true;
            if (pos.Length == 2)
            {
                ret = (pos[0] - pos[1]).magnitude >= PointDistanceInterval;
            }
            else if (pos.Length == 3)
            {
                ret = (pos[1] - pos[2]).magnitude >= PointDistanceInterval;
            }
            else if (pos.Length == 4)
            {
                ret = (pos[2] - pos[3]).magnitude >= PointDistanceInterval;
            }

            if (pos.Length >= 3)
            {
                var w_2 = (Size + sizeOffset[1]) * 0.5f;

                var v01 = pos[1] - pos[0];
                var v12 = pos[2] - pos[1];

                var vline01 = Vector3.Cross(Vector3.back, new Vector3(v01.x, v01.y, 0));
                var vline12 = Vector3.Cross(Vector3.back, new Vector3(v12.x, v12.y, 0));
                var vp1 = new Vector2(vline01.x, vline01.y).normalized;
                var vp2 = new Vector2(vline12.x, vline12.y).normalized;

                _lastP0 = (vp1 + vp2).normalized * w_2 + pos[1];
                _lastP1 = -(vp1 + vp2).normalized * w_2 + pos[1];
            }

            return ret && base.CheckData(pos, sizeOffset);
        }

        public override bool CheckData(PaintPoint[] pos)
        {
            if(willstop || LineCount <= 0 || pos.Length == 0)
            {
                return false;
            }

            var ret = true;
            if (pos.Length == 2)
            {
                ret = (pos[0].UV - pos[1].UV).magnitude >= PointDistanceInterval;
            }
            else if (pos.Length == 3)
            {
                ret = (pos[1].UV - pos[2].UV).magnitude >= PointDistanceInterval;
            }
            else if(pos.Length == 4)
            {
                ret = (pos[2].UV - pos[3].UV).magnitude >= PointDistanceInterval;
            }

            if(pos.Length >= 3)
            {
                var w_2 = (Size + pos[1].SizeOffset) * 0.5f;

                var v01 = pos[1].UV - pos[0].UV;
                var v12 = pos[2].UV - pos[1].UV;

                var vline01 = Vector3.Cross(Vector3.back, new Vector3(v01.x, v01.y, 0));
                var vline12 = Vector3.Cross(Vector3.back, new Vector3(v12.x, v12.y, 0));
                var vp1 = new Vector2(vline01.x, vline01.y).normalized;
                var vp2 = new Vector2(vline12.x, vline12.y).normalized;

                _lastP0 = (vp1 + vp2).normalized * w_2 + pos[1].UV;
                _lastP1 = -(vp1 + vp2).normalized * w_2 + pos[1].UV;
            }

            return ret && base.CheckData(pos);
        }

        protected override void OnRender(PaintPoint[] pos)
        {
            if(pos.Length == 1 && InPreview)
            {
                var p = pos[0];
                var w_2 = Size * 0.25f;
                var hp = new Vector2(w_2, 0);
                var p0 = new PaintPoint
                {
                    UV = p.UV - hp*2,
                    BezierControl = p.UV - hp * 2
                };

                var p1 = new PaintPoint
                {
                    UV = p.UV - hp,
                    BezierControl = p.UV - hp
                };

                var p2 = new PaintPoint
                {
                    UV = p.UV + hp,
                    BezierControl = p.UV + hp
                };

                var p3 = new PaintPoint
                {
                    UV = p.UV + hp*2,
                    BezierControl = p.UV + hp*2
                };
                pos = new PaintPoint[] { p0, p1, p2, p3 };

                CheckData(pos);
            }

            if(LineCount <= 0 || 
                pos.Length <= 3)
            {
                return;
            }

            GL.Begin(GL.LINES);

            if (LineCount == 1)
            {
                GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(pos[1].UV.x, pos[1].UV.y, 0f);
                GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(pos[2].UV.x, pos[2].UV.y, 0f);
            }
            else
            {
                var w_2 = (Size + pos[2].SizeOffset) * 0.5f;

                var v12 = pos[2].UV - pos[1].UV;
                var vline12 = Vector3.Cross(Vector3.back, new Vector3(v12.x, v12.y, 0));
                var v23 = pos[3].UV - pos[2].UV;
                var vline23 = Vector3.Cross(Vector3.back, new Vector3(v23.x, v23.y, 0));

                var vp21 = new Vector2(vline12.x, vline12.y).normalized;
                var vp31 = new Vector2(vline23.x, vline23.y).normalized;

                var p3 = (vp21 + vp31).normalized * w_2 + pos[2].UV;
                var p4 = -(vp21 + vp31).normalized * w_2 + pos[2].UV;

                var dir1 = (_lastP0 - _lastP1);
                var w1 = dir1.magnitude;
                var d1 = w1 / (LineCount-1);
                var ndir1 = dir1.normalized;

                var dir2 = (p3 - p4);
                var w2 = dir2.magnitude;
                var d2 = w2 / (LineCount-1);
                var ndir2 = dir2.normalized;

                //if(d1 > 0.00001f && d2 > 0.00001f)
                {
                    for (var i = 0; i < LineCount; i++)
                    {
                        var uy = i / (float)LineCount;
                        var start = ndir1 * d1 * i + _lastP1;
                        var end = ndir2 * d2 * i + p4;

                        GL.TexCoord2(0.0f, uy); GL.Vertex3(start.x, start.y, 0f);
                        GL.TexCoord2(1.0f, uy); GL.Vertex3(end.x, end.y + 0.001f, 0f);
                    }

                    _lastP0 = p3;
                    _lastP1 = p4;
                } 
            }
            GL.End();
        }
    }
}
