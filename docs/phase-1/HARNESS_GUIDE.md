# Phase 1 — Standalone Harness Guide

## Components

- `IOSUnityVideoReferencePlayer`: owns and configures Unity `VideoPlayer` and `AudioSource`.
- `IOSVideoCompatibilityAnalyzer`: performs URL validation, local format inference, and an optional HTTP HEAD probe.
- `IOSVideoDiagnostics`: writes sanitized JSONL events.
- `IOSVideoRecoveryController`: watches media-time progress and attempts bounded recovery.
- `IOSVideoTestHarness`: immediate-mode UI and output texture display.

## Controls

- **Bundled MP4:** loads the generated local sample.
- **Analyze:** validates the current URL and probes response metadata.
- **Prepare:** loads and prepares without starting.
- **Play/Pause/Stop:** standard transport controls.
- **-10s/+10s:** bounded seeks for finite seekable media.
- **Live/Reload:** reloads HLS media and starts from the backend-selected live position.
- **Export Diagnostics:** closes and copies the current JSONL report to a timestamped export.

## Recovery behavior

When the player claims to be playing but media time does not advance for the configured stall window, recovery proceeds in bounded stages:

1. Call `Play()` again.
2. Reload the URL and restore position for finite VOD media.
3. Reload without restoring position for likely live HLS media.
4. Stop automatic retries after the configured maximum and emit a terminal diagnostic.

Successful media-time progress resets the consecutive failure counter.

## Interpreting audio telemetry

Unity `VideoPlayer` does not expose a reliable first decoded audio-sample callback in this configuration. Phase 1 logs `audio_route_ready` when preparation reports at least one audio track and the routed `AudioSource` is configured. This is not proof that the user heard audio; device recordings and route tests remain required.
