using System;
using System.Collections;
using System.Globalization;
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
        private bool preparationActive;
        private Coroutine prepareTimeoutCoroutine;
        private bool firstFrameLogged;
        private ushort preparedAudioTrackCount;
        private bool audioRouteConfigured;
        private float requestedPlaybackRate = 1f;

        private bool applicationPaused;
        private bool resumePlaybackAfterForeground;
        private bool resumePreparationAfterForeground;
        private double foregroundResumeTime;
        private bool foregroundWasLikelyLive;

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
        public bool HasAudio { get { return preparedAudioTrackCount > 0; } }
        public ushort AudioTrackCount { get { return preparedAudioTrackCount; } }
        public ushort ControlledAudioTrackCount { get { return videoPlayer != null ? videoPlayer.controlledAudioTrackCount : (ushort)0; } }
        public bool AudioRouteConfigured { get { return audioRouteConfigured; } }
        public bool IsLikelyLive { get { return VideoSourceNormalizer.IsLikelyHls(currentUrl) && (Duration <= 0.0 || double.IsInfinity(Duration)); } }

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
            get { return requestedPlaybackRate; }
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
            videoPlayer.playbackSpeed = 1f;

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
            if (videoPlayer == null)
            {
                return;
            }

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
            string normalized;
            string error;
            if (!VideoSourceNormalizer.TryNormalize(url, out normalized, out error))
            {
                CancelPreparation();
                if (videoPlayer != null)
                {
                    videoPlayer.Stop();
                }
                currentUrl = string.Empty;
                ResetPlaybackMetadata();
                Fail(error);
                return;
            }

            CancelPreparation();
            videoPlayer.Stop();
            currentUrl = normalized;
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = currentUrl;
            playAfterPrepare = false;
            pendingResumeTime = -1.0;
            pendingLiveReload = false;
            ResetPlaybackMetadata();
            SetState(VideoPlaybackState.Loading);
            Log("load_requested", "info", "Media source loaded.");
        }

        public void Prepare()
        {
            if (string.IsNullOrWhiteSpace(currentUrl))
            {
                Fail("Load a media URL or rooted local path before preparing playback.");
                return;
            }

            if (applicationPaused)
            {
                resumePreparationAfterForeground = true;
                Log("prepare_deferred", "info", "Preparation deferred while the application is paused.");
                return;
            }

            if (preparationActive)
            {
                Log("prepare_ignored", "info", "Preparation is already active.");
                return;
            }

            if (IsPrepared)
            {
                Log("prepare_ignored", "info", "Media is already prepared; the current playback state was preserved.");
                return;
            }

            StartPreparation(false);
        }

        private void StartPreparation(bool recovering)
        {
            CancelPreparation();
            ConfigureAudioRoutingBeforePrepare();
            prepareGeneration++;
            int generation = prepareGeneration;
            preparationActive = true;
            SetState(recovering ? VideoPlaybackState.Recovering : VideoPlaybackState.Preparing);
            Log(recovering ? "recovery_prepare_started" : "prepare_started", "info", recovering ? "Recovery preparation started." : "Video preparation started.");
            prepareTimeoutCoroutine = StartCoroutine(PrepareTimeout(generation));
            videoPlayer.Prepare();
        }

        private IEnumerator PrepareTimeout(int generation)
        {
            float deadline = Time.realtimeSinceStartup + Mathf.Max(2f, prepareTimeoutSeconds);
            while (preparationActive && generation == prepareGeneration && !videoPlayer.isPrepared && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            if (!preparationActive || generation != prepareGeneration || videoPlayer.isPrepared)
            {
                yield break;
            }

            preparationActive = false;
            prepareTimeoutCoroutine = null;
            prepareGeneration++;
            videoPlayer.Stop();
            ResetPlaybackMetadata();
            Fail("Video preparation timed out after " + prepareTimeoutSeconds.ToString("0.0", CultureInfo.InvariantCulture) + " seconds.");
        }

        private void ConfigureAudioRoutingBeforePrepare()
        {
            preparedAudioTrackCount = 0;
            audioRouteConfigured = false;

            try
            {
                videoPlayer.controlledAudioTrackCount = VideoPlayer.controlledAudioTrackMaxCount > 0 ? (ushort)1 : (ushort)0;
                if (videoPlayer.controlledAudioTrackCount > 0)
                {
                    videoPlayer.EnableAudioTrack(0, true);
                    videoPlayer.SetTargetAudioSource(0, audioSource);
                }
            }
            catch (Exception exception)
            {
                Log("audio_route_setup", "warning", "Audio routing could not be configured before preparation: " + exception.Message);
            }
        }

        public void Play()
        {
            if (string.IsNullOrWhiteSpace(currentUrl))
            {
                Fail("Load a media source before starting playback.");
                return;
            }

            if (applicationPaused)
            {
                playAfterPrepare = true;
                resumePlaybackAfterForeground = true;
                Log("play_deferred", "info", "Playback deferred while the application is paused.");
                return;
            }

            if (preparationActive)
            {
                playAfterPrepare = true;
                Log("play_queued", "info", "Playback will begin after preparation completes.");
                return;
            }

            if (!IsPrepared)
            {
                playAfterPrepare = true;
                StartPreparation(false);
                return;
            }

            ApplyRequestedPlaybackRate();
            videoPlayer.Play();
            SetState(VideoPlaybackState.Playing);
        }

        public void Pause()
        {
            if (videoPlayer == null || (!IsPlaying && state != VideoPlaybackState.Buffering && state != VideoPlaybackState.Recovering))
            {
                Log("pause_rejected", "warning", "Pause was requested while playback was not active.");
                return;
            }

            playAfterPrepare = false;
            videoPlayer.Pause();
            SetState(VideoPlaybackState.Paused);
            Log("playback_paused", "info", "Playback paused by the caller.");
        }

        public void Stop()
        {
            if (videoPlayer == null)
            {
                return;
            }

            CancelPreparation();
            playAfterPrepare = false;
            pendingResumeTime = -1.0;
            pendingLiveReload = false;
            resumePlaybackAfterForeground = false;
            resumePreparationAfterForeground = false;
            videoPlayer.Stop();
            ResetPlaybackMetadata();
            SetState(VideoPlaybackState.Stopped);
            Log("playback_stopped", "info", "Playback stopped and prepared resources were released.");
        }

        public void Seek(double seconds)
        {
            if (!IsPrepared || !videoPlayer.canSetTime)
            {
                Log("seek_rejected", "warning", "The current media is not ready or seekable.");
                return;
            }

            double target;
            if (!VideoSeekUtility.TryClampTarget(seconds, Duration, out target))
            {
                Log("seek_rejected", "warning", "The requested seek target was NaN or infinite.");
                return;
            }

            videoPlayer.time = target;
            Log(
                "seek_requested",
                "info",
                "Seek requested.",
                "{\"requestedSeconds\":" + seconds.ToString("0.###", CultureInfo.InvariantCulture) +
                ",\"targetSeconds\":" + target.ToString("0.###", CultureInfo.InvariantCulture) + "}");
        }

        public void SetVolume(float volume)
        {
            if (audioSource != null)
            {
                audioSource.volume = Mathf.Clamp01(volume);
            }
        }

        public void SetPlaybackRate(float rate)
        {
            float target = Mathf.Clamp(rate, 0.25f, 4f);
            if (Mathf.Abs(requestedPlaybackRate - target) < 0.0001f)
            {
                return;
            }

            requestedPlaybackRate = target;
            if (IsPrepared)
            {
                ApplyRequestedPlaybackRate();
            }
        }

        private void ApplyRequestedPlaybackRate()
        {
            if (videoPlayer == null || !IsPrepared)
            {
                return;
            }

            if (!videoPlayer.canSetPlaybackSpeed)
            {
                if (Mathf.Abs(requestedPlaybackRate - 1f) > 0.0001f)
                {
                    Log("playback_rate_rejected", "warning", "The current platform or media does not allow playback-speed changes; requested rate remains recorded for diagnostics.");
                }
                return;
            }

            videoPlayer.playbackSpeed = requestedPlaybackRate;
        }

        public void ReloadAndResume(double resumeTime, bool live)
        {
            if (string.IsNullOrWhiteSpace(currentUrl))
            {
                Log("reload_rejected", "warning", "Reload was requested without a loaded media source.");
                return;
            }

            if (applicationPaused)
            {
                resumePlaybackAfterForeground = true;
                foregroundResumeTime = Math.Max(0.0, resumeTime);
                foregroundWasLikelyLive = live;
                Log("reload_deferred", "info", "Reload deferred until the application resumes.");
                return;
            }

            CancelPreparation();
            videoPlayer.Stop();
            ResetPlaybackMetadata();
            pendingResumeTime = live ? -1.0 : Math.Max(0.0, resumeTime);
            pendingLiveReload = live;
            playAfterPrepare = true;
            SetState(VideoPlaybackState.Recovering);
            Log("reload_requested", "warning", live ? "Reloading likely-live media." : "Reloading media and restoring position.");
            StartPreparation(true);
        }

        public void MarkStalled(string message)
        {
            SetState(VideoPlaybackState.Buffering);
            Log("stall_suspected", "warning", message);
        }

        public void MarkRecovered(string message)
        {
            SetState(IsPlaying ? VideoPlaybackState.Playing : VideoPlaybackState.Ready);
            Log("playback_recovered", "info", message);
        }

        public void MarkRecoveryFailed(string message)
        {
            Fail(message);
        }

        public void Release()
        {
            CancelPreparation();
            StopAllCoroutines();
            if (videoPlayer != null)
            {
                videoPlayer.Stop();
            }

            currentUrl = string.Empty;
            playAfterPrepare = false;
            pendingResumeTime = -1.0;
            pendingLiveReload = false;
            resumePlaybackAfterForeground = false;
            resumePreparationAfterForeground = false;
            requestedPlaybackRate = 1f;
            ResetPlaybackMetadata();
            SetState(VideoPlaybackState.Idle);
            Log("released", "info", "Player resources released and state reset to Idle.");
        }

        private void OnPrepareCompleted(VideoPlayer source)
        {
            if (!preparationActive)
            {
                Log("prepare_callback_ignored", "warning", "Ignored a stale prepare-completed callback after preparation had already been cancelled or failed.");
                return;
            }

            preparationActive = false;
            prepareGeneration++;
            StopPrepareTimeoutCoroutine();

            preparedAudioTrackCount = source.audioTrackCount;
            audioRouteConfigured = false;
            try
            {
                if (preparedAudioTrackCount > 0 && source.controlledAudioTrackCount > 0)
                {
                    source.EnableAudioTrack(0, true);
                    source.SetTargetAudioSource(0, audioSource);
                    audioRouteConfigured = true;
                }
            }
            catch (Exception exception)
            {
                Log("audio_route_ready", "warning", "Prepared media could not be routed to the Unity AudioSource: " + exception.Message);
            }

            Log(
                "audio_tracks_discovered",
                preparedAudioTrackCount > 0 ? "info" : "warning",
                preparedAudioTrackCount > 0 ? "Prepared media reported audio tracks." : "Prepared media reported no audio tracks.",
                "{\"audioTrackCount\":" + preparedAudioTrackCount +
                ",\"controlledAudioTrackCount\":" + source.controlledAudioTrackCount +
                ",\"routeConfigured\":" + (audioRouteConfigured ? "true" : "false") + "}");

            SetState(VideoPlaybackState.Ready);
            ApplyRequestedPlaybackRate();
            Log("prepared", "info", "Media prepared and source metadata is available.");
            if (Prepared != null)
            {
                Prepared();
            }

            if (!pendingLiveReload && pendingResumeTime >= 0.0 && source.canSetTime)
            {
                double target;
                if (VideoSeekUtility.TryClampTarget(pendingResumeTime, source.length, out target))
                {
                    source.time = target;
                }
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
            if (source.isLooping)
            {
                SetState(VideoPlaybackState.Playing);
                Log("loop_iteration", "info", "Looping media reached its loop point and continued playback.");
                return;
            }

            SetState(VideoPlaybackState.Ready);
            Log("playback_completed", "info", "Playback reached the end of the media and remains prepared.");
        }

        private void OnErrorReceived(VideoPlayer source, string message)
        {
            CancelPreparation();
            Fail(message);
        }

        private void OnSeekCompleted(VideoPlayer source)
        {
            Log("seek_completed", "info", "Seek completed.");
        }

        private void OnFrameReady(VideoPlayer source, long frameIndex)
        {
            if (firstFrameLogged)
            {
                return;
            }

            firstFrameLogged = true;
            Log("first_frame", "info", "First video frame became available.");
        }

        private void OnFrameDropped(VideoPlayer source)
        {
            Log("frame_dropped", "warning", "Unity reported a dropped video frame.");
        }

        private void OnApplicationPause(bool paused)
        {
            if (applicationPaused == paused)
            {
                return;
            }

            applicationPaused = paused;
            if (paused)
            {
                foregroundResumeTime = CurrentTime;
                foregroundWasLikelyLive = IsLikelyLive;
                resumePlaybackAfterForeground = IsPlaying ||
                                                state == VideoPlaybackState.Playing ||
                                                state == VideoPlaybackState.Buffering ||
                                                state == VideoPlaybackState.Recovering;
                resumePreparationAfterForeground = preparationActive || state == VideoPlaybackState.Preparing;

                if (resumePreparationAfterForeground)
                {
                    CancelPreparation();
                    videoPlayer.Stop();
                    ResetPlaybackMetadata();
                    SetState(VideoPlaybackState.Loading);
                }
                else if (resumePlaybackAfterForeground)
                {
                    if (videoPlayer.isPlaying)
                    {
                        videoPlayer.Pause();
                    }
                    SetState(VideoPlaybackState.Paused);
                }

                Log(
                    "application_paused",
                    "info",
                    "Application pause recorded separately from user pause.",
                    "{\"resumePlayback\":" + (resumePlaybackAfterForeground ? "true" : "false") +
                    ",\"resumePreparation\":" + (resumePreparationAfterForeground ? "true" : "false") + "}");
                return;
            }

            bool shouldResumePlayback = resumePlaybackAfterForeground;
            bool shouldResumePreparation = resumePreparationAfterForeground;
            double resumeTime = foregroundResumeTime;
            bool live = foregroundWasLikelyLive;
            resumePlaybackAfterForeground = false;
            resumePreparationAfterForeground = false;

            Log("application_resumed", "info", "Application returned to the foreground.");

            if (shouldResumePlayback && !string.IsNullOrWhiteSpace(currentUrl))
            {
                ReloadAndResume(resumeTime, live);
            }
            else if (shouldResumePreparation && !string.IsNullOrWhiteSpace(currentUrl))
            {
                Prepare();
            }
        }

        private void CancelPreparation()
        {
            prepareGeneration++;
            preparationActive = false;
            StopPrepareTimeoutCoroutine();
        }

        private void StopPrepareTimeoutCoroutine()
        {
            if (prepareTimeoutCoroutine == null)
            {
                return;
            }

            StopCoroutine(prepareTimeoutCoroutine);
            prepareTimeoutCoroutine = null;
        }

        private void ResetPlaybackMetadata()
        {
            firstFrameLogged = false;
            preparedAudioTrackCount = 0;
            audioRouteConfigured = false;
        }

        private void Fail(string message)
        {
            playAfterPrepare = false;
            string safeMessage = string.IsNullOrWhiteSpace(message) ? "Unknown video playback error." : message;
            SetState(VideoPlaybackState.Failed);
            Log("playback_error", "error", safeMessage);
            if (ErrorReceived != null)
            {
                ErrorReceived(safeMessage);
            }
        }

        private bool SetState(VideoPlaybackState next)
        {
            if (state == next)
            {
                return true;
            }

            VideoPlaybackState previous = state;
            if (!VideoPlaybackStatePolicy.CanTransition(previous, next))
            {
                Log(
                    "state_transition_rejected",
                    "error",
                    "Rejected invalid playback-state transition.",
                    "{\"from\":\"" + previous + "\",\"to\":\"" + next + "\"}");
                return false;
            }

            state = next;
            Log(
                "state_transition",
                "info",
                "Playback state changed.",
                "{\"from\":\"" + previous + "\",\"to\":\"" + next + "\"}");
            if (StateChanged != null)
            {
                StateChanged(state);
            }
            return true;
        }

        private void Log(string eventName, string severity, string message, string detailsJson = "")
        {
            if (diagnostics == null)
            {
                return;
            }

            diagnostics.Log("playback", eventName, severity, message, currentUrl, CurrentTime, Duration, CurrentFrame, FrameRate, Width, Height, detailsJson);
        }

        private void OnDestroy()
        {
            CancelPreparation();
            Unsubscribe();
        }
    }
}
