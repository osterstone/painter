using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Wing.uPainter
{
    public class TextureTool
    {
        public static void SaveToFile(Texture texture, string filename, int width, int height, EPictureType type)
        {
            Texture2D tex = null;
            if (texture is Texture2D)
            {
                tex = (Texture2D)texture;
            }
            else if(texture is RenderTexture)
            {
                tex = ToTexture2D(texture as RenderTexture);
            }
            else
            {
                return;
            }

            byte[] bytes = null;
            if (type == EPictureType.JPG)
            {
                bytes = tex.EncodeToJPG();
            }
            else
            {
                bytes = tex.EncodeToPNG();
            }
            tex.Apply();

            tex.Resize(width, height);

            FileStream file = File.Open(filename, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(file);
            writer.Write(bytes);
            file.Close();
        }

        public static Texture2D ToTexture2D(RenderTexture rt)
        {
            var old = RenderTexture.active;
            RenderTexture.active = rt;
            
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false, true);
            tex.filterMode = rt.filterMode;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            RenderTexture.active = old;

            return tex;
        }

        public static void CopyToTexture2D(RenderTexture rt, Texture2D to)
        {
            var old = RenderTexture.active;
            RenderTexture.active = rt;

            to.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            to.Apply();

            RenderTexture.active = old;
        }

        public static void ClearTexture(RenderTexture target, Color background, Texture baseTex = null)
        {
            if (baseTex != null)
            {
                Graphics.Blit(baseTex, target);
            }
            else
            {
                Graphics.SetRenderTarget(target);
                GL.Clear(false, true, background);
                Graphics.SetRenderTarget(null);
            }
        }

        public static RenderTexture CreateRenderTexture(Texture baseTex, int width, int height, bool copy = true, Color? background = null, FilterMode? filterMode = null)
        {
            var rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

            if (baseTex != null)
            {
                rt.filterMode = baseTex.filterMode;
                if (copy)
                {
                    Graphics.Blit(baseTex, rt);
                }
            }
            else
            {
                rt.filterMode = FilterMode.Bilinear;
                if (background != null)
                {
                    //var tex = new Texture2D(1, 1);
                    //tex.SetPixel(0, 0, background.Value);
                    //tex.Apply();
                    //Graphics.Blit(tex, rt);
                    //Object.Destroy(tex);

                    Graphics.SetRenderTarget(rt);
                    GL.Clear(false, true, background.Value);
                    Graphics.SetRenderTarget(null);
                }
            }

            if (filterMode != null)
            {
                rt.filterMode = filterMode.Value;
            }

            return rt;
        }

        public static RenderTexture GetTempRenderTexture(Texture baseTex, int width, int height, bool copy = true, Color? background = null, FilterMode? filterMode = null)
        {
            var rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

            if (baseTex != null)
            {
                rt.filterMode = baseTex.filterMode;
                if (copy)
                {
                    Graphics.Blit(baseTex, rt);
                }
            }
            else
            {
                rt.filterMode = FilterMode.Bilinear;
                if (background != null)
                {
                    //var tex = new Texture2D(1, 1);
                    //tex.SetPixel(0, 0, background.Value);
                    //tex.Apply();
                    //Graphics.Blit(tex, rt);
                    //Object.Destroy(tex);

                    Graphics.SetRenderTarget(rt);
                    GL.Clear(false, true, background.Value);
                    Graphics.SetRenderTarget(null);
                }
            }

            if(filterMode != null)
            {
                rt.filterMode = filterMode.Value;
            }

            return rt;
        }

        public static float[] RenderTextureToArray(RenderTexture texture)
        {
            var tex = ToTexture2D(texture);
            var texWidth = texture.width;
            var texHeight = texture.height;
            var pixels = new float[texWidth * texHeight * 4];

            int pixel = 0;
            for (int y = 0; y < texHeight; y++)
            {
                for (int x = 0; x < texWidth; x++)
                {
                    Color c = tex.GetPixel(x, y);

                    pixels[pixel] = c.r;
                    pixels[pixel + 1] = c.g;
                    pixels[pixel + 2] = c.b;
                    pixels[pixel + 3] = c.a;
                    pixel += 4;
                }
            }

            return pixels;
        }

        public static Color32[] RenderTextureToColors(RenderTexture texture)
        {
            var tex = ToTexture2D(texture);            
            var data = tex.GetPixels32();
            GameObject.DestroyImmediate(tex);
            return data;
        }

        public static Texture2D ColorsToTexture(Color32[] color, int texWidth, int texHeight)
        {
            var texture = new Texture2D(texWidth, texHeight, TextureFormat.ARGB32, false);
            texture.SetPixels32(color);
            texture.Apply();
            return texture;
        }

        public static void FloodFill(Color32[] colors, int texWidth, int texHeight, int x, int y, Color32 paintColor, bool canDrawOnBlack = false, Color32[] target = null, byte threshold = 0)
        {
            // get canvas hit color
            var hitColor = colors[(texWidth * (y) + x)];
            var hitColorR = hitColor.r;
            var hitColorG = hitColor.g;
            var hitColorB = hitColor.b;
            var hitColorA = hitColor.a;

            if (!canDrawOnBlack)
            {
                if (hitColorR == 0 && hitColorG == 0 && hitColorB == 0 && hitColorA != 0) return;
            }

            // early exit if its same color already
            if (paintColor.r == hitColorR && paintColor.g == hitColorG && paintColor.b == hitColorB && paintColor.a == hitColorA) return;

            Queue<int> fillPointX = new Queue<int>();
            Queue<int> fillPointY = new Queue<int>();
            fillPointX.Enqueue(x);
            fillPointY.Enqueue(y);

            int ptsx, ptsy;
            int pixel = 0;

            if (threshold == 0)
            {
                while (fillPointX.Count > 0)
                {
                    ptsx = fillPointX.Dequeue();
                    ptsy = fillPointY.Dequeue();

                    if (ptsy - 1 > -1)
                    {
                        pixel = texWidth * (ptsy - 1) + ptsx; // down
                        var c = colors[pixel];
                        if (c.r == hitColorR
                            && c.g == hitColorG
                            && c.b == hitColorB
                            && c.a == hitColorA)
                        {
                            fillPointX.Enqueue(ptsx);
                            fillPointY.Enqueue(ptsy - 1);
                            colors[pixel] = paintColor;

                            if (target != null)
                            {
                                target[pixel] = paintColor;
                            }
                        }
                    }

                    if (ptsx + 1 < texWidth)
                    {
                        pixel = texWidth * ptsy + ptsx + 1; // right
                        var c = colors[pixel];
                        if (c.r == hitColorR
                            && c.g == hitColorG
                            && c.b == hitColorB
                            && c.a == hitColorA)
                        {
                            fillPointX.Enqueue(ptsx + 1);
                            fillPointY.Enqueue(ptsy);
                            colors[pixel] = paintColor;

                            if (target != null)
                            {
                                target[pixel] = paintColor;
                            }
                        }
                    }

                    if (ptsx - 1 > -1)
                    {
                        pixel = texWidth * ptsy + ptsx - 1; // left
                        var c = colors[pixel];
                        if (c.r == hitColorR
                            && c.g == hitColorG
                            && c.b == hitColorB
                            && c.a == hitColorA)
                        {
                            fillPointX.Enqueue(ptsx - 1);
                            fillPointY.Enqueue(ptsy);
                            colors[pixel] = paintColor;

                            if (target != null)
                            {
                                target[pixel] = paintColor;
                            }
                        }
                    }

                    if (ptsy + 1 < texHeight)
                    {
                        pixel = texWidth * (ptsy + 1) + ptsx; // up
                        var c = colors[pixel];
                        if (c.r == hitColorR
                            && c.g == hitColorG
                            && c.b == hitColorB
                            && c.a == hitColorA)
                        {
                            fillPointX.Enqueue(ptsx);
                            fillPointY.Enqueue(ptsy + 1);
                            colors[pixel] = paintColor;

                            if (target != null)
                            {
                                target[pixel] = paintColor;
                            }
                        }
                    }
                }
            }
            else
            {
                var lockMaskPixels = new byte[texWidth * texHeight * 4];
                while (fillPointX.Count > 0)
                {
                    ptsx = fillPointX.Dequeue();
                    ptsy = fillPointY.Dequeue();

                    if (ptsy - 1 > -1)
                    {
                        pixel = texWidth * (ptsy - 1) + ptsx; // down
                        var c = colors[pixel];
                        if (lockMaskPixels[pixel] == 0
                            && CompareThreshold(c.r, hitColorR, threshold)
                            && CompareThreshold(c.g, hitColorG, threshold)
                            && CompareThreshold(c.b, hitColorB, threshold)
                            && CompareThreshold(c.a, hitColorA, threshold))
                        {
                            fillPointX.Enqueue(ptsx);
                            fillPointY.Enqueue(ptsy - 1);
                            colors[pixel] = paintColor;
                            lockMaskPixels[pixel] = 1;

                            if (target != null)
                            {
                                target[pixel] = paintColor;
                            }
                        }
                    }

                    if (ptsx + 1 < texWidth)
                    {
                        pixel = texWidth * ptsy + ptsx + 1; // right
                        var c = colors[pixel];
                        if (lockMaskPixels[pixel] == 0
                            && CompareThreshold(c.r, hitColorR, threshold)
                            && CompareThreshold(c.g, hitColorG, threshold)
                            && CompareThreshold(c.b, hitColorB, threshold)
                            && CompareThreshold(c.a, hitColorA, threshold))
                        {
                            fillPointX.Enqueue(ptsx + 1);
                            fillPointY.Enqueue(ptsy);
                            colors[pixel] = paintColor;
                            lockMaskPixels[pixel] = 1;

                            if (target != null)
                            {
                                target[pixel] = paintColor;
                            }
                        }
                    }

                    if (ptsx - 1 > -1)
                    {
                        pixel = texWidth * ptsy + ptsx - 1; // left
                        var c = colors[pixel];
                        if (lockMaskPixels[pixel] == 0
                            && CompareThreshold(c.r, hitColorR, threshold)
                            && CompareThreshold(c.g, hitColorG, threshold)
                            && CompareThreshold(c.b, hitColorB, threshold)
                            && CompareThreshold(c.a, hitColorA, threshold))
                        {
                            fillPointX.Enqueue(ptsx - 1);
                            fillPointY.Enqueue(ptsy);
                            colors[pixel] = paintColor;
                            lockMaskPixels[pixel] = 1;

                            if (target != null)
                            {
                                target[pixel] = paintColor;
                            }
                        }
                    }

                    if (ptsy + 1 < texHeight)
                    {
                        pixel = texWidth * (ptsy + 1) + ptsx; // up
                        var c = colors[pixel];
                        if (lockMaskPixels[pixel] == 0
                            && CompareThreshold(c.r, hitColorR, threshold)
                            && CompareThreshold(c.g, hitColorG, threshold)
                            && CompareThreshold(c.b, hitColorB, threshold)
                            && CompareThreshold(c.a, hitColorA, threshold))
                        {
                            fillPointX.Enqueue(ptsx);
                            fillPointY.Enqueue(ptsy + 1);
                            colors[pixel] = paintColor;
                            lockMaskPixels[pixel] = 1;

                            if (target != null)
                            {
                                target[pixel] = paintColor;
                            }
                        }
                    }
                }
            }
        } // floodfill

        public static byte[] RenderTextureToBytes(RenderTexture texture)
        {
            var tex = ToTexture2D(texture);
            var texWidth = texture.width;
            var texHeight = texture.height;
            var pixels = new byte[texWidth * texHeight * 4];
            
            int pixel = 0;
            for (int y = 0; y < texHeight; y++)
            {
                for (int x = 0; x < texWidth; x++)
                {
                    Color c = tex.GetPixel(x, y);

                    pixels[pixel] = (byte)(c.r * 255);
                    pixels[pixel + 1] = (byte)(c.g * 255);
                    pixels[pixel + 2] = (byte)(c.b * 255);
                    pixels[pixel + 3] = (byte)(c.a * 255);
                    pixel += 4;
                }
            }
            GameObject.DestroyImmediate(tex);

            return pixels;
        }

        public static Texture2D BytesToTexture(byte[] pixels, int texWidth, int texHeight)
        {
            var texture = new Texture2D(texWidth, texHeight, TextureFormat.ARGB32, false);
            int pixel = 0;
            for (int y = 0; y < texHeight; y++)
            {
                for (int x = 0; x < texWidth; x++)
                {
                    texture.SetPixel(x, y, new Color32(pixels[pixel], pixels[pixel + 1], pixels[pixel + 2], pixels[pixel + 3]));
                    pixel += 4;
                }
            }
            texture.Apply();

            return texture;
        }

        public static void DrawPoint(byte[] pixels, Color32 paintColor, int pixel)
        {
            pixels[pixel] = paintColor.r;
            pixels[pixel + 1] = paintColor.g;
            pixels[pixel + 2] = paintColor.b;
            pixels[pixel + 3] = paintColor.a;
        }

        public static bool CompareThreshold(byte a, byte b, byte threshold = 0)
        {
            return Mathf.Abs(a-b)<=threshold;
        }

        public static void FloodFill(byte[] pixels,int texWidth, int texHeight, int x, int y, Color32 paintColor, bool canDrawOnBlack = false, byte[] target = null, byte threshold = 0)
        {
            // get canvas hit color
            byte hitColorR = pixels[((texWidth * (y) + x) * 4) + 0];
            byte hitColorG = pixels[((texWidth * (y) + x) * 4) + 1];
            byte hitColorB = pixels[((texWidth * (y) + x) * 4) + 2];
            byte hitColorA = pixels[((texWidth * (y) + x) * 4) + 3];

            if (!canDrawOnBlack)
            {
                if (hitColorR == 0 && hitColorG == 0 && hitColorB == 0 && hitColorA != 0) return;
            }

            // early exit if its same color already
            if (paintColor.r == hitColorR && paintColor.g == hitColorG && paintColor.b == hitColorB && paintColor.a == hitColorA) return;

            Queue<int> fillPointX = new Queue<int>();
            Queue<int> fillPointY = new Queue<int>();
            fillPointX.Enqueue(x);
            fillPointY.Enqueue(y);

            int ptsx, ptsy;
            int pixel = 0;

            if(threshold == 0)
            {
                while (fillPointX.Count > 0)
                {
                    ptsx = fillPointX.Dequeue();
                    ptsy = fillPointY.Dequeue();

                    if (ptsy - 1 > -1)
                    {
                        pixel = (texWidth * (ptsy - 1) + ptsx) * 4; // down
                        if (pixels[pixel + 0] == hitColorR
                            && pixels[pixel + 1] == hitColorG
                            && pixels[pixel + 2] == hitColorB
                            && pixels[pixel + 3] == hitColorA)
                        {
                            fillPointX.Enqueue(ptsx);
                            fillPointY.Enqueue(ptsy - 1);
                            DrawPoint(pixels, paintColor, pixel);

                            if (target != null)
                            {
                                DrawPoint(target, paintColor, pixel);
                            }
                        }
                    }

                    if (ptsx + 1 < texWidth)
                    {
                        pixel = (texWidth * ptsy + ptsx + 1) * 4; // right
                        if (pixels[pixel + 0] == hitColorR
                            && pixels[pixel + 1] == hitColorG
                            && pixels[pixel + 2] == hitColorB
                            && pixels[pixel + 3] == hitColorA)
                        {
                            fillPointX.Enqueue(ptsx + 1);
                            fillPointY.Enqueue(ptsy);
                            DrawPoint(pixels, paintColor, pixel);

                            if (target != null)
                            {
                                DrawPoint(target, paintColor, pixel);
                            }
                        }
                    }

                    if (ptsx - 1 > -1)
                    {
                        pixel = (texWidth * ptsy + ptsx - 1) * 4; // left
                        if (pixels[pixel + 0] == hitColorR
                            && pixels[pixel + 1] == hitColorG
                            && pixels[pixel + 2] == hitColorB
                            && pixels[pixel + 3] == hitColorA)
                        {
                            fillPointX.Enqueue(ptsx - 1);
                            fillPointY.Enqueue(ptsy);
                            DrawPoint(pixels, paintColor, pixel);

                            if (target != null)
                            {
                                DrawPoint(target, paintColor, pixel);
                            }
                        }
                    }

                    if (ptsy + 1 < texHeight)
                    {
                        pixel = (texWidth * (ptsy + 1) + ptsx) * 4; // up
                        if (pixels[pixel + 0] == hitColorR
                            && pixels[pixel + 1] == hitColorG
                            && pixels[pixel + 2] == hitColorB
                            && pixels[pixel + 3] == hitColorA)
                        {
                            fillPointX.Enqueue(ptsx);
                            fillPointY.Enqueue(ptsy + 1);
                            DrawPoint(pixels, paintColor, pixel);

                            if (target != null)
                            {
                                DrawPoint(target, paintColor, pixel);
                            }
                        }
                    }
                }
            }
            else
            {
                var lockMaskPixels = new byte[texWidth * texHeight * 4];
                while (fillPointX.Count > 0)
                {
                    ptsx = fillPointX.Dequeue();
                    ptsy = fillPointY.Dequeue();

                    if (ptsy - 1 > -1)
                    {
                        pixel = (texWidth * (ptsy - 1) + ptsx) * 4; // down
                        if (lockMaskPixels[pixel] == 0
                            && CompareThreshold(pixels[pixel + 0], hitColorR, threshold)
                            && CompareThreshold(pixels[pixel + 1], hitColorG, threshold)
                            && CompareThreshold(pixels[pixel + 2], hitColorB, threshold)
                            && CompareThreshold(pixels[pixel + 3], hitColorA, threshold))
                        {
                            fillPointX.Enqueue(ptsx);
                            fillPointY.Enqueue(ptsy - 1);
                            DrawPoint(pixels, paintColor, pixel);
                            lockMaskPixels[pixel] = 1;

                            if (target != null)
                            {
                                DrawPoint(target, paintColor, pixel);
                            }
                        }
                    }

                    if (ptsx + 1 < texWidth)
                    {
                        pixel = (texWidth * ptsy + ptsx + 1) * 4; // right
                        if (lockMaskPixels[pixel] == 0
                            && CompareThreshold(pixels[pixel + 0], hitColorR, threshold)
                            && CompareThreshold(pixels[pixel + 1], hitColorG, threshold)
                            && CompareThreshold(pixels[pixel + 2], hitColorB, threshold)
                            && CompareThreshold(pixels[pixel + 3], hitColorA, threshold))
                        {
                            fillPointX.Enqueue(ptsx + 1);
                            fillPointY.Enqueue(ptsy);
                            DrawPoint(pixels, paintColor, pixel);
                            lockMaskPixels[pixel] = 1;

                            if (target != null)
                            {
                                DrawPoint(target, paintColor, pixel);
                            }
                        }
                    }

                    if (ptsx - 1 > -1)
                    {
                        pixel = (texWidth * ptsy + ptsx - 1) * 4; // left
                        if (lockMaskPixels[pixel] == 0
                            && CompareThreshold(pixels[pixel + 0], hitColorR, threshold)
                            && CompareThreshold(pixels[pixel + 1], hitColorG, threshold)
                            && CompareThreshold(pixels[pixel + 2], hitColorB, threshold)
                            && CompareThreshold(pixels[pixel + 3], hitColorA, threshold))
                        {
                            fillPointX.Enqueue(ptsx - 1);
                            fillPointY.Enqueue(ptsy);
                            DrawPoint(pixels, paintColor, pixel);
                            lockMaskPixels[pixel] = 1;

                            if (target != null)
                            {
                                DrawPoint(target, paintColor, pixel);
                            }
                        }
                    }

                    if (ptsy + 1 < texHeight)
                    {
                        pixel = (texWidth * (ptsy + 1) + ptsx) * 4; // up
                        if (lockMaskPixels[pixel] == 0
                            && CompareThreshold(pixels[pixel + 0], hitColorR, threshold)
                            && CompareThreshold(pixels[pixel + 1], hitColorG, threshold)
                            && CompareThreshold(pixels[pixel + 2], hitColorB, threshold)
                            && CompareThreshold(pixels[pixel + 3], hitColorA, threshold))
                        {
                            fillPointX.Enqueue(ptsx);
                            fillPointY.Enqueue(ptsy + 1);
                            DrawPoint(pixels, paintColor, pixel);
                            lockMaskPixels[pixel] = 1;

                            if (target != null)
                            {
                                DrawPoint(target, paintColor, pixel);
                            }
                        }
                    }
                }
            }
        } // floodfill

        public static RenderTexture RenderTextureFloodFill(RenderTexture rt, int x, int y, Color32 paintColor, bool canDrawOnBlack = false, byte threshold = 0)
        {
            var st = Time.realtimeSinceStartup;

            var tex = ToTexture2D(rt);
            var colors = tex.GetPixels32();
            FloodFill(colors, rt.width, rt.height, x, y, paintColor, canDrawOnBlack, null, threshold);
            tex.SetPixels32(colors);
            tex.Apply();

            var temp = GetTempRenderTexture(null, rt.width, rt.height, filterMode: rt.filterMode);
            Graphics.Blit(tex, temp);
            GameObject.DestroyImmediate(tex);

            Debug.Log(string.Format("width:{0}, height:{1} new fill time:{2}", rt.width, rt.height, (Time.realtimeSinceStartup - st)));

            return temp;
        }

        public static RenderTexture RenderTextureFloodFill1(RenderTexture rt, int x, int y, Color32 paintColor, bool canDrawOnBlack = false, byte threshold = 0)
        {
            var st = Time.realtimeSinceStartup;

            var pixels = RenderTextureToBytes(rt);
            var target = new byte[pixels.Length];
            FloodFill(pixels, rt.width, rt.height, x, y, paintColor, canDrawOnBlack, target, threshold);
            var result = BytesToTexture(target, rt.width, rt.height);

            var temp = GetTempRenderTexture(null, rt.width, rt.height, filterMode:rt.filterMode);
            Graphics.Blit(result, temp);
            GameObject.DestroyImmediate(result);

            Debug.Log(string.Format("width:{0}, height:{1} old fill time:{2}", rt.width, rt.height, (Time.realtimeSinceStartup - st)));

            return temp;
        }

        public static Color GetColor(RenderTexture rt, int x, int y)
        {
            var texture = ToTexture2D(rt);
            var color = texture.GetPixel(x, y);
            GameObject.DestroyImmediate(texture);
            return color;
        }
    }
}
