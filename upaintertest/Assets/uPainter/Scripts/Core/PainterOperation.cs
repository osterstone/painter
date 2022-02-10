using System.Collections.Generic;
using UnityEngine;


namespace Wing.uPainter
{
    public class PaintCommand : BaseCommand
    {
        PaintCanvas _canvas;
        PaintCanvasLayer[] _layers = null;
        List<RenderTexture> _textures = new List<RenderTexture>();

        public PaintCommand(PaintCanvas canvas, PaintCanvasLayer onlyLayer = null)
        {
            _canvas = canvas;

            _canvas.GetPaintLayers(out _layers);
            if (_layers != null)
            {
                foreach (var layer in _layers)
                {
                    if (onlyLayer != null && onlyLayer == layer || onlyLayer == null)
                    {
                        var paintTex = layer.PaintTexture;
                        var tex = RenderTexture.GetTemporary(paintTex.width, paintTex.height, paintTex.depth, paintTex.format);
                        Graphics.Blit(paintTex, tex);
                        _textures.Add(tex);                        
                    }
                }
            }
        }

        public override void Do()
        {
            base.Do();

            restore();
        }

        public override void Undo()
        {
            base.Undo();

            restore();
        }

        void restore()
        {
            if (_layers != null)
            {
                for (var i = 0; i < _layers.Length; i++)
                {
                    var layer = _layers[i];
                    var rt = _textures[i];
                    var paintTex = layer.PaintTexture;

                    var temp = TextureTool.GetTempRenderTexture(paintTex, paintTex.width, paintTex.height);
                    if (layer.PaintTexture != null)
                    {
                        Graphics.Blit(rt, layer.PaintTexture);
                    }
                    layer.CopyToTemp();

                    Graphics.Blit(temp, rt);

                    RenderTexture.ReleaseTemporary(temp);
                }
            }
        }

        public override void Destroy()
        {
            base.Destroy();

            for (var i = 0; i < _layers.Length; i++)
            {
                RenderTexture.ReleaseTemporary(_textures[i]);
            }
            _textures = null;
            _layers = null;
        }
    }

    public class PainterOperation : NormalSingleton<PainterOperation>
    {
        public bool Enable = true;

        Operation _operation = new Operation();

        public PainterOperation()
        {
            MaxStep = 10;
        }
        
        public int MaxStep
        {
            get
            {
                return _operation.MaxStepNumber;
            }
            set
            {
                _operation.MaxStepNumber = value;
            }
        }

        public bool CanUndo
        {
            get
            {
                return _operation.HasUndo();
            }
        }

        public bool CanRedo
        {
            get
            {
                return _operation.HasRedo();
            }
        }

        public ICommand Undo()
        {
            return _operation.Undo();
        }

        public ICommand Redo()
        {
            return _operation.Redo();
        }

        public void Store(PaintCanvas canvas, PaintCanvasLayer onlyLayer = null)
        {
            if (Enable)
            {
                _operation.DoCommand(new PaintCommand(canvas, onlyLayer), false);
            }
        }
    }
}
