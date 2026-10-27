# 2026-10-14 — 타이틀 메뉴 버튼을 Vertical Layout Group 으로 묶기

- Date: 2026-10-14
- GitHub Issue: #113
- Status: Complete

## Goal

- 저장 데이터가 없어 이어하기 버튼이 꺼져도 새 게임 버튼과 종료 버튼 사이에 빈자리가 남지 않게 한다.
- 저장 데이터가 있으면 이어하기 버튼을 맨 위에 둔다.

## Non-goals

- 타이틀 로고와 배경 배치 변경
- 버튼 문구, 크기, sprite 변경
- `TitleSceneManager` 코드 변경

## Decisions

- Canvas 아래에 `MenuButtons` 를 만들고 `Vertical Layout Group`(Upper Center, Spacing 40, child 크기 제어 끔)을 붙인다. layout group 은 비활성 child 를 배치에서 빼므로, 이어하기 버튼이 꺼지면 아래 버튼이 위로 올라간다.
- child 순서는 `continue` → `MapSceneButton`(새 게임) → `ExitButton` 이다.
- 그룹의 위쪽 끝은 기존 새 게임 버튼의 위쪽 끝(y = 50)에 맞춘다. 크기는 250 × 440(버튼 3개 × 120 + 간격 2개 × 40)이다.
- 기존 간격은 36과 42였으므로 40으로 맞춘다.
- 씬 파일은 Unity 편집기 없이 YAML 로 편집했다. 새 오브젝트의 fileID 는 `2100000000` 부터 쓴다. 버튼의 anchor 와 위치는 layout group 이 계산하는 값을 미리 적었다.

## Approach (Checklist)
- [x] `TitleScene.unity` 에 `MenuButtons` 추가, 버튼 3개 이동
- [x] 저장 유무에 따른 버튼 위치를 검사하는 PlayMode test 추가
- [x] PlayMode test 실행, 두 상태 화면 캡처

## Validation

- PlayMode: `TitleMenuPlayTests`
- EditMode 전체: 217개 중 216개 통과, 1개 skip(`ElementalStatusTests` 미리보기 렌더링)
- PlayMode 전체: 64개 중 63개 통과, 1개 skip(`BalanceSimulation`)
- 화면 캡처: `.plan/assets/2026-10-14-issue-113-title-menu-layout/`
