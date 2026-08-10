# Phase 1 — Implementation Status

## Baseline complete in 0.1.0-phase1

- Unity project and embedded UPM package
- Baseline direct URL and bundled MP4 player
- Basic HLS URL handoff to Unity's platform backend
- API-only texture output and harness display
- Unity `AudioSource` routing
- Playback state and timing telemetry
- URL compatibility report and HTTP metadata probe
- Sensitive URL redaction
- Bounded stall recovery
- Application pause/resume recovery
- Runtime control panel
- Edit-mode tests for deterministic logic
- iOS Xcode export command
- Synthetic test media and regeneration script

## Implemented on main for v0.1.1-dev.1

- Explicit playback states: Idle, Loading, Preparing, Ready, Playing, Paused, Buffering, Recovering, Failed, and Stopped
- Transition-policy validation with rejected-transition diagnostics
- Source normalization and supported-scheme validation
- Embedded URL credential rejection
- Query-safe HLS extension detection
- Deterministic seek target clamping and invalid-number rejection
- Preparation generation/cancellation tracking
- Active teardown on preparation timeout
- Stale `prepareCompleted` callback rejection
- Clean metadata reset on repeated load/stop/reload operations
- Audio track-count and controlled-track visibility
- Explicit audio-route configuration status
- Application lifecycle pause/resume separated from user Pause calls
- Recovery exhaustion promoted to Failed state
- Harness control guard so invalid loads do not fall through to Prepare/Play
- Expanded deterministic edit-mode coverage for states, source validation, HLS detection, and seek behavior

## v0.1.1 validation still required before tagging

- Run Unity edit-mode tests in Unity 2022.3.22f1
- Exercise load/prepare/play/pause/stop/reload loops in Play Mode
- Verify preparation timeout behavior with an intentionally unreachable source
- Verify lifecycle recovery on an iOS player build
- Confirm discovered audio-track and route reporting with the generated H.264/AAC MP4

Until those checks are recorded, `0.1.1-dev.1` is an implementation candidate rather than the final `v0.1.1` release.

## Intentionally deferred

- Direct AVFoundation/AVPlayer native plugin
- Metal/CoreVideo texture sharing
- Unity PCM extraction from native decoding
- Native audio-route notifications
- HLS variant and live-window introspection beyond Unity's public API
- VRChat comparison world
- Multi-device automated matrix aggregation

These are later releases defined by `ROADMAP.md`, not placeholder claims in Phase 1.
