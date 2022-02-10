using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Samples : MonoBehaviour {
    static bool done = false;
    GUIStyle fontStyle = new GUIStyle();
    private void Awake()
    {
        if (!done)
        {
            DontDestroyOnLoad(transform.gameObject);
            done = true;
        }

        fontStyle.normal.background = null;
        fontStyle.normal.textColor = new Color(1, 1, 1);
        fontStyle.fontSize = 40;
        fontStyle.alignment = TextAnchor.MiddleCenter;
    }

    public void OpenScene(string name)
    {
        SceneManager.LoadScene(name);

#if UNITY_WEBGL
        External.postTDGAEvent("loadscene", name);
#endif
    }

    private void OnGUI()
    {
        var cur = SceneManager.GetActiveScene();
        if (cur.name != "Samples")
        {
            GUI.Box(new Rect(Screen.width - 200, Screen.height - 100, 180, 90), "");
            if (GUI.Button(new Rect(Screen.width - 200, Screen.height - 100, 180, 90), "Home", fontStyle))
            {
                OpenScene("Samples");
            }
        }
    }
}
