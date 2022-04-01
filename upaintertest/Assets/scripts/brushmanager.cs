using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Wing.uPainter;

public class brushmanager : MonoBehaviour
{
    public Color[] penColor = new Color[6] { Color.white, Color.red, Color.green, Color.black, Color.blue ,Color.white};
    public float[] softness = { 1.2f, 0.2f, 2.0f,0.075f,1.0f, 0.0f};
    public float[] brushsize = { 0.1f, 0.1f, 0.4f, 0.03f, 4.0f, 0.03f };
    public float brushsizeFactor;
   
    EBlendMode[] brushBlend = { EBlendMode.Normal, EBlendMode.Normal, EBlendMode.Normal, EBlendMode.Normal,EBlendMode.Normal, EBlendMode.Normal };

    public float penPressure = 22f;
    PaintCanvas paintCanvas;
    BaseBrush _brush;
    GameObject rawImage;
    GameObject b;
    GameObject psystemQ;
    GameObject psystemW;
    GameObject psystemE;
    ParticleSystem pQ;
    ParticleSystem pW;
    ParticleSystem pE;


    private int activePen = 0;

void setBrushParameter(Color color, float softness,float size, EBlendMode blend) 
    {
        _brush.BrushColor = color;
        if (paintCanvas.Brush is ScratchBrush)
        {
        var sb = paintCanvas.Brush as ScratchBrush;
        sb.Softness = softness;
        }
        _brush.Size = size*brushsizeFactor;
        _brush.BlendMode = blend;
    }

    // Start is called before the first frame update
    void Start()
    {
        //_brush = new SolidBrush();
       _brush = (BaseBrush)ScriptableObject.CreateInstance("SolidBrush");
        rawImage = GameObject.Find("RawImage");
        paintCanvas = rawImage.GetComponent<RawImagePaintCanvas>();
        paintCanvas.Brush = _brush;
        setBrushParameter(penColor[activePen], softness[activePen], brushsize[activePen], brushBlend[activePen]);
       
        //particle systems
        psystemQ = GameObject.Find("Particle SystemQ");
        pQ = psystemQ.GetComponent<ParticleSystem>();
        pQ.Stop();
        psystemW = GameObject.Find("Particle SystemW");
        pW = psystemW.GetComponent<ParticleSystem>();
        pW.Stop();
        psystemE = GameObject.Find("Particle SystemE");
        pE = psystemE.GetComponent<ParticleSystem>();
        pE.Stop();
        // psystemQ.SetActive(false);
        // psystemW.SetActive(false);
        // psystemE.SetActive(false);

        // cursor hide

        Cursor.visible = false;
        

    }
    // Update is called once per frame
    void Update()
    {
       //painting

        Pen pen = Pen.current;
        penPressure = pen.pressure.ReadValue();
        _brush.Size = brushsize[activePen] * penPressure *brushsizeFactor;
         if (activePen == 0)
        {
            //alpha von penpressure abhängig
            Color c = Color.white;
            c.a = penPressure;
            _brush.BrushColor = c;
            //softness von penpressure abhängig
            var sb = paintCanvas.Brush as ScratchBrush;
            sb.Softness = softness[activePen] * penPressure;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            activePen = 0;
            _brush = new SolidBrush();
             paintCanvas.Brush = _brush;
            setBrushParameter(penColor[activePen], softness[activePen], brushsize[activePen], brushBlend[activePen]);
            penColor[activePen].a = 255;
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
        if (Input.GetKeyDown(KeyCode.Y))
        {
            activePen = 4;
            _brush = new SolidBrush();
            paintCanvas.Brush = _brush;
            setBrushParameter(penColor[activePen], softness[activePen], brushsize[activePen], brushBlend[activePen]);

        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            activePen = 5;
            _brush = new SolidBrush();
            paintCanvas.Brush = _brush;
            setBrushParameter(penColor[activePen], softness[activePen], brushsize[activePen], brushBlend[activePen]);

        }

        // particle systems

        if (Input.GetKeyDown(KeyCode.Q))
        {
            pQ.Play();
            pW.Stop();
            pE.Stop();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            pQ.Stop();
            pW.Play();
            pE.Stop();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
           pQ.Stop();
           pW.Stop();
           pE.Play();

        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            pQ.Stop();
            pW.Stop();
            pE.Stop();

        }


        else if (Input.GetKeyDown("escape"))
        {
            Application.Quit();
        }
    }
}
