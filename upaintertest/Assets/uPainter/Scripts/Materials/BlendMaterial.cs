using UnityEngine;

namespace Wing.uPainter
{
    public enum EBlendType
    {
        NoAlpha,
        WithAlpha,
        Max,
    }

    public class BlendMaterial : NormalSingleton<BlendMaterial>
    {
        int _overlayTexPropertyID;
        int _rawTexPropertyID;
        int _blenTypePropertyID;
        Material _mat;

        public void Blend(Texture main, Texture raw, Texture overlay, RenderTexture target, EBlendMode blendMode, EBlendType blendType = EBlendType.WithAlpha)
        {
            if (_mat == null)
            {
                _mat = GameObject.Instantiate<Material>(Resources.Load<Material>("Materials/uPainter.Blend.Blender"));
                _overlayTexPropertyID = Shader.PropertyToID("_OverlayTex");
                _rawTexPropertyID = Shader.PropertyToID("_RawTex");
                _blenTypePropertyID = Shader.PropertyToID("_BlendType");
            }

            foreach (var key in _mat.shaderKeywords)
                _mat.DisableKeyword(key);
            _mat.EnableKeyword("UPAINTER_LAYER_BLEND_" + blendMode.ToString().ToUpper());

            _mat.SetInt(_blenTypePropertyID, (int)blendType);
            _mat.SetTexture(_rawTexPropertyID, raw);
            _mat.SetTexture(_overlayTexPropertyID, overlay);

            Graphics.Blit(main, target, _mat);
        }
    }
}
