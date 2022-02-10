using UnityEngine;


namespace Wing.uPainter
{
    public class OverlayBlendMaterial : NormalSingleton<OverlayBlendMaterial>
    {
        int _overlayTexPropertyID;
        Material _mat;

        public void Blend(Texture raw, Texture overlay, RenderTexture target)
        {
            if (_mat == null)
            {
                _mat = GameObject.Instantiate<Material>(Resources.Load<Material>("Materials/uPainter.Blend.Overlay"));
                _overlayTexPropertyID = Shader.PropertyToID("_OverlayTex");
            }

            _mat.SetTexture(_overlayTexPropertyID, overlay);
            Graphics.Blit(raw, target, _mat);
        }
    }
}
