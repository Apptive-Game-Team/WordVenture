# 2026-10-27 — ARTEL 테스트 사전 조건용 치트

- Date: 2026-10-27
- GitHub Issue: #145
- Status: Implemented; PlayMode validation blocked by open Unity instance

## Goal
ARTEL QA가 로컬 테스트 저장과 전투 진입 사전 조건을 빠르게 준비한다.
사용자 추가 승인: 모든 적에게 지정 직접 피해를 주고, 현재 웨이브를 정상 사망 처리로 전멸시키는 치트를 추가한다.

## Non-goals
서버 저장 변경, 무적, 직접 보상 지급, 실제 QA 실행. 전멸 이후의 정상 게임 클리어/보상 흐름은 유지한다.

## Context / Constraints
현재 SDK의 ArtelCheat/ARTEL_GAME_FUNCTIONS를 사용하며 에디터와 개발 빌드로 제한한다. 기존 사용자 변경을 보존한다.

## Approach (Checklist)
- [x] **Step 0: Recon** SDK 등록 방식과 저장/맵/전투 경로 확인.
- [x] **Step 1: Implementation** Core/ArtelTestCheats.cs에 저장 준비, 전투 진입, 타이틀 복귀 추가.
- [ ] **Step 2: Tests** 저장 준비와 잘못된 입력 및 잠금 복구 PlayMode 검증.
- [ ] **Step 3: Rollout / Rollback** 개발 빌드에서 catalog 등록 확인 후 사용. 추가 파일만 제거하면 롤백 가능.

## Validation
- **Commands to run:** Unity 2022.3.62f3 Test Runner PlayMode, git diff --check.
- **Expected output:** ArtelTestCheatsPlayTests 통과. SDK가 없거나 출시 빌드이면 치트 코드 제외.
- **Actual:** Unity 내장 Roslyn으로 치트와 테스트 파일을 각각 기존 프로젝트 참조에 대해 컴파일하여 모두 exit 0. 새 C# 파일의 diff 공백 검사 통과.
- **Additional validation:** damage_all_enemies/kill_all_enemies 추가 후 두 파일 컴파일 재통과. 활성/비활성 적, 피해 수치 경계, 분열 자식 전멸, 이후 소환 적 보존, 전투 밖/일시정지 거절 테스트를 추가했다. PlayMode 실행은 기존 Unity 인스턴스가 프로젝트를 열고 있어 미수행이다.
- **Blocked:** Unity 배치 PlayMode 실행은 동일 프로젝트가 다른 Unity 인스턴스에서 열려 있어 테스트 전에 종료됐다. artifacts/artel/cheat-tests.log에 기록. 열린 에디터에서 ArtelTestCheatsPlayTests를 실행해야 한다. 개발 빌드 생성 및 ARTEL catalog/호출 검증은 아직 수행하지 않았다.

## Risks & Rollback
- **Risks:** 테스트 저장 준비는 로컬 진행도/대화/도감 기록을 초기화한다. confirmOverwrite=true가 필요하다. 전투 직행은 맵 진입 대화를 생략한다.
- **Rollback steps:** 이 변경에서 추가한 스크립트와 meta, 테스트, 문서를 제거한다.

## Open Questions
- 없음.
