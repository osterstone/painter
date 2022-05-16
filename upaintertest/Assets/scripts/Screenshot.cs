using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public class Screenshot : MonoBehaviour
{
    
    public string screenshotname = "screen";
    public string filePath;
    private string datetime;
    
    public OSC myosc;

    void OSCScreenshot(OscMessage message)
    {
        datetime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        ScreenCapture.CaptureScreenshot(filePath + "/" + screenshotname + datetime + ".png");
        Debug.Log("screenshot via osc taken");
    }
    void Start()
    {
        myosc.SetAddressHandler("/zeichnen/screenshot", OSCScreenshot);

        filePath = Application.dataPath + "/screenshots";
        try
        {
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            datetime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            ScreenCapture.CaptureScreenshot(filePath + "/" + screenshotname + datetime + ".png");
            Debug.Log("screenshot via keyboard taken");
        }
    }
}
