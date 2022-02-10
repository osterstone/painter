using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wing.uPainter;

public class ScratchCard : MonoBehaviour
{
    public PaintRawImage painter;
    public Text text;

    Texture2D _texture;
    // Start is called before the first frame update
    void Awake()
    {
        _texture = new Texture2D(100, 100, TextureFormat.ARGB32, false);
    }

    // Update is called once per frame
    void Update()
    {
        if(painter.Drawer.CurrentCanvas.Drawing)
        {
            var rt = painter.Drawer.CurrentCanvas.Layers[0].GetActiveTexture();
            var temp = RenderTexture.GetTemporary(100, 100);
            Graphics.Blit(rt, temp);
            TextureTool.CopyToTexture2D(temp, _texture);
            RenderTexture.ReleaseTemporary(temp);

            var total = _texture.width * _texture.height;
            var count = 0f;
            for(var x = 0;x<_texture.width;x++)
            {
                for (var y = 0; y < _texture.height; y++)
                {
                    var color = _texture.GetPixel(x, y);
                    // judge condition by final texture(PaintRawImage.Texture) color 
                    if(color.r == 1 && color.g == 1 && color.b == 1)
                    {
                        count++;

                        text.text = string.Format("{0:F2}%", count / total * 100);
                    }
                }
            }
        }
    }
}
