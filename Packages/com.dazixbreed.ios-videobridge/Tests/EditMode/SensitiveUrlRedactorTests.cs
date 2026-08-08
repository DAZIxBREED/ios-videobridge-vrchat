using NUnit.Framework;

namespace DAZIxBREED.IOSVideoBridge.Tests
{
    public sealed class SensitiveUrlRedactorTests
    {
        [Test]
        public void Redact_RemovesCommonSecretValues()
        {
            const string input = "https://media.example/video.mp4?token=abc123&quality=high&X-Amz-Signature=deadbeef";
            string result = SensitiveUrlRedactor.Redact(input);

            StringAssert.DoesNotContain("abc123", result);
            StringAssert.DoesNotContain("deadbeef", result);
            StringAssert.Contains("quality=high", result);
            StringAssert.Contains("REDACTED", System.Uri.UnescapeDataString(result));
        }

        [Test]
        public void Redact_RemovesUserInfo()
        {
            const string input = "https://user:password@example.com/video.mp4";
            string result = SensitiveUrlRedactor.Redact(input);

            StringAssert.DoesNotContain("user", result);
            StringAssert.DoesNotContain("password", result);
            StringAssert.Contains("example.com", result);
        }


        [Test]
        public void Redact_HidesLocalDirectoryNames()
        {
            string result = SensitiveUrlRedactor.Redact("file:///home/DAZIxBREED/private/known-good.mp4");

            StringAssert.DoesNotContain("DAZIxBREED", result);
            StringAssert.DoesNotContain("private", result);
            StringAssert.Contains("known-good.mp4", result);
        }

        [Test]
        public void IsSensitiveKey_DoesNotFlagOrdinaryQualityField()
        {
            Assert.IsFalse(SensitiveUrlRedactor.IsSensitiveKey("quality"));
            Assert.IsTrue(SensitiveUrlRedactor.IsSensitiveKey("access_token"));
        }
    }
}
