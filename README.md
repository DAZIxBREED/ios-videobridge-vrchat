# iOS VideoBridge for VRChat

**Written and developed by DAZIxBREED**

Independent clean-room compatibility research and reference implementation for reliable Unity and VRChat-style video playback on iPhone and iPad.

> This project is independent and is not affiliated with or endorsed by VRChat, Unity, Apple, RenderHeads, or AVPro Video.

## Current status

Repository version: **0.1.0-phase1**

- **Phase 0 — Research and Test Definition:** initial documents, failure taxonomy, compatibility matrix, logging schema, clean-room boundary, issue template, and synthetic known-good media are present.
- **Phase 1 — Standalone Unity Test Harness:** a runnable Unity `VideoPlayer` reference backend, URL analyzer, diagnostics logger, bounded stall recovery, lifecycle recovery, test UI, scene, tests, and iOS Xcode build command are present.
- Native AVFoundation integration belongs to Phase 2 and is intentionally not included yet.

## Requirements

- Unity **2022.3.22f1**
- iOS Build Support module for Unity
- Xcode on macOS for an actual iPhone or iPad build
- A direct HTTPS H.264/AAC MP4 or HLS URL for network tests

## License

MIT. See [`LICENSE`](LICENSE).
