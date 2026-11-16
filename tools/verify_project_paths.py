#!/usr/bin/env python3
"""Verify static res:// links and C# UID sidecars after folder reorganizing.

Run from any directory: python3 tools/verify_project_paths.py
"""
from __future__ import annotations

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
QUOTED_RESOURCE = re.compile(r'res://[^"\'\r\n]+')
FILE_SUFFIXES = {".tscn", ".cs", ".tres", ".ogg", ".wav", ".png",
                 ".jpg", ".jpeg", ".webp", ".svg", ".ttf", ".otf", ".mp3"}


def main() -> int:
    missing: set[tuple[str, str]] = set()
    checked = 0
    for pattern in ("*.cs", "*.tscn", "*.godot", "*.tres"):
        for source in ROOT.rglob(pattern):
            if ".godot" in source.parts:
                continue
            data = source.read_text(encoding="utf-8-sig", errors="replace")
            for match in QUOTED_RESOURCE.finditer(data):
                resource = match.group(0)
                # Ignore deliberate runtime-interpolated paths.
                if "{" in resource or "}" in resource:
                    continue
                if Path(resource).suffix.lower() not in FILE_SUFFIXES:
                    continue
                checked += 1
                # Legacy JSON save snapshots intentionally retain this old
                # path for matching in SaveLoadService.PrepareLoad.
                if (resource == "res://world.tscn" and
                        source.name == "SaveLoadService.cs"):
                    continue
                if not (ROOT / resource.removeprefix("res://")).exists():
                    missing.add((str(source.relative_to(ROOT)), resource))

    for scene in ROOT.rglob("*.tscn"):
        for script in re.finditer(r'\[ext_resource type="Script"[^\n]*\bpath="(res://[^"]+)"',
                                  scene.read_text(encoding="utf-8-sig")):
            uid_sidecar = ROOT / (script.group(1).removeprefix("res://") + ".uid")
            # A few new runtime scripts intentionally have no existing UID.
            if not uid_sidecar.exists() and " uid=" in script.group(0):
                missing.add((str(scene.relative_to(ROOT)), str(uid_sidecar.relative_to(ROOT))))

    print(f"Checked {checked} concrete resource paths across project source and scenes.")
    if missing:
        for source, path in sorted(missing):
            print("MISSING", source, "->", path)
        print(f"FAILED: {len(missing)} broken paths")
        return 1
    print("PASS: no missing resource links or referenced C# UID sidecars")
    return 0


if __name__ == "__main__":
    sys.exit(main())
