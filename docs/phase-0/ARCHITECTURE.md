# Phase 0 — Architecture Baseline

## Layer model

```text
IOSVideoTestHarness
        |
        v
IIOSVideoReferencePlayer
        |
        +-- IOSUnityVideoReferencePlayer        (Phase 1)
        +-- IOSAVProReferencePlayer             (future independent model)
        +-- IOSVideoNativeBridge                (Phase 2+)

Cross-cutting services:
- IOSVideoDiagnostics
- IOSVideoCompatibilityAnalyzer
- IOSVideoRecoveryController
- SensitiveUrlRedactor
```

## Phase 1 data flow

```text
URL input
  -> compatibility analyzer
  -> sanitized diagnostic event
  -> Unity VideoPlayer preparation
  -> API-only texture output
  -> OnGUI display
  -> Unity AudioSource
  -> state/progress monitoring
  -> bounded recovery when progress stalls
  -> JSONL diagnostic export
```

## Design constraints

- Runtime code is compatible with Unity 2022.3 LTS and IL2CPP.
- Runtime assemblies do not depend on `UnityEditor`.
- Public-facing diagnostics redact common secret-bearing URL fields.
- Recovery is bounded and observable.
- Phase 1 never claims native AVFoundation behavior beyond what Unity exposes.
- VRChat comparison code remains separate from the standalone native/reference application.

## Future native seam

`IIOSVideoReferencePlayer` is the behavior seam for a Phase 2 native backend. The native implementation can provide the same lifecycle and telemetry contract without forcing the test harness to be rewritten.
