using UnityEngine;
using System.Collections;
using System;

namespace Wing.uPainter
{
    public class MonoSingleton<T> : MonoBehaviour,IInitializer where T : MonoBehaviour
    {
        protected static bool s_bEnableAutoCreate = true;
        protected static T s_pInstance;
        //public static T Instance
        //{
        //	get
        //	{
        //		return s_pInstance;
        //	}
        //}

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public virtual void Initial()
        {
            
        }

        public static T Instance
        {
            get
            {
                if (s_pInstance == null)
                {
                    s_pInstance = GameObject.FindObjectOfType<T>();
                    if (s_pInstance == null && s_bEnableAutoCreate)
                    {
                        GameObject singleGO = GameObject.Find("Singletion");
                        if (singleGO == null)
                        {
                            singleGO = new GameObject("Singletion");
                        }

                        GameObject instanceObject = new GameObject(typeof(T).Name);
                        instanceObject.transform.SetParent(singleGO.transform);

                        s_pInstance = instanceObject.AddComponent<T>();
                    }
                    else if (s_pInstance == null)
                    {
                        //Debug.LogError("empty refrenced in this scene : " + typeof(T).Name);
                    }
                }
                return s_pInstance;
            }
        }
    }
}