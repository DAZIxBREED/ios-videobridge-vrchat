# Phase 0 — Diagnostic Logging Schema

Phase 1 writes one JSON object per line. JSONL keeps logs appendable and resilient to an interrupted run.

## Common fields

| Field | Type | Meaning |
|---|---|---|
| timestampUtc | string | ISO-8601 UTC timestamp |
| sessionId | string | Random identifier for a test run |
| sequence | integer | Monotonic event number |
| category | string | lifecycle, playback, network, recovery, diagnostic |
| eventName | string | Stable event identifier |
| severity | string | trace, info, warning, error |
| message | string | Human-readable summary |
| sanitizedUrl | string | Redacted media URL, when applicable |
| mediaTimeSeconds | number | Current player time, when available |
| durationSeconds | number | Duration, when finite |
| frame | integer | Current decoded frame, when available |
| frameRate | number | Reported frame rate |
| width | integer | Reported media width |
| height | integer | Reported media height |
| detailsJson | string | JSON-encoded event-specific details |
| unityVersion | string | Unity runtime version |
| platform | string | Unity runtime platform |
| operatingSystem | string | OS string reported by Unity |
| deviceModel | string | Device model reported by Unity |
| applicationVersion | string | Player application version |
| bridgeVersion | string | iOS VideoBridge version |

## Required state events

- session_started
- url_analyzed
- load_requested
- prepare_started
- prepared
- playback_started
- playback_paused
- playback_stopped
- playback_completed
- first_frame
- frame_dropped
- seek_requested
- seek_completed
- application_paused
- application_resumed
- stall_suspected
- recovery_attempt
- playback_recovered
- playback_error
- diagnostics_exported

## Redaction

The logger removes user info and redacts query parameters whose names contain common secret markers such as token, signature, key, auth, cookie, credential, and policy. Reporters must still manually inspect logs before publishing.
