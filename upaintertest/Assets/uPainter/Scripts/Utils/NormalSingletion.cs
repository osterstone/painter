using UnityEngine;
using System.Collections;

namespace Wing.uPainter
{
    public class NormalSingleton<T> : IInitializer where T : new()
    {
        protected static T s_pInstance = new T();
        public static T Instance
        {
            get
            {
                return s_pInstance;
            }
        }

        public virtual void Initial()
        {

        }
    }
}
