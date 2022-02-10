using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public GameObject target;
    public Material mat;
    public RawImage img;
    public RenderTexture texture;

    MeshFilter mf;
    private void Awake()
    {
        texture = new RenderTexture(Screen.width,
                    Screen.height,
                    24, RenderTextureFormat.ARGB32,
                    RenderTextureReadWrite.Linear);
        img.texture = texture;


        mf = target.GetComponent<MeshFilter>();
    }

    [ContextMenu("Render")]
    public void Render()
    {
        Graphics.SetRenderTarget(texture);
        GL.Clear(true, true, new Color(0, 0, 0, 0));
        GL.PushMatrix();

        mat.SetPass(0);
        // GL.LoadProjectionMatrix(camera.projectionMatrix);
        Graphics.DrawMeshNow(mf.mesh, target.transform.localToWorldMatrix);

        GL.PopMatrix();
        Graphics.SetRenderTarget(null);
    }

    private void OnRenderObject()
    {
        Render();
    }
}
