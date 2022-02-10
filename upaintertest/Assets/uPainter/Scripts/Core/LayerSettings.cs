using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wing.uPainter
{
    [Serializable]
    public class LayerSettings
    {
        /// <summary>
        /// lock layer
        /// </summary>
        public bool Locked = false;
        /// <summary>
        /// when enabled, brush color will support alpha to drawing on texture
        /// </summary>
        public bool EnableAlpha = false;
        /// <summary>
        /// texture background when not give raw image
        /// </summary>
        public Color BackgroundColor = Color.white;
        /// <summary>
        /// blend material with current circle drawing texture and main drawing texture
        /// </summary>
        public Material BlendMaterial = null;
        /// <summary>
        /// texture filter mode, if you want pixel mode, set value to FilterMode.Point
        /// </summary>
        public FilterMode TextureFilterMode = FilterMode.Bilinear;
        /// <summary>
        /// when not give raw image, will create texture with default size
        /// </summary>
        public Vector2 DefaultSize = new Vector2(512, 512);
        [SerializeField]
        public string TextureName = "";
        /// <summary>
        /// first use texture , null value will use texture name
        /// </summary>
        [SerializeField]
        public Texture RawTexture;
    }
}
