using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wing.uPainter;

public class Settings : MonoBehaviour {
    public static Settings Instance;

    public Texture2D MaskTexture;
    public RawImage BrushTexture;
    public TextureBrush UseClipTextureBrush;

    public Slider SizeSlider;
    public Slider SoftnessSlider;
    public Slider AlphaSlider;
    public Toggle ShowPreview;
    public Toggle EnableBlur;
    public Toggle SelfOverlay;
    public Toggle SimulatePressure;
    public Toggle EnableMask;
    public Toggle InvertMask;
    public BlurBrushPost BlurPost;

    public PaintCanvas Canvas;

    BlurBrushPost _blurPost;
    BaseBrush _lastBrush;
    bool _firstSeal = true;
    void Start ()
    {
        Instance = this;

        _blurPost = Instantiate<BlurBrushPost>(BlurPost);
        SimulatePressure.isOn = Canvas.Drawer.SimulatePressure;
        ShowPreview.isOn = Canvas.Drawer.ShowPreview;
    }
	
	void Update ()
    {
        if (Canvas && Canvas.Brush)
        {
            Canvas.Drawer.SimulatePressure = SimulatePressure.isOn;
            Canvas.Drawer.ShowPreview = ShowPreview.isOn;
            if (EnableMask.isOn)
            {
                Canvas.MaskTexture = MaskTexture;
            }
            else
            {
                Canvas.MaskTexture = null;
            }
            Canvas.InvertMask = InvertMask.isOn;

            var brush = Canvas.Brush;
            if (_lastBrush != brush)
            {
                BrushTexture.gameObject.SetActive(false);

                AlphaSlider.value = brush.BrushColor.a;
                SizeSlider.value = brush.Size;
                SelfOverlay.isOn = brush.SelfOverlay;
                ColorSelector.SetColor(brush.BrushColor);

                if (Canvas.Brush is ScratchBrush)
                {
                    var sb = Canvas.Brush as ScratchBrush;
                    SoftnessSlider.value = sb.Softness;
                }
                else if(Canvas.Brush is DropperBrush)
                {
                    var db = (Canvas.Brush as DropperBrush);                    
                    db.SetDropColorEventHandler((color) =>
                    {
                        ColorSelector.SetColor(color);
                    });
                }
                else if(Canvas.Brush is SealBrush)
                {
                    if(_firstSeal)
                    {
                        _firstSeal = false;
                        ShowPreview.isOn = true;
                    }

                    BrushTexture.gameObject.SetActive(true);
                    var db = (Canvas.Brush as SealBrush);
                    db.SetClipTextureEventHandler((texture) =>
                    {
                        BrushTexture.texture = texture;
                        if(UseClipTextureBrush != null && !db.InPreview)
                        {
                            if(UseClipTextureBrush.BrushTexture != null)
                            {
                                Object.DestroyImmediate(UseClipTextureBrush.BrushTexture);
                            }

                            UseClipTextureBrush.BrushTexture = texture;
                            Canvas.EndDraw();
                            Canvas.Brush = UseClipTextureBrush;
                        }
                    });
                }
                
                if(Canvas.Brush == UseClipTextureBrush)
                {
                    BrushTexture.gameObject.SetActive(true);
                }

                _lastBrush = Canvas.Brush;
            }
            else
            {
                var color = ColorSelector.GetColor();
                color.a = AlphaSlider.value;
                Canvas.Brush.BrushColor = color;
                Canvas.Brush.Size = SizeSlider.value;
                Canvas.Brush.SelfOverlay = SelfOverlay.isOn;

                if (Canvas.Brush is ScratchBrush)
                {
                    var sb = Canvas.Brush as ScratchBrush;
                    sb.Softness = SoftnessSlider.value;
                }

                if (!EnableBlur.isOn && Canvas.Brush.WithPostProcess)
                {
                    Canvas.Brush.PostEffects = new BaseBrushPost[0];
                }
                if (EnableBlur.isOn && !Canvas.Brush.WithPostProcess)
                {
                    _blurPost.BlurWidth = SizeSlider.value / 5;
                    //_blurPost.BlurColor = Canvas.Brush.BrushColor;
                    Canvas.Brush.PostEffects = new[] { _blurPost };
                }
            }
        }
    }
}
