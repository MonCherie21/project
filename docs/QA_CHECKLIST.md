# Stability / acceptance checklist

This is the review checklist before merging `feature/organized-wawa-polish` into `main`. Items marked `[x]` are checked in this workspace; items marked `[ ]` require a human session in **Godot 4.7.2 .NET**.

## Automated and structural

- [x] Organized all former root `.cs`/`.tscn` files under explicit `Scripts/` and `Scenes/` directories.
- [x] Preserved `.cs.uid` sidecars, added the existing collision generator's missing UID sidecar.
- [x] Updated static source/scene paths and autoload/startup entries.
- [x] `python3 tools/verify_project_paths.py` validates concrete project links.
- [x] `git diff --check` has no whitespace conflicts.
- [x] Opening OGG is valid and decodable (`ffprobe`).
- [x] Legacy save-scene string is accepted by `SaveLoadService.PrepareLoad`.
- [ ] Full C# build via Godot **.NET** editor / `dotnet build Yokai.csproj`.

## Narrative and presentation

- [ ] New Game -> character creation -> opening cutscene: no missing sprites, text overlaps or broken click targets.
- [ ] Whisper and candy moment reads warmly; heartwarming new OGG music is audible and volume slider works.
- [ ] Scene changes fade smoothly without black screens, input lockups or duplicate transitions.
- [ ] Walking animation has gentle, small 12-pose cycle for both genders, no funny arm raising or exaggerated bobbing.
- [ ] Building damage/debris effects remain visible, but do not cover interactable doors or prevent walking.
- [ ] Stage 7 displays the Orcanos title card, which may be skipped or closes on its own.
- [ ] First-time victories at stages 2–7 show companion welcome; a candy gift reduces inventory by exactly one and increments friendship.
- [ ] Choosing the free hug/welcome does **not** remove a candy.
- [ ] Replaying a completed battle does not grant a second companion or double-count progress.

## Progression and gameplay

- [ ] Winning stage 1 unlocks stage 2, and every subsequent stage requires its prior win.
- [ ] **Run** from a Yokai battle: no story advancement, no new companion, no cleared marker.
- [ ] Lose a battle: no level advancement, and retry works.
- [ ] Change active Yokai in Party; next battle uses the correct Yokai name and sprite (verify Cheeksqueek in particular).
- [ ] Candy pickups cannot be collected repeatedly after loading a save.
- [ ] Player does not walk through buildings, trees, water or hill boundaries; previous collision edits remain intact.
- [ ] All seven building triggers and escape buttons work with relocated scenes.

## Save and account regression

- [ ] New player profile and first checkpoint save successfully to Supabase.
- [ ] Continue opens an existing account, restores position, level, candy, friendship, and active Yokai.
- [ ] An older save that stores `res://world.tscn` loads after the migration.
- [ ] Three save slots remain isolated: swapping accounts never leaks party/progress between players.
- [ ] Pause, Settings, Collection, Party, Field Notes, manual Save/Load, and Exit all open/close without freezing input.
- [ ] Final epilogue returns to Wawa, stores final progress and restores town presentation.

## Known test limitations

The available headless executable is **Godot 4.6.3 without C# support**, whereas `project.godot` requests Godot 4.7 and the C# project uses the 4.7.2 .NET SDK. Running that non-.NET binary reports C# loader/autoload errors by design, so it is not a functional gameplay test. The team must perform the in-editor build and manual QA before merging.
