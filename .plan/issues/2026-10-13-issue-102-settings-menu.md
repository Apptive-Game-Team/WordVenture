# 2026-10-13 — 모든 씬에서 여는 설정 메뉴와 창 모드·전체 화면 전환

- Date: 2026-10-13
- GitHub Issue: #102
- Status: Complete

## Goal

- 어느 씬에서나 Esc 키나 왼쪽 위 메뉴 버튼으로 설정 메뉴를 연다.
- 설정 메뉴에서 창 모드와 전체 화면, 언어, 크레딧, 타이틀로 돌아가기를 고른다.
- F11 로도 화면 모드를 바꾸고, 창 모드에서 창 크기를 조절할 수 있다.

## Non-goals

- 음량, 해상도 목록 같은 다른 설정 항목
- localization 구조 변경

## Decisions

- 메뉴가 열려 있는 동안 `Time.timeScale` 을 0 으로 두고 `InteractionLock.IsPaused` 로 입력을 막는다.
- 언어를 바꾸면 TitleScene 과 MapScene 은 씬을 다시 열고, 전투와 대화 중에는 다음 씬부터 적용된다고 안내한다.
- 타이틀의 크레딧 버튼과 언어 버튼, MapScene 의 뒤로 가기 버튼은 메뉴로 옮기고 지운다.
- 화면 모드는 Unity 가 PlayerPrefs 에 저장하는 값을 그대로 쓴다. Alt+Enter 는 `allowFullscreenSwitch` 가 Windows 에서 처리한다.

## Approach (Checklist)
- [x] `Scenes.SettingsMenu`, `Scenes.Credits` 추가, `InteractionLock.IsPaused` 추가
- [x] 대화를 넘기는 `Input.anyKeyDown` 이 메뉴를 여닫는 입력을 넘기지 않도록 수정
- [x] 일회용 editor script 로 `Assets/Resources/UI/SettingsMenu.prefab` 생성, TitleScene·MapScene 버튼 제거
- [x] `TitleCredits`, `LanguageButton`, `BackButton` 삭제
- [x] EditMode·PlayMode test 실행, 씬별 screen capture 확인

## Validation

- EditMode 158개 중 156개 통과. 실패 2개는 2부 대사와 `2부 맵으로 (Tab)` 의 영어 번역 누락으로 #92 범위다.
- PlayMode 51개 중 50개 통과, 1개(`BalanceSimulation`)는 원래 건너뛴다.
- MapScene 은 맵 그림의 로고와 겹치지 않도록 메뉴 버튼을 `Stage` 글자 아래로 내린다.
