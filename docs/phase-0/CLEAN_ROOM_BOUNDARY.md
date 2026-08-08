# Phase 0 — Clean-Room Boundary

## Purpose

This repository develops original compatibility and diagnostic code using public Unity and Apple APIs. It must remain independently implementable and safe to share with VRChat, Unity, Apple, RenderHeads, and the community.

## Allowed inputs

- Public Unity documentation and APIs
- Public Apple developer documentation and frameworks
- Public VRChat creator documentation
- Public AVPro product documentation and observed public behavior
- Original black-box tests using software the tester is authorized to run
- Original logs, screen recordings, test streams, and synthetic media
- Original code written for this project

## Prohibited inputs

- Decompiled or disassembled VRChat client or SDK code
- Modified VRChat client binaries
- Proprietary AVPro source code or redistributed paid package files
- Leaked source, symbols, credentials, or internal documentation
- Authentication bypasses, DRM circumvention, stream extraction, or token theft
- Native plugins injected into an uploaded VRChat world
- Private user data or unredacted signed URLs in public reports

## Behavioral comparison rules

Black-box comparison is limited to externally visible behavior: load result, state transition, output texture, audible output, timing, error event, and recovery result. Reports must distinguish direct observations from hypotheses.

Use language such as:

- **Observed:** playback entered an error state after 12.4 seconds.
- **Reference result:** the same media played in the standalone Unity harness.
- **Inference:** the failure may be between URL handoff and native audio routing.
- **Unknown:** no native VRChat error code is available.

Do not claim knowledge of proprietary implementation details without public evidence.

## Contribution attestation

By contributing, a contributor confirms that the submitted material is original or permissively licensed, contains no proprietary/decompiled implementation, and can be redistributed under the repository license.
