using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Wing.uPainter;

public class DrawToUIWindow : EditorWindow
{
    public static DrawToUIWindow Instance
    {
        get;
        private set;
    }

    Rect _drawPosition;
    EditorCanvas _canvas;
    Drawer _drawer = new Drawer();
    bool _mouseDown = false;
    Vector2 _mousePoint;

    static void create()
    {
        if (Instance == null)
        {
            DrawToUIWindow window = (DrawToUIWindow)EditorWindow.GetWindow(typeof(DrawToUIWindow));
            Instance = window;
        }
    }

    private void Check()
    {
        if (_canvas == null)
        {
            _canvas = Camera.main.gameObject.GetComponent<EditorCanvas>();
            if (_canvas == null)
            {
                _canvas = Camera.main.gameObject.AddComponent<EditorCanvas>();
            }
            _canvas.Initial();
            _drawer.Catch(_canvas);
        }
    }

    private void OnEnable()
    {
        Check();
    }

    private void OnDestroy()
    {
        if (_canvas != null)
        {
            DestroyImmediate(_canvas);
        }
    }

    [MenuItem("Tools/uPainter/EditorDemo")]
    private static void InitEmpty()
    {
        create();
    }    

    private void Update()
    {
        if (_canvas == null || _canvas.Layer == null)
        {
            return;
        }
        if (focusedWindow != this)
            return;

        Check();

        var pos = _mousePoint;
        if (pos.x >= 0 && pos.y >= 0 && pos.x <= _drawPosition.width && pos.y <= _drawPosition.height)
        {
            if (_mouseDown)
            {
                _drawer.TouchMove(pos, Vector3.zero);
            }
            else
            {
                _drawer.HoverMove(pos, Vector3.zero);
            }
        }
        else
        {
            _drawer.End();
        }

        _canvas.OnUpdate();
        Repaint();
    }

    private void OnGUI()
    {
        if (_canvas == null || _canvas.Layer == null)
        {
            return;
        }
        _canvas.CanvasPosition = _drawPosition;

        var LEFT_WIDTH = 300;
        _drawPosition = position;
        _drawPosition.width -= LEFT_WIDTH;
        _drawPosition.height -= 0;
        _drawPosition.x = LEFT_WIDTH;
        _drawPosition.y = 0;

        var touchDown = Event.current.button == 0 && Event.current.type == EventType.MouseDown;
        if (touchDown)
        {
            _drawer.Begin();
            _mouseDown = true;
        }
        if (Event.current.button == 0 && Event.current.type == EventType.MouseUp)
        {
            _drawer.End();
            _mouseDown = false;
        }
        _mousePoint = GUIUtility.GUIToScreenPoint(Event.current.mousePosition) - new Vector2(position.x, position.y) - new Vector2(_drawPosition.x, _drawPosition.y); ;

        EditorGUILayout.BeginHorizontal(GUILayout.Width(LEFT_WIDTH - 20));

        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("Clear"))
        {
            _canvas.ClearAll();
        }
        if (GUILayout.Button("Save"))
        {
            var filename = EditorUtility.SaveFilePanel("Save File", UtilsHelper.GetDataPath(), "upainter", "png");
            if (!string.IsNullOrEmpty(filename))
            {
                _canvas.Layer.Save(filename, 1024, 512, EPictureType.PNG);
            }
        }

        var _oldBrush = _canvas.Brush;
        _canvas.Brush = (BaseBrush)EditorGUILayout.ObjectField(new GUIContent("Brush"), _canvas.Brush, typeof(BaseBrush), false);
        if(_oldBrush != _canvas.Brush)
        {
            _canvas.Brush = Instantiate<BaseBrush>(_canvas.Brush);
            if (_oldBrush != null)
            {
                DestroyImmediate(_oldBrush);
            }
            else
            {
#if UNITY_STANDALONE_OSX
                _canvas.ClearAll();
#endif
            }
        }
        _drawer.ShowPreview = EditorGUILayout.Toggle(new GUIContent("Enable Preview"), _drawer.ShowPreview);
        if(_canvas.Brush != null)
        {
            _canvas.Brush.BrushColor = EditorGUILayout.ColorField(new GUIContent("Brush Color"), _canvas.Brush.BrushColor);
            _canvas.Brush.Size = EditorGUILayout.Slider(new GUIContent("Brush Size"), _canvas.Brush.Size, 0f, 0.3f);
            if (_canvas.Brush is ScratchBrush)
            {
                var sb = _canvas.Brush as ScratchBrush;
                sb.PaintMode = (EPaintMode)EditorGUILayout.EnumPopup(new GUIContent("PaintMode"), sb.PaintMode);
                sb.CapStyle = (EBrushCapStyle)EditorGUILayout.EnumPopup(new GUIContent("Cap Style"), sb.CapStyle);
                sb.Softness = EditorGUILayout.Slider(new GUIContent("Softness"), sb.Softness, 0f, 1f);
            }
        }
        EditorGUILayout.EndVertical();

        GUI.DrawTexture(_drawPosition, _canvas.PaintTexture);
        EditorGUILayout.EndHorizontal();
    }
}
