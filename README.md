# iOS VideoBridge for VRChat

**Written and developed by DAZIxBREED**

Independent clean-room compatibility research and reference implementation for reliable Unity and VRChat-style video playback on iPhone and iPad.

> This project is independent and is not affiliated with or endorsed by VRChat, Unity, Apple, RenderHeads, or AVPro Video.

## Current status

Repository version: **0.1.1-dev.1** — v0.1.1 implementation candidate pending Unity/iOS validation.

- **Phase 0 — Research and Test Definition:** initial documents, failure taxonomy, compatibility matrix, logging schema, clean-room boundary, issue template, and reproducible synthetic-media generation tooling are present.
- **Phase 1 — Standalone Unity Test Harness:** a runnable Unity `VideoPlayer` reference backend, URL analyzer, diagnostics logger, bounded stall recovery, lifecycle recovery, test UI, scene, tests, and iOS Xcode build command are present.
- **v0.1.1 stabilization work on `main`:** explicit playback-state transitions, source normalization, stale-prepare protection, timeout teardown, deterministic seek clamping, improved audio-track reporting, lifecycle/user-pause separation, bounded recovery failure state, and expanded edit-mode tests.
- Native AVFoundation integration remains deferred until the v0.1.x stabilization train reaches the Phase 1 API freeze.

The final `v0.1.1` tag is intentionally **not** published until the release-policy validation gate is satisfied in Unity 2022.3.22f1 and the relevant device checks are recorded.

## Development contract

The canonical release and programming plan is [`ROADMAP.md`](ROADMAP.md). Release numbering, subsystem dependency order, and completion gates in that document are normative for this project.

Release/version mechanics are defined in [`docs/RELEASE_POLICY.md`](docs/RELEASE_POLICY.md).

**Current milestone:** `v0.1.1` — Phase 1 stabilization and harness/runtime fixes. Native AVFoundation work begins only after the v0.1.x reference-player state contract is sufficiently stable for comparison testing.

Material changes to the roadmap require an explicit **Roadmap Amendment** and a matching `CHANGELOG.md` entry.

## Requirements

- Unity **2022.3.22f1**
- iOS Build Support module for Unity
- Xcode on macOS for an actual iPhone or iPad build
- FFmpeg when generating the bundled synthetic MP4/HLS fixtures
- A direct HTTPS H.264/AAC MP4 or HLS URL for network tests

The repository can be opened and its Xcode project generated from a Unity Editor host with iOS Build Support. The generated Xcode project must then be compiled and signed with Xcode on macOS for local device installation.

## Open and run

1. Clone the repository.
2. Generate the synthetic H.264/AAC MP4 and HLS fixtures:

   ```bash
   ./scripts/generate-test-media.sh
   ```

3. Add the repository through Unity Hub using Unity 2022.3.22f1.
4. Open `Assets/Scenes/IOSVideoBridgeTest.unity`.
5. Enter Play Mode.
6. Use **Bundled MP4** for the generated local sample, or paste an absolute HTTPS media URL/rooted local path.
7. Use **Export Diagnostics** to write a JSON Lines report under `Application.persistentDataPath/IOSVideoBridge/Diagnostics`.

The four generated binary media files are reproducible fixtures rather than source code. Their Unity `.meta` files, generation script, HLS playlist template, and expected test structure are tracked in the repository.

## Build an iOS Xcode project

From Unity, choose:

`iOS VideoBridge > Build iOS Xcode Project`

The project is written to `Builds/iOS`. The build tool adds the test scene to Build Settings and applies conservative iOS defaults. Generate the synthetic media first if you want the build validator to include the bundled-media test.

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
  StreamingAssets/IOSVideoBridge Synthetic-media metadata and generated fixture location
Packages/
  com.dazixbreed.ios-videobridge/
    Runtime/                      Player, state policy, recovery, analyzer, diagnostics, UI
    Editor/                       iOS build and project validation tools
    Tests/EditMode/               Deterministic edit-mode tests
    Samples~/                     Package sample documentation
docs/phase-0/                     Research and reproducibility documents
docs/phase-1/                     Harness design and operating guide
scripts/                          Test-media generation/server and repository checks
```

## Phase 1 scope

The Phase 1 backend wraps Unity's public `VideoPlayer` API. It supports:

- Direct MP4 and basic HLS URL preparation
- Explicit Idle/Loading/Preparing/Ready/Playing/Paused/Buffering/Recovering/Failed/Stopped states
- Play, pause, stop, seek, looping, speed, and volume
- API-only texture output drawn by the test harness
- Unity `AudioSource` routing with discovered/controlled track counts
- First-frame, error, state-transition, recovery, and lifecycle logging
- Application pause/resume recovery kept distinct from user pause commands
- Bounded stall detection and reload/resume recovery
- URL validation/redaction before diagnostics are written
- Reproducible local synthetic H.264/AAC media generation for repeatable tests

It does **not** claim to fix VRChat's installed iOS client. It creates independent evidence and reference behavior that can be compared with VRChat's own backends.

## Clean-room policy

See [`docs/phase-0/CLEAN_ROOM_BOUNDARY.md`](docs/phase-0/CLEAN_ROOM_BOUNDARY.md). Do not submit proprietary VRChat or AVPro code, decompiled material, authentication tokens, DRM-protected streams, or private URLs.

## License

MIT. See [`LICENSE`](LICENSE).
