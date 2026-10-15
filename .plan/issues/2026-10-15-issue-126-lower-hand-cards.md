# 2026-10-15 — 전투 손패를 내리고 마우스를 올린 카드만 올리기

- Date: 2026-10-15
- GitHub Issue: #126
- Status: Complete

## Goal

- 다가오는 슬라임이 손패 뒤에 가려지지 않게 한다.

## Non-goals

- 슬라임과 워드의 높이 변경 (전투 배경의 땅 높이에 맞춰져 있다)
- 카드 크기 변경

## Decisions

- 손패의 쉬는 자리를 29칸(`29 / 27` unit) 내렸다. 카드 높이 1.85 unit 중 위쪽 약 절반(이름 띠와 그림 윗부분)이 보인다.
- 마우스를 올리거나 끄는 카드는 원래 높이로 올라와 전부 보이고 맨 앞에 그린다. 한 번에 한 장만 올라온다. `InteractionLock` 이 잠기면 내린다.
- 버리기 칸과 넘친 카드가 타는 자리는 `cardLeft` 를 그대로 읽으므로 위치가 바뀌지 않는다.
- 튜토리얼 안내 테두리는 화면 안쪽으로 잘라, 내려간 카드의 보이는 부분만 가리킨다.
- #120 의 손패 코드 위에서 작업해서 #120 브랜치를 대상으로 pull request 를 연다.

## Approach (Checklist)
- [x] `CardManager` 손패 높이, hover, drag
- [x] `TutorialGuidance` 화면 안쪽으로 자르기
- [x] PlayMode 테스트와 캡처
- [x] EditMode, PlayMode 전체를 다른 복사본과 겹치지 않게 다시 실행

## Validation

- EditMode 289개 통과
- PlayMode 84개 중 77개 통과, 실패 0, 건너뜀 7(밸런스 시뮬레이션, 캡처)
- 처음에 다른 복사본과 동시에 돌렸을 때 나온 실패는 PlayerPrefs 가 섞인 탓이었다.

## Risks & Rollback

- 올라온 카드는 옆 카드와 슬라임을 잠깐 가린다.
- 되돌리기: `git revert`
