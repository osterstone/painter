using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseTool : MonoBehaviour {

    Button _button;
    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    public virtual void OnClick()
    {

    }

    protected virtual void Update()
    {
        if(_button != null)
        {
            _button.interactable = Enable;
        }
    }

    public virtual bool Enable
    {
        get
        {
            return true;
        }
    }
}
