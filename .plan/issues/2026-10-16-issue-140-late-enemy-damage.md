# 2026-10-16 — 후반 적 공격력 상향

- Date: 2026-10-16
- GitHub Issue: #140
- Status: Implemented; manual balance validation pending

## Goal
후반에 공격을 허용하면 큰 피해를 받아 선제 처치와 공격 차단이 중요해지도록 한다.

## Non-goals
체력, 초반 적, 웨이브, 공격 주기, 분노 증가율, 치유량 변경.

## Context / Constraints
플레이어 기본 체력 100, 강화마다 20 증가. 기존 후반 일반 적 공격력은 대부분 11~17이다.
전투와 도감은 EnemyDataContainer.asset의 damage를 함께 읽는다. 이름과 행동 설명 및 번역은 변경되지 않는다.
Hindsight 조회는 ENOTFOUND home으로 실패하여 현재 코드로 확인했다.

## Approach (Checklist)
- [x] **Step 0: Recon** 공격 및 보스 패턴, 도감 데이터 경로 확인.
- [x] **Step 1: Implementation** EnemyDataContainer.asset의 공격형 적 18종 damage만 변경.
- [x] **Step 2: Tests** 데이터 diff 검증 및 Unity EditMode/PlayMode 실행.
- [x] **Step 3: Rollout / Rollback** draft PR로 제출. 해당 커밋을 revert하여 복구.

## Validation
- **Commands to run:** git diff --check, Unity 2022.3.62f3 -runTests -testPlatform EditMode / PlayMode.
- **Expected output:** 기존 체력/초반/회복 수치 보존, 기존 테스트 통과.
- 체감 난이도는 실제 플레이 검증이 필요하며 정적 검증으로 보장하지 않는다.

## Risks & Rollback
- **Risks:** 강화가 낮은 플레이어는 여러 적의 공격에 빠르게 사망할 수 있다. 돌진의 소환 아군 대상 2배 피해와 분노도 유지된다.
- **Rollback steps:** 이 변경 커밋을 revert.

## Open Questions
- 없음. 수치는 첫 밸런스 시안이며 실전 플레이 후 추가 조정한다.

## Damage Changes
| ID | Enemy | Before | After |
| --- | --- | --- | --- |
| 16 | S5 Ranged | 11 | 22 |
| 17 | S5 Melee | 12 | 26 |
| 18 | S5 Elite | 10 | 24 |
| 19 | S6 Ranged | 12 | 28 |
| 20 | S6 Mortar | 13 | 34 |
| 21 | S6 Charge | 13 | 34 |
| 22 | S6 Shield | 6 | 12 |
| 23 | S7 Ranged | 13 | 32 |
| 24 | S7 Split | 12 | 30 |
| 25 | S7 Split Small | 8 | 18 |
| 26 | S8 Explode | 17 | 42 |
| 28 | S8 Ranged | 14 | 36 |
| 29 | S9 Surge | 14 | 36 |
| 30 | S5 Snow Boss | 26 | 45 |
| 31 | S6 Thunder Boss | 26 | 52 |
| 32 | S7 Furnace Boss | 10 | 32 |
| 33 | S8 Graft Boss | 14 | 50 |
| 35 | S9 Tree Shell | 14 | 60 |

방패와 작은 분열체는 역할/개체 수를 고려해 낮게 유지. 화로는 최대 열기에서 41 피해(분노 전).
폭주 보스는 각 저장 마법에 기본 공격력 절반인 18과 형태 배율을 적용한다.

## Results
- 정적 비교: damage 18개 외 모든 필드가 HEAD와 동일. 초반, 체력, 치유량, 눈덩이 보존.
- git diff --check 통과.
- Unity 2022.3.62f3 EditMode: 308 통과, 0 실패, 그래픽 장치가 필요한 이미지 렌더링 테스트 1개 건너뜀.
- 실전 플레이 및 도감 육안 확인은 미실시.
- Unity 2022.3.62f3 PlayMode: 139 통과, 0 실패, 14 건너뜀. 결과 XML total 153.
