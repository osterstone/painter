using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wing.uPainter;

public class Recoder : MonoBehaviour
{
    public PaintRawImage painter;

    class Position
    {
        public int action;
        public float time;
        public Vector2? uv;
    }

    private List<Position> points = new List<Position>();
    private bool _playing = false;
    private float _timer = 0;
    private float _playTimer = 0;
    private int _playIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        painter.Drawer.AddPointEvent += Drawer_AddPointEvent;
        painter.Drawer.StartEvent += Drawer_StartEvent;
        painter.Drawer.EndEvent += Drawer_EndEvent;
    }

    private void Drawer_EndEvent(Drawer obj)
    {
        if(_playing)
        {
            return;
        }

        points.Add(new Position
        {
            action = 2,
        });
    }

    private void Drawer_StartEvent(Drawer obj)
    {
        if (_playing)
        {
            return;
        }

        points.Add(new Position
        {
            action = 0,
        });
    }

    private void Drawer_AddPointEvent(Drawer sender, Vector3 mousePos, Vector3 worldPos, Vector2? uv = null)
    {
        if (_playing)
        {
            return;
        }

        float t = 0;
        if(_timer > 0)
        {
            t = Time.realtimeSinceStartup - _timer;
        }
        _timer = Time.realtimeSinceStartup;

        points.Add(new Position
        {
            action = 1,
            time = t,
            uv = uv,
        });
    }

    public void Recode()
    {
        _playing = false;
        _timer = 0;
        points.Clear();
        painter.EnablePaint = true;
        painter.PaintCanvas.Layers[0].Clear(false);
    }

    public void Play()
    {
        _playing = true;
        _timer = 0;
        _playIndex = 0;
        painter.EnablePaint = false;
        painter.PaintCanvas.Layers[0].Clear(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(_playing && points.Count > _playIndex)
        {
            var p = points[_playIndex];
            _playTimer += Time.deltaTime;

            if (p.action == 0)
            {
                painter.Drawer.Catch(painter.PaintCanvas);
                painter.Drawer.Begin();
                _playIndex++;
            }
            else if(p.action == 2)
            {
                painter.Drawer.End();
                _playIndex++;
            }
            else
            {
                if (_playTimer >= p.time)
                {
                    painter.Drawer.TouchMove(Vector3.zero, Vector3.zero, p.uv);
                    _playIndex++;
                }
            }
        }
    }
}
