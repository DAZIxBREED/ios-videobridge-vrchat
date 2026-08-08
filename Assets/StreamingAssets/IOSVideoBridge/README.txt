iOS VideoBridge synthetic test media
Written by DAZIxBREED

known-good-h264-aac.mp4
  H.264 Baseline, AAC-LC stereo, 640x360, 30 fps, six seconds.

hls-vod/index.m3u8
  HLS VOD generated from the same synthetic source.

Use scripts/generate-test-media.sh to regenerate these files.
For physical iOS network tests, host them using trusted HTTPS.
