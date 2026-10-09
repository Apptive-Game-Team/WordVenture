# Issue 70: 2부 나머지 지역, 적 4종, 그림, 에필로그

- Issue: https://github.com/Apptive-Game-Team/WordVenture/issues/70
- Branch: `feat/act-two-complete` (PR #69 `feat/thunder-canyon` 위)

## 그림

- 전투 배경 5장, 대화 초상화 5장, 2부 적 7종 × 프레임 8장을 Codex 이미지 생성으로 만들었다.
  방법과 규격은 `docs/design/act-two-art/README.md`에 있다.
- 경로와 GUID를 `docs/design/act-two-art/manifest.json`에 먼저 정해 두고, 그림 생성과 데이터
  작업을 동시에 진행했다.

## 적

| 적 | 클래스 | 행동 |
| --- | --- | --- |
| 분열 슬라임 | `SplitEnemy` (`SwordEnemy` 상속) | 쓰러지면 작은 분열 슬라임(id 25) 두 마리를 `BattleWaveController.SpawnExtraEnemy`로 부른다. 웨이브 목록에 더해지므로 작은 슬라임까지 쓰러뜨려야 웨이브가 끝난다 |
| 폭발 슬라임 | `ExplodeEnemy` | 맨 앞 칸 사거리(3) 안이면 "폭발 준비"를 띄우고, 다음 적 턴에 아군 전체(없으면 워드)에 공격력만큼 피해를 주고 쓰러진다. 예고 중에 먼저 쓰러지면 2.5 unit 안의 적이 피해를 받는다 |
| 치유 슬라임 | `HealEnemy` | 움직이지 않고 매 적 턴에 가장 많이 다친 다른 적을 공격력만큼 회복한다 |
| 폭주한 마력의 형상 | `SurgeBossEnemy` (`BossEnemy` 상속) | 곡사 포격을 조준·착탄으로 번갈아 쓴다. HP가 처음 절반 아래로 떨어진 다음 적 턴에 작은 분열 슬라임 두 마리를 부른다 |

- 곡사 포격은 `MortarStrike`로 분리해 곡사 슬라임과 최종 보스가 같이 쓴다.
- `Enemy.Death`를 virtual로 바꾸고 `Enemy.Heal`, `Enemy.MissingHp`를 추가했다.
- `AllyFormation.HitAllAllies`가 폭발 피해를 처리한다.

## 지역과 이야기

- 스테이지 7·8·9 데이터와 웨이브 3개씩. 2부 맵에서 9까지 들어갈 수 있다.
- 대화: 스테이지 5~9의 입장·클리어, 잿빛 유적 3번째 웨이브 앞, 뒤틀린 숲 2번째 웨이브 앞,
  세계수의 심장 보스 웨이브 앞.
- 세계수의 심장을 클리어하면 `GameClearController`가 엔딩 씬으로 보낸다. `StoryController`는
  `StagePosition`이 10 이상이면 `ActTwoEndingScript`를 보여 주고, 끝나면 타이틀로 간다.

## 검증

- EditMode: 스테이지 7~9 데이터, prefab 프레임 연결, 대화 시점, 에필로그 연결
- PlayMode: 분열·폭발·치유·보스 동작, 2부 완료 맵, 2부 에필로그 선택
