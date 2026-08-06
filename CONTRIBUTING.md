# Contributing

Contributions must preserve the project's clean-room boundary and produce reproducible evidence.

## Required for playback reports

- Device model and iOS version
- Unity and repository version
- Backend name
- Sanitized media description or a redistributable test URL
- Exact reproduction steps
- Expected and observed behavior
- Exported JSONL diagnostics
- Whether the same media succeeds in another backend or platform

Never commit access tokens, cookies, signed query strings, private stream URLs, DRM material, proprietary AVPro source, VRChat client binaries, or decompiled code.

## Code expectations

- Keep runtime code compatible with Unity 2022.3 LTS and IL2CPP.
- Avoid Editor-only APIs in Runtime assemblies.
- Add edit-mode tests for deterministic parsing and redaction logic.
- Treat recovery as bounded; no infinite retries.
- Log state transitions and errors without logging secrets.
