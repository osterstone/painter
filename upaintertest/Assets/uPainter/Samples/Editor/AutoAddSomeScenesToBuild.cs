using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AutoAddSomeScenesToBuild
{
    public class SceneComparer : IEqualityComparer<EditorBuildSettingsScene>
    {
        public bool Equals(EditorBuildSettingsScene x, EditorBuildSettingsScene y)
        {
            if (x == null)
                return y == null;
            return x.guid == y.guid;
        }


        public int GetHashCode(EditorBuildSettingsScene obj)
        {
            if (obj == null)
                return 0;
            return obj.guid.GetHashCode();
        }
    }

    [MenuItem("uPainter/AddSamples")]
    static void AddScenes()
    {
        var scenesDir = new DirectoryInfo(Application.dataPath + "/uPainter/Samples");
        if (scenesDir.Exists)
        {
            var oldscnes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            oldscnes.AddRange(new EditorBuildSettingsScene[] {
                new EditorBuildSettingsScene("Assets/uPainter/Samples/Samples.unity", true),
                new EditorBuildSettingsScene("Assets/uPainter/Samples/Painter/Painter.unity", true),
                new EditorBuildSettingsScene("Assets/uPainter/Samples/RawImage/AddMosaic.unity", true),
                new EditorBuildSettingsScene("Assets/uPainter/Samples/RawImage/Benchmark.unity", true),
                new EditorBuildSettingsScene("Assets/uPainter/Samples/RawImage/PixelPaint.unity", true),
                new EditorBuildSettingsScene("Assets/uPainter/Samples/RawImage/RawImage.unity", true),
                new EditorBuildSettingsScene("Assets/uPainter/Samples/RawImage/RawImageWithRawTexture.unity", true),
                new EditorBuildSettingsScene("Assets/uPainter/Samples/RawImage/ScratchCard.unity", true),
                new EditorBuildSettingsScene("Assets/uPainter/Samples/RawImage/ScratchCardEx.unity", true),
                new EditorBuildSettingsScene("Assets/uPainter/Samples/PaintMesh/PaintMesh.unity", true),
                new EditorBuildSettingsScene("Assets/uPainter/Samples/PaintMesh/PaintMesh2.unity", true),
            });
            EditorBuildSettings.scenes = oldscnes.Distinct(new SceneComparer()).ToArray();
        }
    }

}