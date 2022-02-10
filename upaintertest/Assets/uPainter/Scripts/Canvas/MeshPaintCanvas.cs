using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Es.InkPainter;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wing.uPainter
{
    /// <summary>
    /// Just for simple mesh
    /// </summary>
    [RequireComponent(typeof(Renderer), typeof(MeshCollider))]
    public class MeshPaintCanvas : PaintCanvas
    {
        [NonSerialized]
        [HideInInspector]
        public TextureChangedEvent TextureChangedHandle;

        /// <summary>
        /// not support yet!
        /// </summary>
        internal bool EnableWorldSpacePaint = false;
        private RenderTexture _screenUVTexture;
        private Texture2D _screenUVRemapTexture;
        private bool _needRenderUV = false;

        [SerializeField]
        public List<MeshLayerSetting> LayerSettings = new List<MeshLayerSetting>();

        private MeshOperator meshOperator;
        public MeshOperator MeshOperator
        {
            get
            {
                if (meshOperator == null)
                    Debug.LogError("To take advantage of the features must Mesh filter or Skinned mesh renderer component associated Mesh.");

                return meshOperator;
            }
        }

        protected virtual Mesh GetMesh()
        {
            var meshFilter = GetComponent<MeshFilter>();
            var skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            if (meshFilter != null)
                return meshFilter.sharedMesh;
            else if (skinnedMeshRenderer != null)
                return skinnedMeshRenderer.sharedMesh;
            else
                return null;
        }

        /// <summary>
        /// Cach data from the mesh.
        /// </summary>
        protected void InitialMeshData()
        {
            var mesh = GetMesh();
            if (mesh != null)
                meshOperator = new MeshOperator(mesh);
            else
                Debug.LogWarning("Sometimes if the MeshFilter/SkinnedMeshRenderer or other mesh component does not exist in the component part does not work correctly.");
        }

        protected override void OnInitialStart()
        {
            base.OnInitialStart();

            InitialMeshData();
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_screenUVTexture != null)
            {
                DestroyImmediate(_screenUVTexture);
                _screenUVTexture = null;
            }
        }

        private void OnRenderObject()
        {
            if(!EnableWorldSpacePaint)
            {
                if(_screenUVTexture != null)
                {
                    DestroyImmediate(_screenUVTexture);
                    _screenUVTexture = null;
                }
                return;
            }
            if(!_needRenderUV)
            {
                return;
            }


            var width = Screen.width;
            var height = Screen.height;

            if (_screenUVTexture == null)
            {
                _screenUVTexture = TextureTool.CreateRenderTexture(null, width, height, background: new Color(0, 0, 0));
            }

            Graphics.SetRenderTarget(_screenUVTexture);
            GL.Clear(true, true, new Color(0, 0, 0, 0));
            GL.PushMatrix();

            RenderUVMaterial.Instance.Material.SetPass(0);
            // GL.LoadProjectionMatrix(camera.projectionMatrix);
            Graphics.DrawMeshNow(meshOperator.Target, transform.localToWorldMatrix);

            GL.PopMatrix();
            Graphics.SetRenderTarget(null);

            //TextureTool.SaveToFile(_screenUVTexture, "screenuv.jpg", width, width, EPictureType.JPG);

            // create remap uv
            if (_screenUVRemapTexture == null)
            {
                _screenUVRemapTexture = new Texture2D(width, height);
                _screenUVRemapTexture.filterMode = FilterMode.Point;
            }
            var uvTex = TextureTool.ToTexture2D(_screenUVTexture);
            var rw = 1 / (float)width;
            var rh = 1 / (float)height;
            //StringBuilder sb = new StringBuilder();
            for (var hi = 0; hi < height; hi++)
            {
                for (var wi = 0; wi < width; wi++)
                {
                    //sb.Append(wi == 0 ? "\n---------\n" : "");

                    var pixel = uvTex.GetPixel(wi, hi);
                    if(pixel.a == 0)
                    {
                        continue;
                    }

                    var r = pixel.r;
                    var g = pixel.g;

                    _screenUVRemapTexture.SetPixel((int)(r * width+0.5f), (int)(g * height + 0.5f), new Color(wi* rw, hi*rh, 0, 1));
                    //sb.Append(string.Format("({0},{1},{2},{3},{4},{5})", r,g, (int)(r * width + 0.5f), (int)(g * height + 0.5f), wi / (float)width, hi / (float)height));
                    //_screenUVRemapTexture.SetPixel(wi, hi, new Color(1, 0, 0));
                }
            }
            // File.WriteAllText("./array.txt", sb.ToString());

            _screenUVRemapTexture.Apply();
            // TextureTool.SaveToFile(_screenUVRemapTexture, "screen.jpg", width, width, EPictureType.JPG);

            Graphics.Blit(_screenUVRemapTexture, _screenUVTexture);

            _needRenderUV = false;
        }

        #region Public Methods

        public override void BeginDraw()
        {
            base.BeginDraw();

            _needRenderUV = true;
        }

        public override void EndDraw()
        {
            base.EndDraw();

            _needRenderUV = false;
        }

        private Vector3 GetNearestTriangleSurface(Vector3 worldPos)
        {
            var p = transform.worldToLocalMatrix.MultiplyPoint(worldPos);
            var pd = MeshOperator.NearestLocalSurfacePoint(p);
            var uv = transform.localToWorldMatrix.MultiplyPoint(pd);
            return uv;
        }

        /// <summary>
        /// Paint of points close to the given world-space position on the Mesh surface(must initial mesh).
        /// </summary>
        /// <param name="worldPos">Approximate point.</param>
        /// <param name="renderCamera">Camera to use to render the object.</param>
        /// <param name="bezierControls">Bezier control points In World space.</param>
        /// <returns>The success or failure of the paint.</returns>
        public EPaintStatus PaintNearestTriangleSurface(Vector3[] worldPos, Vector4[] bezierControls = null, float[] sizeOffsets = null, Camera renderCamera = null)
        {
            if (MeshOperator == null)
            {
                return EPaintStatus.NoMeshOperatorError;
            }

            var uvs = new List<Vector2>();
            var cpos = new List<Vector4>();
            for (var i = 0; i < worldPos.Length; i++)
            {
                var pos = GetNearestTriangleSurface(worldPos[i]);
                var uv = WorldToPaintUV(pos, renderCamera);
                if(uv == null)
                {
                    return EPaintStatus.Failed;
                }
                uvs.Add(uv.Value);

                if(bezierControls != null)
                {
                    var cp = bezierControls[i];
                    var c1 = WorldToPaintUV(new Vector3(cp.x, cp.y, 0), renderCamera);
                    var c2 = WorldToPaintUV(new Vector3(cp.z, cp.w, 0), renderCamera);
                    if(c1 == null)
                    {
                        c1 = uv;
                    }
                    if(c2 == null)
                    {
                        c2 = uv;
                    }
                    cpos.Add(new Vector4(c1.Value.x, c1.Value.y, c2.Value.x, c2.Value.y));
                }
            }

            return Paint(uvs.ToArray(), cpos.ToArray(), sizeOffsets, renderCamera);
        }

        public override Vector2? WorldToPaintUV(Vector3 worldPos, Camera renderCamera = null)
        {
            if(EnableWorldSpacePaint)
            {
                return null;
            }

            if (renderCamera == null)
                renderCamera = Camera.main;

            Vector2 uv;
            Vector3 p = transform.InverseTransformPoint(worldPos);
            Matrix4x4 mvp = renderCamera.projectionMatrix * renderCamera.worldToCameraMatrix * transform.localToWorldMatrix;
            if (MeshOperator.LocalPointToUV(p, mvp, out uv))
            {
                //if(uv.x > 1 || uv.y > 1)
                //{
                //    uv = GetNearestTriangleSurface(worldPos);
                //}
                return uv;
            }
            return null;
        }

        public override Vector2? MousePointToPaintUV(Vector3 mousePos, Camera renderCamera = null)
        {
            if (renderCamera == null)
                renderCamera = Camera.main;

            if (EnableWorldSpacePaint)
            {
                var pos = renderCamera.ScreenToViewportPoint(mousePos);
                pos.y = 1 - pos.y;
                Debug.Log(string.Format("POS({0},{1})", pos.x, pos.y));
                return pos;
            }

            var ray = renderCamera.ScreenPointToRay(mousePos);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                var paintObject = hitInfo.transform.GetComponent<PaintCanvas>();
                if (paintObject == this)
                {
                    return WorldToPaintUV(hitInfo.point, renderCamera);
                }
            }
            return null;
        }

        /// <summary>
        /// Paint processing that use world-space surface position(must initial mesh).
        /// </summary>
        /// <param name="brush">Brush data.</param>
        /// <param name="uvs">paint uv list.</param>
        /// <param name="renderCamera">Camera to use to render the object.</param>
        /// <returns>The success or failure of the paint.</returns>
        public override EPaintStatus Paint(Vector2[] uvs, Vector4[] bezierControls = null, float[] sizeOffsets = null, Camera renderCamera = null)
        {
            if (MeshOperator == null)
            {
                return EPaintStatus.NoMeshOperatorError;
            }

            return base.Paint(uvs, bezierControls, sizeOffsets, renderCamera);
        }

        /// <summary>
		/// Paint processing that use raycast hit data(must initial mesh).
		/// Must MeshCollider is set to the canvas.
		/// </summary>
		/// <param name="hitInfo">Raycast hit info.</param>
		/// <returns>The success or failure of the paint.</returns>
		public EPaintStatus Paint(RaycastHit hitInfo)
        {
            if (MeshOperator == null)
            {
                return EPaintStatus.NoMeshOperatorError;
            }

            if (hitInfo.collider != null)
            {
                if (hitInfo.collider is MeshCollider)
                    return PaintUVDirect(new []{ new PaintPoint(hitInfo.textureCoord,0) });
                Debug.LogWarning("If you want to paint using a RaycastHit, need set MeshCollider for object.");
                return PaintNearestTriangleSurface(new[] { hitInfo.point }, null);
            }
            return EPaintStatus.NoColliderError;
        }

        #endregion

        /**
         *auto add layer 
         **/
