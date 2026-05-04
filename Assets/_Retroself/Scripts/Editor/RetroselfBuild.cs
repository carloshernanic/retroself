#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Retroself.EditorTools
{
    public static class RetroselfBuild
    {
        static readonly string[] Scenes = new[]
        {
            "Assets/_Retroself/Scenes/MainMenu.unity",
            "Assets/_Retroself/Scenes/Prologue.unity",
            "Assets/_Retroself/Scenes/Hub.unity",
            "Assets/_Retroself/Scenes/Phase1.unity",
            "Assets/_Retroself/Scenes/Phase2.unity",
            "Assets/_Retroself/Scenes/Epilogue.unity",
        };

        [MenuItem("Retroself/Setup Build Scenes")]
        public static void SetupBuildScenes()
        {
            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            foreach (var s in Scenes)
            {
                if (!File.Exists(s))
                {
                    Debug.LogWarning($"Scene missing: {s}");
                    continue;
                }
                list.Add(new EditorBuildSettingsScene(s, true));
            }
            EditorBuildSettings.scenes = list.ToArray();
            Debug.Log($"Retroself: {list.Count} scenes added to Build Settings.");
        }

        [MenuItem("Retroself/Build WebGL")]
        public static void BuildWebGL()
        {
            SetupBuildScenes();

            string outDir = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "WebGL");
            Directory.CreateDirectory(outDir);

            PlayerSettings.companyName = "Retroself Team";
            PlayerSettings.productName = "retroself";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled; // safer for itch.io
            PlayerSettings.WebGL.template = "PROJECT:Default";

            var opts = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = outDir,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(opts);
            var summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
                Debug.Log($"Retroself: WebGL build succeeded — {summary.totalSize / (1024 * 1024)} MB at {outDir}");
            else
                Debug.LogError($"Retroself: WebGL build failed — {summary.totalErrors} errors");
        }
    }
}
#endif
