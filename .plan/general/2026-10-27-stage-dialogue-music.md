# 2026-10-27 — 스테이지 전후 스토리 BGM

- Date: 2026-10-27
- GitHub Issue: None (외부 게시 승인 대기)
- Status: Implemented locally

## Goal
스테이지 입장 및 클리어 대화, 승리/패배 결과와 새 카드 보상 화면에 해당 지역 음악을 재생한다.

## Non-goals
새 음원 제작, 전투 및 보스 음악 변경, 배포.

## Context / Constraints
StageManager는 전투 씬에서만 음악을 재생한다. MapScene에는 맵 BGM이 있지만 GameClearScene에는 AudioSource가 없다.
현재 음악 PR의 지역 곡을 대화 에셋에 명시적으로 연결한다.
맵 BackgroundMusic도 Loop가 꺼져 있어 한 번 재생한 뒤 무음이 된다. 반복을 켜고 대화 동안 일시정지 대상에 포함한다.
추가 요청: 승리/패배 공통 GameClearController가 지역 곡을 씬 수명 동안 소유한다. 읽은 대화를 생략해도 재생하고 새 카드 단계에서도 유지한다. 대화창은 이미 같은 곡을 재생 중이면 재시작하지 않는다. 맵/엔딩으로 씬이 바뀌면 결과 음악도 제거된다.

## Approach (Checklist)
- [x] **Step 0: Recon** — 대화 생명주기, 씬과 기존 오디오 확인.
- [x] **Step 1: Implementation** — 전후 대화에 지역 곡 연결; 대화 소스에서 반복 재생, 기존 반복 음악 일시정지 및 복귀.
- [ ] **Step 2: Tests** — 모든 전후 대화의 곡 연결, 재생 및 복귀, Wave 음악 유지 검증.
- [ ] **Step 3: Rollout / Rollback** — 로컬 검토 후 음악 PR에 반영. 필요시 이 변경만 되돌림.

## Validation
- **Commands to run:** git diff --check; Unity StageMusicAssetTests 및 StageMusicPlayTests.
- **Expected output:** 전후 대화에 지역 곡이 연결되고 전투 도중 대화는 기존 곡을 유지한다.
- **Results:** 정적 검사로 전후 대화 16곳의 지역 곡 일치와 모든 Wave 대화의 음악 미지정 확인. git diff --check 통과. Unity 테스트는 프로젝트 중복 실행 잠금 때문에 실행하지 못했으며 실제 청취도 미확인.
- **Result screens:** 10개 스테이지 모두 결과 음악 참조가 있으며 승리/패배 씬에 공통 컨트롤러가 연결됨을 정적 확인. 결과 음악이 대화 시작/종료에도 유지되는 PlayMode 테스트 추가(미실행). 결과 화면부터 새 카드 보상까지 같은 씬 소스가 재생된다.

## Risks & Rollback
- **Risks:** 씬 전환과 맵 BGM 복귀는 실제 재생 확인 필요. Unity 테스트는 앞서 프로젝트 중복 실행 잠금으로 실행하지 못함.
- **Rollback steps:** 대화 music 필드, 참조와 재생 코드를 함께 복원.

## Open Questions
- 외부 게시 승인 대기.
