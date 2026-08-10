# iOS VideoBridge for VRChat — Release Policy

**Maintainer:** DAZIxBREED  
**Status:** Normative  
**Companion document:** [`ROADMAP.md`](../ROADMAP.md)

This policy defines how releases are numbered, when a version may advance, and what evidence is required before a release is considered complete.

## Version model

The project uses semantic-style versioning with prerelease identifiers during active development.

### Patch releases

Examples: `0.1.1`, `0.1.2`, `0.3.1`

Use for:

- bug fixes
- diagnostics improvements
- deterministic tests
- documentation corrections
- bounded recovery improvements that do not introduce a new subsystem
- compatibility fixes within the current subsystem release

### Minor releases

Examples: `0.2.0`, `0.3.0`, `0.5.0`

Use when the roadmap introduces a new subsystem or major development milestone.

### Alpha releases

Format: `0.x.0-alpha` or `0.x.0-alpha.N`

Alpha means the subsystem is under active implementation and its public behavior may still change.

Roadmap alpha subsystem releases are:

- `v0.2.0-alpha` — Native Apple playback
- `v0.3.0-alpha` — Native texture bridge
- `v0.4.0-alpha` — AudioBridge
- `v0.5.0-alpha` — HLS/livestream engine
- `v0.6.0-alpha` — Recovery/lifecycle engine

### Beta releases

Format: `0.x.0-beta` or `0.x.0-beta.N`

Beta begins only after the native playback stack is complete enough for broad comparison testing.

Roadmap beta releases are:

- `v0.7.0-beta` — VRChat comparison framework
- `v0.8.0-beta` — Automated compatibility laboratory

### Release candidates

Format: `0.9.0-rc` or `0.9.0-rc.N`

Release candidates are reserved for vendor-submission-quality builds. RC builds should focus on reproducibility, packaging, regression fixes, and documentation rather than major new architecture.

### Stable release

`v1.0.0` is reserved for the documented stable reference implementation described in `ROADMAP.md`.

## Release-gate rule

Version advancement is gate-driven.

A release number may not be advanced merely because work has started on the next subsystem. The current release gate must be satisfied or the next work must remain on a feature branch/prerelease branch.

If a gate is intentionally waived or changed, the repository must contain:

1. an explicit Roadmap Amendment,
2. the technical rationale,
3. the resulting risk or limitation,
4. a matching `CHANGELOG.md` entry.

## Required release artifacts

Every tagged release must include or reference:

- release version
- implementation summary
- known limitations
- relevant test results
- documentation changes
- compatibility-impact notes
- migration notes when public APIs changed
- clean-room compliance confirmation when native/vendor comparison work is involved

## Tagging

Release tags must match the published version exactly.

Examples:

```text
v0.1.1
v0.2.0-alpha
v0.2.0-alpha.2
v0.7.0-beta.1
v0.9.0-rc.1
v1.0.0
```

Do not reuse a published tag for different code. If a release is wrong, publish the next patch or prerelease identifier.

## Main-branch rule

`main` is the newest usable project state.

Do not intentionally merge:

- uncompilable Runtime code
- known infinite recovery loops
- secret-bearing test URLs
- proprietary/decompiled vendor code
- experimental native changes that destroy the working Unity reference baseline

Experimental work belongs on short-lived branches until it meets the merge gate.

## Branch naming

Use:

```text
feature/<description>
fix/<description>
test/<description>
docs/<description>
release/<version>
```

The project does not use a permanent `develop` branch.

## Release checklist

Before tagging a release:

- repository validation passes
- relevant deterministic tests pass
- current release gate is satisfied
- no secrets or private URLs are present
- `CHANGELOG.md` is updated
- `README.md` current-status/version text is updated when appropriate
- known limitations are documented
- release tag is created from the intended `main` commit

## Roadmap amendment procedure

`ROADMAP.md` is intentionally difficult to change casually.

A material amendment must use a commit or pull request title beginning with:

```text
Roadmap Amendment:
```

The amendment must explain what is changing, why the original dependency or gate is no longer appropriate, and what new evidence justifies the change.

Normal implementation discoveries may refine tasks inside an existing release without constituting a roadmap amendment, provided the release purpose, dependency order, and completion gate remain intact.
