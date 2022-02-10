using UnityEngine;


namespace Wing.uPainter
{
    public class RenderUVMaterial : NormalSingleton<RenderUVMaterial>
    {
        Material _mat;
        public Material Material
        {
            get
            {
                if(_mat == null)
                {
                    _mat = GameObject.Instantiate<Material>(Resources.Load<Material>("Materials/uPainter.Msic.RenderUV"));
                }
                return _mat;
            }
        }
    }
}
