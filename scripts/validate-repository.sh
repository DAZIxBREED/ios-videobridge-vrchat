#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

required=(
  "README.md"
  "ROADMAP.md"
  "docs/PROJECT_SPECIFICATION.md"
  "docs/RELEASE_POLICY.md"
  "docs/phase-0/CLEAN_ROOM_BOUNDARY.md"
  "Assets/Scenes/IOSVideoBridgeTest.unity"
  "Assets/StreamingAssets/IOSVideoBridge/known-good-h264-aac.mp4"
  "Assets/StreamingAssets/IOSVideoBridge/hls-vod/index.m3u8"
  "Packages/com.dazixbreed.ios-videobridge/Runtime/IOSUnityVideoReferencePlayer.cs"
  "Packages/com.dazixbreed.ios-videobridge/Runtime/IOSVideoDiagnostics.cs"
  "Packages/com.dazixbreed.ios-videobridge/Runtime/VideoPlaybackState.cs"
  "Packages/com.dazixbreed.ios-videobridge/Runtime/VideoPlaybackStatePolicy.cs"
  "Packages/com.dazixbreed.ios-videobridge/Runtime/VideoSourceNormalizer.cs"
  "Packages/com.dazixbreed.ios-videobridge/Runtime/VideoSeekUtility.cs"
  "Packages/com.dazixbreed.ios-videobridge/Tests/EditMode/PlaybackStabilizationTests.cs"
)

for path in "${required[@]}"; do
  test -e "$path" || { echo "Missing: $path" >&2; exit 1; }
done

python3 - <<'PYCODE'
from pathlib import Path
source = Path('scripts/serve-test-media.py').read_text(encoding='utf-8')
compile(source, 'scripts/serve-test-media.py', 'exec')
print('Python syntax check passed.')
PYCODE

video_codec="$(ffprobe -v error -select_streams v:0 -show_entries stream=codec_name -of default=nw=1:nk=1 Assets/StreamingAssets/IOSVideoBridge/known-good-h264-aac.mp4)"
audio_codec="$(ffprobe -v error -select_streams a:0 -show_entries stream=codec_name -of default=nw=1:nk=1 Assets/StreamingAssets/IOSVideoBridge/known-good-h264-aac.mp4)"
pixel_format="$(ffprobe -v error -select_streams v:0 -show_entries stream=pix_fmt -of default=nw=1:nk=1 Assets/StreamingAssets/IOSVideoBridge/known-good-h264-aac.mp4)"

test "$video_codec" = "h264" || { echo "Expected h264, got $video_codec" >&2; exit 1; }
test "$audio_codec" = "aac" || { echo "Expected aac, got $audio_codec" >&2; exit 1; }
test "$pixel_format" = "yuv420p" || { echo "Expected yuv420p, got $pixel_format" >&2; exit 1; }
grep -q '#EXTM3U' Assets/StreamingAssets/IOSVideoBridge/hls-vod/index.m3u8
grep -q '#EXT-X-ENDLIST' Assets/StreamingAssets/IOSVideoBridge/hls-vod/index.m3u8

(
  cd Assets/StreamingAssets/IOSVideoBridge
  sha256sum -c SHA256SUMS
)

python3 - <<'PY'
from pathlib import Path
import json
import re

root = Path('Packages/com.dazixbreed.ios-videobridge')
for path in root.rglob('*.cs'):
    text = path.read_text(encoding='utf-8')
    balance = 0
    in_string = False
    escaped = False
    for char in text:
        if in_string:
            if escaped:
                escaped = False
            elif char == '\\':
                escaped = True
            elif char == '"':
                in_string = False
            continue
        if char == '"':
            in_string = True
        elif char == '{':
            balance += 1
        elif char == '}':
            balance -= 1
        if balance < 0:
            raise SystemExit(f'Brace underflow in {path}')
    if balance != 0:
        raise SystemExit(f'Brace imbalance ({balance}) in {path}')
print('C# structural checks passed.')

package = json.loads(Path('Packages/com.dazixbreed.ios-videobridge/package.json').read_text(encoding='utf-8'))
version_source = Path('Packages/com.dazixbreed.ios-videobridge/Runtime/IOSVideoBridgeVersion.cs').read_text(encoding='utf-8')
match = re.search(r'Value\s*=\s*"([^"]+)"', version_source)
if not match:
    raise SystemExit('Could not read IOSVideoBridgeVersion.Value')
runtime_version = match.group(1)
if package['version'] != runtime_version:
    raise SystemExit(f"Version mismatch: package={package['version']} runtime={runtime_version}")
print(f'Version consistency passed: {runtime_version}')

forbidden = (
    'VideoPlaybackState.Loaded',
    'VideoPlaybackState.Prepared',
    'VideoPlaybackState.Stalled',
    'VideoPlaybackState.Error',
    'VideoPlaybackState.Completed',
    'VideoPlaybackState.Released',
)
for path in root.rglob('*.cs'):
    text = path.read_text(encoding='utf-8')
    for symbol in forbidden:
        if symbol in text:
            raise SystemExit(f'Legacy playback state reference {symbol} remains in {path}')
print('Legacy playback-state scan passed.')
PY

echo "Repository validation passed. Unity compilation and runtime tests still require Unity 2022.3.22f1."
