using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Screenshot : MonoBehaviour
{
    
    public string screenshotname = "screen";
    private string datetime;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            datetime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            ScreenCapture.CaptureScreenshot("screenshots"+"/"+screenshotname+datetime+".png");
            Debug.Log("taken");
        }
    }
}
