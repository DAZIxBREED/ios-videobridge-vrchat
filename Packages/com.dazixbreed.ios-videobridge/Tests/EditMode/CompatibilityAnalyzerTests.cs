using NUnit.Framework;

namespace DAZIxBREED.IOSVideoBridge.Tests
{
    public sealed class CompatibilityAnalyzerTests
    {
        [Test]
        public void AnalyzeLocal_HttpsMp4ProducesHighConditionalEstimate()
        {
            VideoCompatibilityReport report = IOSVideoCompatibilityAnalyzer.AnalyzeLocal("https://cdn.example/media/video.mp4");

            Assert.IsTrue(report.isValid);
            Assert.IsTrue(report.isHttps);
            Assert.IsFalse(report.isHls);
            Assert.AreEqual("MP4", report.inferredContainer);
            StringAssert.Contains("High", report.expectedCompatibility);
        }

        [Test]
        public void AnalyzeLocal_HlsIsRecognized()
        {
            VideoCompatibilityReport report = IOSVideoCompatibilityAnalyzer.AnalyzeLocal("https://cdn.example/live/index.m3u8?token=abc");

            Assert.IsTrue(report.isValid);
            Assert.IsTrue(report.isHls);
            Assert.AreEqual("HLS playlist", report.inferredContainer);
            Assert.That(report.warnings, Has.Some.Contains("signed or temporary"));
        }

        [Test]
        public void AnalyzeLocal_QueryTextDoesNotCreateFalseHlsPositive()
        {
            VideoCompatibilityReport report = IOSVideoCompatibilityAnalyzer.AnalyzeLocal("https://cdn.example/video.mp4?next=.m3u8");

            Assert.IsTrue(report.isValid);
            Assert.IsFalse(report.isHls);
            Assert.AreEqual("MP4", report.inferredContainer);
        }

        [Test]
        public void AnalyzeLocal_HttpWarnsAboutAppTransportSecurity()
        {
            VideoCompatibilityReport report = IOSVideoCompatibilityAnalyzer.AnalyzeLocal("http://cdn.example/video.mp4");

            Assert.IsTrue(report.isValid);
            Assert.IsFalse(report.isHttps);
            Assert.That(report.warnings, Has.Some.Contains("App Transport Security"));
        }

        [Test]
        public void AnalyzeLocal_EmptyInputIsInvalid()
        {
            VideoCompatibilityReport report = IOSVideoCompatibilityAnalyzer.AnalyzeLocal(string.Empty);

            Assert.IsFalse(report.isValid);
            Assert.IsNotEmpty(report.warnings);
        }

        [Test]
        public void AnalyzeLocal_RelativeInputIsInvalid()
        {
            VideoCompatibilityReport report = IOSVideoCompatibilityAnalyzer.AnalyzeLocal("media/video.mp4");

            Assert.IsFalse(report.isValid);
            Assert.That(report.warnings, Has.Some.Contains("absolute URL"));
        }

        [Test]
        public void AnalyzeLocal_EmbeddedCredentialsAreRejected()
        {
            VideoCompatibilityReport report = IOSVideoCompatibilityAnalyzer.AnalyzeLocal("https://user:password@example.com/video.mp4");

            Assert.IsFalse(report.isValid);
            Assert.That(report.warnings, Has.Some.Contains("password"));
        }
    }
}
