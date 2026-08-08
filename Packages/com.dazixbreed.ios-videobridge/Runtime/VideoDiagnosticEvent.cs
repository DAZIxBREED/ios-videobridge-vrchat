using System;

namespace DAZIxBREED.IOSVideoBridge
{
    [Serializable]
    public sealed class VideoDiagnosticEvent
    {
        public string timestampUtc;
        public string sessionId;
        public long sequence;
        public string category;
        public string eventName;
        public string severity;
        public string message;
        public string sanitizedUrl;
        public double mediaTimeSeconds;
        public double durationSeconds;
        public long frame;
        public float frameRate;
        public uint width;
        public uint height;
        public string detailsJson;
        public string unityVersion;
        public string platform;
        public string operatingSystem;
        public string deviceModel;
        public string applicationVersion;
        public string bridgeVersion;
    }
}
