using System;
using System.Collections.Generic;
using System.Text;

namespace DAZIxBREED.IOSVideoBridge
{
    [Serializable]
    public sealed class VideoCompatibilityReport
    {
        public string inputUrl;
        public string sanitizedUrl;
        public bool isValid;
        public bool isHttps;
        public bool isLocalFile;
        public bool isHls;
        public bool isLikelyLive;
        public string extension;
        public string inferredContainer;
        public string inferredVideoCodec;
        public string inferredAudioCodec;
        public string mimeType;
        public long httpStatusCode;
        public string networkResult;
        public string expectedCompatibility;
        public string finalUrl;
        public List<string> warnings = new List<string>();

        public string ToMultilineString()
        {
            var builder = new StringBuilder(512);
            builder.AppendLine("Valid: " + isValid);
            builder.AppendLine("Sanitized URL: " + sanitizedUrl);
            builder.AppendLine("Delivery: " + (isLocalFile ? "Local" : (isHttps ? "HTTPS" : "Other")));
            builder.AppendLine("Container: " + EmptyAsUnknown(inferredContainer));
            builder.AppendLine("Video: " + EmptyAsUnknown(inferredVideoCodec));
            builder.AppendLine("Audio: " + EmptyAsUnknown(inferredAudioCodec));
            builder.AppendLine("HLS: " + isHls + (isHls ? (isLikelyLive ? " (likely live)" : " (VOD or unknown)") : string.Empty));
            builder.AppendLine("MIME: " + EmptyAsUnknown(mimeType));
            if (httpStatusCode > 0)
            {
                builder.AppendLine("HTTP: " + httpStatusCode + " / " + EmptyAsUnknown(networkResult));
            }
            builder.AppendLine("Expected compatibility: " + EmptyAsUnknown(expectedCompatibility));
            if (warnings != null && warnings.Count > 0)
            {
                builder.AppendLine("Warnings:");
                for (int i = 0; i < warnings.Count; i++)
                {
                    builder.AppendLine("- " + warnings[i]);
                }
            }
            return builder.ToString().TrimEnd();
        }

        private static string EmptyAsUnknown(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "Unknown" : value;
        }
    }
}
