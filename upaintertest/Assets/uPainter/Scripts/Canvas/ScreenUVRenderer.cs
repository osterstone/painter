using UnityEngine;

namespace Wing.uPainter
{
    public class ScreenUVRenderer
    {
        static Material sUVMaterial;

        public RenderTexture Texture
        {
            get;
            private set;
        }

        private GameObject _target;
        private MeshFilter _meshFilter;

        public ScreenUVRenderer(GameObject target)
        {
            _target = target;
            _meshFilter = target.GetComponent<MeshFilter>();
            if(_meshFilter == null)
            {
                throw new System.Exception("Not found mesh filter on this GameObject!");
            }

            Texture = new RenderTexture(Screen.width,
                                        Screen.height,
                                        0, RenderTextureFormat.ARGB32);
        }

        public void Destory()
        {
            if (Texture != null)
            {
                GameObject.DestroyImmediate(Texture);
            }
        }

        public void OnRenderObject()
        {
            Graphics.SetRenderTarget(Texture);
            GL.Clear(true, true, new Color(0, 0, 0, 0));
            GL.PushMatrix();

            RenderUVMaterial.Instance.Material.SetPass(0);
            // GL.LoadProjectionMatrix(camera.projectionMatrix);
            Graphics.DrawMeshNow(_meshFilter.mesh, _target.transform.localToWorldMatrix);

            GL.PopMatrix();
            Graphics.SetRenderTarget(null);
        }
    }
}
