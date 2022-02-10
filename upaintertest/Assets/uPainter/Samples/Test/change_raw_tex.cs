using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.uPainter;

public class change_raw_tex : MonoBehaviour
{
    public RawImagePaintCanvas _canvas;
    public Texture2D[] textures;

    float _timer = 0;
    int _idx = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _timer += Time.deltaTime;
        if(_timer > 3)
        {
            _canvas.Layers[0].SetRawTexture(textures[++_idx % textures.Length]);
            _canvas.Layers[0].Clear();

            _timer = 0;
        }
    }
}
