# Changelog

## Unreleased

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
