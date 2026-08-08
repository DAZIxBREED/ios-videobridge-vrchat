# iOS VideoBridge for VRChat — Project Specification

**Author:** DAZIxBREED  
**Project type:** Independent clean-room Unity and iOS video-playback compatibility project  
**Primary target:** VRChat video behavior on iPhone and iPad  
**Supporting targets:** Unity Editor, Windows, Android, Meta Quest, iPhone and iPad  
**Languages:** C#, Objective-C++, Swift, HLSL and ShaderLab  
**License goal:** Permissive open-source licensing suitable for technical evaluation and possible adoption

## Mission

iOS VideoBridge for VRChat investigates video-playback failures affecting VRChat-compatible environments on Apple devices and produces original reference implementations, reproducible failures, diagnostics, test applications and technically specific proposed fixes. It does not modify, decompile or redistribute proprietary VRChat or AVPro implementation code.

The project is intended to help distinguish world-side, stream-side, Unity, Apple-framework, AVPro/vendor and VRChat client integration failures. Typical targets include missing audio, HLS/live-start failures, flipped textures, unsupported stream selection, failed recovery, poor synchronization, incomplete diagnostics, lifecycle failures, route changes and live-edge reconnection.

## Core deliverables

1. A standalone Unity iOS test application.
2. An original Unity-video reference backend.
3. An independent AVPro-style behavior reference for later phases.
4. A native Apple playback backend using public frameworks in later phases.
5. Audio and texture bridge experiments.
6. A compatibility analyzer and structured diagnostic logger.
7. A bounded recovery system.
8. Synchronization reference tests.
9. A VRChat comparison world using only supported world components.
10. Reproduction packages and engineering reports suitable for VRChat, Unity, RenderHeads or Apple when evidence points to those layers.

## Initial required media targets

- HTTPS MP4
- H.264/AVC video
- AAC audio
- HLS `.m3u8`
- HLS VOD
- HLS livestreams

Experimental targets may include HEVC, fragmented MP4, alternate audio tracks, captions, HDR, Low-Latency HLS and audio-only HLS. DRM circumvention, authentication bypasses, unauthorized stream extraction and proprietary-source redistribution are explicitly out of scope.

## Reference components

### `IOSUnityVideoReferencePlayer`

Models the expected behavior of a Unity-based player: load, prepare, play, pause, stop, seek, loop, speed, duration/time reporting, audio routing, texture output, buffering/stall observation, error reporting and lifecycle recovery.

### `IOSAVProReferencePlayer` — later phase

An independent behavioral reference focused on HLS, livestreams, live-edge behavior, adaptive transitions, reconnect logic, seeking inside live windows, texture output and audio routing. It will not contain AVPro source code.

### `IOSVideoBridgeNative` — Phase 2+

Native Apple backend built from public APIs such as AVFoundation, AVPlayer, AVPlayerItem, AVPlayerItemVideoOutput, AVAudioSession, CoreVideo, CoreMedia, Metal and VideoToolbox.

### `IOSVideoAudioBridge` — later phase

Compares native playback and Unity PCM paths so the project can document latency, reliability, world-space audio, AudioLink and routing tradeoffs.

### `IOSVideoTextureBridge` — later phase

Tests CoreVideo/Metal texture transfer, orientation, color space, range conversion, NV12/BGRA paths, aspect ratio, frame timing and dropped-frame behavior.

### `IOSVideoCompatibilityAnalyzer`

Reports URL/delivery characteristics, inferred container and HLS type, MIME/network information, warnings, signed/temporary URL risk and an expected compatibility estimate. Native/probe phases can later expand this to exact codec, bitrate, resolution, variant and encryption reporting.

### `IOSVideoRecoveryController`

Detects stalled progress and applies bounded retries. Recovery may resume playback, reload media, restore a VOD position, return a live stream to backend-selected live playback, and finally emit a terminal diagnostic rather than retry forever.

### `IOSVideoDiagnostics`

Records device/runtime information, sanitized media identifiers, state changes, preparation and startup timing, buffering/stall events, route/lifecycle events, frame information, retry attempts and final outcomes. Secret-bearing URL fields must be redacted before sharing.

## Test applications

The standalone Unity application accepts a media URL, selects/uses a reference backend, exposes playback controls, displays video output, routes audio, shows analyzer results and exports diagnostics. The VRChat comparison world remains separate and uses only VRChat-supported world components; a native reference backend is never injected through an uploaded world.

## Result vocabulary

Tests use: **Pass**, **Partial pass**, **Fail**, **Unsupported**, **Inconclusive**, **Client-side limitation**, **World-side limitation**, or **Stream-side limitation**. Reports should clearly separate observations, hypotheses and unknowns.

## Development phases

### Phase 0 — Research and Test Definition
Establish clean-room rules, failure taxonomy, supported formats, compatibility matrix, logging requirements and known-good media.

### Phase 1 — Standalone Unity Test Harness
Create the Unity project, URL-entry interface, Unity `VideoPlayer` reference backend, diagnostics, recovery, synthetic media, edit-mode tests and iOS Xcode export tooling.

### Phase 2 — Native iOS Playback Bridge
Add original AVPlayer integration, controls, state callbacks, native error reporting and baseline MP4 playback.

### Phase 3 — Video Texture Integration
Retrieve native frames, transfer them into Unity, correct orientation/color, test timing and build iOS-safe shaders.

### Phase 4 — Audio Integration
Configure AVAudioSession, test native output, prototype Unity PCM transfer and evaluate synchronization, Bluetooth/headphone behavior and spatial-audio compatibility.

### Phase 5 — HLS and Livestream Support
Add HLS VOD/live handling, live-edge tracking, reconnect behavior, adaptive reporting and interruption tests.

### Phase 6 — Recovery and Lifecycle Handling
Harden foreground/background, lock/unlock, route change, decoder reset and bounded-retry behavior.

### Phase 7 — VRChat Comparison World
Build a minimal comparison world and test the same media against VRChat's available video backends across iOS, Android/Quest and PC.

### Phase 8 — Automated Compatibility Testing
Add repeatable cases, machine-readable results, reports and multi-device test aggregation.

### Phase 9 — Technical Submission
Reduce issues to minimal reproductions with logs, recordings, device/software information, reference behavior and a specific proposed correction.

### Phase 10 — Community Release
Publish the suite, documentation, samples and compatibility matrix; accept device results and maintain future revisions.

## Initial MVP

The first usable version requires a standalone Unity iOS application, H.264/AAC MP4 playback, basic HLS handoff, play/pause/stop/seek, video texture output, audio output, orientation controls, error reporting, application-resume handling, URL input, exportable diagnostics and at least one future comparison against each VRChat video backend.

## Definition of success

A technical success is a reproducible iOS failure that succeeds in the independent reference application, is narrowed to a defensible integration layer, and is documented with original reference code and enough evidence for another engineer to repeat the experiment. The larger project succeeds when those findings materially help improve iPhone/iPad video playback in VRChat-compatible environments.

## Independence statement

iOS VideoBridge for VRChat is written and developed by DAZIxBREED as an independent compatibility research project. It is not affiliated with or endorsed by VRChat, Unity, Apple or RenderHeads.
