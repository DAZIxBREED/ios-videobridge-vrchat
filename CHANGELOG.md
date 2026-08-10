# Changelog

## Unreleased

### v0.1.1 development candidate

- Replaced the loose Phase 1 playback enum with the explicit Idle, Loading, Preparing, Ready, Playing, Paused, Buffering, Recovering, Failed, and Stopped state model.
- Added `VideoPlaybackStatePolicy` so runtime transitions are checked and invalid transitions are logged instead of silently mutating state.
- Added `VideoSourceNormalizer` for whitespace trimming, supported-scheme validation, embedded-credential rejection, rooted local-path handling, and query-safe HLS detection.
- Added `VideoSeekUtility` for deterministic negative/end-of-media clamping and NaN/infinity rejection.
- Hardened preparation timeouts so timed-out preparation is actively stopped and stale `prepareCompleted` callbacks are ignored.
- Hardened repeated load/stop/reload paths so prepared/audio/frame metadata is reset consistently.
- Separated application lifecycle pause/resume handling from explicit user pause commands.
- Expanded audio visibility with discovered track count, controlled track count, and route-configuration status.
- Updated bounded stall recovery to use Buffering/Recovering/Failed states and fail explicitly when the retry budget is exhausted.
- Improved the runtime harness so invalid source input cannot fall through into Prepare/Play calls.
- Expanded analyzer and stabilization edit-mode tests for transition legality, URL validation, HLS query handling, and seek edge cases.
- Marked package/runtime version as `0.1.1-dev.1` until Unity 2022.3.22f1 and device validation satisfy the final v0.1.1 release gate.

### Project governance

- Added `ROADMAP.md` as the canonical development and release contract through `v1.0.0`.
- Locked the programming dependency order from Phase 1 stabilization through native AVPlayer, texture, audio, HLS, recovery, VRChat comparison, automation, submission tooling, and stable release.
- Added `docs/RELEASE_POLICY.md` defining version advancement, prerelease naming, release gates, tagging rules, and roadmap-amendment procedure.
- Updated `README.md` and `CONTRIBUTING.md` to make roadmap compliance normative for future development.
- Locked `v0.1.1` as the next development milestone.

## 0.1.0-phase1 — 2026-08-05

### Added

- Phase 0 clean-room boundary, architecture, test plan, failure taxonomy, logging schema, matrices, and report templates.
- Full Unity 2022.3.22f1 project shell.
- Functional Unity `VideoPlayer` reference backend.
- Runtime test harness with URL entry, bundled-media loading, playback controls, texture display, and status panel.
- JSONL diagnostics with sensitive URL redaction.
- Bounded stall and foreground/background recovery.
- URL compatibility analyzer using local heuristics and HTTP response metadata.
- iOS Xcode-project build menu and project validator.
- Edit-mode tests.
- Synthetic H.264/AAC MP4 and HLS VOD media generation workflow.
