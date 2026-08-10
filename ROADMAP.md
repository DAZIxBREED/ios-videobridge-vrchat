# iOS VideoBridge for VRChat — Canonical Development Roadmap

**Author and maintainer:** DAZIxBREED  
**Status:** Normative development contract  
**Applies to:** `main`, all feature/fix/test branches, prereleases, public releases, vendor-submission builds

> This roadmap is the canonical development sequence for iOS VideoBridge for VRChat. Release ordering, subsystem dependencies, and release gates defined here are intentional. They must not be silently skipped, renumbered, or collapsed. Any material change requires an explicit **Roadmap Amendment** commit or pull request plus a matching `CHANGELOG.md` entry explaining why the contract changed.

## Governing principles

1. `main` must represent the newest usable project state.
2. Development proceeds by release gate, not by feature excitement.
3. A release gate is not complete until its required behavior is implemented, documented, and reproducible.
4. Native Apple playback, texture transfer, audio, HLS, recovery, VRChat comparison, automation, and submission tooling are separate engineering milestones.
5. No subsystem may depend on reverse-engineered or redistributed proprietary VRChat or AVPro implementation code.
6. Recovery must always be bounded; no infinite retry loops.
7. Diagnostics must never log secrets, authentication tokens, cookies, signed query strings, or private media URLs.
8. Claims about VRChat failures must be backed by reproducible comparison evidence.

---

# Release train

## v0.1.x — Phase 1 stabilization

**Purpose:** Make the current Unity `VideoPlayer` reference harness dependable enough to serve as the control implementation for all later A/B testing.

### Planned patch releases

- **v0.1.1 — Harness/runtime fixes**
- **v0.1.2 — Diagnostics and compatibility-analyzer hardening**
- **v0.1.3 — Explicit playback state machine and recovery stabilization**
- **v0.1.4 — iOS baseline validation and Phase 1 API freeze**

### Required programming

- Formalize playback states: Idle, Loading, Preparing, Ready, Playing, Paused, Buffering, Recovering, Failed, Stopped.
- Normalize URL handling and rejection paths.
- Harden preparation timeout behavior.
- Harden repeated load/unload cycles.
- Test seek edge cases and looping.
- Improve audio-track visibility.
- Stabilize lifecycle pause/resume behavior.
- Add deterministic edit-mode tests and appropriate play-mode/device checks.
- Freeze the common reference-player contract before native backend work begins.

### Release gate

`v0.1.4` is complete only when the Unity reference backend is stable enough to function as a trustworthy comparison control and the player interface is frozen for Phase 2.

---

## v0.2.0-alpha — Native Apple playback foundation

**Purpose:** Introduce the first real iOS-native playback backend using public Apple frameworks.

### Architecture target

```text
IIOSVideoReferencePlayer
        ↑
        ├── IOSUnityVideoReferencePlayer
        │
        └── IOSNativeVideoReferencePlayer
                       │
                       ↓
               IOSVideoNativeBridge
                       │
                       ↓
                DAZIPlayerController
                       │
                       ↓
                    AVPlayer
```

### Required native technologies

- AVPlayer
- AVPlayerItem
- AVURLAsset
- AVAudioSession
- NSNotificationCenter
- KVO/player state observation

### Required bridge API

- Initialize
- LoadUrl
- Prepare
- Play
- Pause
- Stop
- Seek
- SetVolume
- SetPlaybackRate
- GetCurrentTime
- GetDuration
- GetBufferedTime
- GetPlaybackState
- GetLastError
- Release

### Required callbacks

- OnPrepared
- OnPlaybackStarted
- OnPlaybackPaused
- OnPlaybackStalled
- OnPlaybackRecovered
- OnPlaybackCompleted
- OnStreamVariantChanged
- OnAudioRouteChanged
- OnNativeError

### Release gate

Direct HTTPS H.264/AAC MP4 playback must prepare, play, pause, seek, stop, report state, report errors, and release cleanly through the native backend on an actual Apple-device build.

---

