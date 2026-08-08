#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUT="$ROOT/Assets/StreamingAssets/IOSVideoBridge"
HLS="$OUT/hls-vod"
mkdir -p "$HLS"
rm -f "$OUT/known-good-h264-aac.mp4" "$HLS"/*

ffmpeg -hide_banner -loglevel error -y \
  -f lavfi -i "testsrc2=size=640x360:rate=30:duration=6" \
  -f lavfi -i "sine=frequency=880:sample_rate=48000:duration=6" \
  -filter_complex "[0:v]drawtext=text='iOS VideoBridge - DAZIxBREED':x=(w-text_w)/2:y=24:fontsize=24:fontcolor=white:box=1:boxcolor=black@0.55,drawtext=text='%{pts\\:hms}':x=(w-text_w)/2:y=h-54:fontsize=24:fontcolor=white:box=1:boxcolor=black@0.55[v]" \
  -map "[v]" -map 1:a \
  -c:v libx264 -profile:v baseline -level 3.0 -pix_fmt yuv420p -preset veryfast -crf 20 \
  -g 60 -keyint_min 60 -sc_threshold 0 \
  -c:a aac -profile:a aac_low -b:a 128k -ac 2 \
  -movflags +faststart -shortest \
  "$OUT/known-good-h264-aac.mp4"

ffmpeg -hide_banner -loglevel error -y \
  -i "$OUT/known-good-h264-aac.mp4" \
  -c copy -hls_time 2 -hls_playlist_type vod \
  -hls_segment_filename "$HLS/segment-%03d.ts" \
  "$HLS/index.m3u8"

(
  cd "$OUT"
  sha256sum known-good-h264-aac.mp4 hls-vod/index.m3u8 hls-vod/segment-*.ts > SHA256SUMS
)

echo "Generated test media in $OUT"
