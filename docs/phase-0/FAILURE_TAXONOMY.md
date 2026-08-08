# Phase 0 — Failure Taxonomy

| Code | Category | Examples | Likely evidence |
|---|---|---|---|
| URL-001 | URL validation | malformed URL, unsupported scheme | analyzer report |
| NET-001 | Transport | DNS, timeout, TLS, redirect failure | HTTP/native error, timing |
| FMT-001 | Container/codec | unsupported container, profile, audio codec | MIME, manifest, player error |
| HLS-001 | Playlist | invalid master/media playlist, stale live playlist | playlist text, reload timing |
| HLS-002 | Live edge | starts too far behind, cannot return to edge | current time, seekable window |
| VID-001 | Decode | no frames, decoder reset, frame drops | first-frame time, frame count |
| VID-002 | Texture | flipped image, wrong aspect, color conversion | screenshot, shader/output mode |
| AUD-001 | Decode/output | video with no audio, wrong track | track count, first-audio estimate |
| AUD-002 | Route/session | Bluetooth/headphones/interruption failure | route event, lifecycle event |
| SYN-001 | A/V sync | audible drift, discontinuity | media time and wall clock |
| SYN-002 | Shared sync | owner/late-join drift | server state and observed time |
| LIFE-001 | Lifecycle | foreground resume failure, device lock | pause/focus events |
| REC-001 | Recovery | infinite retries, reload loses position | recovery attempt log |
| APP-001 | World/client transition | fails after switching world or reopening | sequence log |
| DIAG-001 | Diagnostics | error not surfaced or secret leaked | exported log review |

## Ownership labels

Every result should use one of:

- Reference-backend limitation
- VRChat client-side limitation
- World-side limitation
- Stream/provider limitation
- Unity limitation
- Apple-framework limitation
- AVPro/vendor limitation
- Environmental/network limitation
- Unknown or inconclusive