## v0.3.0-alpha — Native video texture bridge

**Purpose:** Move Apple-decoded video frames into Unity correctly and reproducibly.

### Required programming

- AVPlayerItemVideoOutput
- CVPixelBuffer acquisition
- CoreVideo/Metal texture transfer
- Unity texture handoff
- Frame timestamp handling
- Dropped/late-frame policy
- Texture lifetime management
- Orientation handling
- Aspect-ratio correctness
- NV12 handling
- BGRA fallback
- Full-range versus video-range handling
- BT.709 baseline color behavior
- Initial HDR metadata observation hooks without claiming full HDR support

### Required explicit data model

- VideoOrientation
- VideoColorSpace
- VideoPixelFormat
- VideoRange

### Release gate

Native-decoded frames must reach Unity with correct orientation, timing, aspect ratio, and baseline color behavior without relying on undocumented platform assumptions.

---

## v0.4.0-alpha — AudioBridge

**Purpose:** Establish reliable iOS audio playback and a research path for Unity/world integration.

### Path A — Native reliability baseline

```text
AVPlayer → AVAudioSession → iOS audio output
```

### Path B — Experimental Unity path

```text
Native decoded PCM
      ↓
Ring buffer
      ↓
IOSVideoAudioBridge
      ↓
Unity AudioSource
```

### Required programming

- AVAudioSession setup and restoration
- Device route observation
- Headphone/Bluetooth route changes
- Volume and mute behavior
- Native audio/video synchronization observations
- PCM ring-buffer prototype
- Unity AudioSource feed prototype
- Latency, underrun, and performance diagnostics
- Documentation of AudioLink/spatial-audio implications

### Release gate

Native audio output must be reliable and documented. The Unity PCM path must be explicitly classified as working, partial, experimental, or rejected based on measured behavior.

---

## v0.5.0-alpha — HLS and livestream engine

**Purpose:** Make HLS VOD and live-stream behavior a first-class test target.

### Required programming

- HLS VOD
- HLS live detection
- Master playlist handling
- Variant-playlist behavior
- Adaptive-bitrate transition observations
- Live-window detection
- Live-edge positioning
- Playlist refresh behavior
- Presentation-size changes
- Stream-duration semantics

### Required runtime observability

- IsLive
- LiveEdgeSeconds
- BufferedSeconds
- CurrentVariant
- ObservedBitrate
- PresentationSize
- LikelyToKeepUp
- BufferEmpty
- BufferFull

### Release gate

The same known-good HLS media can be tested through the native reference backend and compared meaningfully against VRChat Unity/AVPro backends, with live-edge and variant behavior observable in diagnostics.

---

## v0.6.0-alpha — Recovery and lifecycle engine

**Purpose:** Convert playback recovery from ad-hoc reactions into a typed, bounded policy engine.

### Required recovery reasons

- NetworkTimeout
- BufferUnderrun
- DecoderFailure
- StreamExpired
- PlaylistFailure
- ApplicationResume
- AudioRouteChange
- LiveEdgeLost
- Unknown

### Ordered recovery ladder

1. Resume current item.
2. Reassert current timestamp when appropriate.
3. Seek to live edge when appropriate.
4. Replace AVPlayerItem.
5. Reload asset.
6. Stop with a terminal typed failure.

Every recovery attempt must be counted and logged.

### Required lifecycle coverage

- background/foreground
- device lock/unlock
- audio-route changes
- network interruption/reconnection
- decoder interruption
- world/app reopen style cycles in the standalone harness

### Release gate

Recovery behavior must be bounded, typed, logged, and reproducible across the supported recovery scenarios. No infinite retries are permitted.

---

## v0.7.0-beta — VRChat comparison framework

**Purpose:** Turn the project into an instrumented comparison environment rather than only a standalone player.

### VRChat comparison world must expose

- Test ID
- URL/media fixture ID
- Platform
- Backend
- Playback state
- Current time
- Duration
- Audio state
- Last error
- Synchronization error
- Expected orientation
- Observed orientation

