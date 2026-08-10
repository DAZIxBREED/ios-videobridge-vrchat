using System.Collections;
using UnityEngine;

namespace DAZIxBREED.IOSVideoBridge
{
    [DisallowMultipleComponent]
    public sealed class IOSVideoRecoveryController : MonoBehaviour
    {
        [SerializeField] private IOSUnityVideoReferencePlayer player;
        [SerializeField] private IOSVideoDiagnostics diagnostics;
        [SerializeField, Min(2f)] private float stallThresholdSeconds = 6f;
        [SerializeField, Min(0.25f)] private float sampleIntervalSeconds = 0.5f;
        [SerializeField, Range(1, 5)] private int maximumConsecutiveAttempts = 3;
        [SerializeField, Min(0.5f)] private float recoveryObservationSeconds = 3f;
        [SerializeField, Min(0f)] private float delayBetweenAttemptsSeconds = 0.5f;

        private double lastMediaTime;
        private long lastFrame;
        private float lastProgressRealtime;
        private float nextSampleRealtime;
        private int consecutiveAttempts;
        private bool recoveryInProgress;

        public int ConsecutiveAttempts { get { return consecutiveAttempts; } }
        public bool RecoveryInProgress { get { return recoveryInProgress; } }

        private void Awake()
        {
            if (player == null)
            {
                player = GetComponent<IOSUnityVideoReferencePlayer>();
            }
            if (diagnostics == null)
            {
                diagnostics = GetComponent<IOSVideoDiagnostics>();
            }
            ResetProgressClock();
        }

        private void Update()
        {
            if (player == null || recoveryInProgress || Time.realtimeSinceStartup < nextSampleRealtime)
            {
                return;
            }

            nextSampleRealtime = Time.realtimeSinceStartup + sampleIntervalSeconds;
            if (player.State != VideoPlaybackState.Playing || !player.IsPlaying)
            {
                ResetProgressClock();
                return;
            }

            double time = player.CurrentTime;
            long frame = player.CurrentFrame;
            bool advanced = time > lastMediaTime + 0.02 || frame > lastFrame;
            if (advanced)
            {
                lastMediaTime = time;
                lastFrame = frame;
                lastProgressRealtime = Time.realtimeSinceStartup;
                consecutiveAttempts = 0;
                return;
            }

            if (Time.realtimeSinceStartup - lastProgressRealtime >= stallThresholdSeconds)
            {
                StartCoroutine(RecoverRoutine());
            }
        }

        private IEnumerator RecoverRoutine()
        {
            recoveryInProgress = true;
            double resumeTime = player.CurrentTime;
            bool live = player.IsLikelyLive;
            player.MarkStalled("Playback reported Playing but media time and frame did not advance for " + stallThresholdSeconds.ToString("0.0") + " seconds.");

            for (int attempt = 1; attempt <= maximumConsecutiveAttempts; attempt++)
            {
                consecutiveAttempts = attempt;
                Log(
                    "recovery_attempt",
                    "warning",
                    "Recovery attempt " + attempt + " of " + maximumConsecutiveAttempts + ".",
                    "{\"attempt\":" + attempt + ",\"live\":" + (live ? "true" : "false") + "}");

                double observationStartTime = player.CurrentTime;
                long observationStartFrame = player.CurrentFrame;

                if (attempt == 1 && player.IsPrepared)
                {
                    player.Play();
                }
                else
                {
                    player.ReloadAndResume(resumeTime, live);
                }

                float deadline = Time.realtimeSinceStartup + recoveryObservationSeconds;
                while (Time.realtimeSinceStartup < deadline)
                {
                    if (player.CurrentTime > observationStartTime + 0.02 || player.CurrentFrame > observationStartFrame)
                    {
                        consecutiveAttempts = 0;
                        player.MarkRecovered("Playback progress resumed during recovery observation.");
                        ResetProgressClock();
                        recoveryInProgress = false;
                        yield break;
                    }

                    if (player.State == VideoPlaybackState.Failed)
                    {
                        break;
                    }

                    yield return null;
                }

                if (attempt < maximumConsecutiveAttempts && delayBetweenAttemptsSeconds > 0f)
                {
                    yield return new WaitForSecondsRealtime(delayBetweenAttemptsSeconds);
                }
            }

            string failureMessage = "Automatic recovery stopped after the configured attempt limit.";
            Log("recovery_exhausted", "error", failureMessage);
            player.MarkRecoveryFailed(failureMessage);
            recoveryInProgress = false;
            enabled = false;
        }

        public void ResetRecoveryBudget()
        {
            consecutiveAttempts = 0;
            recoveryInProgress = false;
            enabled = true;
            StopAllCoroutines();
            ResetProgressClock();
        }

        private void ResetProgressClock()
        {
            if (player != null)
            {
                lastMediaTime = player.CurrentTime;
                lastFrame = player.CurrentFrame;
            }
            else
            {
                lastMediaTime = 0.0;
                lastFrame = -1;
            }

            lastProgressRealtime = Time.realtimeSinceStartup;
            nextSampleRealtime = Time.realtimeSinceStartup + sampleIntervalSeconds;
        }

        private void Log(string eventName, string severity, string message, string detailsJson = "")
        {
            if (diagnostics != null)
            {
                diagnostics.Log(
                    "recovery",
                    eventName,
                    severity,
                    message,
                    player != null ? player.CurrentUrl : null,
                    player != null ? player.CurrentTime : -1.0,
                    player != null ? player.Duration : -1.0,
                    player != null ? player.CurrentFrame : -1,
                    player != null ? player.FrameRate : 0f,
                    player != null ? player.Width : 0,
                    player != null ? player.Height : 0,
                    detailsJson);
            }
        }
    }
}
