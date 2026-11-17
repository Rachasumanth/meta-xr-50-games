using System;
using System.IO;
using UnityEngine;

namespace MetaXRSim.App
{
    public static class Screenshot
    {
        public static string DirectoryPath
        {
            get
            {
#if UNITY_EDITOR
                string root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Screenshots"));
#else
                string root = Path.Combine(Application.persistentDataPath, "Screenshots");
#endif
                Directory.CreateDirectory(root);
                return root;
            }
        }

        public static string Capture(string tag)
        {
            try
            {
                string name = string.Format("shapeforge_{0}_{1:yyyyMMdd_HHmmss}.png", tag, DateTime.Now);
                string path = Path.Combine(DirectoryPath, name);
                ScreenCapture.CaptureScreenshot(path);
                Debug.Log("[Screenshot] " + path);
                return path;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Screenshot] failed: " + e.Message);
                return null;
            }
        }
    }
}
