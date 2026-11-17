# Yokai Wave — The Wawa Chronicles

A story-driven, top-down 2D Godot RPG about an exorcist family protecting Wawa through friendship with Yokai. The player bonds with Whisper, shares candy to strengthen companions, and confronts the disturbances affecting seven locations around town. Story, battles, and save progression are intertwined.

## Requirements

- Godot **4.7.2 .NET** editor (C# support) and its compatible **.NET 8 SDK**.
- Access to the project team's configured Supabase database/authentication for player registration and cloud save slots. Do not commit private credentials.
- Open `project.godot` from Godot. Allow the editor to import files and build the C# solution before pressing Play.

## Starting a game

1. From the main menu select **New Game** and create a player.
2. The opening story explains why Wawa is in danger. Meet **Whisper**, offer the first candy, and learn the exorcist family's promise to protect Yokai rather than fear them.
3. In Wawa, use **WASD / arrow keys** to move, the **clipboard** for objectives, and **I** for the Party inventory. Choose an active Yokai and feed candies to grow friendship.
4. Follow the white flag to the current location. Win the battle to clear a district; **running away does not clear it or unlock its Yokai**.
5. After first-time victories from stage 2 onward, a new Yokai joins the team and a bonding moment offers a free hug/welcome or the choice to spend a real candy.
6. After stage 7, view the ending and return to a restored Wawa.

## Project map

| Folder | Contents |
| --- | --- |
| `Scripts/Core` | Game progress, save model, player profile, companion catalog and candy/friendship state |
| `Scripts/Audio` | Shared music player and volume preferences |
| `Scripts/Battle` | Battle loop, results, effects, final boss card, and friendship feed logic |
| `Scripts/Characters` | Character creation and overworld character movement/animation |
| `Scripts/Story` | Opening/ending, encounter dialogues and post-battle bonding cutscene |
| `Scripts/World` | Town scene behavior, entrance triggers, candy pickups and collision generation |
| `Scripts/UI` | Menu, party, inventory, pause, settings, progress and tutorial overlays |
| `Scripts/Services` | Supabase HTTP/session and three-slot cloud saves |
| `Scripts/Visual` | Runtime silhouette generation |
| `Scenes/Battles` | Seven story-battle scenes and battle-related layouts |
| `Scenes/Story` | Opening/ending and seven dialogue scenes |
| `Scenes/World` | Town, player character variants and candy pickups |
| `Scenes/Menus` | Character creation and menu layouts |
| `Scenes/Systems` | Persistent music scene |
| `Assets`, `Sprites`, `Characters`, `Music` | Existing visual and audio resources; `Music/WhisperPromise.ogg` is the new original opening piece |
| `docs` | Architecture, flowchart, QA checklist, complete file rundown and PDF |
| `tools` | Original opening-music generator, migration script, and link verifier |

## Maintenance

After moving or adding scenes/scripts, run:

```bash
python3 tools/verify_project_paths.py
git diff --check
```

`tools/reorganize_project.py` records the one-time root-folder migration and is **not** meant for repeated use on an already organized tree. The moved `.cs.uid` sidecars preserve Godot's script identities. The save loader accepts both old `res://world.tscn` checkpoints and the new `res://Scenes/World/world.tscn` path.

## Documentation

- [Game flowchart](docs/GAME_FLOW.md)
- [Code architecture](docs/ARCHITECTURE.md)
- [File-by-file rundown](docs/FILE_RUNDOWN.md)
- [Manual QA plan](docs/QA_CHECKLIST.md)
- [Illustrated project rundown](docs/Yokai_Wave_Project_Rundown.pdf)

**Integration note:** Source-path checks pass, but full in-editor C# compilation and gameplay testing must be done with the .NET-enabled Godot editor, which is not present in the current build environment. Do not merge this branch before that check.
