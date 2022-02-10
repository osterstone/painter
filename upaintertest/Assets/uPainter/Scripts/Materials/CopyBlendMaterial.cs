using UnityEngine;


namespace Wing.uPainter
{
    public class CopyBlendMaterial : NormalSingleton<CopyBlendMaterial>
    {
        Material _mat;

        public void Copy(Texture texture, RenderTexture target)
        {
            if (_mat == null)
            {
                _mat = GameObject.Instantiate<Material>(Resources.Load<Material>("Materials/uPainter.Blend.Copy"));
            }

            Graphics.Blit(texture, target, _mat);
        }
    }
}
