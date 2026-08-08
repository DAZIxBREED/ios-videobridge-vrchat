# Phase 0 — Known-Good Media

The repository contains synthetic media generated locally with FFmpeg. It contains no third-party copyrighted content.

## Files

- `Assets/StreamingAssets/IOSVideoBridge/known-good-h264-aac.mp4`
  - 640×360, 30 fps
  - H.264 video
  - AAC-LC stereo audio
  - five-second color/test pattern and tone
- `Assets/StreamingAssets/IOSVideoBridge/hls-vod/index.m3u8`
  - HLS VOD generated from the same source

Regenerate the files with:

```bash
./scripts/generate-test-media.sh
```

Validate codecs and checksums with:

```bash
./scripts/validate-repository.sh
```

For remote iOS tests, host the files over trusted HTTPS. Plain HTTP and untrusted self-signed certificates may be rejected by App Transport Security and should not be confused with a codec failure.
