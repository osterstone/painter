using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Wing.uPainter
{
    public class InputManager : MonoSingleton<InputManager>
    {
        public delegate void KeyMapEventHandler(object data=null);

        public class KeyMap
        {
            public KeyGroup KeyGroup;
            public KeyMapEventHandler Callback;
            public object Data;

            public KeyMap(KeyGroup keygroup, KeyMapEventHandler callback, object data = null)
            {
                KeyGroup = keygroup;
                Callback = callback;
                Data = data;
            }

            public void Call()
            {
                if(Callback != null)
                {
                    Callback(Data);
                }
            }
        }

        public static bool IsMatch(KeyGroup keyMap, SceneInteraction interaction)
        {
            bool ret = true;

            ret &= (interaction.IsAlt == keyMap.IsAlt);
            ret &= (interaction.IsControl == keyMap.IsControl);
            ret &= (interaction.IsShift == keyMap.IsShift);

            if (ret)
            {
                foreach (KeyCode code in keyMap.Codes)
                {
                    ret &= interaction.CodeStates[code] == EKeyState.Down;
                    if (!ret)
                    {
                        break;
                    }
                }
            }

            return ret;
        }

        private Dictionary<string, KeyMap> _keymaps = new Dictionary<string, KeyMap>();

        public void AddKeyGroup(KeyGroup kg, KeyMapEventHandler callback, object data = null)
        {
            if(!_keymaps.ContainsKey(kg.GetKey()))
            {
                _keymaps.Add(kg.GetKey(), new KeyMap(kg, callback, data));
            }            
        }

        public void RemoveKeyGroup(KeyGroup kg)
        {
            _keymaps.Remove(kg.GetKey());
        }

        void Start()
        {
            
        }

        void OnEnable()
        {
            SceneInteraction.Instance.OnKeyDown += Instance_OnKeyDown;
        }

        void OnDisable()
        {
            SceneInteraction.Instance.OnKeyDown -= Instance_OnKeyDown;
        }

        private void Instance_OnKeyDown(SceneInteraction sender, UnityEngine.KeyCode code)
        {
            List<KeyCode> downCodes = new List<KeyCode>();
            foreach(KeyValuePair<KeyCode,EKeyState> pair in sender.CodeStates)
            {
                if(pair.Value == EKeyState.Down && 
                    (pair.Key != KeyCode.LeftControl && pair.Key != KeyCode.RightControl) &&
                    (pair.Key != KeyCode.LeftAlt && pair.Key != KeyCode.RightAlt) &&
                    (pair.Key != KeyCode.LeftShift && pair.Key != KeyCode.RightShift))
                {
                    downCodes.Add(pair.Key);
                }
            }
            
            KeyGroup kg = new KeyGroup(sender.IsShift, sender.IsControl, sender.IsAlt, downCodes.ToArray());
            if(_keymaps.ContainsKey(kg.GetKey()))
            {
                _keymaps[kg.GetKey()].Call();
            }
        }
    }
}
