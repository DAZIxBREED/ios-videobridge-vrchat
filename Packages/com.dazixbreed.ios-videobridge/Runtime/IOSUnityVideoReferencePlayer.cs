using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

namespace DAZIxBREED.IOSVideoBridge
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(VideoPlayer))]
    [RequireComponent(typeof(AudioSource))]
    public sealed class IOSUnityVideoReferencePlayer : MonoBehaviour, IIOSVideoReferencePlayer
    {
        [SerializeField, Min(2f)] private float prepareTimeoutSeconds = 30f;
        [SerializeField] private IOSVideoDiagnostics diagnostics;

        private VideoPlayer videoPlayer;
        private AudioSource audioSource;
        private VideoPlaybackState state = VideoPlaybackState.Idle;
        private string currentUrl = string.Empty;
        private bool playAfterPrepare;
        private double pendingResumeTime = -1.0;
        private bool pendingLiveReload;
        private int prepareGeneration;
        private bool firstFrameLogged;
        private bool wasPlayingBeforePause;
        private double pauseResumeTime;

        public event Action<VideoPlaybackState> StateChanged;
        public event Action<string> ErrorReceived;
        public event Action Prepared;

        public VideoPlaybackState State { get { return state; } }
        public string CurrentUrl { get { return currentUrl; } }
        public Texture OutputTexture { get { return videoPlayer != null ? videoPlayer.texture : null; } }
        public double CurrentTime { get { return videoPlayer != null && videoPlayer.isPrepared ? videoPlayer.time : 0.0; } }
        public double Duration { get { return videoPlayer != null && videoPlayer.isPrepared ? videoPlayer.length : 0.0; } }
        public long CurrentFrame { get { return videoPlayer != null ? videoPlayer.frame : -1; } }
        public float FrameRate { get { return videoPlayer != null ? videoPlayer.frameRate : 0f; } }
        public uint Width { get { return videoPlayer != null ? videoPlayer.width : 0; } }
        public uint Height { get { return videoPlayer != null ? videoPlayer.height : 0; } }
        public bool IsPrepared { get { return videoPlayer != null && videoPlayer.isPrepared; } }
        public bool IsPlaying { get { return videoPlayer != null && videoPlayer.isPlaying; } }
        public bool HasAudio { get { return videoPlayer != null && videoPlayer.isPrepared && videoPlayer.audioTrackCount > 0; } }
        public bool IsLikelyLive { get { return IsHls(currentUrl) && (Duration <= 0.0 || double.IsInfinity(Duration)); } }
        public bool Loop
        {
            get { return videoPlayer != null && videoPlayer.isLooping; }
            set { if (videoPlayer != null) videoPlayer.isLooping = value; }
        }
        public float Volume
        {
            get { return audioSource != null ? audioSource.volume : 0f; }
            set { SetVolume(value); }
        }
        public float PlaybackRate
        {
            get { return videoPlayer != null ? videoPlayer.playbackSpeed : 1f; }
            set { SetPlaybackRate(value); }
        }

        private void Awake()
        {
            videoPlayer = GetComponent<VideoPlayer>();
            audioSource = GetComponent<AudioSource>();
            if (diagnostics == null)
            {
                diagnostics = GetComponent<IOSVideoDiagnostics>();
            }
            ConfigurePlayer();
            Subscribe();
        }

        private void ConfigurePlayer()
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.waitForFirstFrame = true;
            videoPlayer.skipOnDrop = true;
            videoPlayer.renderMode = VideoRenderMode.APIOnly;
            videoPlayer.source = VideoSource.Url;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.sendFrameReadyEvents = true;
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
        }

        private void Subscribe()
        {
            videoPlayer.prepareCompleted += OnPrepareCompleted;
            videoPlayer.started += OnStarted;
            videoPlayer.loopPointReached += OnLoopPointReached;
            videoPlayer.errorReceived += OnErrorReceived;
            videoPlayer.seekCompleted += OnSeekCompleted;
            videoPlayer.frameReady += OnFrameReady;
            videoPlayer.frameDropped += OnFrameDropped;
        }

        private void Unsubscribe()
        {
            if (videoPlayer == null) return;
            videoPlayer.prepareCompleted -= OnPrepareCompleted;
            videoPlayer.started -= OnStarted;
            videoPlayer.loopPointReached -= OnLoopPointReached;
            videoPlayer.errorReceived -= OnErrorReceived;
            videoPlayer.seekCompleted -= OnSeekCompleted;
            videoPlayer.frameReady -= OnFrameReady;
            videoPlayer.frameDropped -= OnFrameDropped;
        }

        public void LoadUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                Fail("A media URL or local path is required.");
                return;
            }

            prepareGeneration++;
            videoPlayer.Stop();
            currentUrl = url.Trim();
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = currentUrl;
            firstFrameLogged = false;
            playAfterPrepare = false;
            pendingResumeTime = -1.0;
            pendingLiveReload = false;
            SetState(VideoPlaybackState.Loaded);
            Log("load_requested", "info", "Media URL loaded.");
        }

        public void Prepare()
        {
            if (string.IsNullOrWhiteSpace(currentUrl))
            {
                Fail("Load a media URL before preparing playback.");
                return;
            }
            StartPreparation();
        }

        private void StartPreparation()
        {
            prepareGeneration++;
            int generation = prepareGeneration;
            ConfigureAudioRouting();
            SetState(VideoPlaybackState.Preparing);
            Log("prepare_started", "info", "Video preparation started.");
            videoPlayer.Prepare();
            StartCoroutine(PrepareTimeout(generation));
        }

        private IEnumerator PrepareTimeout(int generation)
        {
            float deadline = Time.realtimeSinceStartup + Mathf.Max(2f, prepareTimeoutSeconds);
            while (generation == prepareGeneration && !videoPlayer.isPrepared && state == VideoPlaybackState.Preparing && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }
            if (generation == prepareGeneration && !videoPlayer.isPrepared && state == VideoPlaybackState.Preparing)
            {
                Fail("Video preparation timed out after " + prepareTimeoutSeconds.ToString("0.0") + " seconds.");
            }
        }

        private void ConfigureAudioRouting()
        {
            try
            {
                videoPlayer.controlledAudioTrackCount = 1;
                videoPlayer.EnableAudioTrack(0, true);
                videoPlayer.SetTargetAudioSource(0, audioSource);
            }
            catch (Exception exception)
            {
                Log("audio_route_setup", "warning", "Audio routing could not be configured before preparation: " + exception.Message);
            }
        }

        public void Play()
        {
            if (!IsPrepared)
            {
                playAfterPrepare = true;
                Prepare();
                return;
            }
            videoPlayer.Play();
            SetState(VideoPlaybackState.Playing);
        }

        public void Pause()
        {
            if (videoPlayer == null) return;
            videoPlayer.Pause();
            SetState(VideoPlaybackState.Paused);
            Log("playback_paused", "info", "Playback paused.");
        }

        public void Stop()
        {
            if (videoPlayer == null) return;
            prepareGeneration++;
            playAfterPrepare = false;
            pendingResumeTime = -1.0;
            videoPlayer.Stop();
            SetState(VideoPlaybackState.Stopped);
            Log("playback_stopped", "info", "Playback stopped.");
        }

        public void Seek(double seconds)
        {
            if (!IsPrepared || !videoPlayer.canSetTime)
            {
                Log("seek_rejected", "warning", "The current media is not ready or seekable.");
                return;
            }

            double target = Math.Max(0.0, seconds);
            if (Duration > 0.0 && !double.IsInfinity(Duration))
            {
                target = Math.Min(target, Math.Max(0.0, Duration - 0.05));
            }
            videoPlayer.time = target;
            Log("seek_requested", "info", "Seek requested.", "{\"targetSeconds\":" + target.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + "}");
        }

        public void SetVolume(float volume)
        {
            if (audioSource != null) audioSource.volume = Mathf.Clamp01(volume);
        }

        public void SetPlaybackRate(float rate)
        {
            if (videoPlayer != null && videoPlayer.canSetPlaybackSpeed)
            {
                videoPlayer.playbackSpeed = Mathf.Clamp(rate, 0.25f, 4f);
            }
        }

        public void ReloadAndResume(double resumeTime, bool live)
        {
            if (string.IsNullOrWhiteSpace(currentUrl)) return;
            prepareGeneration++;
            videoPlayer.Stop();
            pendingResumeTime = live ? -1.0 : Math.Max(0.0, resumeTime);
            pendingLiveReload = live;
            playAfterPrepare = true;
            SetState(VideoPlaybackState.Recovering);
            Log("reload_requested", "warning", live ? "Reloading likely-live media." : "Reloading media and restoring position.");
            StartPreparation();
        }

        public void MarkStalled(string message)
        {
            SetState(VideoPlaybackState.Stalled);
            Log("stall_suspected", "warning", message);
        }

        public void MarkRecovered(string message)
        {
            SetState(VideoPlaybackState.Playing);
            Log("playback_recovered", "info", message);
        }

        public void Release()
        {
            prepareGeneration++;
            StopAllCoroutines();
            if (videoPlayer != null) videoPlayer.Stop();
            currentUrl = string.Empty;
            SetState(VideoPlaybackState.Released);
            Log("released", "info", "Player resources released.");
        }

        private void OnPrepareCompleted(VideoPlayer source)
        {
            prepareGeneration++;
            try
            {
                if (source.audioTrackCount > 0)
                {
                    source.controlledAudioTrackCount = 1;
                    source.EnableAudioTrack(0, true);
                    source.SetTargetAudioSource(0, audioSource);
                    Log("audio_route_ready", "info", "At least one media audio track is routed to the Unity AudioSource.");
                }
            }
            catch (Exception exception)
            {
                Log("audio_route_ready", "warning", "Prepared media could not be routed to the Unity AudioSource: " + exception.Message);
            }

            SetState(VideoPlaybackState.Prepared);
            Log("prepared", "info", "Media prepared.");
            if (Prepared != null) Prepared();

            if (!pendingLiveReload && pendingResumeTime >= 0.0 && source.canSetTime)
            {
                source.time = pendingResumeTime;
            }
            pendingResumeTime = -1.0;
            pendingLiveReload = false;

            if (playAfterPrepare)
            {
                playAfterPrepare = false;
                source.Play();
            }
        }

        private void OnStarted(VideoPlayer source)
        {
            SetState(VideoPlaybackState.Playing);
            Log("playback_started", "info", "Playback started.");
        }

        private void OnLoopPointReached(VideoPlayer source)
        {
            if (source.isLooping) return;
            SetState(VideoPlaybackState.Completed);
            Log("playback_completed", "info", "Playback reached the end of the media.");
        }

        private void OnErrorReceived(VideoPlayer source, string message)
        {
            Fail(message);
        }

        private void OnSeekCompleted(VideoPlayer source)
        {
            Log("seek_completed", "info", "Seek completed.");
        }

        private void OnFrameReady(VideoPlayer source, long frameIndex)
        {
            if (firstFrameLogged) return;
            firstFrameLogged = true;
            Log("first_frame", "info", "First video frame became available.");
        }

        private void OnFrameDropped(VideoPlayer source)
        {
            Log("frame_dropped", "warning", "Unity reported a dropped video frame.");
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                wasPlayingBeforePause = IsPlaying;
                pauseResumeTime = CurrentTime;
                if (wasPlayingBeforePause) Pause();
            }
            else if (wasPlayingBeforePause && !string.IsNullOrWhiteSpace(currentUrl))
            {
                wasPlayingBeforePause = false;
                ReloadAndResume(pauseResumeTime, IsHls(currentUrl));
            }
        }

        private void Fail(string message)
        {
            SetState(VideoPlaybackState.Error);
            Log("playback_error", "error", string.IsNullOrWhiteSpace(message) ? "Unknown video playback error." : message);
            if (ErrorReceived != null) ErrorReceived(message ?? string.Empty);
        }

        private void SetState(VideoPlaybackState next)
        {
            if (state == next) return;
            state = next;
            if (StateChanged != null) StateChanged(state);
        }

        private void Log(string eventName, string severity, string message, string detailsJson = "")
        {
            if (diagnostics == null) return;
            diagnostics.Log("playback", eventName, severity, message, currentUrl, CurrentTime, Duration, CurrentFrame, FrameRate, Width, Height, detailsJson);
        }

        private static bool IsHls(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.IndexOf(".m3u8", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }
    }
}
