#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY="${UNITY:-}"

if [[ -z "$UNITY" ]]; then
  echo "Set UNITY to the Unity 2022.3.22f1 executable path." >&2
  exit 2
fi

mkdir -p "$ROOT/TestResults"
"$UNITY" -batchmode -quit \
  -projectPath "$ROOT" \
  -runTests -testPlatform EditMode \
  -testResults "$ROOT/TestResults/editmode.xml" \
  -logFile "$ROOT/TestResults/unity-editmode.log"
