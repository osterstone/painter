using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearTool : BaseTool
{
    public override void OnClick()
    {
        base.OnClick();

        Settings.Instance.Canvas.ClearAll();
    }
}
