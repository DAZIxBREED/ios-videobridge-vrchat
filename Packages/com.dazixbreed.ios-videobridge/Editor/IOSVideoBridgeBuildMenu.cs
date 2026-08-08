using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DAZIxBREED.IOSVideoBridge.Editor
{
    public static class IOSVideoBridgeBuildMenu
    {
        public const string TestScenePath = "Assets/Scenes/IOSVideoBridgeTest.unity";
        private const string DefaultBuildPath = "Builds/iOS";

        [MenuItem("iOS VideoBridge/Validate Phase 0 and 1 Repository")]
        public static void ValidateRepository()
        {
            bool valid = true;
            valid &= RequireAsset(TestScenePath);
            valid &= RequireAsset("Assets/StreamingAssets/IOSVideoBridge/known-good-h264-aac.mp4");
            valid &= RequireAsset("Assets/StreamingAssets/IOSVideoBridge/hls-vod/index.m3u8");
            valid &= RequireAsset("Packages/com.dazixbreed.ios-videobridge/Runtime/IOSUnityVideoReferencePlayer.cs");
            valid &= RequireAsset("Packages/com.dazixbreed.ios-videobridge/Runtime/IOSVideoDiagnostics.cs");

            if (valid)
            {
                Debug.Log("[iOS VideoBridge] Phase 0/1 repository validation passed.");
            }
            else
            {
                throw new InvalidOperationException("iOS VideoBridge repository validation failed. See the Unity Console.");
            }
        }

        [MenuItem("iOS VideoBridge/Add Test Scene to Build Settings")]
        public static void AddTestSceneToBuildSettings()
        {
            var existing = EditorBuildSettings.scenes;
            for (int i = 0; i < existing.Length; i++)
            {
                if (string.Equals(existing[i].path, TestScenePath, StringComparison.Ordinal))
                {
                    if (!existing[i].enabled)
                    {
                        existing[i].enabled = true;
                        EditorBuildSettings.scenes = existing;
                    }
                    return;
                }
            }

            var updated = new EditorBuildSettingsScene[existing.Length + 1];
            Array.Copy(existing, updated, existing.Length);
            updated[updated.Length - 1] = new EditorBuildSettingsScene(TestScenePath, true);
            EditorBuildSettings.scenes = updated;
            Debug.Log("[iOS VideoBridge] Added test scene to Build Settings.");
        }

        [MenuItem("iOS VideoBridge/Build iOS Xcode Project")]
        public static void BuildIOSInteractive()
        {
            BuildIOS(DefaultBuildPath);
        }

        public static void BuildIOSFromCommandLine()
        {
            string output = GetCommandLineArgument("-iosVideoBridgeOutput");
            BuildIOS(string.IsNullOrWhiteSpace(output) ? DefaultBuildPath : output);
        }

        private static void BuildIOS(string outputPath)
        {
            ValidateRepository();
            AddTestSceneToBuildSettings();

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS))
            {
                throw new InvalidOperationException("Unable to switch the active Unity build target to iOS. Confirm iOS Build Support is installed.");
            }

            PlayerSettings.productName = "iOS VideoBridge for VRChat";
            PlayerSettings.companyName = "DAZIxBREED";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.dazixbreed.iosvideobridge");
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);

            string absoluteOutput = Path.GetFullPath(outputPath);
            Directory.CreateDirectory(absoluteOutput);
            var options = new BuildPlayerOptions
            {
                scenes = new[] { TestScenePath },
                locationPathName = absoluteOutput,
                target = BuildTarget.iOS,
                targetGroup = BuildTargetGroup.iOS,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException("iOS Xcode export failed: " + summary.result + ". Errors: " + summary.totalErrors);
            }

            Debug.Log("[iOS VideoBridge] iOS Xcode project exported to: " + absoluteOutput + " (" + summary.totalSize + " bytes)");
        }

        private static bool RequireAsset(string path)
        {
            if (File.Exists(path) || Directory.Exists(path))
            {
                return true;
            }

            Debug.LogError("[iOS VideoBridge] Missing required repository asset: " + path);
            return false;
        }

        private static string GetCommandLineArgument(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                {
                    return args[i + 1];
                }
            }
            return string.Empty;
        }
    }
}