#if UNITY_EDITOR
        bool _materialInitialed = false;
#endif
        protected virtual Material[] GetMaterials()
        {
            var materials = GetComponent<Renderer>().sharedMaterials;
#if UNITY_EDITOR
            if (!_materialInitialed)
            {
                materials = materials.Select(m => UnityEngine.Object.Instantiate<Material>(m)).ToArray();
                GetComponent<Renderer>().sharedMaterials = materials;
                for (var i = 0; i < materials.Length; i++)
                {
                    if (materials[i] != null)
                    {
                        materials[i].name = materials[i].name.Replace(" (Instance)", "").Replace("(Clone)", "");
                    }
                }

                _materialInitialed = true;
            }
#endif
            return materials;
        }

        protected override void Reset()
        {
            base.Reset();

            var materials = GetMaterials();
            if (LayerSettings == null || LayerSettings.Count == 0)
            {
                for (var i = 0; i < materials.Length; i++)
                {
                    var mat = materials[i];
                    var mattex = new MeshLayerSetting();
                    mattex.Material = mat;
                    mattex.TextureName = mat.mainTexture ? mat.mainTexture.name : "";
                    LayerSettings.Add(mattex);
                }
            }

            if (Application.isPlaying)
            {
                for (var i = 0; i < LayerSettings.Count; i++)
                {
                    var setting = LayerSettings[i];
                    var rawTexture = setting.RawTexture;
                    if (rawTexture == null && setting.Material != null)
                    {
                        rawTexture = setting.TextureName != "" ? setting.Material.GetTexture(setting.TextureName) : null;
                        setting.RawTexture = rawTexture;
                    }
                    if (rawTexture != null)
                    {
                        setting.DefaultSize = new Vector2(rawTexture.width, rawTexture.height);
                    }

                    var paintLayer = AddLayer(setting,
                        (layer, textName, texture, data) =>
                        {
                            if(TextureChangedHandle != null)
                            {
                                TextureChangedHandle(layer, textName, texture, data);
                                return;
                            }

                            var layerSetting = (MeshLayerSetting)data;
                            if (layerSetting.Material != null)
                            {
                                if (string.IsNullOrEmpty(textName))
                                {
                                    layerSetting.Material.mainTexture = texture;
                                }
                                else
                                {
                                    layerSetting.Material.SetTexture(textName, texture);
                                }
                            }
                            else
                            {
                                for(var mi=0;mi< materials.Length;mi++)
                                {
                                    var mat = materials[mi];
                                    if (string.IsNullOrEmpty(textName))
                                    {
                                        mat.mainTexture = texture;
                                    }
                                    else
                                    {
                                        mat.SetTexture(textName, texture);
                                    }
                                }
                            }
                        }, setting);

                    paintLayer.OnBeforeBlend += OnBeforeLayerBlend;
                }
            }
        }

        private void OnBeforeLayerBlend(PaintCanvasLayer layer, RenderTexture texture, out RenderTexture uvMap)
        {
            uvMap = null;
            return;

            //if(EnableWorldSpacePaint)
            //{
            //    uvMap = _screenUVTexture;
            //}
        }
    }
}
