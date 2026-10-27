---
name: artel-test-case
description: >
  Procedure for writing WordVenture test cases (TC) into the ARTEL QA platform
  with the `artel` command line. Use when the user asks to add, write, or
  update a test case, TC, QA case, or ARTEL case, or says ARTEL 에 TC 만들어,
  테스트 케이스 추가, TC 작성, QA 케이스. Covers loading the CLI through nvm,
  choosing the project, checking existing cases for duplicates, the case JSON
  fields, the wording existing cases use, and verifying the created cases.
  A request for a TC means an ARTEL case, not a Unity Test Runner test, unless
  the user names Unity, EditMode, or PlayMode.
---

# ARTEL Test Case

ARTEL runs a QA agent against a game build and judges each test case by what
is on the screen. A case is one player action with the state before it and
the result the player should see after it. Write cases for that reader: it
plays the game, it does not read the code.

## Load the CLI

`artel` is a Node package installed under nvm, so it is not on `PATH` in a
plain shell. Load nvm in the same command that calls it:

```bash
source ~/.nvm/nvm.sh >/dev/null; artel case list --project 11
```

`artel --help` and `artel case <command> --help` list the commands. Add
`--json` to any command when a script reads the result.

## Projects

| id | Name | Use |
|---|---|---|
| 11 | WordVenture 사용자 관점 테스트 | Default. Cases written from what the player sees, with concrete stage numbers, names, and on-screen text |
| 5 | WordVenture | Older, coarser cases from 2026-08. Do not add here unless the user asks |

Run `artel project list` when the table may be out of date.

## Procedure

1. **Read the behavior.** Find the code and data that decide the result, so
   the expected value names real values: stage number and Korean stage name,
   slime display names, button labels, counts. Do not guess on-screen text;
   read it from the scene, the `Localization` table, or the asset.
2. **Check for duplicates.** List the project's cases and search the scene and
   feature words before writing:

   ```bash
   source ~/.nvm/nvm.sh >/dev/null
   artel case list --project 11 --json | python3 -c "
   import json, sys
   for c in json.load(sys.stdin)['items']:
       text = json.dumps(c, ensure_ascii=False)
       if '도감' in text: print(c['id'], c['scene'], c['step'], '|', c['precondition'], '|', c['expectedValue'])"
   ```

   Update an existing case with `artel case update <case-id>` instead of
   adding a near copy.
3. **Write the JSON.** Put the cases in a JSON array in the scratchpad
   directory, one object per case, with these four fields:

   | Field | Content |
   |---|---|
   | `scene` | Unity scene where the action happens: `TitleScene`, `StoryScene`, `MapScene`, `TurnBattleScene`, `GameClearScene`, `GameOverScene`, `EndingScene` |
   | `step` | One player action, ending with `~한다`: `E 키를 눌러 슬라임 도감을 연다` |
   | `precondition` | The save and screen state before the action: 진행도, which stage was cleared, which dialogue was already seen, what is open |
   | `expectedValue` | What the player sees after the action, with the exact on-screen text in double quotes |

   The server fills `id`, `status`, and `verificationStatus` (`DRAFT`).
4. **Create.** `artel case create --project 11 --file <path>` creates every
   case in the array in one call. Add `--json` to get the new ids.
5. **Verify.** Run `artel case show <case-id> --project 11` for one or two of
   the new ids and confirm the Korean text arrived intact. Report the new ids
   to the user.

## Wording

- One action per case. Split `A 하고 B 한다` into two cases unless B is the
  only way to observe A.
- Name stages by Korean name and number together: `벼락 협곡(스테이지 6)`.
  `진행도` is `MapMove.StagePosition`, the number of stages cleared; clearing
  stage k for the first time makes it k + 1.
- Write keys and buttons as the player sees them: `Enter 키`,
  `"슬라임 도감 (E)" 버튼`, `"닫기 (Esc)" 버튼`.
- Write results the screen can show. `PlayerPrefs` values, field names, and
  log lines are not visible to the QA agent; write the label or count they
  produce instead.
- Clearing 마왕 성(스테이지 4) or 세계수의 심장(스테이지 9) for the first time
  goes to `EndingScene`, not back to the map. A case that needs the map after
  those stages starts from `TitleScene` → Continue.

## WordVenture facts for cases

| Fact | Source |
|---|---|
| Stage number, Korean name | `stageID`, `stageName` in `Assets/ScriptableObjects/Stages/*Stage.asset` |
| Enemies in each wave | `enemySpawnDatasInWave` in the `*BattleWaveData.asset` that the stage's `enemyWaves[0]` points to |
| Slime display names | `displayName` in `Assets/ScriptableObjects/Combat/EnemyDataContainer.asset` (Korean is stored as `\uXXXX` escapes) |
| Slimes summoned during a wave | `childEnemyId` on split slime prefabs, `splitEnemyId` on the Act Two final boss prefab |
| Slime encyclopedia rules | `.agents/skills/add-slime/SKILL.md` and `SlimeEncyclopediaView` |
| On-screen English text | `Assets/Resources/Localization/English.json` |
