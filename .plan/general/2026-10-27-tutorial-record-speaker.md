# 2026-10-27 — 튜토리얼 할아버지 기록 화자 표시

- Date: 2026-10-27
- GitHub Issue: None (자동 승인 검토에서 외부 이슈 게시 거절)
- Status: Implemented locally

## Goal
튜토리얼의 할아버지 기록을 워드의 말과 구분한다.

## Non-goals
진행 조건 변경, 음악 PR 변경, 머지 및 배포.

## Context / Constraints
13개 대사의 이름이 비어 있고 워드 초상화가 화자로 강조된다.
공통 대화창의 이름 표시 및 listenerColor를 재사용한다.
영문 번역 Grandpa's Notes는 이미 등록되어 있다.
추가 피드백: 주민 대화 뒤에도 결과 안내 기록이 이어지므로 기록 전체가 끝났다는 표현을 제거한다.
튜토리얼 마지막은 기초 설명의 마무리와 다음 장 안내로 바꾸고, 주민 대화는 잠시 기록을 되새기는 장면으로 연결한다. 영어 번역도 함께 수정한다.
외부 이슈 등록 거절로 브랜치/커밋/PR 작업은 보류한다.

## Approach (Checklist)
- [x] **Step 0: Recon** — 데이터와 공통 대화창, 번역 확인.
- [x] **Step 1: Implementation** — 13개 화자 이름 지정, 워드 초상화 청자 밝기 적용.
- [ ] **Step 2: Tests** — 데이터 검증, Unity의 이름 및 초상화 표시 확인.
- [ ] **Step 3: Rollout / Rollback** — 검토 후 별도 수정 PR, 필요시 해당 변경만 복원.

## Validation
- **Commands to run:** git diff --check; Unity EditMode TutorialTests 및 PlayMode SharedDialoguePlayTests.
- **Expected output:** 화자 이름과 초상화 검증 통과. 도입은 이름 숨김, 기록은 화자 표시.
- **Results:** git diff --check 통과. 13개 화자 지정 및 기존 영어 번역 확인. 기존 EditMode/PlayMode 검증에 이름과 청자 밝기 확인 추가.
- **Unavailable:** Unity batchmode 실행은 HandleProjectAlreadyOpenInAnotherInstance로 중단. 테스트 결과와 실제 화면 캡처는 확보하지 못함. 열린 에디터의 작업 보호를 위해 종료하거나 잠금을 제거하지 않음.

## Risks & Rollback
- **Risks:** 실제 화면에서 이름 가독성 확인 필요.
- **Rollback steps:** TutorialScript.asset의 이름과 TutorialChatWindow.cs의 밝기 변경만 되돌린다.

## Open Questions
- 외부 이슈/PR 게시 승인 대기.
