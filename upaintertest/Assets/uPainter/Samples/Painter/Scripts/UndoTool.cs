using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.uPainter;

public class UndoTool : BaseTool
{
    public override void OnClick()
    {
        base.OnClick();

        PainterOperation.Instance.Undo();
    }

    public override bool Enable
    {
        get
        {
            return PainterOperation.Instance.CanUndo;
        }
    }
}
