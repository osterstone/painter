using UnityEngine;


namespace Wing.uPainter
{
    public class MaskBlendMaterial : NormalSingleton<MaskBlendMaterial>
    {
        int _maskTexPropertyID;
        Material _mat;

        public void Blend(Texture raw, Texture mask, RenderTexture target, bool invert = false)
        {
            if (_mat == null)
            {
                _mat = GameObject.Instantiate<Material>(Resources.Load<Material>("Materials/uPainter.Blend.Mask"));

                _maskTexPropertyID = Shader.PropertyToID("_MaskTex");
            }
            foreach (var key in _mat.shaderKeywords)
            {
                _mat.DisableKeyword(key);
            }
            if (invert)
            {
                _mat.EnableKeyword("INVERT");
            }

            _mat.SetTexture(_maskTexPropertyID, mask);

            Graphics.Blit(raw, target, _mat);
        }
    }
}
