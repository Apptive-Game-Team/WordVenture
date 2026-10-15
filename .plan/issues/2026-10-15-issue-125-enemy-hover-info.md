# 2026-10-15 — 전투에서 적 슬라임 정보 창 표시

- Date: 2026-10-15
- GitHub Issue: #125
- Status: Complete

## Goal

- 적 슬라임에 마우스를 올리면 이름, 속성, HP, 공격력, 상태, 약점/저항/회복을 보여 준다.

## Non-goals

- 적의 다음 행동 예고
- 도감에서 만나지 않은 슬라임 가리기

## Decisions

- 장면에 하나만 있는 정보 창이 매 프레임 마우스 아래의 적을 `Physics2D.OverlapPointAll` 로 찾는다. 쓰러지거나 꺼진 적은 `OnMouseExit` 를 받지 못해서 `OnMouseEnter`/`OnMouseExit` 대신 이 방식을 썼다. `SelectableObject` 는 고치지 않았다.
- 마우스 왼쪽 버튼을 누르고 있는 동안(카드를 끌거나 대상을 고르는 중)에는 숨긴다. `InteractionLock` 이 잠겨 있을 때도 숨긴다.
- 상성은 도감과 같은 `MagicAffinityTable` 로, 도감과 같은 배율 표기(`1.5배`)를 쓴다. 배율이 정확히 1이면 표시하지 않는다.
- 정보 창은 슬라임의 왼쪽 위에 둬서 손패를 가리지 않는다. 클릭을 가로채지 않도록 raycast 를 끈다.

## Approach (Checklist)
- [x] `EnemyHoverInfo` 와 `Enemy` 의 읽기 전용 값
- [x] 영어 번역
- [x] PlayMode 테스트와 캡처
- [x] EditMode, PlayMode 전체를 다른 복사본과 겹치지 않게 다시 실행

## Validation

- EditMode 217개 통과
- PlayMode 70개 중 69개 통과, 실패 0, 건너뜀 1(밸런스 시뮬레이션)
- 처음에 다른 복사본과 동시에 돌렸을 때 나온 실패 3개는 PlayerPrefs 가 섞인 탓이었고, 혼자 다시 돌리자 통과했다.

## Risks & Rollback

- #120 도 `Enemy.cs` 의 `Awake` 를 고쳤으므로 병합할 때 충돌을 풀어야 한다.
- 되돌리기: `git revert`
