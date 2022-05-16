using System;
using System.IO;
using UnityEngine;

public class CameraCapture : MonoBehaviour
{
    public string screenshotname = "screen";
    private string datetime;
    public string filePath;
    public OSC myosc;

    void OSCScreenshot(OscMessage message)
    {
        Capture();
        Debug.Log("screenshot taken by OSC");
    }
    private Camera Camera
    {
        get
        {
            if (!_camera)
            {
                _camera = Camera.main;
            }
            return _camera;
        }
    }
    private Camera _camera;
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

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Capture();
            Debug.Log("screenshot taken by keyboard");
        }
    }

    public void Capture()
    {
        RenderTexture activeRenderTexture = RenderTexture.active;
        RenderTexture.active = Camera.targetTexture;

        Camera.Render();

        Texture2D image = new Texture2D(Camera.targetTexture.width, Camera.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, Camera.targetTexture.width, Camera.targetTexture.height), 0, 0);
        image.Apply();
        RenderTexture.active = activeRenderTexture;

        byte[] bytes = image.EncodeToPNG();
        Destroy(image);
        datetime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        File.WriteAllBytes(Application.dataPath + "/screenshots/" + screenshotname+datetime + ".png", bytes);
  
    }
}