# 2026-10-27 — ARTEL CLI 설치 및 WordVenture TC 보강

- Date: 2026-10-27
- GitHub Issue: None
- Status: Complete

## Goal
공식 alpha CLI 설치 후 WordVenture 프로젝트(5)에 현재 구현 근거가 있는 TC를 등록한다.

## Non-goals
게임 코드 변경, 기존 TC 수정, QA 실행 및 통과 판정.

## Context / Constraints
기존 프로젝트 5에 123개, 별도 사용자 관점 프로젝트 11에 553개 TC가 있다. 사용자 작업 파일을 보존한다. 기존 인증을 재사용한다.

## Approach (Checklist)
- [x] **Step 0: Recon** 공식 설치 문서, CLI 도움말, 현재 PlayMode 테스트 및 저장 코드를 확인한다.
- [x] **Step 1: Implementation** npm alpha 설치 및 버전/인증 확인. artifacts/artel/2026-10-27-cases.json 작성.
- [x] **Step 2: Tests** JSON 필수 필드와 중복 검사 후 CLI 등록 및 재조회로 일치 확인.
- [x] **Step 3: Rollout / Rollback** 등록 ID를 결과 파일로 남긴다. 취소 시 이번에 생성한 ID만 삭제 대상으로 삼는다.

## Validation
- **Commands to run:** node --version; npm install -g @project-artel/cli@alpha; artel --version; artel auth status; artel project list; artel case create/list; git diff --check
- **Expected output:** CLI 설치 성공, 등록 결과와 재조회 내용 일치. TC는 미실행 초안이다.

## Risks & Rollback
- **Risks:** 배포 빌드가 현재 소스보다 오래되었으면 기능이 없을 수 있다. 실제 실행 검증은 별도다.
- **Rollback steps:** 결과 JSON의 신규 TC ID만 지정하여 artel case delete로 제거한다. 기존 TC는 변경하지 않는다.

## Open Questions
- 없음. 정확한 게임 이름의 프로젝트 5를 대상으로 한다.
