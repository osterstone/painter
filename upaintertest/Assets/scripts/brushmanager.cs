using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Wing.uPainter;

public class brushmanager : MonoBehaviour
{
    public Color[] penColor = new Color[7] { Color.white, Color.red, Color.green, Color.black, Color.blue ,Color.white, Color.white };
    public float[] softness = { 0.6f, 0.2f, 2.0f,0.075f,1.0f, 0.0f, 0.0f};
    public float[] brushsize = { 0.1f, 0.1f, 0.4f, 0.03f, 4.0f, 0.03f,0.1f };
    public float brushsizeFactor;
   
    EBlendMode[] brushBlend = { EBlendMode.Normal, EBlendMode.Normal, EBlendMode.Normal, EBlendMode.Normal,EBlendMode.Normal, EBlendMode.Normal, EBlendMode.Normal };

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

    private const int Solidbrush = 0;
    private const int Grapicbrush = 1;

    public OSC myosc;
    private int pencommand;
    private int effect;


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

void setActiveBrush(int penno,int brush)
    {
        activePen = penno;
        if (brush == Solidbrush)
        {
            _brush = new SolidBrush();
        }
        else
        {
            _brush = new GrapicBrush();
        }

        paintCanvas.Brush = _brush;
        setBrushParameter(penColor[activePen], softness[activePen], brushsize[activePen], brushBlend[activePen]);
       // penColor[activePen].a = 255;
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

        //osc init
        myosc.SetAddressHandler("/zeichnen/pen", OnReceivePenselect);
        myosc.SetAddressHandler("/zeichnen/clear", OnReceiveClear);
        myosc.SetAddressHandler("/zeichnen/effect", OnReceiveEffect);


    }
    void OnReceiveEffect(OscMessage message)
    {
        effect = message.GetInt(0);
        switch (effect)
        {
            case 0:
                pQ.Play();
                pW.Stop();
                pE.Stop();
                break;
            case 1:
                pQ.Stop();
                pW.Play();
                pE.Stop();
                break;
            case 2:
                pQ.Stop();
                pW.Stop();
                pE.Play();
                break;
            case 3:
                pQ.Stop();
                pW.Stop();
                pE.Stop();
                break;
        }
    }
    void OnReceiveClear(OscMessage message)
    {
        paintCanvas.ClearAll(); //clear paint texture completely
    }
    void OnReceivePenselect(OscMessage message)
    {
        Debug.Log(message.address);
        Debug.Log(message.GetInt(0));
        pencommand= message.GetInt(0);
        switch (pencommand)
        {
            case 0:
                setActiveBrush(0, Solidbrush);
                break;
            case 1:
                setActiveBrush(1, Grapicbrush);
                break;
            case 2:
                setActiveBrush(2, Solidbrush);
                break;
            case 3:
                setActiveBrush(3, Solidbrush);
                break;
            case 4:
                setActiveBrush(4, Solidbrush);
                break;
            case 5:
                setActiveBrush(5, Solidbrush);
                break;
            case 6:
                setActiveBrush(6, Solidbrush);
                break;
        }
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
            //var sb = paintCanvas.Brush as ScratchBrush;
            //sb.Softness = softness[activePen] * penPressure;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            setActiveBrush(0, Solidbrush);
           // activePen = 0;
           //_brush = new SolidBrush();
           // paintCanvas.Brush = _brush;
           // setBrushParameter(penColor[activePen], softness[activePen], brushsize[activePen], brushBlend[activePen]);
           // penColor[activePen].a = 255;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            setActiveBrush(1, Grapicbrush);
           /* activePen = 1;
            _brush = new GrapicBrush();
            paintCanvas.Brush = _brush;
            setBrushParameter(penColor[activePen], softness[activePen], brushsize[activePen], brushBlend[activePen]);
*/
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            setActiveBrush(2, Solidbrush);
           
            //activePen = 2;
            //_brush = new SolidBrush();
            //paintCanvas.Brush = _brush;
            //setBrushParameter(penColor[activePen], softness[activePen], brushsize[activePen], brushBlend[activePen]);

        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            setActiveBrush(3, Solidbrush);

            //activePen = 3;
            //_brush = new SolidBrush();
            //paintCanvas.Brush = _brush;
            //setBrushParameter(penColor[activePen], softness[activePen], brushsize[activePen], brushBlend[activePen]);

        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            setActiveBrush(4, Solidbrush);

            //activePen = 4;
            //_brush = new SolidBrush();
            //paintCanvas.Brush = _brush;
            //setBrushParameter(penColor[activePen], softness[activePen], brushsize[activePen], brushBlend[activePen]);

        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            setActiveBrush(5, Solidbrush);

            //activePen = 5;
            //_brush = new SolidBrush();
            //paintCanvas.Brush = _brush;
            //setBrushParameter(penColor[activePen], softness[activePen], brushsize[activePen], brushBlend[activePen]);

        }
        if (Input.GetKeyDown(KeyCode.Alpha6)) //pen to delete web in leonies last scene
        {
            setActiveBrush(6, Solidbrush);

            //activePen = 6;
            //_brush = new SolidBrush();
            //paintCanvas.Brush = _brush;
            //setBrushParameter(penColor[activePen], softness[activePen], brushsize[activePen], brushBlend[activePen]);

        }
        if (Input.GetKeyDown(KeyCode.X)) //clear texture
        {
           paintCanvas.ClearAll();

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
