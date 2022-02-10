using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Wing.uPainter
{
    public class KeyGroup : IComparer<KeyGroup>
    {
        private List<KeyCode> _codes = new List<KeyCode>();
        private string _key = null;

        private bool _isShift, _isControl, _isAlt;

        public KeyGroup(string key)
        {
            string[] keys = key.Split(':');
            this._isControl = keys[0] == "1";
            this._isShift = keys[1] == "1";
            this._isAlt = keys[2] == "1";

            string codestr = keys[3];
            if (!string.IsNullOrEmpty(codestr))
            {
                string[] codes = codestr.Split(',');
                foreach (string c in codes)
                {
                    int code = int.Parse(c);
                    KeyCode keycode = (KeyCode)code;
                    _codes.Add(keycode);
                }
                _codes.Sort();
            }
        }

        public KeyGroup(bool isShift, bool isControl, bool isAlt, params KeyCode[] keys)
        {
            this._isShift = isShift;
            this._isControl = isControl;
            this._isAlt = isAlt;

            foreach (KeyCode code in keys)
            {
                if (!_codes.Contains(code))
                {
                    _codes.Add(code);
                }
            }
            _codes.Sort();
        }

        public List<KeyCode> Codes
        {
            get
            {
                return _codes;
            }
        }

        public bool IsShift
        {
            get
            {
                return _isShift;
            }
        }

        public bool IsControl
        {
            get
            {
                return _isControl;
            }
        }

        public bool IsAlt
        {
            get
            {
                return _isAlt;
            }
        }

        public string GetKey()
        {
            if (string.IsNullOrEmpty(_key))
            {
                string[] codestrs = new string[_codes.Count];
                for (int i = 0; i < codestrs.Length; i++)
                {
                    KeyCode code = _codes[i];
                    codestrs[i] = ((int)code).ToString();
                }

                _key = string.Format("{0}:{1}:{2}:{3}",
                    IsControl ? 1 : 0,
                    IsShift ? 1 : 0,
                    IsAlt ? 1 : 0,
                    string.Join(",", codestrs.ToArray()));
            }

            return _key;
        }

        public int Compare(KeyGroup x, KeyGroup y)
        {
            bool ret = false;
            ret &= x.IsShift == y.IsShift;
            ret &= x.IsControl == y.IsControl;
            ret &= x.IsAlt == y.IsAlt;

            if (ret)
            {
                ret &= x.Codes.Count == y.Codes.Count;
                ret &= x.GetKey() == y.GetKey();
            }//end if

            return ret ? 0 : 1;
        }
    }
}
