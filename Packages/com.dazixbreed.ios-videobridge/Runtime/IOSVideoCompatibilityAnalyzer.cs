using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace DAZIxBREED.IOSVideoBridge
{
    [DisallowMultipleComponent]
    public sealed class IOSVideoCompatibilityAnalyzer : MonoBehaviour
    {
        [SerializeField] private int requestTimeoutSeconds = 15;
        [SerializeField] private IOSVideoDiagnostics diagnostics;

        private void Awake()
        {
            if (diagnostics == null)
            {
                diagnostics = GetComponent<IOSVideoDiagnostics>();
            }
        }

        public static VideoCompatibilityReport AnalyzeLocal(string input)
        {
            var report = new VideoCompatibilityReport
            {
                inputUrl = input ?? string.Empty,
                sanitizedUrl = SensitiveUrlRedactor.Redact(input),
                inferredVideoCodec = "Unknown until probed or decoded",
                inferredAudioCodec = "Unknown until probed or decoded",
                expectedCompatibility = "Unknown"
            };

            string normalized;
            string normalizationError;
            if (!VideoSourceNormalizer.TryNormalize(input, out normalized, out normalizationError))
            {
                report.warnings.Add(normalizationError);
                return report;
            }

            report.inputUrl = normalized;
            report.sanitizedUrl = SensitiveUrlRedactor.Redact(normalized);

            Uri uri;
            bool absoluteUri = Uri.TryCreate(normalized, UriKind.Absolute, out uri);
            report.isLocalFile = IsLocalSource(normalized, absoluteUri ? uri : null);
            report.isValid = true;

            if (absoluteUri)
            {
                report.isHttps = string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
                if (!report.isLocalFile && !report.isHttps)
                {
                    report.warnings.Add("Network media is not using HTTPS. iOS App Transport Security may block it.");
                }
            }

            string path = absoluteUri ? uri.AbsolutePath : normalized;
            report.extension = Path.GetExtension(path).ToLowerInvariant();
            report.isHls = VideoSourceNormalizer.IsLikelyHls(normalized);

            switch (report.extension)
            {
                case ".mp4":
                case ".m4v":
                    report.inferredContainer = "MP4";
                    report.inferredVideoCodec = "Likely H.264 or HEVC; verify with ffprobe";
                    report.inferredAudioCodec = "Likely AAC; verify with ffprobe";
                    report.expectedCompatibility = report.isLocalFile || report.isHttps ? "High when encoded as H.264/AAC" : "Conditional";
                    break;
                case ".m3u8":
                    report.inferredContainer = "HLS playlist";
                    report.inferredVideoCodec = "Declared by selected HLS variant";
                    report.inferredAudioCodec = "Declared by selected HLS variant";
                    report.expectedCompatibility = report.isHttps ? "Platform-dependent; generally suitable for iOS" : "Conditional";
                    break;
                case ".mov":
                    report.inferredContainer = "QuickTime/MOV";
                    report.expectedCompatibility = "Codec-dependent";
                    break;
                case ".webm":
                    report.inferredContainer = "WebM";
                    report.expectedCompatibility = "Low for the Phase 1 iOS target";
                    report.warnings.Add("WebM is not an initial required format for iOS VideoBridge.");
                    break;
                default:
                    report.inferredContainer = string.IsNullOrEmpty(report.extension) ? "Unknown" : report.extension.TrimStart('.').ToUpperInvariant();
                    report.warnings.Add("The file extension does not identify an initial required format.");
                    break;
            }

            if (normalized.IndexOf("token=", StringComparison.OrdinalIgnoreCase) >= 0 ||
                normalized.IndexOf("signature=", StringComparison.OrdinalIgnoreCase) >= 0 ||
                normalized.IndexOf("x-amz-", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                report.warnings.Add("The URL appears signed or temporary. Re-test before expiry and never publish the original URL.");
            }

            return report;
        }

        public Coroutine Analyze(string input, Action<VideoCompatibilityReport> completed)
        {
            return StartCoroutine(AnalyzeRoutine(input, completed));
        }

        private IEnumerator AnalyzeRoutine(string input, Action<VideoCompatibilityReport> completed)
        {
            VideoCompatibilityReport report = AnalyzeLocal(input);
            if (!report.isValid || report.isLocalFile)
            {
                Finish(report, completed);
                yield break;
            }

            string normalized = report.inputUrl;
            using (UnityWebRequest head = UnityWebRequest.Head(normalized))
            {
                head.timeout = Mathf.Max(1, requestTimeoutSeconds);
                head.redirectLimit = 8;
                yield return head.SendWebRequest();
                ApplyNetworkResult(report, head);
            }

            if (report.isHls)
            {
                using (UnityWebRequest playlist = UnityWebRequest.Get(normalized))
                {
                    playlist.timeout = Mathf.Max(1, requestTimeoutSeconds);
                    playlist.redirectLimit = 8;
                    yield return playlist.SendWebRequest();
                    ApplyNetworkResult(report, playlist);

                    if (playlist.result == UnityWebRequest.Result.Success && playlist.downloadHandler != null)
                    {
                        string text = playlist.downloadHandler.text;
                        if (!string.IsNullOrEmpty(text) && text.IndexOf("#EXTM3U", StringComparison.Ordinal) >= 0)
                        {
                            report.isLikelyLive = text.IndexOf("#EXT-X-ENDLIST", StringComparison.Ordinal) < 0;
                            if (text.IndexOf("#EXT-X-KEY", StringComparison.Ordinal) >= 0)
                            {
                                report.warnings.Add("The HLS playlist declares encryption. DRM-protected playback is out of scope.");
                            }
                            if (text.IndexOf("#EXT-X-STREAM-INF", StringComparison.Ordinal) >= 0)
                            {
                                report.inferredContainer = "HLS master playlist";
                            }
                            else
                            {
                                report.inferredContainer = report.isLikelyLive ? "HLS live media playlist" : "HLS VOD media playlist";
                            }
                        }
                        else
                        {
                            report.warnings.Add("The .m3u8 response did not contain an #EXTM3U header.");
                        }
                    }
                }
            }

            Finish(report, completed);
        }

        private static bool IsLocalSource(string normalized, Uri uri)
        {
            if (uri != null && uri.IsFile)
            {
                return true;
            }

            if (uri != null && string.Equals(uri.Scheme, "jar", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (Path.IsPathRooted(normalized))
            {
                return true;
            }

            return normalized.Length >= 3 &&
                   char.IsLetter(normalized[0]) &&
                   normalized[1] == ':' &&
                   (normalized[2] == '\\' || normalized[2] == '/');
        }

        private static void ApplyNetworkResult(VideoCompatibilityReport report, UnityWebRequest request)
        {
            report.httpStatusCode = request.responseCode;
            report.networkResult = request.result.ToString();
            report.mimeType = request.GetResponseHeader("Content-Type") ?? report.mimeType;
            report.finalUrl = SensitiveUrlRedactor.Redact(request.url);

            if (request.result != UnityWebRequest.Result.Success)
            {
                string error = string.IsNullOrWhiteSpace(request.error) ? "Unknown network error" : request.error;
                report.warnings.Add("Network probe failed: " + error);
            }
        }

        private void Finish(VideoCompatibilityReport report, Action<VideoCompatibilityReport> completed)
        {
            if (diagnostics != null)
            {
                diagnostics.Log(
                    "network",
                    "url_analyzed",
                    report.isValid ? "info" : "warning",
                    report.expectedCompatibility,
                    report.inputUrl,
                    detailsJson: JsonUtility.ToJson(report, false));
            }

            if (completed != null)
            {
                completed(report);
            }
        }
    }
}
