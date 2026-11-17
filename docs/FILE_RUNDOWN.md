# File-by-file project rundown

This inventory covers all **C# behavior scripts and Godot scene files** that were in the project root before the folder migration, plus the new polish features. `*.cs.uid` sidecars keep the same names and live next to their scripts. Existing art, audio, fonts, textures, and third-party assets remain in their original asset folders.

## `Scripts/Core` — shared game state

| File | Responsibility |
| --- | --- |
| `SceneManager.cs` | Persistent scene fading, back navigation, pause overlay, settings overlay. |
| `GameProgress.cs` | Seven-stage route, required next building, victory-only advancement, checkpoint flags. |
| `GameSaveData.cs` | Versioned JSON snapshot of location, progress, candy, friendship, active partner and pickups. |
| `CharacterData.cs` | Character data definitions shared across game logic. |
| `PlayerData.cs` | Current profile, gender, tokens, location and local account-slot selection. |
| `YokaiRoster.cs` | Fixed names, portraits, battle sprites, skills and damage values for seven Yokai. |
| `CandyInventory.cs` | Candy count, pickups, selected partner, friendship feeds and change notifications. |

## `Scripts/Audio`

| File | Responsibility |
| --- | --- |
| `MusicPlayer.cs` | Persistent audio player: town, battle, Whisper opening score, fades and saved volume setting. |

## `Scripts/Battle`

| File | Responsibility |
| --- | --- |
| `BattleScene.cs` | Core encounter loop: stage selection, HP, skills, results, win/run/lose handling and companion flow. |
| `BattlePixelEffects.cs` | Programmatic battle pixel effects and hit feedback. |
| `BattleResultUI.cs` | Battle result panel and continue button. |
| `DisturbedBattleBackdrop.cs` | Dynamic atmosphere/effects on disturbed encounter backgrounds. |
| `EFeed1.cs` | Shared feeding/interaction behavior used across seven feeding scenes. |
| `BossTitleCard.cs` | New dramatic but skippable Orcanos reveal before stage 7. |

## `Scripts/Characters`

| File | Responsibility |
| --- | --- |
| `CharacterCreation.cs` | Player setup and creation flow; starts the opening after saving a profile. |
| `Player.cs` | Male/female top-down movement, collision, direction and 12 subtle stepped walk poses. |

## `Scripts/Story`

| File | Responsibility |
| --- | --- |
| `OpeningCutscene.cs` | Whisper's introduction, first candy, family promise and Wawa invasion cinematic. |
| `Ending.cs` | Post-stage-7 epilogue and return to restored Wawa. |
| `Dialogue1.cs` | Stage-1 encounter introduction; single-confirm transition. |
| `Dialogue2.cs` | Stage-2 introduction; delegates confirm handling to DialogueAdvance. |
| `Dialogue3.cs` | Stage-3 introduction; delegates confirm handling to DialogueAdvance. |
| `Dialogue4.cs` | Stage-4 introduction; delegates confirm handling to DialogueAdvance. |
| `Dialogue5.cs` | Stage-5 introduction; delegates confirm handling to DialogueAdvance. |
| `Dialogue6.cs` | Stage-6 introduction; delegates confirm handling to DialogueAdvance. |
| `Dialogue7.cs` | Stage-7 introduction; delegates confirm handling to DialogueAdvance. |
| `DialogueAdvance.cs` | Shared single-press dialogue confirm; prevents repeated battle transitions. |
| `YokaiBondCutscene.cs` | New first-win bonding moment; optional real candy or free welcome/hug. |

## `Scripts/World`

| File | Responsibility |
| --- | --- |
| `World.cs` | Wawa initialization, player spawn, HUD, party/journal controls, decorations and autosave. |
| `WorldCollisionGenerator.cs` | Programmatic blocking geometry for the existing town building layout. |
| `DisturbedBuildingEffect.cs` | Visual disturbance effects around buildings until the story restores them. |
| `Area2d.cs` | House 2 encounter entry trigger (stage 1). |
| `SariSariStoreArea2d.cs` | Sari-Sari Store entry trigger (stage 2). |
| `GasStationArea2d.cs` | Gas Station entry trigger (stage 3). |
| `PuregoldArea2d.cs` | Puregold entry trigger (stage 4). |
| `McdoArea2d.cs` | McDonald's entry trigger (stage 5). |
| `SchoolArea2d.cs` | School entry trigger (stage 6). |
| `HauntedHouseArea2d.cs` | Haunted House entry trigger (stage 7). |
| `CandyPickup.cs` | World pickup collection and prevention of collecting the same candy twice. |
| `ObjectiveClipboardIcon.cs` | Draws the in-world journal/clipboard icon. |

## `Scripts/UI`

