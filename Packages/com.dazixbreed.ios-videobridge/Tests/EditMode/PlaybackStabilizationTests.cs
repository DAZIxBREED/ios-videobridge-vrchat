using NUnit.Framework;

namespace DAZIxBREED.IOSVideoBridge.Tests
{
    public sealed class PlaybackStabilizationTests
    {
        [Test]
        public void StatePolicy_AllowsExpectedHappyPath()
        {
            Assert.IsTrue(VideoPlaybackStatePolicy.CanTransition(VideoPlaybackState.Idle, VideoPlaybackState.Loading));
            Assert.IsTrue(VideoPlaybackStatePolicy.CanTransition(VideoPlaybackState.Loading, VideoPlaybackState.Preparing));
            Assert.IsTrue(VideoPlaybackStatePolicy.CanTransition(VideoPlaybackState.Preparing, VideoPlaybackState.Ready));
            Assert.IsTrue(VideoPlaybackStatePolicy.CanTransition(VideoPlaybackState.Ready, VideoPlaybackState.Playing));
            Assert.IsTrue(VideoPlaybackStatePolicy.CanTransition(VideoPlaybackState.Playing, VideoPlaybackState.Paused));
            Assert.IsTrue(VideoPlaybackStatePolicy.CanTransition(VideoPlaybackState.Paused, VideoPlaybackState.Playing));
            Assert.IsTrue(VideoPlaybackStatePolicy.CanTransition(VideoPlaybackState.Playing, VideoPlaybackState.Buffering));
            Assert.IsTrue(VideoPlaybackStatePolicy.CanTransition(VideoPlaybackState.Buffering, VideoPlaybackState.Recovering));
            Assert.IsTrue(VideoPlaybackStatePolicy.CanTransition(VideoPlaybackState.Recovering, VideoPlaybackState.Ready));
        }

        [Test]
        public void StatePolicy_RejectsImpossibleDirectTransitions()
        {
            Assert.IsFalse(VideoPlaybackStatePolicy.CanTransition(VideoPlaybackState.Idle, VideoPlaybackState.Playing));
            Assert.IsFalse(VideoPlaybackStatePolicy.CanTransition(VideoPlaybackState.Loading, VideoPlaybackState.Ready));
            Assert.IsFalse(VideoPlaybackStatePolicy.CanTransition(VideoPlaybackState.Failed, VideoPlaybackState.Playing));
        }

        [Test]
        public void SourceNormalizer_TrimsHttpsUrlWithoutChangingSignatureText()
        {
            string normalized;
            string error;

            bool valid = VideoSourceNormalizer.TryNormalize("  https://cdn.example/video.mp4?token=abc123  ", out normalized, out error);

            Assert.IsTrue(valid, error);
            Assert.AreEqual("https://cdn.example/video.mp4?token=abc123", normalized);
        }

        [Test]
        public void SourceNormalizer_RejectsRelativeInput()
        {
            string normalized;
            string error;

            bool valid = VideoSourceNormalizer.TryNormalize("media/video.mp4", out normalized, out error);

            Assert.IsFalse(valid);
            StringAssert.Contains("absolute URL", error);
        }

        [Test]
        public void SourceNormalizer_RejectsEmbeddedCredentials()
        {
            string normalized;
            string error;

            bool valid = VideoSourceNormalizer.TryNormalize("https://user:password@example.com/video.mp4", out normalized, out error);

            Assert.IsFalse(valid);
            StringAssert.Contains("password", error);
        }

        [Test]
        public void SourceNormalizer_RejectsUnsupportedScheme()
        {
            string normalized;
            string error;

            bool valid = VideoSourceNormalizer.TryNormalize("ftp://example.com/video.mp4", out normalized, out error);

            Assert.IsFalse(valid);
            StringAssert.Contains("Unsupported", error);
        }

        [Test]
        public void SourceNormalizer_HlsDetectionIgnoresQueryString()
        {
            Assert.IsTrue(VideoSourceNormalizer.IsLikelyHls("https://cdn.example/live/index.m3u8?token=abc"));
            Assert.IsFalse(VideoSourceNormalizer.IsLikelyHls("https://cdn.example/video.mp4?next=.m3u8"));
        }

        [Test]
        public void SeekUtility_ClampsNegativeToZero()
        {
            double target;
            Assert.IsTrue(VideoSeekUtility.TryClampTarget(-50.0, 120.0, out target));
            Assert.AreEqual(0.0, target, 0.0001);
        }

        [Test]
        public void SeekUtility_ClampsPastFiniteDuration()
        {
            double target;
            Assert.IsTrue(VideoSeekUtility.TryClampTarget(999.0, 120.0, out target));
            Assert.AreEqual(119.95, target, 0.0001);
        }

        [Test]
        public void SeekUtility_AllowsUnboundedLiveStyleTarget()
        {
            double target;
            Assert.IsTrue(VideoSeekUtility.TryClampTarget(75.0, double.PositiveInfinity, out target));
            Assert.AreEqual(75.0, target, 0.0001);
        }

        [Test]
        public void SeekUtility_RejectsNaNAndInfinity()
        {
            double target;
            Assert.IsFalse(VideoSeekUtility.TryClampTarget(double.NaN, 120.0, out target));
            Assert.IsFalse(VideoSeekUtility.TryClampTarget(double.PositiveInfinity, 120.0, out target));
        }
    }
}
