# 2026-10-27 — 배포 PR 충돌 해결

- Date: 2026-10-27
- GitHub Issue: None (PR #144)
- Status: Validated

## Goal
main의 웨이브 전환 대상 선택 수정과 develop의 호버 크기 복원·아군 생존 판정을 보존해 PR #144 충돌을 해결한다.

## Non-goals
PR 최종 머지, 배포 보호 설정 변경, 기존 작업 폴더 수정.

## Context / Constraints
분리된 작업 폴더에서 main을 develop에 역병합한다. 원격 develop에는 정상 fast-forward만 수행한다.

## Approach (Checklist)
- [x] **Step 0: Recon** 두 파일의 충돌과 원래 수정 의도 확인.
- [x] **Step 1: Implementation** 동적 대상 선택 및 현재 크기 복원 결합.
- [x] **Step 2: Tests** 대상 대기 후 생성, 아군 생존 판정, 호버 복원 회귀 검사.
- [ ] **Step 3: Rollout / Rollback** 충돌 해결 커밋을 PR head에 푸시하고 mergeable 확인.

## Validation
- **Commands to run:** Unity PlayMode TargetSelectionMergeTests, git diff --check.
- **Expected output:** 4개 테스트 통과 및 PR mergeable.

## Risks & Rollback
- **Risks:** main의 bundleVersion 1.4.0도 역병합으로 유지된다.
- **Rollback steps:** 역병합 커밋을 되돌린다.

## Open Questions
없음.
Unity 2022.3.62f3 PlayMode TargetSelectionMergeTests 4/4 통과. 별도 작업 폴더의 저장소 패키지 그대로 테스트. git diff --check 통과.
