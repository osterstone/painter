using System;
using UnityEngine;

namespace Wing.uPainter
{
    [Serializable]
    public class MeshLayerSetting : LayerSettings
    {
        [HideInInspector]
        [NonSerialized]
        public Material Material;
    }
}
