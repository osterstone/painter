using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wing.uPainter;

public class RawImageWithRawTexture : MonoBehaviour
{
    public Button btnChangeImage;
    public List<Texture2D> textures;
    public PaintRawImage rawImage;

    private int index = 0;
    // Start is called before the first frame update
    void Start()
    {
        btnChangeImage.onClick.AddListener(() =>
        {
            index = (index + 1) % textures.Count;
            rawImage.texture = textures[index];
            rawImage.PaintCanvas.Initial();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
