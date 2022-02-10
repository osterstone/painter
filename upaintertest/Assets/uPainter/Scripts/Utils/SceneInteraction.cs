using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Wing.uPainter
{
    public enum EMouseButton
    {
        None = -1,
        Left = 0,
        Right = 1,
        Middle = 2,
    }

    public enum EKeyState
    {
        Down,
        Up,
    }

    public class SceneInteraction : MonoSingleton<SceneInteraction>
    {
        private Camera m_camera = null;

        private GameObject m_currentGameObject = null;

        private KeyCode _currentCode;
        public KeyCode CurrentCode
        {
            get
            {
                return _currentCode;
            }
        }

        private EMouseButton _currentMouseCode;
        public EMouseButton CurrentMouseCode
        {
            get
            {
                return _currentMouseCode;
            }
        }
        public bool IsShift
        {
            get;
            private set;
        }

        public bool IsControl
        {
            get;
            private set;
        }

        public bool IsAlt
        {
            get;
            private set;
        }

        public delegate void KeyEventHandler(SceneInteraction sender, KeyCode code);
        public event KeyEventHandler OnKeyDown;
        public event KeyEventHandler OnKeyUp;
        public event KeyEventHandler OnKey;

        private Dictionary<KeyCode, EKeyState> _codeStates = new Dictionary<KeyCode, EKeyState>();
        private List<KeyCode> _downCodes = new List<KeyCode>();

        public Dictionary<KeyCode, EKeyState> CodeStates
        {
            get
            {
                return _codeStates;
            }
        }

        private void Start()
        {
            m_camera = Camera.main;
        }

        private void UpdateState()
        {
            if (Event.current != null && Input.anyKey)
            {
                IsShift = Event.current.shift;
                IsControl = Event.current.control;
                IsAlt = Event.current.alt;

                if (Event.current.isKey)
                {
                    _currentCode = Event.current.keyCode;
                }

                if (Event.current.isMouse)
                {
                    _currentMouseCode = (EMouseButton)Event.current.button;
                }
            }
            else
            {
                IsShift = false;
                //_currentMouseCode = EMouseButton.None;
                IsControl = false;
                IsAlt = false;
            }
        }

        private void UpdateKeyboard()
        {
            if(_codeStates.Count == 0)
            {
                var keys = Enum.GetValues(typeof(KeyCode));
                foreach (KeyCode key in keys)
                {
                    if (!_codeStates.ContainsKey(key))
                    {
                        _codeStates.Add(key, Input.GetKeyDown(key) ? EKeyState.Down : EKeyState.Up);
                    }
                }
            }

            for(int i=0;i< _downCodes.Count;i++)
            {
                KeyCode c = _downCodes[i];
                if (!Input.GetKey(c))
                {
                    _codeStates[c] = EKeyState.Up;
                    _downCodes.Remove(c);
                    i--;

                    if (OnKeyUp != null)
                    {
                        OnKeyUp(this, c);
                    }
                }
            }

            KeyCode code = Event.current.keyCode;
            if (_codeStates[code] != EKeyState.Down && Input.GetKeyDown(code))
            {
                _codeStates[code] = EKeyState.Down;
                _downCodes.Add(code);
                if (OnKeyDown != null)
                {
                    OnKeyDown(this, code);
                }
            }
            
            if(Input.GetKey(code))
            {
                if(OnKey != null)
                {
                    OnKey(this, code);
                }
            }
        }

        void OnGUI()
        {
            UpdateState();
            UpdateKeyboard();
        }

        void Update()
        {     
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            GameObject go = null;
            if (Input.touches.Length > 0)
            {
                go = DetectiveGameObject(m_camera, Input.touches[0].position);
            }
            else
            {
                go = DetectiveGameObject(m_camera, Input.mousePosition);
            }
            if (go != m_currentGameObject)
            {
            }
            m_currentGameObject = go;
        }

        public static GameObject DetectiveGameObject(Camera camera, Vector2 screenPoint)
        {
            Ray ray = camera.ScreenPointToRay(new Vector3(screenPoint.x, screenPoint.y, 0));
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo))
            {
                GameObject go = hitInfo.collider.gameObject;

                return go;
            }

            return null;
        }
    }
}