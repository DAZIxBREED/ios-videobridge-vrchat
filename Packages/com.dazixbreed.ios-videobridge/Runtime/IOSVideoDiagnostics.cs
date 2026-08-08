using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace DAZIxBREED.IOSVideoBridge
{
    [DisallowMultipleComponent]
    public sealed class IOSVideoDiagnostics : MonoBehaviour
    {
        [SerializeField] private bool mirrorWarningsAndErrorsToUnityConsole = true;
        [SerializeField] private int flushEveryEvents = 1;

        private readonly object writeLock = new object();
        private StreamWriter writer;
        private long sequence;
        private int eventsSinceFlush;
        private string sessionId;
        private string currentLogPath;

        public string SessionId { get { return sessionId; } }
        public string CurrentLogPath { get { return currentLogPath; } }

        private void Awake()
        {
            EnsureOpen();
            Log("diagnostic", "session_started", "info", "Diagnostic session started.");
        }

        public void Log(
            string category,
            string eventName,
            string severity,
            string message,
            string url = null,
            double mediaTimeSeconds = -1.0,
            double durationSeconds = -1.0,
            long frame = -1,
            float frameRate = 0f,
            uint width = 0,
            uint height = 0,
            string detailsJson = "")
        {
            EnsureOpen();

            var item = new VideoDiagnosticEvent
            {
                timestampUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                sessionId = sessionId,
                sequence = ++sequence,
                category = category ?? string.Empty,
                eventName = eventName ?? string.Empty,
                severity = severity ?? "info",
                message = message ?? string.Empty,
                sanitizedUrl = SensitiveUrlRedactor.Redact(url),
                mediaTimeSeconds = mediaTimeSeconds,
                durationSeconds = durationSeconds,
                frame = frame,
                frameRate = frameRate,
                width = width,
                height = height,
                detailsJson = detailsJson ?? string.Empty,
                unityVersion = Application.unityVersion,
                platform = Application.platform.ToString(),
                operatingSystem = SystemInfo.operatingSystem,
                deviceModel = SystemInfo.deviceModel,
                applicationVersion = Application.version,
                bridgeVersion = IOSVideoBridgeVersion.Value
            };

            string json = JsonUtility.ToJson(item, false);
            lock (writeLock)
            {
                writer.WriteLine(json);
                eventsSinceFlush++;
                if (eventsSinceFlush >= Mathf.Max(1, flushEveryEvents))
                {
                    writer.Flush();
                    eventsSinceFlush = 0;
                }
            }

            if (mirrorWarningsAndErrorsToUnityConsole)
            {
                if (string.Equals(item.severity, "error", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogError("[iOS VideoBridge] " + item.eventName + ": " + item.message, this);
                }
                else if (string.Equals(item.severity, "warning", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogWarning("[iOS VideoBridge] " + item.eventName + ": " + item.message, this);
                }
            }
        }

        public string ExportCopy()
        {
            EnsureOpen();
            Flush();

            string exportDirectory = Path.Combine(Application.persistentDataPath, "IOSVideoBridge", "Exports");
            Directory.CreateDirectory(exportDirectory);
            string exportName = "ios-videobridge-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) + "-" + sessionId + ".jsonl";
            string exportPath = Path.Combine(exportDirectory, exportName);
            File.Copy(currentLogPath, exportPath, true);

            Log("diagnostic", "diagnostics_exported", "info", "Diagnostics copied for sharing.", detailsJson: "{\"path\":\"" + EscapeJson(exportPath) + "\"}");
            return exportPath;
        }

        public void Flush()
        {
            lock (writeLock)
            {
                if (writer != null)
                {
                    writer.Flush();
                    eventsSinceFlush = 0;
                }
            }
        }

        private void EnsureOpen()
        {
            if (writer != null)
            {
                return;
            }

            sessionId = Guid.NewGuid().ToString("N");
            string directory = Path.Combine(Application.persistentDataPath, "IOSVideoBridge", "Diagnostics");
            Directory.CreateDirectory(directory);
            string fileName = "session-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) + "-" + sessionId + ".jsonl";
            currentLogPath = Path.Combine(directory, fileName);
            writer = new StreamWriter(new FileStream(currentLogPath, FileMode.Append, FileAccess.Write, FileShare.Read));
        }

        private static string EscapeJson(string value)
        {
            return (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            Log("lifecycle", pauseStatus ? "application_paused" : "application_resumed", "info", pauseStatus ? "Application paused." : "Application resumed.");
            Flush();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            Log("lifecycle", hasFocus ? "application_focus_gained" : "application_focus_lost", "info", hasFocus ? "Application focus gained." : "Application focus lost.");
        }

        private void OnDestroy()
        {
            lock (writeLock)
            {
                if (writer != null)
                {
                    writer.Flush();
                    writer.Dispose();
                    writer = null;
                }
            }
        }
    }
}