| File | Responsibility |
| --- | --- |
| `MainMenu.cs` | New Game, Continue, Collections, settings and Exit handling. |
| `Continue.cs` | Local player account selection and restoration of latest cloud save. |
| `CollectionManager.cs` | Collection-screen entries and locked/unlocked Yokai previews. |
| `Settings.cs` | Settings overlay including music controls. |
| `Escape.cs` | Pause/back navigation behavior. |
| `PauseButton.cs` | Buttons that open pause functionality from relevant scenes. |
| `BackButton.cs` | Common menu navigation. |
| `Load.cs` | Load-game screen and save-panel opening. |
| `SaveSlotPanel.cs` | Modal UI for three cloud save slots, save/load confirmation and status. |
| `PartyInventory.cs` | Active companion selection and candy/level/friendship displays. |
| `StoryNoticeUI.cs` | Styled progress notice, locked-area message and Field Notes journal. |
| `TutorialOverlay.cs` | Confirmations, short tutorials and guidance panels. |
| `PixelCloseIcon.cs` | Consistent pixel close X used across themed UI. |
| `YokaiTheme.cs` | Dark palette, borders, typography and button/panel styling. |
| `YokaiRuneSeal.cs` | Runtime decorative rune graphic. |

## `Scripts/Services` and `Scripts/Visual`

| File | Responsibility |
| --- | --- |
| `SaveLoadService.cs` | Cloud checkpoint listing, three-slot save/load/delete and old-save compatibility. |
| `SupabaseSession.cs` | Authentication/session restoration for the selected account. |
| `SupabaseHttp.cs` | HTTP request/response support for Supabase calls. |
| `SupabaseConfig.cs` | Supabase endpoint and client configuration. |
| `SilhouetteFactory.cs` | Runtime silhouette preparation for character/collection visuals. |

## `Scenes/Battles` — interaction layouts

| File(s) | Purpose |
| --- | --- |
| `BattleScene.tscn` | Stage-1 battle layout. |
| `BattleScene2.tscn`–`BattleScene7.tscn` | Stage-specific battle layouts reusing BattleScene.cs. |
| `BattleResultUI.tscn` | Reusable victory/defeat overlay. |
| `EFeed1.tscn`–`EFeed7.tscn` | Seven Yokai feeding scenes. |
| `EncounterA.tscn` | Encounter interaction scene. |
| `encounterOptions1.tscn`–`encounterOptions7.tscn` | Stage-specific encounter option layouts. |

## `Scenes/Story` — narrative layouts

| File(s) | Purpose |
| --- | --- |
| `OpeningCutscene.tscn` | Runtime-generated opening cinematic root. |
| `Dialogue1.tscn`–`Dialogue7.tscn` | Seven location dialogue screens and their image assets. |
| `Ending.tscn` | Ending scene. |

## `Scenes/World`, `Scenes/Menus` and `Scenes/Systems`

| File | Purpose |
| --- | --- |
| `Scenes/World/world.tscn` | Main Wawa map with existing buildings, collision bodies and encounter areas. |
| `Scenes/World/player.tscn` | Female protagonist scene. |
| `Scenes/World/player_2.tscn` | Male protagonist scene. |
| `Scenes/World/CandyPickup.tscn` | Reusable candy pickup scene. |
| `Scenes/Menus/main_menu.tscn` | Entry scene; main menu artwork/buttons. |
| `Scenes/Menus/character_creation.tscn` | Character-creation form. |
| `Scenes/Menus/Continue.tscn` | Account Continue UI. |
| `Scenes/Menus/Collection.tscn` | Yokai collection UI. |
| `Scenes/Menus/Settings.tscn` | Settings UI. |
| `Scenes/Menus/Escape.tscn` | Pause/escape menu. |
| `Scenes/Menus/Pause.tscn` | Pause layout. |
| `Scenes/Menus/Load.tscn` | Save-load layout. |
| `Scenes/Menus/PauseButton.tscn` | Reusable pause control. |
| `Scenes/Menus/PartyInventory.tscn` | Party/feeding layout. |
| `Scenes/Systems/MusicPlayer.tscn` | Autoloaded shared music player. |

## Supporting files and assets

| Path | Purpose |
| --- | --- |
| `project.godot` | Godot project configuration, input map, autoloads and main scene path. |
| `Yokai.csproj` | Godot C# project for .NET 8. |
| `Characters/` | Companion portrait art shown in the Party and Collection panels. |
| `Sprites/` | Player sprites, enemy battle sprites and move/skill art. |
| `Assets/` | World/map textures, menu graphics, fonts, dialogue art and other imported assets. |
| `Music/` | World/battle themes, SFX and new `WhisperPromise.ogg` opening theme. |
| `docs/` | Architecture, gameplay flowchart, test plan, this rundown and PDF. |

## Editor-owned / generated material

The `.godot/`, `bin/`, and `obj/` trees (if present locally) are generated by Godot/.NET. They are **not** the project source of truth and should not be committed. Keep `.cs.uid` files alongside source to preserve stable scene links.

## What calls what?

The entrypoint `main_menu.tscn` uses `MainMenu.cs`; it calls CharacterCreation/Continue. After the intro, `World.cs` instantiates one of the two player scenes and shows the Party/Field Notes HUD. Each building trigger calls `GameProgress.EnterBuilding`, which sends the player through a `DialogueN` scene into a `BattleSceneN` instance. On win, `BattleScene` asks `GameProgress.CompleteLevel` to advance, shows a result screen, and optionally displays `YokaiBondCutscene`. `World` and `SaveLoadService` then persist the new stage via `GameSaveData`. Every scene can use `SceneManager` for fade transitions; `MusicPlayer` listens for scene changes and handles its soundtrack.
