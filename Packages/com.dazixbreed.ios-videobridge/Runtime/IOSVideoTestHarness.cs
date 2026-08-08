using System;
using System.IO;
using UnityEngine;

namespace DAZIxBREED.IOSVideoBridge
{
    [DisallowMultipleComponent]
    public sealed class IOSVideoTestHarness : MonoBehaviour
    {
        private IOSUnityVideoReferencePlayer player;
        private IOSVideoCompatibilityAnalyzer analyzer;
        private IOSVideoDiagnostics diagnostics;
        private IOSVideoRecoveryController recovery;
        private string mediaUrl = string.Empty;
        private string analyzerText = "No compatibility analysis has been run yet.";
        private string lastMessage = "Ready.";
        private Vector2 reportScroll;
        private bool flipY;

        private void Awake()
        {
            diagnostics = GetOrAdd<IOSVideoDiagnostics>();
            player = GetOrAdd<IOSUnityVideoReferencePlayer>();
            analyzer = GetOrAdd<IOSVideoCompatibilityAnalyzer>();
            recovery = GetOrAdd<IOSVideoRecoveryController>();

            player.ErrorReceived += OnPlayerError;
            player.StateChanged += OnStateChanged;

            mediaUrl = GetBundledMp4Path();
            lastMessage = "Harness initialized. Generate the bundled test media with scripts/generate-test-media.sh if it is not present.";
        }

        private T GetOrAdd<T>() where T : Component
        {
            T component = GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }

        private static string GetBundledMp4Path()
        {
            return Path.Combine(Application.streamingAssetsPath, "IOSVideoBridge", "known-good-h264-aac.mp4");
        }

        private void OnGUI()
        {
            const float margin = 16f;
            float panelWidth = Mathf.Min(470f, Screen.width * 0.42f);
            Rect panel = new Rect(margin, margin, panelWidth, Screen.height - margin * 2f);
            GUILayout.BeginArea(panel, GUI.skin.box);

            GUILayout.Label("iOS VideoBridge for VRChat — Phase 1");
            GUILayout.Label("DAZIxBREED | " + IOSVideoBridgeVersion.Value);
            GUILayout.Space(6f);
            GUILayout.Label("Media URL or local path");
            mediaUrl = GUILayout.TextField(mediaUrl ?? string.Empty);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Bundled MP4"))
            {
                mediaUrl = GetBundledMp4Path();
                LoadCurrentUrl();
            }
            if (GUILayout.Button("Analyze"))
            {
                analyzerText = "Analyzing…";
                analyzer.Analyze(mediaUrl, report =>
                {
                    analyzerText = report != null ? report.ToMultilineString() : "No report returned.";
                });
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load")) LoadCurrentUrl();
            if (GUILayout.Button("Prepare")) { EnsureLoaded(); player.Prepare(); }
            if (GUILayout.Button("Play")) { EnsureLoaded(); player.Play(); }
            if (GUILayout.Button("Pause")) player.Pause();
            if (GUILayout.Button("Stop")) player.Stop();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-10s")) player.Seek(player.CurrentTime - 10.0);
            if (GUILayout.Button("+10s")) player.Seek(player.CurrentTime + 10.0);
            if (GUILayout.Button("Reload / Live")) player.ReloadAndResume(player.CurrentTime, player.IsLikelyLive || IsHls(mediaUrl));
            if (GUILayout.Button("Reset Recovery")) recovery.ResetRecoveryBudget();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Volume", GUILayout.Width(56f));
            player.Volume = GUILayout.HorizontalSlider(player.Volume, 0f, 1f);
            GUILayout.Label(player.Volume.ToString("0.00"), GUILayout.Width(38f));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Speed", GUILayout.Width(56f));
            player.PlaybackRate = GUILayout.HorizontalSlider(player.PlaybackRate, 0.5f, 2f);
            GUILayout.Label(player.PlaybackRate.ToString("0.00x"), GUILayout.Width(48f));
            GUILayout.EndHorizontal();

            player.Loop = GUILayout.Toggle(player.Loop, "Loop VOD");
            flipY = GUILayout.Toggle(flipY, "Flip preview vertically");

            GUILayout.Space(6f);
            GUILayout.Label("State: " + player.State);
            GUILayout.Label("Time: " + player.CurrentTime.ToString("0.00") + " / " + FormatDuration(player.Duration));
            GUILayout.Label("Frame: " + player.CurrentFrame + " | " + player.Width + "×" + player.Height + " @ " + player.FrameRate.ToString("0.##") + " fps");
            GUILayout.Label("Audio tracks detected: " + (player.HasAudio ? "yes" : "not yet / none"));
            GUILayout.Label("Recovery attempts: " + recovery.ConsecutiveAttempts + (recovery.RecoveryInProgress ? " (active)" : string.Empty));
            GUILayout.Label("Last: " + lastMessage);

            if (GUILayout.Button("Export Diagnostics"))
            {
                try
                {
                    lastMessage = "Diagnostics exported: " + diagnostics.ExportCopy();
                }
                catch (Exception exception)
                {
                    lastMessage = "Diagnostics export failed: " + exception.Message;
                }
            }

            GUILayout.Space(6f);
            GUILayout.Label("Compatibility report");
            reportScroll = GUILayout.BeginScrollView(reportScroll, GUILayout.ExpandHeight(true));
            GUILayout.TextArea(analyzerText ?? string.Empty, GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();
            GUILayout.EndArea();

            DrawVideo(new Rect(panel.xMax + margin, margin, Mathf.Max(32f, Screen.width - panel.xMax - margin * 2f), Screen.height - margin * 2f));
        }

        private void DrawVideo(Rect area)
        {
            GUI.Box(area, GUIContent.none);
            Texture texture = player != null ? player.OutputTexture : null;
            if (texture == null)
            {
                GUI.Label(new Rect(area.x + 12f, area.y + 12f, area.width - 24f, 40f), "No video frame available yet.");
                return;
            }

            if (!flipY)
            {
                GUI.DrawTexture(area, texture, ScaleMode.ScaleToFit, false);
                return;
            }

            Matrix4x4 previous = GUI.matrix;
            Vector2 pivot = new Vector2(area.center.x, area.center.y);
            GUIUtility.ScaleAroundPivot(new Vector2(1f, -1f), pivot);
            GUI.DrawTexture(area, texture, ScaleMode.ScaleToFit, false);
            GUI.matrix = previous;
        }

        private void LoadCurrentUrl()
        {
            player.LoadUrl(mediaUrl);
            lastMessage = "Loaded: " + SensitiveUrlRedactor.Redact(mediaUrl);
        }

        private void EnsureLoaded()
        {
            if (!string.Equals(player.CurrentUrl, mediaUrl, StringComparison.Ordinal))
            {
                LoadCurrentUrl();
            }
        }

        private void OnPlayerError(string message)
        {
            lastMessage = "Error: " + message;
        }

        private void OnStateChanged(VideoPlaybackState next)
        {
            lastMessage = "State changed to " + next + ".";
        }

        private static string FormatDuration(double seconds)
        {
            if (seconds <= 0.0 || double.IsInfinity(seconds) || double.IsNaN(seconds)) return "live/unknown";
            return seconds.ToString("0.00");
        }

        private static bool IsHls(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.IndexOf(".m3u8", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void OnDestroy()
        {
            if (player != null)
            {
                player.ErrorReceived -= OnPlayerError;
                player.StateChanged -= OnStateChanged;
            }
        }
    }
}
