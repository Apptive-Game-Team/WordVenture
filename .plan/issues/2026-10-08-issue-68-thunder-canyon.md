# Issue 68: 벼락 협곡과 곡사·돌진·방패 슬라임

- Issue: https://github.com/Apptive-Game-Team/WordVenture/issues/68
- Branch: `feat/thunder-canyon` (PR #66 `feat/act-two-frost-village` 위)

## 적 행동 구조

- `Enemy.PlayTurnAction`은 빙결 검사 뒤 `TakeTurnAction`을 부른다. 기본 동작(사거리 밖이면 이동,
  안이면 공격)은 그대로이고, 예고 후 공격하는 적이 이 메서드를 덮어쓴다. 빙결된 턴에는
  `TakeTurnAction`이 불리지 않으므로 예고도 공격도 하지 않는다.
- `Enemy.SetIntent`는 다음 턴 행동의 예고를 머리 위 글자로 보여 준다. 속성 반응 글자가 떠 있는
  1.5초 동안은 반응 글자가 먼저 보인다.

## 새 적

| 적 | 클래스 | 행동 |
| --- | --- | --- |
| 곡사 슬라임 | `MortarEnemy` | 표시가 없으면 맨 뒤 아군 슬라임(없으면 워드) 위치에 착탄 표시를 놓는다. 표시가 있으면 그 자리에 공격력만큼 피해를 주고 표시를 지운다. 표시 0.7 unit 안의 아군 슬라임이 맞고, 없으면 워드가 그 안에 있을 때 맞는다 |
| 돌진 슬라임 | `ChargeEnemy` | 맨 앞 아군 칸(없으면 워드)까지 6 unit 안이면 "돌진 준비"를 표시한다. 다음 턴에 그 1.2 unit 앞까지 0.3초에 달려와 들이받는다. 아군 슬라임이 막으면 피해 2배 |
| 방패 슬라임 | `ShieldEnemy` (`SwordEnemy` 상속) | 근접 공격. 오른쪽 2.5 unit 안에 선 다른 적이 받는 주문 피해를 절반으로 줄이고 "방패 보호"를 표시한다 |

- 새 prefab은 기존 슬라임 prefab을 복사해 스크립트만 바꿨다. 곡사는 RangedPurple, 돌진은
  MeleeCrab, 방패는 MeleeDark(1.3배 크기)다.
- 착탄 표시 그림 `Assets/Art/Combat/MortarMarker.png`는 빨간 타원(96×32)이다.

## 벼락 협곡

- 적 데이터 id 19~22: 번개 원거리(HP 30), 곡사(HP 25, 이동 0), 돌진(HP 35), 방패(HP 60, 공격력 5).
- 웨이브: [돌진, 원거리] → [방패, 원거리, 곡사] → [방패, 돌진, 곡사].
- 배경과 음악은 빗길 스테이지 것을 임시로 쓴다.
- 입장·클리어 대화를 추가했고, 2부 맵에서 들어갈 수 있는 마지막 스테이지를 6으로 올렸다.

## 검증

- EditMode: 스테이지 6 데이터와 prefab 연결, 곡사 표시 그림, 대화
- PlayMode: 곡사 표시와 착탄(아군 있음·없음), 빙결 시 표시하지 않음, 돌진 예고와 2배 피해,
  방패 뒤 피해 절반, `StagePosition` 7에서 잿빛 유적이 잠겨 있음
