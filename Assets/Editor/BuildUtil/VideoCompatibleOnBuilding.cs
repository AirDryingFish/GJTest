using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class VideoCompatibleOnBuilding : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    static readonly string TEMP_ROOT = "Temp/WebGLCompatibleTEMP";


    public void OnPreprocessBuild(BuildReport report)
    {
        bool isWebGL = report.summary.platform == BuildTarget.WebGL;

        if (!isWebGL)
        {
            return;
        }


        var compas = Resources.FindObjectsOfTypeAll<VideoPlayerWebCompatible>();
        foreach (var compa in compas)
        {
            if (!compa.useClip || compa.clip == null)
            {
                continue;
            }
            string vdPath = AssetDatabase.GetAssetPath(compa.clip);
            string dst = CopyVideoToTemp(vdPath, compa.webglCompatiblePath);
            Debug.Log($"Copy from {vdPath} to temp {dst}");
        }
    }

    public string CopyVideoToTemp(string path, string to)
    {
        string tmpRoot = Path.Combine(TEMP_ROOT, VideoPlayerWebCompatible.VIDEO_PATH);
        string dst = Path.Combine(tmpRoot, to.TrimStart('/','\\'));
        Directory.CreateDirectory(Path.GetDirectoryName(dst));
        // FileUtil.CopyFileOrDirectory(path, dst);
        FileUtil.ReplaceFile(path, dst);
        return dst;
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        string tmpRoot = Path.Combine(TEMP_ROOT, VideoPlayerWebCompatible.VIDEO_PATH);
        string saRoot = Path.Combine(report.summary.outputPath, "StreamingAssets", VideoPlayerWebCompatible.VIDEO_PATH);
        FileUtil.CopyFileOrDirectory(tmpRoot, saRoot);
    }
}
