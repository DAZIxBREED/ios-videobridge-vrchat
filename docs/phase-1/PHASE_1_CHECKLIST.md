# Phase 1 Checklist

## 0.1.0-phase1 baseline

- [x] Create Unity 2022.3.22f1 project structure
- [x] Create embedded UPM package
- [x] Implement URL input and bundled media selection
- [x] Implement Unity VideoPlayer load and preparation
- [x] Implement play, pause, stop, seek, loop, volume, and speed
- [x] Route media audio to Unity AudioSource
- [x] Display API-only output texture with aspect preservation
- [x] Add manual Y-flip comparison control
- [x] Add preparation timeout
- [x] Add state, timing, first-frame, error, and lifecycle logs
- [x] Add bounded stall recovery
- [x] Add application pause/resume recovery
- [x] Add URL analyzer and HLS manifest probe
- [x] Add JSONL diagnostics export
- [x] Add edit-mode tests for analyzer and URL redaction
- [x] Add iOS Xcode export menu and command-line entry point
- [x] Add repository and media validation scripts

## v0.1.1 stabilization implementation

- [x] Formalize playback state model and transition policy
- [x] Harden URL normalization and invalid-input handling
- [x] Reject embedded URL credentials
- [x] Harden preparation timeout teardown and stale callback handling
- [x] Reset metadata consistently across repeated load/stop/reload operations
- [x] Harden seek edge cases and end-of-media clamping
- [x] Log loop iterations/completion using the explicit state model
- [x] Expose discovered/controlled audio-track counts and route status
- [x] Separate application lifecycle pause/resume from user Pause calls
- [x] Promote exhausted automatic recovery to Failed state
- [x] Prevent rejected loads from falling through to Prepare/Play in the harness
- [x] Expand deterministic tests for state, URL, HLS, and seek logic

## Validation gates still open

- [ ] Run Unity edit-mode tests in Unity 2022.3.22f1
- [ ] Run repeated Play Mode load/prepare/play/pause/stop/reload cycles
- [ ] Exercise an intentional preparation timeout
- [ ] Export an Xcode project with Unity iOS Build Support
- [ ] Install and run on a physical iPhone/iPad
- [ ] Verify app background/foreground recovery on-device
- [ ] Verify audio-track/route reporting with known-good H.264/AAC media

The v0.1.1 source implementation is on `main` as `0.1.1-dev.1`. The final `v0.1.1` tag remains blocked by the validation gates above, in accordance with `docs/RELEASE_POLICY.md`.
