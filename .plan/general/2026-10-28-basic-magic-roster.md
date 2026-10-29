# 2026-10-28 — 기본 마법 구성 개편

- Date: 2026-10-28
- GitHub Issue: None
- Status: 요구사항 확인 중

## Goal

사용자가 요청한 기본 마법 구성을 카드와 실제 전투 동작에 반영한다.

| 분류 | 요청 마법 |
| --- | --- |
| 슛 공격 | 화염슛, 전기체인 슛 |
| 무리 | 불씨 정령 무리, 물 슬라임 무리 |
| 지대공 | towerback, 덩쿨 정령 |
| 땅 | 돌 골렘, 나무 골렘 |
| 폭발 | 폭발 마법 |

## Non-goals

관계없는 에셋 변경, 스테이지 확장, 배포와 릴리스.

## Context / Constraints

- 현재 WordList는 Shoot, Explode, Drop, Spawn 주문과 속성 단어로 구성된다.
- CombineZone은 주문 1장과 속성 1장을 받아 Shoot/Drop/Explode 또는 AllyFormation.Spawn을 실행한다.
- 현재 아군 소환은 AllySlime 하나를 사용한다. 요청한 정령·골렘·towerback의 전용 동작은 없다.
- MagicType은 에셋에 정수로 저장되므로 기존 enum 값은 보존한다.
- 튜토리얼, 스테이지별 카드 해금, 현지화, 카드 이미지 매핑까지 영향을 확인해야 한다.
- 시작 시 ThirdParty 아트 .meta에 다수 기존 변경이 있다. 이번 작업에서 포함하지 않는다.
- GitHub CLI 조회는 네트워크 연결 오류로 실패했다. 추적 개발 전 이슈 연결을 재시도한다.

## Approach (Checklist)
- [x] **Step 0: Recon** — WordList, CombineZone, 주문 실행기, AllyFormation/AllySlime, SpawnCardDataTests 확인.
- [ ] **Step 1: Implementation** — 사용자 답변으로 카드 체계와 towerback 정의를 확정한 후 이슈 및 작업 브랜치를 마련한다. 카드 데이터, 실행 동작, 에셋, 현지화, 튜토리얼을 일관되게 수정한다.
- [ ] **Step 2: Tests** — 기존 MagicType 정수 호환성, 새 마법의 타격 대상·연쇄·무리 수·소환 제한, 카드 조합과 튜토리얼을 확인한다.
- [ ] **Step 3: Rollout / Rollback** — 범위를 검토하고 검증 결과를 기록한다. 배포는 별도 요청 시 진행한다.

## Validation
- **Commands to run:** Unity Test Runner의 관련 EditMode/PlayMode 테스트. 실행 전 에디터 점유 상태와 프로젝트 설정 확인.
- **Expected output:** 카드 구성과 실제 실행이 확정된 요청과 일치하고 기존 저장 enum 및 전투 턴 진행이 유지된다.
- 현재 검증은 소스와 데이터 조사까지이며 게임 실행 검증은 하지 않았다.

## Risks & Rollback
- **Risks:** 카드 체계 개편 여부에 따라 기존 속성 조합·해금·튜토리얼에 미치는 영향이 다르다. towerback과 지대공의 구체적인 동작이 불명확하다.
- **Rollback steps:** 이번 작업에서 추가하거나 수정한 파일만 되돌리고 기존 사용자 변경은 보존한다.

## Open Questions
- 각 줄의 두 항목은 함께 제공할 마법 2종인가? 총 9종의 기본 마법 카드로 개편하는가, 기존 조합 결과를 바꾸는가?
- towerback의 외형과 공격 방식은 무엇인가? 지대공은 지상 소환수가 공중 적을 공격한다는 의미인가?
