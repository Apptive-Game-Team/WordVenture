# Issue 56: Spawn 주문과 아군 슬라임

- Issue: https://github.com/Apptive-Game-Team/WordVenture/issues/56
- Branch: `feat/spawn-ally-slime`

## 범위

이번 PR은 Spawn 주문, 아군 슬라임, 적 공격이 아군 슬라임에게 맞는 처리를 만든다.
적 슬라임 6종은 2부 스테이지 데이터가 생긴 뒤 다음 PR에서 만든다. 지금 적 pool은
`enemyData.id / 3 == stagePosition`인 적만 만들기 때문에, 스테이지가 없으면 새 적이
게임에 나오지 않는다.

## 규칙

- `MagicType.Spawn`을 enum 맨 끝(값 9)에 추가한다. 카드 데이터가 enum을 정수로 저장한다.
- Spawn 카드는 속성 카드와 조합하고, 대상을 고르지 않는다.
- 아군 자리는 3칸이다. 먼저 소환한 슬라임이 맨 앞(적과 가장 가까운 칸)에 서고, 나중에
  소환한 슬라임이 그 뒤에 선다. 맨 앞 슬라임이 쓰러지면 뒤 슬라임이 한 칸씩 앞으로 나온다.
- 3칸이 모두 차면 Spawn 조합 버튼이 나오지 않는다.
- 아군 슬라임은 HP 8, 공격력 4다. 적 슬라임은 HP 10~30, 공격력 10이다.
- 플레이어 턴 종료 버튼을 누르면, 적 턴 전에 아군 슬라임이 앞줄부터 차례로 가장 앞의
  적을 한 번씩 공격한다. 피해는 `Enemy.TakeSpellHit`으로 줘서 상성표와 속성 상태가 적용된다.
- 신성 슬라임은 적을 공격하지 않고 워드를 4 회복한다. 상성표에서 신성은 언데드를 뺀 모든
  속성에 음수라서, 공격하면 적이 회복된다.
- 적은 워드 대신 가장 앞줄 아군 슬라임까지의 거리로 이동과 공격을 결정한다.
  근접 공격은 가장 앞줄 슬라임이 받고, 직선 원거리 탄은 처음 닿은 슬라임이 받는다.
- Spawn 카드는 스테이지 5(2부 첫 지역)부터 손패에 섞인다. 에디터에서는 S 키로 손패에
  Spawn 카드를 한 장 넣는다.

## 변경 파일

- `Scripts/Combat/Allies/AllySlime.cs`, `AllyFormation.cs` 신규
- `Resources/Combat/AllySlime.prefab` 신규 (`RangedLight.prefab` 기반)
- `Cards/Card.cs`: enum 값 추가
- `Cards/CardManager.cs`: Spawn 출현 확률, 에디터 S 키
- `ScriptableObjects/Cards/WordList.asset`: Spawn 단어 추가
- `Combat/UI/CombineZone.cs`: Spawn 시전, 자리가 찼을 때 버튼 숨김
- `Battle/Turns/TurnBattleSystem.cs`: 턴 종료 때 아군 공격 단계
- `Combat/Enemies/Enemy.cs`, `SwordEnemy.cs`, `BossEnemy.cs`, `MagicEnemy.cs`,
  `EnemyProjectile.cs`, `EnemyTestManager.cs`: 공격 대상을 아군 슬라임까지 확장
- 테스트: EditMode(enum 값, 단어 데이터, prefab 연결), PlayMode(자리, 피해, 공격)

## 검증

- Unity 2022.3.34f1 batchmode로 EditMode·PlayMode 테스트 실행
- 에디터에서 TurnBattleScene 실행 후 S 키로 Spawn을 받아 소환, 턴 종료, 피격 확인

## 위험

- 적 거리 기준이 바뀌어서 아군이 있을 때 근접 적이 더 앞에서 멈춘다. 아군이 없으면 기존과 같다.
- 턴 종료 버튼을 누른 뒤 아군 공격이 끝날 때까지(슬라임 한 마리당 0.4초) 적 턴이 늦게 시작한다.
