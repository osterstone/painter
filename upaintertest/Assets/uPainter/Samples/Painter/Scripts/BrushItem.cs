using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wing.uPainter;

public class BrushItem : MonoBehaviour {
    public BaseBrush Brush;

    BaseBrush _brush;
    private void Start()
    {
        _brush = Instantiate<BaseBrush>(Brush);

        var toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(ret =>
        {
            if(ret)
            {
                Settings.Instance.Canvas.Brush = _brush;
#if UNITY_WEBGL
                External.postTDGAEvent("selectbrush", name);
#endif
            }
        });

        if(toggle.isOn)
        {
            Settings.Instance.Canvas.Brush = _brush;
        }
    }
}
