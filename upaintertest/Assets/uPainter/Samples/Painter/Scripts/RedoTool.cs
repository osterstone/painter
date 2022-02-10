using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.uPainter;

public class RedoTool : BaseTool
{
    public override void OnClick()
    {
        base.OnClick();

        PainterOperation.Instance.Redo();
    }

    public override bool Enable
    {
        get
        {
            return PainterOperation.Instance.CanRedo;
        }
    }
}
