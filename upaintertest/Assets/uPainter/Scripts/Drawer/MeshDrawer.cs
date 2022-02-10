using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Wing.uPainter;

namespace Wing.uPainter
{
    class MeshDrawer : MonoBehaviour
    {
        public bool EnablePaint = true;

        [SerializeField]
        Drawer _drawer = new Drawer();
        public Drawer Drawer
        {
            get
            {
                return _drawer;
            }
        }

        private void Awake()
        {
            InputManager.Instance.AddKeyGroup(new KeyGroup(false, true, true, KeyCode.Z), (dt) =>
            {
                PainterOperation.Instance.Undo();
            });

            InputManager.Instance.AddKeyGroup(new KeyGroup(false, true, true, KeyCode.Y), (dt) =>
            {
                PainterOperation.Instance.Redo();
            });
        }

        protected virtual void Update()
        {
            if(!EnablePaint)
            {
                return;
            }

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                var paintObject = hitInfo.transform.GetComponent<PaintCanvas>();
                var touchDown = Input.GetMouseButton(0);
                _drawer.Catch(paintObject);

                if (paintObject is MeshPaintCanvas)
                {
                    var mpcanvas = paintObject as MeshPaintCanvas;

                    Vector3? uv = null;
                    if (!mpcanvas.EnableWorldSpacePaint)
                    {
                        if (hitInfo.collider is MeshCollider)
                        {
                            uv = hitInfo.textureCoord;
                        }
                    }

                    if (touchDown)
                    {
                        _drawer.Begin();
                        _drawer.TouchMove(Input.mousePosition, hitInfo.point, uv);
                    }
                    else
                    {
                        _drawer.End();
                        _drawer.HoverMove(Input.mousePosition, hitInfo.point, uv);
                    }
                }
            }
            else
            {
                _drawer.End();
            }
        }
    }
}
