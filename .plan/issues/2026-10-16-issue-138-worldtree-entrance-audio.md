# 2026-10-16 — 세계수 등장 효과음

- Date: 2026-10-16
- GitHub Issue: #138
- Status: Complete

## Goal
먼 반짝임, 가속 비행, 착지와 본체 응집에 동기화한 효과음을 추가한다.

## Non-goals
배경음악/보스 그림/전투 수치/전역 음량 UI 변경.

## Context / Constraints
PR #137은 develop에 병합되어 별도 후속 브랜치로 진행한다. 기존 효과음은 화염 폭발 한 종류라 직접 합성한 짧은 PCM WAV 4개를 사용한다. 기존 전투 소리와 동일하게 2D AudioSource와 AudioListener 음량을 따른다. 연출의 재생 시간과 일시정지/스킵/종료 수명에 묶고 외부 소리나 라이선스 의존성을 추가하지 않는다.

## Approach (Checklist)
- [x] **Step 0: Recon** 등장/응집 시점과 기존 오디오/설정 확인.
- [x] **Step 1: Implementation** 합성 WAV, 연출 전용 오디오와 정리/일시정지 연결.
- [x] **Step 2: Tests** 시점/가속 음량/일시정지/스킵/재생 반복과 전환 취소 검사, 재생 가능한 예제 제공.
- [x] **Step 3: Rollout / Rollback** 병합된 PR #137의 후속 Draft PR에 커밋/푸시와 검증 정보 추가.

## Validation
- **Commands to run:** WAV 피크/길이/무음 경계 검사, Unity 관련 PlayMode 및 EditMode.
- **Expected output:** 반짝임→가속음→착지가 한 번씩 재생되고 본체 응집이 끝나면 소리가 남지 않는다.

## Risks & Rollback
- **Risks:** 기기별 저음 재생 차이와 기존 BGM 대비 체감 음량은 사용자 청취 확인이 필요하다.
- **Rollback steps:** 효과음 커밋을 revert한다.

## Open Questions
없음.

## Result

WAV 6개(게임 4개/미리듣기 2개)의 헤더/클리핑/페이드 끝점 검증 통과. Unity EditMode 308 통과/1 GPU 생략, 관련 PlayMode 13개 통과. 첫 PlayMode 실행의 스킵 실패는 메뉴를 닫은 프레임의 입력 잠금 규칙을 fixture가 반영하지 못한 것이어서 다음 프레임에 스킵하도록 검사만 수정했다. 전체 PlayMode와 플랫폼별 빌드는 재실행하지 않았다.
