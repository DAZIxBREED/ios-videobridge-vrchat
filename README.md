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

The repository can be opened and its Xcode project generated from a Unity Editor host with iOS Build Support. The generated Xcode project must then be compiled and signed with Xcode on macOS for local device installation.

## Open and run

1. Add this repository through Unity Hub using Unity 2022.3.22f1.
2. Open `Assets/Scenes/IOSVideoBridgeTest.unity`.
3. Enter Play Mode.
4. Use **Bundled MP4** for the generated local H.264/AAC sample, or paste an HTTPS media URL.
5. Use **Export Diagnostics** to write a JSON Lines report under `Application.persistentDataPath/IOSVideoBridge/Diagnostics`.

## Build an iOS Xcode project

From Unity, choose:

`iOS VideoBridge > Build iOS Xcode Project`

The project is written to `Builds/iOS`. The build tool adds the test scene to Build Settings and applies conservative iOS defaults.

Command-line equivalent:

```bash
Unity -batchmode -quit \
  -projectPath /path/to/ios-videobridge-vrchat \
  -executeMethod DAZIxBREED.IOSVideoBridge.Editor.IOSVideoBridgeBuildMenu.BuildIOSFromCommandLine \
  -logFile -
```

## Repository layout

```text
Assets/
  Scenes/                         Runnable Phase 1 test scene
  StreamingAssets/IOSVideoBridge Generated known-good MP4 and HLS test media
Packages/
  com.dazixbreed.ios-videobridge/
    Runtime/                      Player, recovery, analyzer, diagnostics, UI
    Editor/                       iOS build and project validation tools
    Tests/EditMode/               Deterministic edit-mode tests
    Samples~/                     Package sample documentation
docs/phase-0/                     Research and reproducibility documents
docs/phase-1/                     Harness design and operating guide
scripts/                          Test-media server and repository checks
```

## Phase 1 scope

The Phase 1 backend wraps Unity's public `VideoPlayer` API. It supports:

- Direct MP4 and basic HLS URL preparation
- Play, pause, stop, seek, looping, speed, and volume
- API-only texture output drawn by the test harness
- Unity `AudioSource` routing
- First-frame, first-audio approximation, dropped-frame, error, and state logging
- Application pause/resume recovery
- Bounded stall detection and reload/resume recovery
- URL redaction before diagnostics are written
- Local synthetic H.264/AAC media for repeatable tests

It does **not** claim to fix VRChat's installed iOS client. It creates independent evidence and reference behavior that can be compared with VRChat's own backends.

## Clean-room policy

See [`docs/phase-0/CLEAN_ROOM_BOUNDARY.md`](docs/phase-0/CLEAN_ROOM_BOUNDARY.md). Do not submit proprietary VRChat or AVPro code, decompiled material, authentication tokens, DRM-protected streams, or private URLs.

## License

MIT. See [`LICENSE`](LICENSE).
