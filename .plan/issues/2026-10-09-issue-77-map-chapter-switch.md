# Issue 77: 맵 1부·2부 장 전환과 다시 깬 스테이지의 진행도

- Issue: https://github.com/Apptive-Game-Team/WordVenture/issues/77
- Branch: `feat/map-chapter-switch` (`feat/act-two-complete` 위)

## 장 전환

- `MapMove`에 `chapterSwitchButton`과 `chapterSwitchLabel`을 추가했다. 버튼은 1부를 끝낸 뒤
  (`StagePosition` 5 이상)에만 보이고, 누르거나 Tab 키를 누르면 1부 맵과 2부 맵이 바뀐다.
- 지점 오브젝트 5개는 2부 맵에서 2부 좌표로 옮겨지므로, `Awake`에서 1부 좌표를
  `actOneStageLocations`에 저장해 두고 1부로 돌아올 때 되돌린다.
- 1부 맵의 배경은 `StagePosition`이 5 이상이면 1부를 끝낸 배경(`stage4`)을 쓴다.
- 마지막으로 본 장은 `SaveLoadController.ShowsActOneMap`(PlayerPrefs `ShowsActOneMap`)에 저장한다.
  기록이 없으면 지금처럼 2부 맵을 연다. `InitPlayData`가 이 기록을 지운다.
- 버튼은 `MapScene`의 Enter 안내 상자 왼쪽에 같은 색과 높이로 놓았다. 오른쪽 위에 두면 1부 맵의
  마왕성을 가린다.

## 다시 깬 스테이지

- `BattleWaveController`가 `StageDataSingleton.isFirstClear`를 기록하고, 처음 깬 전투에서만
  `StagePosition`을 올린다. 그전에는 1부 스테이지를 다시 깨면 2부 진행도가 올라갔다.
- `GameClearController`는 이 기록으로 새 카드와 엔딩을 결정한다. `StagePosition - 1`로 판단하면
  바로 직전 스테이지를 다시 깬 경우(예: `StagePosition` 5에서 마왕 4)를 첫 클리어로 잘못 판단한다.

## 검증

- EditMode: 장 전환 버튼의 씬 연결, `ShowsActOneMap` 저장과 새 게임 초기화
- PlayMode: 버튼으로 1부·2부를 오가며 배경·캐릭터 위치·해금 상태 확인, 맵을 다시 열 때 마지막 장 유지,
  1부 진행 중 버튼 숨김, 마왕을 다시 깨도 엔딩으로 가지 않음
- Screen capture: `StagePosition` 7에서 2부 맵과 1부 맵 (`.plan/assets/2026-10-09-issue-77-map-chapter-switch/`)
