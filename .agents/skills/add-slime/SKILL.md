---
name: add-slime
description: >
  Procedure for adding a new slime enemy or changing an existing one in
  WordVenture. Use when the user asks to add a slime, add an enemy, add a boss,
  add an enemy to a wave or stage, change an enemy's stats, element, or attack
  behavior, or says 슬라임 추가, 적 추가, 보스 추가, 새 몬스터, 웨이브에 적 추가.
  Covers the enemy class, prefab, EnemyDataContainer entry, wave data, and the
  slime encyclopedia (도감) name, attack description, and English translation,
  which must be updated in the same change.
---

# Add Slime

Every slime that can appear in battle must also appear correctly in the slime
encyclopedia (`SlimeEncyclopediaView`, opened from the map with the
`슬라임 도감 (E)` button). A slime added without its encyclopedia entry is
recorded when the player meets it but shows no name, or shows a blank attack
description. Treat the encyclopedia steps below as part of adding the slime,
not as follow-up work.

## What the encyclopedia reads

| Shown in the encyclopedia | Source |
|---|---|
| Name | `EnemyData.displayName` in `Assets/ScriptableObjects/Combat/EnemyDataContainer.asset` |
| Picture | First sprite of `SlimeAnimator.sprites` on the prefab (`SlimeAnimator.FirstFrame`) |
| HP, attack, move distance, range | `maxHp`, `damage`, `moveDistance`, `attackRange` of the same entry |
| Element | `type` of the same entry |
| Attack description | `AttackDescription` property of the `Enemy` subclass on the prefab |
| Weak, resisted, healed-by elements | `Assets/ScriptableObjects/Combat/Magic Affinity Table.asset` |
| Element status resistance | `ElementalStatus.ResistanceDescription` |
| English text | `Assets/Resources/Localization/English.json`, keyed by the Korean text |
| Discovered or not | PlayerPrefs `SeenSlimes0`, `SeenSlimes1`, written by `EnemyPoolController.SpawnObject` and by `SlimeEncyclopediaView.RecordClearedStages` |

## Procedure

1. **Behavior.** Reuse an existing `Enemy` subclass in
   `Assets/Scripts/Combat/Enemies/` when the slime acts like one that exists.
   For a new behavior, add a subclass and in it:
   - Override `AttackDescription` with one or two plain Korean sentences that
     say what the slime does on its turn. Write numbers as literals (for
     example `거리 6 안에 들어오면`) and keep the comment that says the sentence
     and `English.json` must change together with the constants.
   - If it spawns another enemy during a wave, override `SummonedEnemyId` to
     return that enemy id. `RecordClearedStages` follows this id so that old
     saves record the summoned slime too.
   - `BossEnemy` subclasses get the boss freeze sentence automatically.
2. **Prefab.** Put the prefab under `Assets/Prefabs/Combat/Enemies/Slime/` with
   the `Enemy` subclass, `SlimeAnimator`, and `SelectableObject`. The first
   entry of `SlimeAnimator.sprites` is the encyclopedia picture, so it must be
   the idle frame, not a placeholder. Commit the `.meta` file with it.
3. **Data entry.** Append an entry to `EnemyDataContainer.asset`.
   - `id` must equal the entry's index in the list, because
     `EnemyDataContainer.GetGearData(id)` indexes the list.
   - `name` is the internal name (`S10_Ranged_Slime`). `displayName` is the
     Korean name players see. It must be unique. Leave `displayName` empty only
     for an entry no stage or summon ever spawns; an empty name removes the
     entry from the encyclopedia.
   - `type` uses the `MagicType` values: Holy 3, Fire 4, Ice 5, Rock 6,
     Lightning 7, Undead 8.
   - `moveDistance: 0` shows as `이동 거리 없음`. `attackRange: 99` shows as
     `사거리 화면 전체`.
   - Unity writes Korean strings in this asset as double-quoted `\uXXXX`
     escapes with uppercase hex. Match that when editing the YAML by hand.
4. **Waves.** Add the id to the `enemySpawnDatasInWave` list of the stage's
   `*BattleWaveData.asset`. `StageManager` only plays `enemyWaves[0]` of a
   `StageData`, and `RecordClearedStages` reads the same wave container.
5. **Translation.** Add an entry to `English.json` for the new `displayName`
   and for a new `AttackDescription` sentence. The font atlas
   (`NeoDunggeunmoPro-Regular SDF`) holds only ASCII and Hangul, so do not use
   `·`, `×`, `—`, or other symbols in the Korean or English text.
6. **Element changes.** If the change adds a `MagicType` or edits the affinity
   table, check that `SlimeEncyclopediaView.ElementName` has a name and color
   for the type and that `ElementalStatus.ResistanceDescription` describes its
   status resistance.
7. **Id range.** Discovery is stored as bits, 31 ids per key, in
   `SaveLoadController.SeenSlimeSlots` keys. Two keys hold ids 0 to 61. Raise
   `SeenSlimeSlots` before adding id 62.

## Validation

Run both suites in Unity 2022.3.34f1 batchmode. On this machine the editor is
the Windows install, so copy `Assets`, `Packages`, `ProjectSettings`, and `docs`
to a folder under `C:\temp` first (some EditMode tests read `docs`), then run:

```bash
"/mnt/c/Program Files/Unity/Hub/Editor/2022.3.34f1/Editor/Unity.exe" -batchmode -nographics -runTests -testPlatform EditMode -projectPath 'C:\temp\<copy>' -testResults 'C:\temp\<copy>\results-EditMode.xml' -logFile 'C:\temp\<copy>\test-EditMode.log'
```

For PlayMode use `-testPlatform PlayMode` and drop `-nographics`.

- `SlimeEncyclopediaDataTests` (EditMode) fails when a slime in a wave or a
  summon has no `displayName`, or when an encyclopedia entry has no English
  name, no `AttackDescription`, no English translation of it, no first frame,
  or a duplicate name.
- `SlimeEncyclopediaPlayTests` (PlayMode) counts the listed slimes from the
  asset. Its cleared-stage test lists the ids of stages 0 to 7 by hand; update
  that list when the new slime is added to one of those stages.
- Open the encyclopedia once with the slime discovered and check the picture,
  the two-column stats, and that the attack description fits the panel in both
  Korean and English.

## Report

State in the pull request which encyclopedia fields the new slime uses: its
`displayName`, its English name, the `AttackDescription` class, and whether it
summons another enemy.
