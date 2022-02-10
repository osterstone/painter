using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.uPainter;

public class Benchmark : MonoBehaviour {

    public RawImagePaintCanvas canvas;

    ScratchBrush brush;
    private void Start()
    {
        PainterOperation.Instance.Enable = false;

        canvas.Brush = Instantiate<BaseBrush>(canvas.Brush);
        brush = canvas.Brush as ScratchBrush;
    }

    private void OnDestroy()
    {
        PainterOperation.Instance.Enable = true;
    }

    void Update ()
    {
        brush.Size = Random.Range(0.01f, 0.03f);
        brush.BrushColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        brush.CapStyle = Random.Range(0f, 1f) < 0.5f ? EBrushCapStyle.Flat : EBrushCapStyle.Round;
        brush.Softness = Random.Range(0f, 1f) < 0.5f ? 0 : Random.Range(0f, 0.8f);
        brush.NoiseRatio = Random.Range(0f, 1f) < 0.5f ? 0 : Random.Range(0f, 0.4f);
        brush.NoiseSize = Random.Range(0f, 0.5f);

        var isPoint = Random.Range(0f, 1f) < 0.5f;
        var pos1 = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
        if (isPoint)
        {
            canvas.DrawPoint(pos1);
        }
        else
        {
            var pos2 = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
            canvas.DrawLine(pos1, pos2);
        }
    }
}
