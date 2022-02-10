using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/*
 *	
 *  Singleton
 *
 *	by Xuanyi
 *
 */

namespace Wing.uPainter
{
    public static class Singleton<T> where T : class
    {
        /*	Instance	*/
        private static T _instance;

        /* Static constructor	*/
        static Singleton()
        {
            return;
        }

        public static T Create()
        {
            _instance = (T)Activator.CreateInstance(typeof(T), true);

            return _instance;
        }

        /* Serve the single instance to callers	*/
        public static T Instance
        {
            get
            {
                return _instance;
            }
        }

        /*	Destroy	*/
        public static void Destroy()
        {

            _instance = null;

            return;
        }
    }
}
