using System;
using UnityEngine;

namespace DAZIxBREED.IOSVideoBridge
{
    public interface IIOSVideoReferencePlayer
    {
        event Action<VideoPlaybackState> StateChanged;
        event Action<string> ErrorReceived;
        event Action Prepared;

        VideoPlaybackState State { get; }
        string CurrentUrl { get; }
        Texture OutputTexture { get; }
        double CurrentTime { get; }
        double Duration { get; }
        bool IsPrepared { get; }
        bool IsPlaying { get; }
        bool IsLikelyLive { get; }
        bool HasAudio { get; }
        ushort AudioTrackCount { get; }
        ushort ControlledAudioTrackCount { get; }
        bool AudioRouteConfigured { get; }

        void LoadUrl(string url);
        void Prepare();
        void Play();
        void Pause();
        void Stop();
        void Seek(double seconds);
        void SetVolume(float volume);
        void SetPlaybackRate(float rate);
        void Release();
    }
}
