# 2026-10-13 — 설정 메뉴 prefab 을 씬마다 배치

- Date: 2026-10-13
- GitHub Issue: #107
- Status: Complete

## Goal

- 빌드 설정의 씬 7개에 `SettingsMenu` prefab instance 를 배치해서, 메뉴 버튼이 생기지 않는 경우를 없앤다.
- 씬마다 배치하는 규칙을 `.agents/docs/project.md` 에 적는다.
- 타이틀에만 있던 계정 창을 설정 메뉴로 옮겨 어느 씬에서나 연다. 불러오기는 타이틀에서만 보인다.

## Non-goals

- 메뉴 항목과 디자인 변경
- 메뉴 동작(Esc 키, 일시 정지, 언어 전환, 타이틀로 돌아가기) 변경

## Decisions

- prefab 을 `Assets/Resources/UI/` 에서 `Assets/Prefabs/UI/` 로 옮긴다. 이름으로 불러오지 않으므로 Resources 에 둘 이유가 없다. GUID 는 그대로 둔다.
- `RuntimeInitializeOnLoadMethod`, `DontDestroyOnLoad`, `sceneLoaded` 처리를 지운다. 씬을 바꾸면 이전 씬의 메뉴는 씬과 함께 사라진다.
- MapScene 의 메뉴 버튼 위치(`y = -170`)는 코드의 씬 이름 분기 대신 MapScene 의 prefab override 로 둔다.
- EventSystem 이 없던 GameOverScene 에 EventSystem 을 배치하고 `EnsureEventSystem` 을 지운다.
- 씬 파일은 Unity 편집기 없이 YAML 로 편집했다. prefab instance 의 fileID 는 `1107000001`, GameOverScene 의 EventSystem 은 `1107000011` 부터 쓴다.

- 계정 창은 설정 메뉴 prefab 의 하위 창이 되고, 크레딧처럼 Esc 키를 설정 메뉴가 받아 닫는다. `AccountPanel.HoldsEscape` 를 지운다.
- 불러오기(`CloudSave.ApplyToLocal`)는 PlayerPrefs 와 `MapMove.StagePosition` 을 바로 덮어써서, 전투나 대화 도중에 쓰면 진행 중인 씬과 저장된 stage 가 맞지 않는다. 타이틀이 아닌 씬에서는 불러오기 버튼을 숨긴다. 로그인, 올리기, 피드백은 어디서나 된다.
- `MemberView` 는 prefab 에 있어서 TitleScene 의 `TitleStageView` 를 참조하지 못한다. 같은 stage 목록을 prefab 에 따로 두고, 두 목록이 같은지 EditMode test 로 검사한다.
- 계정 창 계층은 일회용 editor script 로 TitleScene 에서 prefab 으로 복사하고, 씬을 가리키던 참조를 prefab 안의 대상으로 다시 연결했다. editor script 는 commit 하지 않는다.

## Approach (Checklist)
- [x] prefab 이동, `SettingsMenu.cs` 에서 runtime 생성 코드 삭제
- [x] 씬 7개에 prefab instance 배치, GameOverScene 에 EventSystem 추가
- [x] 빌드 설정의 씬마다 prefab 과 EventSystem 이 있는지 검사하는 EditMode test 추가
- [x] `.agents/docs/project.md` 에 Scene Conventions 추가
- [x] EditMode·PlayMode test 실행
- [x] develop merge, TitleScene conflict 해결
- [x] 계정 창을 설정 메뉴 prefab 으로 이동, TitleScene 의 계정 버튼 삭제
- [x] 타이틀이 아닌 씬에서 불러오기 버튼 숨김
- [x] 계정 창 이동 후 EditMode·PlayMode test 실행

## Validation

- WSL 의 대소문자 구분 파일 시스템에서는 Unity 2022.3.62f3 이 `The project is on case sensitive file system` 으로 종료되어, `Assets`·`Packages`·`ProjectSettings`·`docs` 를 Windows 디스크에 복사해서 batchmode 로 실행했다.
- EditMode 179개 모두 통과. 새 test `빌드_설정의_씬마다_설정_메뉴와_EventSystem이_있다` 7개 포함.
- PlayMode 59개 중 58개 통과, 1개(`BalanceSimulation`)는 원래 건너뛴다. `SettingsMenuPlayTests` 5개 통과.
- import 와 test 를 거친 뒤에도 Unity 가 씬 파일 7개를 다시 저장하지 않았다.
- screen capture: `.plan/assets/2026-10-13-issue-107-settings-menu-per-scene/` 의 맵(메뉴 버튼이 `Stage` 글자 아래)과 전투(왼쪽 위).
- develop merge 후: EditMode 214개 모두 통과, PlayMode 59개 중 58개 통과(1개는 원래 건너뜀).
- 계정 창 이동 후: EditMode 217개 모두 통과. 새 test `계정_창의_stage_목록이_타이틀_배경과_같다` 포함.
- 계정 창 이동 후 PlayMode 60개 중 1개가 실패했다. 타이틀에서는 `타이틀로` 버튼이 숨겨져 마지막 버튼이 크레딧에서 계정으로 바뀌었는데 test 기대값이 예전 순서였다. 기대값을 고친 뒤 `SettingsMenuPlayTests` 6개를 다시 실행해 모두 통과했다.
- 타이틀 메뉴 screen capture 는 메뉴를 연 프레임에 찍으면 패널 크기가 갱신되기 전이라 계정 버튼이 잘려 보였다. 한 프레임 기다린 뒤 찍도록 test 를 고쳤다.
- screen capture: `settings-menu-title.png`(계정 버튼 포함 5개), `settings-menu-account-MapScene.png`(맵에서 연 계정 창).
