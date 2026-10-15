# 2026-10-15 — 지역 대화창 제목이 설정 메뉴 버튼에 가려지는 문제 수정

- Date: 2026-10-15
- GitHub Issue: #121
- Status: Complete

## Goal

- 지역 대화창 제목이 왼쪽 위의 설정 메뉴 버튼과 겹치지 않게 한다.

## Non-goals

- 설정 메뉴 버튼 위치 변경
- 대화창 본문과 초상화 배치 변경

## Decisions

- 제목은 화면 너비의 5%(1920x1080 기준 x 96)에서 시작했고, 메뉴 버튼은 x 24~112를 차지했다. 제목의 왼쪽 끝을 화면 왼쪽에서 160만큼 떨어진 자리로 옮겼다.
- 메뉴 Canvas 는 화면 높이에만 맞춰 커지고(`matchWidthOrHeight` 1), 대화창 Canvas 는 너비와 높이를 반씩 따른다(0.5). 4:3이나 5:4 화면에서는 대화창이 더 작아지므로, 버튼 오른쪽 끝(112)에 딱 맞추지 않고 여유를 두었다. 5:4(1280x1024)에서도 제목은 x 127, 버튼은 x 106에서 끝난다.

## Approach (Checklist)
- [x] `StageDialogueView` 제목의 왼쪽 여백 변경
- [x] 제목과 메뉴 버튼의 화면 영역이 겹치지 않는지 확인하는 PlayMode 테스트
- [x] 대화창 미리보기 캡처 갱신

## Validation

- PlayMode: `StageDialoguePlayTests`, `SharedDialoguePlayTests`, `SettingsMenuPlayTests` 18개 통과
- 화면 캡처: `.plan/assets/2026-10-15-issue-121-dialogue-title-menu-overlap/dialogue-title-menu.png`

## Risks & Rollback

- 제목이 오른쪽으로 64 옮겨져 긴 제목은 오른쪽 끝(화면 너비의 95%)에 더 빨리 닿는다. 가장 긴 한국어 제목(20자, `세계수의 심장 · 폭주한 마력의 형상`)은 x 760 근처에서 끝난다. 영어 제목은 더 길 수 있으므로 영어로 보고 확인한다.
- 되돌리기: `git revert`
