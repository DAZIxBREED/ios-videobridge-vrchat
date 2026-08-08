using System;
using System.Collections.Generic;
using System.IO;

namespace DAZIxBREED.IOSVideoBridge
{
    public static class SensitiveUrlRedactor
    {
        private static readonly string[] SensitiveMarkers =
        {
            "token", "signature", "sig", "key", "auth", "authorization",
            "credential", "cookie", "policy", "secret", "password", "passwd",
            "x-amz-credential", "x-amz-signature", "x-goog-credential", "x-goog-signature"
        };

        public static bool IsSensitiveKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            string normalized = Uri.UnescapeDataString(key).Trim().ToLowerInvariant();
            for (int i = 0; i < SensitiveMarkers.Length; i++)
            {
                if (normalized.Contains(SensitiveMarkers[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public static string Redact(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            Uri uri;
            if (!Uri.TryCreate(input, UriKind.Absolute, out uri))
            {
                return RedactQueryOnly(input);
            }

            if (uri.IsFile)
            {
                string fileName = Path.GetFileName(uri.LocalPath);
                return "file:///[LOCAL]/" + Uri.EscapeDataString(string.IsNullOrWhiteSpace(fileName) ? "media" : fileName);
            }

            try
            {
                UriBuilder builder = new UriBuilder(uri)
                {
                    UserName = string.Empty,
                    Password = string.Empty,
                    Query = RedactQuery(uri.Query)
                };
                return builder.Uri.AbsoluteUri;
            }
            catch
            {
                return RedactQueryOnly(input);
            }
        }

        private static string RedactQueryOnly(string input)
        {
            int queryIndex = input.IndexOf('?');
            if (queryIndex < 0)
            {
                return input;
            }

            int fragmentIndex = input.IndexOf('#', queryIndex);
            string prefix = input.Substring(0, queryIndex + 1);
            string query = fragmentIndex >= 0
                ? input.Substring(queryIndex + 1, fragmentIndex - queryIndex - 1)
                : input.Substring(queryIndex + 1);
            string fragment = fragmentIndex >= 0 ? input.Substring(fragmentIndex) : string.Empty;
            return prefix + RedactQuery(query) + fragment;
        }

        private static string RedactQuery(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return string.Empty;
            }

            string trimmed = query[0] == '?' ? query.Substring(1) : query;
            string[] pairs = trimmed.Split('&');
            var output = new List<string>(pairs.Length);

            for (int i = 0; i < pairs.Length; i++)
            {
                string pair = pairs[i];
                if (pair.Length == 0)
                {
                    continue;
                }

                int equalsIndex = pair.IndexOf('=');
                string key = equalsIndex >= 0 ? pair.Substring(0, equalsIndex) : pair;
                string value = equalsIndex >= 0 ? pair.Substring(equalsIndex + 1) : string.Empty;

                if (IsSensitiveKey(key))
                {
                    value = Uri.EscapeDataString("[REDACTED]");
                }

                output.Add(equalsIndex >= 0 ? key + "=" + value : key);
            }

            return string.Join("&", output.ToArray());
        }
    }
}
