using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.uPainter;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SaveTool : BaseTool
{
    public override void OnClick()
    {
        base.OnClick();
        var filename = "";
#if UNITY_EDITOR
        filename = EditorUtility.SaveFilePanel("Save File", UtilsHelper.GetDataPath(), "upainter", "png");

#else        
        filename = UtilsHelper.GetResourcePath() + "upainter.png";
#endif
        if (!string.IsNullOrEmpty(filename))
        {
            Settings.Instance.Canvas.Layers[0].Save(filename, 1024, 512, EPictureType.PNG);
        }
    }
}
