# 2026-10-13 — 설정 메뉴 prefab 을 씬마다 배치

- Date: 2026-10-13
- GitHub Issue: #107
- Status: Complete

## Goal

- 빌드 설정의 씬 7개에 `SettingsMenu` prefab instance 를 배치해서, 메뉴 버튼이 생기지 않는 경우를 없앤다.
- 씬마다 배치하는 규칙을 `.agents/docs/project.md` 에 적는다.

## Non-goals

- 메뉴 항목과 디자인 변경
- 메뉴 동작(Esc 키, 일시 정지, 언어 전환, 타이틀로 돌아가기) 변경

## Decisions

- prefab 을 `Assets/Resources/UI/` 에서 `Assets/Prefabs/UI/` 로 옮긴다. 이름으로 불러오지 않으므로 Resources 에 둘 이유가 없다. GUID 는 그대로 둔다.
- `RuntimeInitializeOnLoadMethod`, `DontDestroyOnLoad`, `sceneLoaded` 처리를 지운다. 씬을 바꾸면 이전 씬의 메뉴는 씬과 함께 사라진다.
- MapScene 의 메뉴 버튼 위치(`y = -170`)는 코드의 씬 이름 분기 대신 MapScene 의 prefab override 로 둔다.
- EventSystem 이 없던 GameOverScene 에 EventSystem 을 배치하고 `EnsureEventSystem` 을 지운다.
- 씬 파일은 Unity 편집기 없이 YAML 로 편집했다. prefab instance 의 fileID 는 `1107000001`, GameOverScene 의 EventSystem 은 `1107000011` 부터 쓴다.

## Approach (Checklist)
- [x] prefab 이동, `SettingsMenu.cs` 에서 runtime 생성 코드 삭제
- [x] 씬 7개에 prefab instance 배치, GameOverScene 에 EventSystem 추가
- [x] 빌드 설정의 씬마다 prefab 과 EventSystem 이 있는지 검사하는 EditMode test 추가
- [x] `.agents/docs/project.md` 에 Scene Conventions 추가
- [x] EditMode·PlayMode test 실행

## Validation

- WSL 의 대소문자 구분 파일 시스템에서는 Unity 2022.3.62f3 이 `The project is on case sensitive file system` 으로 종료되어, `Assets`·`Packages`·`ProjectSettings`·`docs` 를 Windows 디스크에 복사해서 batchmode 로 실행했다.
- EditMode 179개 모두 통과. 새 test `빌드_설정의_씬마다_설정_메뉴와_EventSystem이_있다` 7개 포함.
- PlayMode 59개 중 58개 통과, 1개(`BalanceSimulation`)는 원래 건너뛴다. `SettingsMenuPlayTests` 5개 통과.
- import 와 test 를 거친 뒤에도 Unity 가 씬 파일 7개를 다시 저장하지 않았다.
- screen capture: `.plan/assets/2026-10-13-issue-107-settings-menu-per-scene/` 의 맵(메뉴 버튼이 `Stage` 글자 아래)과 전투(왼쪽 위).
