# Project Context

Agents must verify commands against repository configuration before running them.

## Overview

- Product: WordVenture (워드의 모험), a 2D turn-based battle game where the
  player combines magic cards and element cards. Team 6203's GIGDC 2024 entry.
- Primary users: players of the downloadable builds and the web build.
- Core domain: card combination, elemental matchups, stage progression, story
  and tutorial.
- Runtime environment: Unity 2022.3.34f1 (C#). Release builds target
  StandaloneWindows64, StandaloneOSX, and WebGL.

## Architecture

- Entry points: `Assets/Scenes/TitleScene.unity`. Build settings register
  TitleScene, StoryScene, MapScene, GameClearScene, GameOverScene,
  TurnBattleScene, and EndingScene.
- Main modules (`Assets/Scripts/`):
  - `Battle/`: turn progression
  - `Cards/`: card data and management
  - `Combat/`: spells, enemies, stages, battle UI
  - `Core/`: save and load, shared features
  - `Map/`: map movement
  - `Scenes/`: per-scene flow and title credits
  - `Story/`: story and dialogue
  - `Tutorial/`: tutorial progression and action guidance
- Dependency direction: TODO
- External systems: GitHub Actions with GameCI (`game-ci/unity-builder@v4`) in
  `.github/workflows/release.yml` and `.github/workflows/deploy.yml`; GitHub
  Pages hosts the WebGL build.
- Persistent data: game data and dialogue in ScriptableObjects
  (`Assets/ScriptableObjects/`); player progress in PlayerPrefs.

## Commands

| Purpose | Command |
|---|---|
| Install dependencies | Open the project in Unity Hub with Unity 2022.3.34f1; packages resolve on import |
| Run locally | Open `Assets/Scenes/TitleScene.unity` in the editor and press Play |
| Format | TODO |
| Lint | TODO |
| Type-check | Unity editor compilation |
| Unit tests | Unity Test Runner, EditMode (`Assets/Tests/EditMode`) |
| Integration tests | Unity Test Runner, PlayMode (`Assets/Tests/PlayMode`) |
| Build | File → Build Settings in the editor; CI builds via `.github/workflows/release.yml` on release tags |

## Constraints

- Supported platforms: Windows (StandaloneWindows64), macOS (StandaloneOSX,
  unsigned), WebGL.
- Compatibility requirements: keep the Unity version at 2022.3.34f1, matching
  `ProjectSettings/ProjectVersion.txt` and `UNITY_VERSION` in both workflows.
  Commit Unity `.meta` files together with every added or moved asset.
- Performance constraints: TODO
- Security or privacy requirements: TODO

## Ownership

- Maintainers: Team 6203 (see `README.md`).
- Sensitive modules: TODO
- Changes requiring explicit review: TODO
