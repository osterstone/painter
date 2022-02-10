
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Wing.uPainter
{
    public class UtilsHelper
    {
        public const int OpenVersion3 = 0x00030000;


        //不同平台下StreamingAssets的路径是不同的，这里需要注意一下。  
        public static string WWWStreamAssetsPath
        {
            get
            {
				string path = "";
                if (Application.platform == RuntimePlatform.Android)
                {
					path = "jar:file:///" + Application.dataPath + "!/assets/";
				}
                else if(Application.platform == RuntimePlatform.IPhonePlayer)
				{
					path = "file://" + Application.dataPath + "/Raw/";
				}
                //else if(Application.platform == RuntimePlatform.OSXEditor ||  Application.platform == RuntimePlatform.OSXPlayer)
                //{
                //    path = "file:/" + Application.dataPath + "/StreamingAssets/";
                //}
				else
				{
					path = "file://" + Application.dataPath + "/StreamingAssets/";
				}

				return path;   
            }
        }

		public static string StreamAssetsPath
		{
			get
			{
				string path = "";
				if (Application.platform == RuntimePlatform.Android)
				{
					path = Application.dataPath + "!/assets/";
				}
				else if (Application.platform == RuntimePlatform.IPhonePlayer)
				{
					path = Application.dataPath + "/Raw/";
				}
				//else if(Application.platform == RuntimePlatform.OSXEditor ||  Application.platform == RuntimePlatform.OSXPlayer)
				//{
				//    path = "file:/" + Application.dataPath + "/StreamingAssets/";
				//}
				else
				{
					path = Application.dataPath + "/StreamingAssets/";
				}

				return path;
			}
		}

        public static string GetDataPath()
        {
            string path = "";
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                path = Application.persistentDataPath + "/";
            }
            else if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                path = Application.dataPath + "/";
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                path = Application.dataPath + "/../";
            }
            else
            {
                path = Application.persistentDataPath + "/";
            }

            return path;
        }

        public static string GetResourcePath()
        {
            return GetDataPath() + "ResData/";
        }

        public static string GetExtentAssetsPath()
        {
            return GetDataPath() + "AppAssets/";
        }

        public static void SaveTextureFile(Texture2D incomingTexture, string filename, bool png = true)
        {
            byte[] bytes = png ? incomingTexture.EncodeToPNG() : incomingTexture.EncodeToJPG();
            string dir = Path.GetDirectoryName(filename);
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllBytes(filename, bytes);
        }

        public static bool SaveRenderTextureToPNG(Texture inputTex, Material mat, string filename)
        {
            RenderTexture temp = RenderTexture.GetTemporary(inputTex.width, inputTex.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(inputTex, temp, mat);
            bool ret = SaveRenderTextureToPNG(temp, filename);
            RenderTexture.ReleaseTemporary(temp);
            return ret;
        }
        
        public static bool SaveRenderTextureToPNG(RenderTexture rt, string filename)
        {
            Texture2D png = CreateTexture2DFromRT(rt);

            SaveTextureFile(png, filename);

            Texture2D.DestroyImmediate(png);
            png = null;
            return true;
        }

        public static Texture2D CreateTexture2DFromRT(RenderTexture rt, Rect? rect = null)
        {
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            if(rect == null)
            {
                rect = new Rect(0, 0, rt.width, rt.height);
            }
            else
            {
                float width = rect.Value.width;
                if(rect.Value.width + rect.Value.x > rt.width)
                {
                    width = rt.width - rect.Value.x;
                }
                float height = rect.Value.height;
                if(rect.Value.height + rect.Value.y > rt.height)
                {
                    height = rt.height - rect.Value.y;
                }
                rect = new Rect(rect.Value.x, rect.Value.y, width, height);
            }
            Texture2D texture = new Texture2D((int)rect.Value.width, (int)rect.Value.height, TextureFormat.ARGB32, false);
            texture.ReadPixels(rect.Value, 0 , 0);
            texture.Apply();

            RenderTexture.active = prev;
            return texture;

        }

        public static Texture2D LoadPNGFromDisk(string url)
        {
            if (!File.Exists(url))
            {
                return null;
            }
            FileStream fileStream = new FileStream(url, FileMode.Open, System.IO.FileAccess.Read);

            fileStream.Seek(0, SeekOrigin.Begin);

            byte[] binary = new byte[fileStream.Length]; //创建文件长度的buffer
            fileStream.Read(binary, 0, (int)fileStream.Length);

            fileStream.Close();

            fileStream.Dispose();

            fileStream = null;

            var tex = new Texture2D(1, 1);
            tex.LoadImage(binary);
            return tex;
        }

		public static string StringMD5(string data)
		{
			byte[] result = Encoding.Default.GetBytes(data.Trim());
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] output = md5.ComputeHash(result);
			return BitConverter.ToString(output).Replace("-", "");
		}

	    public static string GetFileHash(string filePath)
	    {
	        try
	        {
	            FileStream fs = new FileStream(filePath, FileMode.Open);
	            int len = (int)fs.Length;
	            byte[] data = new byte[len];
	            fs.Read(data, 0, len);
	            fs.Close();
	            System.Security.Cryptography.MD5 md5 = new MD5CryptoServiceProvider();
	            byte[] result = md5.ComputeHash(data);
	            string fileMD5 = "";
	            foreach (byte b in result)
	            {
	                fileMD5 += Convert.ToString(b, 16);
	            }
	            return fileMD5;
	        }
	        catch (FileNotFoundException e)
	        {
	            Console.WriteLine(e.Message);
	            return "";
	        }
	    }

        public static double GetUTCMilliseconds()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return ts.TotalMilliseconds;
        }

        private static int sOpenGLVersion = -1;
        public static bool IsAndroidGLVersionLGT3()
        {
            return GetOpenGLVersion() < 0 || GetOpenGLVersion() >= OpenVersion3;
        }

        public static int GetOpenGLVersion()
        {
            int version = -1;
#if (UNITY_ANDROID && !UNITY_EDITOR) || ANDROID_CODE_VIEW
            if(sOpenGLVersion > 0) 
            {   
                return sOpenGLVersion;
            }
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        using (AndroidJavaObject curApplication = currentActivity.Call<AndroidJavaObject>("getApplication"))
                        {
                            using (AndroidJavaObject curSystemService = curApplication.Call<AndroidJavaObject>("getSystemService", "activity"))
                            {
                                using (AndroidJavaObject curConfigurationInfo = curSystemService.Call<AndroidJavaObject>("getDeviceConfigurationInfo"))
                                {
                                    version = curConfigurationInfo.Get<int>("reqGlEsVersion");
                                    //using (AndroidJavaClass curInteger = AndroidJavaClass("java.lang.Integer"))
                                    //{
                                    //    version = curInteger.CallStatic<string>("toString",reqGlEsVersion,16); //30000
                                    //}

                                    sOpenGLVersion = version;

                                }
                            }
                        }
                    } 
                }
            }
            catch (Exception e)
            {
                 Debug.Log("GetOpenGL, Exception: " + e.ToString());
            }
#endif
            return version;
        }
    }
}
