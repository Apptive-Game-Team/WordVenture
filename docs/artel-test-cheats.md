# ARTEL 테스트 치트

개발 빌드의 ARTEL QA 에이전트는 `invoke_cheat`로 아래 함수를 호출할 수 있다.
SDK가 `ARTEL_GAME_FUNCTIONS`를 설정하면 자동 등록된다. 출시 빌드에는 이 코드가 포함되지 않는다.
현재 CLI에는 직접 cheat 호출 명령이 없으므로 개발 빌드를 다시 만들어 ARTEL에 연결해야 한다.

| 이름 | 인자 | 용도 |
|---|---|---|
| `prepare_test_save` | `progress`, `confirmOverwrite`, `tutorialEnded`, `showActOneMap` | 타이틀에서 로컬 저장 준비 |
| `enter_test_battle` | `stage` | 맵에서 해금된 스테이지 0~9 전투로 이동 |
| `return_to_test_title` | 없음 | 현재 진행도를 저장하고 타이틀로 복귀 |
| `damage_all_enemies` | `damage` (1~1,000,000) | 현재 살아 있는 적 전체에 직접 피해. 속성/방패 배율을 적용하지 않음 |
| `kill_all_enemies` | 없음 | 현재 웨이브 적과 분열 자식을 정상 사망 처리로 전멸 |

`prepare_test_save`는 기존 대화/도감 기록을 초기화한다. 폐기 가능한 테스트 저장에서
`confirmOverwrite=true`를 명시해야 한다. 계정이나 서버 저장은 변경하지 않는다.
진행도 -1은 새 저장, 0~9는 다음 미완료 스테이지, 10은 전체 완료다.
`tutorialEnded` 기본값은 true지만 진행도 -1에서는 튜토리얼 완료 기록을 만들지 않는다.
`showActOneMap=true`는 1부 완료(진행도 5 이상)일 때만 허용된다.

## TC 준비 예시

- 1457: 타이틀에서 `prepare_test_save(progress=5, confirmOverwrite=true)` 후 UI의 이어하기로 맵 진입.
- 1459~1461: `prepare_test_save(progress=7, confirmOverwrite=true)` 후 이어하기와 장 전환 UI 사용.
- 1462: `prepare_test_save(progress=10, confirmOverwrite=true)` 후 이어하기.
- 1464: `prepare_test_save(progress=0, confirmOverwrite=true)` 후 이어하기. 전투 전에 도감을 연다.
- 1468: `prepare_test_save(progress=8, confirmOverwrite=true)` 후 이어하기. 맵이 기존 저장의 도감 기록을 복구한다.
- 전투 테스트: 저장을 준비하고 이어하기로 맵 진입, 메뉴/대화를 닫은 뒤 `enter_test_battle(stage=...)` 호출.

전투 직행은 맵 진입 대화를 생략한다. 대화 자체를 검사하는 TC는 정상 맵 입장 UI를 사용한다.
치트는 사전 조건 준비용이며 TC 통과 판정을 대신하지 않는다. 현재 실행 중인 예전 빌드에는 자동 반영되지 않는다.

`damage_all_enemies(damage=10)`은 호출 시 살아 있던 적만 한 번씩 타격한다. 사망 연쇄로
이미 쓰러진 적은 다시 때리지 않으며 새로 분열한 자식은 이번 피해 대상에서 제외한다.
`kill_all_enemies()`는 분열 자식까지 처리한 뒤 멈추며 다음 웨이브는 정상 게임 로직이 연다.
둘 다 메뉴/대화 중, 전투 밖, 살아 있는 적이 없을 때는 거절한다.
피해량·사망 효과·처치 자체를 검증하는 단계에는 사용하지 않는다.

## 검증 상태

2026-10-27: Unity 2022.3.62f3의 Roslyn으로 치트 및 테스트 C# 컴파일을 통과했다.
별도 Unity 테스트 프로세스는 프로젝트가 다른 에디터에서 열려 있어 실행되지 못했다.
열린 에디터의 Test Runner에서 `ArtelTestCheatsPlayTests`를 실행한 뒤 개발 빌드를 만들고
ARTEL cheat catalog에 위 5개 함수가 나타나는지 확인해야 한다.

2026-10-29: main 리베이스 후 PlayMode 배치 검증을 재시도했으나 Unity 라이선스
클라이언트 연결 단계에서 진행되지 않아 테스트 결과를 얻지 못했다. 소스와 테스트는
SDK가 제공하는 심볼로 활성화하며, 개발자 PC의 로컬 SDK 절대 경로와 프로젝트 설정은
커밋에 포함하지 않는다.
