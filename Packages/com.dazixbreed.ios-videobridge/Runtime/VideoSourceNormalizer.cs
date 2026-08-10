using System;
using System.IO;

namespace DAZIxBREED.IOSVideoBridge
{
    public static class VideoSourceNormalizer
    {
        public static bool TryNormalize(string input, out string normalized, out string error)
        {
            normalized = string.Empty;
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                error = "A media URL or rooted local path is required.";
                return false;
            }

            string candidate = input.Trim();
            for (int i = 0; i < candidate.Length; i++)
            {
                if (char.IsControl(candidate[i]))
                {
                    error = "The media source contains control characters.";
                    return false;
                }
            }

            if (IsRootedLocalPath(candidate))
            {
                normalized = candidate;
                return true;
            }

            Uri uri;
            if (!Uri.TryCreate(candidate, UriKind.Absolute, out uri))
            {
                error = "The media source must be an absolute URL or rooted local path.";
                return false;
            }

            if (!string.IsNullOrEmpty(uri.UserInfo))
            {
                error = "URLs containing embedded user names or passwords are not accepted.";
                return false;
            }

            string scheme = uri.Scheme.ToLowerInvariant();
            if (scheme != Uri.UriSchemeHttp &&
                scheme != Uri.UriSchemeHttps &&
                scheme != Uri.UriSchemeFile &&
                scheme != "jar")
            {
                error = "Unsupported media URL scheme: " + scheme + ".";
                return false;
            }

            normalized = candidate;
            return true;
        }

        public static bool IsLikelyHls(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            Uri uri;
            if (Uri.TryCreate(value.Trim(), UriKind.Absolute, out uri))
            {
                return Path.GetExtension(uri.AbsolutePath).Equals(".m3u8", StringComparison.OrdinalIgnoreCase);
            }

            return Path.GetExtension(value.Trim()).Equals(".m3u8", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsRootedLocalPath(string value)
        {
            if (Path.IsPathRooted(value))
            {
                return true;
            }

            return value.Length >= 3 &&
                   char.IsLetter(value[0]) &&
                   value[1] == ':' &&
                   (value[2] == '\\' || value[2] == '/');
        }
    }
}
