//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;
//using Wing.uPainter;

//public class MouseDrawer : MonoBehaviour
//{

//    public EventSystem eventSystem;
//    public GraphicRaycaster graphicRaycaster;
//    public PointerEventData pointEventData;

//    PaintCanvas _lastCanvas;
//    Vector3? _lastPos;
//    Vector3? _lastPos2;
//    Vector3? _lastPos3;

//    float _lastOffset, _lastOffset2, _lastOffset3;

//    private void Start()
//    {
//        pointEventData = new PointerEventData(eventSystem);
//    }

//    private void Update()
//    {
//        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//        EPaintStatus success = EPaintStatus.Failed;

//        pointEventData.position = Input.mousePosition;
//        List<RaycastResult> results = new List<RaycastResult>();
//        graphicRaycaster.Raycast(pointEventData, results);
//        for (var i = 0; i < results.Count; i++)
//        {
//            var paintObject = results[i].gameObject.GetComponent<PaintCanvas>();
//            if (paintObject is RawImagePaintCanvas)
//            {
//                var rimgcanvas = (paintObject as RawImagePaintCanvas);
//                rimgcanvas.Brush.InPreview = !Input.GetMouseButton(0);
//                success = rimgcanvas.Paint(Input.mousePosition);
//            }
//        }

//        if (success != EPaintStatus.Success)
//        {
//            RaycastHit hitInfo;
//            if (Physics.Raycast(ray, out hitInfo))
//            {
//                var paintObject = hitInfo.transform.GetComponent<PaintCanvas>();
//                if (paintObject is MeshPaintCanvas)
//                {
//                    var mcanvas = paintObject as MeshPaintCanvas;
//                    var lastInPreview = mcanvas.Brush.InPreview;
//                    var curInPreview = !Input.GetMouseButton(0);
//                    if (!lastInPreview && curInPreview)
//                    {
//                        if (_lastCanvas != null)
//                        {
//                            _lastCanvas.EndDraw();
//                            _lastCanvas = null;
//                        }
//                    }
//                    mcanvas.Brush.InPreview = curInPreview;

//                    var pos = new List<Vector3>();
//                    var offsets = new List<float>();
//                    float offset = 0;
//                    if (_lastCanvas == mcanvas && !mcanvas.Brush.InPreview)
//                    {
//                        if (_lastPos3 != null)
//                        {
//                            pos.Add(_lastPos3.Value);
//                            offsets.Add(_lastOffset3);
//                        }
//                        if (_lastPos2 != null)
//                        {
//                            pos.Add(_lastPos2.Value);
//                            offsets.Add(_lastOffset2);
//                        }
//                        if (_lastPos != null)
//                        {
//                            pos.Add(_lastPos.Value);
//                            offsets.Add(_lastOffset);

//                            offset = -Mathf.Min(((_lastPos.Value - hitInfo.point).magnitude - 1) / 50f, mcanvas.Brush.Width * 0.5f);
//                            offset = (offset + _lastOffset) * 0.5f;
//                        }

//                        pos.Add(hitInfo.point);
//                        offsets.Add(offset);
//                    }
//                    else
//                    {
//                        pos.Add(hitInfo.point);
//                        offsets.Add(0);

//                        _lastPos3 = null;
//                        _lastPos2 = null;
//                        _lastPos = null;

//                        _lastOffset = 0;
//                        _lastOffset2 = 0;
//                        _lastOffset3 = 0;
//                    }

//                    var showPreview = mcanvas.Brush.InPreview && mcanvas.Brush.ShowPreview;
//                    if (pos.Count > 1 || showPreview)
//                    {
//                        success = mcanvas.Paint(pos.ToArray(), offsets.ToArray());
//                    }
//                    else
//                    {
//                        success = EPaintStatus.Success;
//                    }

//                    if (!mcanvas.Brush.InPreview && success == EPaintStatus.Success)
//                    {
//                        if (_lastCanvas != mcanvas)
//                        {
//                            if (_lastCanvas != null)
//                            {
//                                _lastCanvas.EndDraw();
//                            }
//                            _lastCanvas = mcanvas;
//                            _lastCanvas.BeginDraw();
//                        }

//                        if (_lastPos != hitInfo.point)
//                        {
//                            _lastPos3 = _lastPos2;
//                            _lastPos2 = _lastPos;
//                            _lastPos = hitInfo.point;

//                            _lastOffset3 = _lastOffset2;
//                            _lastOffset2 = _lastOffset;
//                            _lastOffset = offset;
//                        }
//                    }
//                    else if (success != EPaintStatus.BrushCheckFailed && success != EPaintStatus.Success)
//                    {
//                        if (_lastCanvas != null)
//                        {
//                            _lastCanvas.EndDraw();
//                        }
//                        _lastCanvas = null;
//                        _lastPos3 = null;
//                        _lastPos2 = null;
//                        _lastPos = null;

//                        _lastOffset = 0;
//                        _lastOffset2 = 0;
//                        _lastOffset3 = 3;
//                    }
//                }
//            }
//            else
//            {
//                if (_lastCanvas != null)
//                {
//                    _lastCanvas.EndDraw();
//                }
//                _lastCanvas = null;
//                _lastPos3 = null;
//                _lastPos2 = null;
//                _lastPos = null;

//                _lastOffset = 0;
//                _lastOffset2 = 0;
//                _lastOffset3 = 0;
//            }
//        }
//    }
//}
