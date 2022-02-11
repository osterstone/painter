using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Wing.uPainter;

public class brushmanager : MonoBehaviour
{
    Color[] penColor = {Color.white,Color.red,Color.green,Color.black};
    float[] softness = { 1.2f, 0.2f, 2.0f,0.075f};
    float[] brushsize = { 0.1f, 0.1f, 0.4f ,0.03f};
    EBlendMode[] brushBlend = { EBlendMode.Normal, EBlendMode.Normal, EBlendMode.Normal, EBlendMode.Normal };

    public float penPressure = 22f;
    PaintCanvas paintCanvas;
    BaseBrush _brush;
    GameObject rawImage;
    GameObject b;
    private int activePen = 0;

void setBrushParameter(Color color, float softness,float size, EBlendMode blend) 
    {
        _brush.BrushColor = color;
        if (paintCanvas.Brush is ScratchBrush)
        {
        var sb = paintCanvas.Brush as ScratchBrush;
        sb.Softness = softness;
        }
        _brush.Size = size;
        _brush.BlendMode = blend;
    }

    // Start is called before the first frame update
    void Start()
    {
        _brush = new SolidBrush();  
        rawImage = GameObject.Find("RawImage");
        paintCanvas = rawImage.GetComponent<RawImagePaintCanvas>();
        paintCanvas.Brush = _brush;
        setBrushParameter(penColor[activePen], softness[activePen], brushsize[activePen], brushBlend[activePen]);

    }
    // Update is called once per frame
    void Update()
    {
        Pen pen = Pen.current;
        penPressure = pen.pressure.ReadValue();
        _brush.Size = brushsize[activePen] * penPressure;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            activePen = 0;
            _brush = new SolidBrush();
             paintCanvas.Brush = _brush;
            setBrushParameter(penColor[activePen], softness[activePen], brushsize[activePen], brushBlend[activePen]);

        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            activePen = 1;
            _brush = new GrapicBrush();
            paintCanvas.Brush = _brush;
            setBrushParameter(penColor[activePen], softness[activePen], brushsize[activePen], brushBlend[activePen]);

        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            activePen = 2;
            _brush = new SolidBrush();
            paintCanvas.Brush = _brush;
            setBrushParameter(penColor[activePen], softness[activePen], brushsize[activePen], brushBlend[activePen]);

        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            activePen = 3;
            _brush = new SolidBrush();
            paintCanvas.Brush = _brush;
            setBrushParameter(penColor[activePen], softness[activePen], brushsize[activePen], brushBlend[activePen]);

        }

        else if (Input.GetKeyDown("escape"))
        {
            Application.Quit();
        }
    }
}