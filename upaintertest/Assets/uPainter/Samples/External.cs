using System;
using UnityEngine;
using System.Runtime.InteropServices;

public class External
{
#if UNITY_WEBGL
#if UNITY_EDITOR
    public static void postTDGAEvent(string eventid, string name)
    {
        Debug.Log(string.Format("event={0},type={1}", eventid, name));
    }
#else
    [DllImport("__Internal")]
    public static extern void postTDGAEvent(string eventid, string name);
#endif
#endif
}