# 2026-10-16 — 보스 가장자리 배경색 정리

- Date: 2026-10-16
- GitHub Issue: #127
- Status: Complete

## Goal
지역 보스 4종과 세계수 외피/정수의 48개 프레임에서 배경색 잔여물을 제거하고 기존 PR #137에 반영한다.

## Non-goals
보스 디자인, 몸 색, 크기, 위치, 전투 수치와 애니메이션 순서 변경.

## Context / Constraints
사용자가 필수 픽셀 아트 스킬 참조를 AGENTS.md에서 제거하고 누끼 정리와 기존 PR 반영을 승인했다. imagegen으로 투명 배경을 편집하며 기존 프레임의 색과 정렬을 보존한다. .meta의 GUID, 피벗, PPU와 프리팹 참조는 유지한다.

## Approach (Checklist)
- [x] **Step 0: Recon** 새 규칙과 보스 원본, 48개 프레임/팔레트/투명도 확인.
- [x] **Step 1: Implementation** 보스별 시트의 가장자리 배경색을 정리하고 프레임에 반영.
- [x] **Step 2: Tests** 전 프레임 전후 비교, 밝고 어두운 배경의 경계와 실제 전투 화면, Unity 에셋 검사.
- [x] **Step 3: Rollout / Rollback** 커밋/푸시 후 PR #137에 비교 이미지와 검증 결과 갱신. Draft 유지.

## Validation
- **Commands to run:** 이미지 크기/알파/프레임 정렬과 .meta 불변 확인, Unity 2022.3.62f3 관련 EditMode 및 전투 캡처.
- **Expected output:** 배경색 테두리가 없어지고 기존 장식과 눈, 불꽃/번개/가지가 유지된다.

## Risks & Rollback
- **Risks:** 밝은 하이라이트나 가는 가지를 배경으로 오인할 수 있어 원본 비교로 확인한다.
- **Rollback steps:** 해당 이미지 정리 커밋만 revert한다.

## Open Questions
없음.

## 세계수 붉은 선 후속

1차 마스크가 잎의 유색 테두리를 일부 남긴 것을 사용자 피드백과 6배 확대 비교로 확인했다. 내장 imagegen의 잎 영역과 원본 색을 대조해 녹슨 색의 외곽 2,359픽셀을 추가 정리했다. 8프레임의 나머지 RGB와 크기/메타데이터는 유지했다. 상세 근거는 자료 폴더의 `worldtree-red-fringe/`에 보관한다.

## Result

- 6종 48프레임, 경계 10,329픽셀 투명 처리. 남겨 둔 RGB 변경 0개/새 불투명 픽셀 0개. 크기와 .meta 불변.
- 독립 PNG 검사 통과, 최대 64색/이진 알파 유지.
- Unity 2022.3.62f3 전체 EditMode 308 통과/1 GPU 검사 생략, 실제 전투 캡처 PlayMode 2개 통과.
- 게임 코드 변경이 없는 에셋 정리로 전체 PlayMode 행동 검사는 다시 실행하지 않았다. 전후 8프레임과 전투 화면 6장을 PR에 첨부한다.

추가 수정 후 실제 세계수 등장/전투 캡처 PlayMode 1개 통과. 8프레임 독립 대조와 확대 비교를 확인했다.
