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
            VideoCompatibilityReport report = IOSVideoCompatibilityAnalyzer.AnalyzeLocal("https://cdn.example/live/index.m3u8");

            Assert.IsTrue(report.isValid);
            Assert.IsTrue(report.isHls);
            Assert.AreEqual("HLS playlist", report.inferredContainer);
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
    }
}
