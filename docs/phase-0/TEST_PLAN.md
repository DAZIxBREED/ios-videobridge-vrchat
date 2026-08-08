# Phase 0 — Test Plan

## Objectives

1. Establish repeatable baseline playback in a standalone Unity application.
2. Reproduce each VRChat iOS failure with the smallest media and scene possible.
3. Compare the same media, device, network, and lifecycle sequence across backends.
4. Record enough evidence to separate stream, world, Unity, Apple, AVPro, and VRChat causes.

## Required Phase 1 tests

### Direct MP4

- Bundled synthetic H.264/AAC MP4 prepares and starts.
- Remote HTTPS H.264/AAC MP4 prepares and starts.
- Pause and resume preserve position.
- Seek reaches the requested position within a reasonable tolerance.
- Stop clears active playback.
- Looping restarts without a terminal error.

### HLS VOD

- HTTPS media playlist prepares.
- Master playlist selects a playable variant.
- Audio and video start.
- A transient network interruption either recovers or produces a bounded terminal error.

### Lifecycle

- Home/background interruption while playing.
- Foreground return after less than 30 seconds.
- Device lock and unlock.
- Audio route changed before and during playback.

### Diagnostics/privacy

- Every run has a session identifier.
- Error and recovery events are ordered.
- Signed query fields are redacted.
- Exported log remains valid JSONL after forced application termination.

## Comparison protocol

For a VRChat comparison, keep constant:

- Device and iOS build
- Network path
- Media URL and expiry window
- Stream variant where controllable
- Reproduction sequence
- Observation duration

Run in this order:

1. Standalone Unity reference
2. VRChat Unity backend
3. VRChat AVPro backend
4. Standalone reference again

The second reference run helps detect an expiring URL or changing network condition.

## Result statuses

Pass, Partial pass, Fail, Unsupported, Inconclusive, Client-side limitation, World-side limitation, Stream-side limitation.
