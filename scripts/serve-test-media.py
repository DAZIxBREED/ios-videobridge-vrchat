#!/usr/bin/env python3
"""Serve the generated test media for LAN/editor testing.

This is intentionally an HTTP development server. For an actual iOS device,
prefer trusted HTTPS so App Transport Security behavior is not mixed into a
codec/playback test.
"""

from __future__ import annotations

import argparse
import functools
import http.server
from pathlib import Path


class NoCacheHandler(http.server.SimpleHTTPRequestHandler):
    extensions_map = {
        **http.server.SimpleHTTPRequestHandler.extensions_map,
        ".m3u8": "application/vnd.apple.mpegurl",
        ".ts": "video/mp2t",
        ".mp4": "video/mp4",
    }

    def end_headers(self) -> None:
        self.send_header("Cache-Control", "no-store, no-cache, must-revalidate")
        self.send_header("Access-Control-Allow-Origin", "*")
        super().end_headers()


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--bind", default="127.0.0.1")
    parser.add_argument("--port", type=int, default=8080)
    args = parser.parse_args()

    root = Path(__file__).resolve().parents[1] / "Assets" / "StreamingAssets" / "IOSVideoBridge"
    handler = functools.partial(NoCacheHandler, directory=str(root))
    server = http.server.ThreadingHTTPServer((args.bind, args.port), handler)
    print(f"Serving {root} at http://{args.bind}:{args.port}/")
    print("MP4: /known-good-h264-aac.mp4")
    print("HLS: /hls-vod/index.m3u8")
    server.serve_forever()


if __name__ == "__main__":
    main()
