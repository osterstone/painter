using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.uPainter;

public class rawimagetest : MonoBehaviour
{
    public PaintRawImage painter;
    private Drawer _drawer;
    private RawImagePaintCanvas _canvas;
    // Start is called before the first frame update
    void Start()
    {
        _canvas = painter.GetComponent<RawImagePaintCanvas>();
        _drawer = painter.Drawer;
        debug();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void debug()
    {
        _drawer.Catch(_canvas);
        _drawer.Begin();
        _drawer.TouchMove(new Vector3(200, 200, 0.0f), Vector3.zero);
        _drawer.TouchMove(new Vector3(300, 300, 0.0f), Vector3.zero);
        _drawer.TouchMove(new Vector3(500, 500, 0.0f), Vector3.zero);
        _drawer.TouchMove(new Vector3(400, 300, 0.0f), Vector3.zero);
        _drawer.TouchMove(new Vector3(200, 400, 0.0f), Vector3.zero);
        _drawer.TouchMove(new Vector3(100, 100, 0.0f), Vector3.zero);
        _drawer.TouchMove(new Vector3(100, 100, 0.0f), Vector3.zero);
        _drawer.End();
    }
}