### Comparison target

```text
Same media fixture
       │
       ├── VRChat Unity backend
       ├── VRChat AVPro backend
       └── iOS VideoBridge native reference backend
```

### Release gate

At least one reproducible comparison exists for direct MP4 and one for HLS, using the same test identifiers and media definitions across reference and VRChat test environments.

---

## v0.8.0-beta — Automated compatibility laboratory

**Purpose:** Make compatibility results repeatable and machine-readable.

### Required programming

- Machine-readable test definitions
- Machine-readable test results
- Repeatable playback timing capture
- Prepare-time measurement
- First-frame measurement
- First-audio measurement when measurable
- Dropped-frame capture
- Recovery-attempt capture
- Device/platform metadata
- Regression comparison tooling
- Compatibility-matrix generation

### Canonical result layout

```text
results/
├── raw/
├── devices/
├── streams/
├── regressions/
└── compatibility-matrix.json
```

### Release gate

A fresh run can produce structured results that can be compared with an earlier run to identify regressions without manually reading every log file.

---

## v0.9.0-rc — Vendor submission candidate

**Purpose:** Turn confirmed findings into professional, reproducible engineering submissions.

### Canonical submission package

```text
submissions/
└── <ISSUE-ID>/
    ├── README.md
    ├── reproduction.md
    ├── expected.md
    ├── observed.md
    ├── logs/
    ├── media/
    ├── recordings/
    └── reference-code/
```

### Finding confidence levels

- Confirmed
- Strongly indicated
- Suspected
- Inconclusive

### Release gate

At least one vendor-quality reproduction package must be independently repeatable and must distinguish evidence from inference. No speculative claim may be labeled confirmed.

---

## v1.0.0 — Stable public reference release

**Purpose:** Publish a stable, documented, reproducible iOS video compatibility reference implementation and test framework.

### v1.0.0 completion criteria

- Direct H.264/AAC MP4 playback works through the native backend.
- Supported HLS VOD and live playback work through the native backend.
- Video orientation, timing, aspect ratio, and baseline color behavior are correct.
- Native audio is reliable and the Unity PCM experiment is documented with measured limitations.
- Application lifecycle and route changes recover according to bounded policy.
- Network/playback stalls have typed recovery outcomes.
- Diagnostics make failures actionable without leaking secrets.
- Automated compatibility tests and machine-readable results exist.
- VRChat comparison tests exist for both VRChat video backends.
- Vendor-reproduction packages can be generated from confirmed findings.
- Another engineer can reproduce the published results from documentation and redistributable fixtures.

---

# Fixed programming dependency order

The following order is intentional and must not be casually reordered:

```text
Phase 1 stabilization
        ↓
Stable common player interface
        ↓
AVPlayer control/state bridge
        ↓
Video frame extraction
        ↓
Texture bridge
        ↓
Native audio baseline
        ↓
Unity PCM experiment
        ↓
HLS/live streaming
        ↓
Recovery/lifecycle engine
        ↓
VRChat comparisons
        ↓
Automated compatibility lab
        ↓
Vendor submission tooling
        ↓
1.0 release
```

The reason is practical: debugging AVPlayer, CoreVideo, Metal, AVAudioSession, HLS, lifecycle recovery, and VRChat synchronization simultaneously would destroy fault isolation.

---

# Branching contract

This project does **not** use a permanent `develop` branch.

Use short-lived branches from `main`:

```text
feature/<description>
fix/<description>
test/<description>
docs/<description>
release/<version>
```

Examples:

```text
feature/native-avplayer
feature/video-texture
feature/audio-pcm
feature/hls-live
fix/ios-resume
test/hls-reconnect
```

`main` must remain usable and must not intentionally contain knowingly broken intermediate work.

---

# Immediate locked next milestone

The next development target is:

**v0.1.1 — Phase 1 stabilization / harness-runtime fixes**

The native AVFoundation backend does **not** begin until the v0.1.x state contract is stable enough to serve as the comparison control.
