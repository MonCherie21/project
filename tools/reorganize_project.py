#!/usr/bin/env python3
"""One-time Godot 4 scene/script layout migration (run from project root).

Usage: python3 tools/reorganize_project.py --apply

Moves only the explicit files below, keeps Godot C# .uid sidecars beside their
scripts and updates resource paths in tracked project text. Never touches art,
player data, or generated .godot files. Keep this script as an audit trail.
"""
from __future__ import annotations

import argparse
import re
import shutil
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

SCRIPTS = {
    "Core": "SceneManager GameProgress GameSaveData CharacterData PlayerData YokaiRoster CandyInventory",
    "Audio": "MusicPlayer",
    "Battle": "BattleScene BattlePixelEffects BattleResultUI DisturbedBattleBackdrop EFeed1 BossTitleCard",
    "World": "World WorldCollisionGenerator DisturbedBuildingEffect Area2d SchoolArea2d PuregoldArea2d GasStationArea2d McdoArea2d HauntedHouseArea2d SariSariStoreArea2d CandyPickup ObjectiveClipboardIcon",
    "Characters": "Player CharacterCreation",
    "Story": "OpeningCutscene Ending Dialogue1 Dialogue2 Dialogue3 Dialogue4 Dialogue5 Dialogue6 Dialogue7 DialogueAdvance YokaiBondCutscene",
    "UI": "PartyInventory StoryNoticeUI PixelCloseIcon YokaiTheme YokaiRuneSeal MainMenu Continue Settings Escape SaveSlotPanel TutorialOverlay PauseButton BackButton Load CollectionManager",
    "Services": "SaveLoadService SupabaseSession SupabaseHttp SupabaseConfig",
    "Visual": "SilhouetteFactory",
}
SCENES = {
    "Battles": "BattleScene BattleScene2 BattleScene3 BattleScene4 BattleScene5 BattleScene6 BattleScene7 BattleResultUI EFeed1 EFeed2 EFeed3 EFeed4 EFeed5 EFeed6 EFeed7 EncounterA encounterOptions1 encounterOptions2 encounterOptions3 encounterOptions4 encounterOptions5 encounterOptions6 encounterOptions7",
    "Story": "Dialogue1 Dialogue2 Dialogue3 Dialogue4 Dialogue5 Dialogue6 Dialogue7 OpeningCutscene Ending",
    "Menus": "main_menu character_creation Continue Collection Settings Escape Pause Load PauseButton PartyInventory",
    "World": "world player player_2 CandyPickup",
    "Systems": "MusicPlayer",
}


def manifest() -> dict[str, str]:
    moves: dict[str, str] = {}
    for folder, names in SCRIPTS.items():
        for name in names.split():
            old = f"{name}.cs"
            if old in moves:
                raise ValueError("Script listed twice: " + old)
            moves[old] = f"Scripts/{folder}/{old}"
            if (ROOT / f"{old}.uid").exists():
                moves[f"{old}.uid"] = f"Scripts/{folder}/{old}.uid"
    for folder, names in SCENES.items():
        for name in names.split():
            old = f"{name}.tscn"
            if old in moves:
                raise ValueError("Scene listed twice: " + old)
            moves[old] = f"Scenes/{folder}/{old}"
    return moves


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--apply", action="store_true", help="actually move files")
    args = parser.parse_args()
    moves = manifest()
    all_root = {p.name for pattern in ("*.cs", "*.tscn", "*.cs.uid")
                for p in ROOT.glob(pattern)}
    missing = sorted(all_root - set(moves))
    unknown = sorted(k for k in moves if not (ROOT / k).exists())
    if missing or unknown:
        raise SystemExit(f"Manifest incomplete! unmapped={missing}, missing={unknown}")
    print(f"{len(moves)} explicit files in {len(SCRIPTS)} script + "
          f"{len(SCENES)} scene categories")
    if not args.apply:
        print("Dry run OK. Add --apply to perform changes.")
        return

    for old, new in moves.items():
        source, target = ROOT / old, ROOT / new
        target.parent.mkdir(parents=True, exist_ok=True)
        is_tracked = subprocess.run(["git", "ls-files", "--error-unmatch", old],
                                    cwd=ROOT, stdout=subprocess.DEVNULL,
                                    stderr=subprocess.DEVNULL).returncode == 0
        if is_tracked:
            subprocess.run(["git", "mv", old, new], cwd=ROOT, check=True)
        else:
            shutil.move(source, target)

    # Replace whole, literal resource paths; other nested paths (Music/Assets)
    # are left untouched. Sorting longest first is defensive for shared names.
    replacements = [(f"res://{old}", f"res://{new}") for old, new in moves.items()
                    if old.endswith((".cs", ".tscn"))]
    replacements.sort(key=lambda pair: len(pair[0]), reverse=True)
    changed = []
    for source in ROOT.rglob("*"):
        if not source.is_file() or source.suffix not in {".tscn", ".cs", ".godot"}:
            continue
        if ".godot" in source.parts:
            continue
        original = source.read_text(encoding="utf-8")
        text = original
        for old, new in replacements:
            # Only absolute paths beginning at res://, where a filename ends.
            text = re.sub(re.escape(old) + r"(?=[\"'\s\)\],;]|$)", new, text)
        if text != original:
            source.write_text(text, encoding="utf-8")
            changed.append(str(source.relative_to(ROOT)))

    # Preserve compatibility with save slots serialized before reorganization.
    service = ROOT / "Scripts/Services/SaveLoadService.cs"
    content = service.read_text(encoding="utf-8")
    content = content.replace(
        'slot.Data.Scene != "res://Scenes/World/world.tscn" &&\n'
        '            slot.Data.Scene != "res://Scenes/World/world.tscn"',
        'slot.Data.Scene != "res://world.tscn" &&\n'
        '            slot.Data.Scene != "res://Scenes/World/world.tscn"')
    service.write_text(content, encoding="utf-8")
    print("Moved", len(moves), "files; updated", len(changed), "source/scene files")


if __name__ == "__main__":
    main()
