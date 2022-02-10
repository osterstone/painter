using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wing.uPainter
{

    /// <summary>
    /// Just for sprite
    /// </summary>
    [RequireComponent(typeof(Renderer), typeof(BoxCollider))]
    public class SpritePaintCanvas : PaintCanvas
    {
        public bool PaintToFullImage = false;
        bool _paintToFullImage = false;

        [SerializeField]
        LayerSettings _layerSetting = new LayerSettings();

        SpriteRenderer _render;
        Texture2D _texture;
        BoxCollider _collider;
        Texture2D _rawTeture;
        Rect _rawRect;
        Rect _rawTextureRect;
        Vector2 _rawPivot = Vector2.zero;

        PaintCanvasLayer _canvasLayer;
        Vector2 _uvOffset = Vector2.zero;
        Vector2 _uvScale = new Vector2(0.5f, 0.5f);
        public PaintCanvasLayer CanvasLayer
        {
            get
            {
                return _canvasLayer;
            }
        }

        public Texture2D PaintingTexture
        {
            get
            {
                return _texture;
            }
        }

        protected override void Awake()
        {
            _render = GetComponent<SpriteRenderer>();
            _collider = GetComponent<BoxCollider>();
            _paintToFullImage = PaintToFullImage;

            ResetSprite();
            base.Awake();
        }

        public void ResetSprite()
        {
            if (_render.sprite)
            {
                var sprite = _render.sprite;
                _rawTeture = sprite.texture;
                _rawRect = sprite.rect;
                _rawTextureRect = sprite.textureRect;
                _rawPivot.Set(sprite.pivot.x / _rawRect.width, sprite.pivot.y / _rawRect.height);
            }
            else
            {
                _rawTeture = null;
                _uvOffset.Set(0, 0);
                _uvScale.Set(1, 1);
                _rawPivot.Set(0.5f, 0.5f);
            }
            this.Initial();
        }

        public void EnablePaintToFullImage(bool enable)
        {
            if(_paintToFullImage != enable)
            {
                _paintToFullImage = enable;
                Initial();
            }
        }

        protected override void OnInitialStart()
        {
            base.OnInitialStart();

            updateTexture();
        }

        private void updateTexture()
        {
            _uvOffset.Set(0, 0);
            _uvScale.Set(1, 1);

            var oldTex = _texture;

            var sprite = _render.sprite;
            if (_rawTeture != null)
            {
                var rect = _rawRect;
                var tex = _rawTeture;
                var texRect = _rawTextureRect;

                if (_paintToFullImage)
                {
                    _texture = new Texture2D(tex.width, tex.height);
                    var pixels = tex.GetPixels();
                    _texture.SetPixels(pixels);
                    _texture.Apply();
                    _render.sprite = Sprite.Create(_texture, rect, _rawPivot);

                    _uvOffset.Set(rect.x / tex.width, rect.y / tex.height);
                    _uvScale.Set(rect.width / tex.width, rect.height / tex.height);
                }
                else
                {
                    _texture = new Texture2D((int)rect.width, (int)rect.height);
                    var pixels = tex.GetPixels(
                        (int)texRect.x,
                        (int)texRect.y,
                        (int)rect.width,
                        (int)rect.height);
                    _texture.SetPixels(pixels);
                    _texture.Apply();
                    _render.sprite = Sprite.Create(_texture, new Rect(0, 0, _texture.width, _texture.height), _rawPivot);
                }
            }
            else
            {
                _texture = new Texture2D((int)_layerSetting.DefaultSize.x, (int)_layerSetting.DefaultSize.y);
                _render.sprite = Sprite.Create(_texture, new Rect(0, 0, _texture.width, _texture.height), new Vector2(0.5f, 0.5f));
            }

            sprite = _render.sprite;
            _collider.center = new Vector3(-(sprite.pivot.x - sprite.rect.width/2) / sprite.pixelsPerUnit, -(sprite.pivot.y - sprite.rect.height / 2) / sprite.pixelsPerUnit, 0);
            _collider.size = new Vector3(_render.sprite.rect.width / _render.sprite.pixelsPerUnit, _render.sprite.rect.height / _render.sprite.pixelsPerUnit, 0.0001f);
        
            if(oldTex != null && _rawTeture != oldTex)
            {
                Object.DestroyImmediate(oldTex);
            }
        }

        protected override void OnInitialEnd()
        {
            base.OnInitialEnd();

            if (_render.sprite != null)
            {
                if (_layerSetting.RawTexture == null)
                {
                    _layerSetting.RawTexture = _render.sprite.texture;
                }
            }

            _canvasLayer = AddLayer(_layerSetting,
                (layer, textureName, texture, data) => {
                    var old = _render.sprite;
                }, null);
        }

        public override Vector2? WorldToPaintUV(Vector3 worldPos, Camera renderCamera = null)
        {
            if (renderCamera == null)
                renderCamera = Camera.main;

            // Change coordinates to local coordinates of this image
            Vector3 localPos = transform.InverseTransformPoint(worldPos);

            var sprite = _render.sprite;
            // Change these to coordinates of pixels
            float pixelWidth = sprite.rect.width;
            float pixelHeight = sprite.rect.height;
            float unitsToPixels = pixelWidth / sprite.bounds.size.x;

            // Need to center our coordinates
            float centered_x = localPos.x * unitsToPixels + pixelWidth * _rawPivot.x;
            float centered_y = localPos.y * unitsToPixels + pixelHeight * _rawPivot.y;

            // Round current mouse position to nearest pixel
            Vector2 pixelPos = new Vector2(Mathf.RoundToInt(centered_x), Mathf.RoundToInt(centered_y));

            // convert pixel to uv
            pixelPos.x = (pixelPos.x / pixelWidth) * _uvScale.x + _uvOffset.x;
            pixelPos.y = (pixelPos.y / pixelHeight) * _uvScale.y + _uvOffset.y;

            return pixelPos;
        }

        public override Vector2? MousePointToPaintUV(Vector3 mousePos, Camera renderCamera = null)
        {
            if (_render == null)
            {
                return Vector2.zero;
            }

            if (renderCamera == null)
                renderCamera = Camera.main;

            var ray = renderCamera.ScreenPointToRay(mousePos);

            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                var canvas = hitInfo.transform.GetComponent<SpritePaintCanvas>();
                if (canvas == this)
                {
                    return WorldToPaintUV(hitInfo.point, renderCamera);
                }
            }

            return null;
        }

        public override EPaintStatus PaintUVDirect(PaintPoint[] points)
        {
            var ret = base.PaintUVDirect(points);
            if(ret == EPaintStatus.Success)
            {
                TextureTool.CopyToTexture2D(_canvasLayer.GetActiveTexture(), _texture);
            }
            return ret;
        }
    }
}
