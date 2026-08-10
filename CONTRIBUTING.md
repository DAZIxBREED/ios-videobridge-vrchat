# Contributing

Contributions must preserve the project's clean-room boundary, follow the canonical development sequence, and produce reproducible evidence.

## Canonical development contract

All implementation work must follow [`ROADMAP.md`](ROADMAP.md) and [`docs/RELEASE_POLICY.md`](docs/RELEASE_POLICY.md).

Release ordering, subsystem dependencies, and release gates must not be silently skipped, renumbered, or collapsed. A material change requires an explicit commit or pull request whose title begins with:

```text
Roadmap Amendment:
```

The amendment must explain the technical reason for the change and must add a matching `CHANGELOG.md` entry.

## Branch contract

The project does not use a permanent `develop` branch. Branch from `main` using:

```text
feature/<description>
fix/<description>
test/<description>
docs/<description>
release/<version>
```

`main` must remain the newest usable project state.

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
- Add device/play-mode validation when behavior depends on runtime playback, lifecycle, audio, Metal, or Apple frameworks.
- Treat recovery as bounded; no infinite retries.
- Log state transitions and errors without logging secrets.
- Do not merge experimental native changes that destroy the working Unity reference baseline.
- Do not advance a release version until its gate in `ROADMAP.md` is satisfied or explicitly amended.
