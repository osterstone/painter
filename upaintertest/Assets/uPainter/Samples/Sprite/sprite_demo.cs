using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wing.uPainter;

public class sprite_demo : MonoBehaviour
{
    public Toggle toggle;
    public SpriteRenderer render;
    public SpritePaintCanvas canvas;
    public RawImage image;
    public List<Sprite> sprites;
    public Button changeSprite;

    private int index = 0;
    // Start is called before the first frame update
    void Start()
    {
        canvas.EnablePaintToFullImage(true);

        toggle.onValueChanged.AddListener((val) =>
        {
            canvas.EnablePaintToFullImage(val);
        });

        changeSprite.onClick.AddListener(() =>
        {
            index = (index + 1) % sprites.Count;
            render.sprite = sprites[index];
            canvas.ResetSprite();
        });
    }

    // Update is called once per frame
    void Update()
    {
        if(image.texture != canvas.PaintingTexture)
        {
            image.texture = canvas.PaintingTexture;
        }
    }
}
