# Security and privacy

Playback diagnostics may contain sensitive URLs. The runtime redacts common token, signature, cookie, authorization, and key query parameters before writing logs. Redaction is defense in depth, not a guarantee.

Before publishing a report:

1. Open the exported JSONL file.
2. Confirm media URLs are sanitized.
3. Remove user names, local paths, IP addresses, and account identifiers when they are not required.
4. Never publish DRM licenses, bearer tokens, cookies, signed URLs, or private media.

Security issues should be reported privately to the repository owner before public disclosure.
