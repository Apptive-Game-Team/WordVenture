# 2026-10-19 — 스테이지 및 보스전 배경음악 게임 적용

- Date: 2026-10-19
- GitHub Issue: #142
- Status: Completed locally; draft PR

## Goal
승인한 스테이지 10곡과 보스전 2곡을 실제 게임에 연결한다.

## Non-goals
다른 장면 음악과 전투 규칙 변경, 새 곡 생성.

## Context / Constraints
Unity 2022.3.62f3. 반복용 OGG를 사용한다. 루프는 요청 BPM 기반 초안이므로 음악적 연결은 플레이 청취로 최종 확인한다. 기존 음량 설정을 유지하고 기존 AudioSource의 0.5 pitch는 새 곡에서 1로 보정한다. 무관한 패키지 변경은 보존한다.

## Approach (Checklist)
- [x] **Step 0: Recon** StageManager, EnemyPoolController, 보스 출현과 세계수 본체 전환 확인.
- [x] **Step 1: Implementation** Assets/Audio/StageMusic 에 12곡과 meta 추가, StageData에 bossMusic 추가, 소환 시 전환.
- [x] **Step 2: Tests** 스테이지 연결, 일반 적/보스 소환, 반복과 중복 전환을 Unity 테스트로 확인.
- [x] **Step 3: Rollout / Rollback** 데이터 연결과 변경 파일만 추적, 관련 파일 복원으로 롤백.

## Validation
- **Commands to run:** Unity 2022.3.62f3 batchmode EditMode/PlayMode focused tests, git diff --check.
- **Expected output:** 10곳 정상 music, 두 곳 bossMusic, 보스 소환시 반복 곡 전환과 중복 재시작 없음.

## Risks & Rollback
- **Risks:** 루프의 음악적 연결은 청취 확인 필요. 실제 출력 장치 청취는 자동 테스트가 증명하지 않는다.
- **Rollback steps:** 해당 C# 변경과 StageData music 연결, 추가한 Audio 폴더를 복원한다.

## Open Questions
- 없음.

## Results
- Assets/Audio/StageMusic: 12개 OGG (약 8 MB), 검토 페이지의 반복용 파일과 SHA256 일치.
- 모든 StageData.music 연결, 마왕 성과 세계수의 심장만 bossMusic 지정.
- EnemyPoolController의 실제 소환 경로에서 StageManager.OnEnemySpawned 호출. BossEnemy 계열이고 전용곡이 있을 때만 전환한다.
- AudioSource.loop=true, pitch=1; 기존 volume 유지. 같은 곡 알림은 재생 위치를 유지한다.
- Unity 2022.3.62f3 EditMode StageMusicAssetTests: 10/10 통과 (music-editmode-results.xml).
- Unity 2022.3.62f3 PlayMode StageMusicPlayTests: 3/3 통과 (music-playmode-results.xml). 실제 보스 id 15/35 소환 및 세계수 본체 id 29 소환을 포함한다. 도감 저장값은 테스트 후 복원한다.
- git diff --check 통과. Hindsight는 home:8888 연결 시간 초과로 조회/기록하지 못했다.
- 실제 스피커 청취와 전체 전투 진행을 통한 루프 감상은 자동 테스트 범위 밖이다. 기존 청취 페이지에 반복용 초안으로 공개된 편집을 그대로 적용했다.
- 테스트 첫 실행 중 중복 실행 하나는 프로젝트 잠금으로 종료됐다. 원래 실행은 EditMode 10개 통과했고, 이후 PlayMode도 종료 코드 0이다.
## 2026-10-27 후속 변경
사용자 승인으로 기존 PR #143에 튜토리얼 화자·기록 연결 문구, 전후 스토리와 승패·새 카드 화면 BGM을 포함한다. 마왕 등장 곡은 기존 Pixel 5.wav, 세계수 보스는 v5, 마력 응축은 A로 확정했다. 생성 후보와 로컬 청취 페이지는 커밋하지 않는다. 기존 Non-goals의 다른 장면 음악 제외는 이 후속 요청으로 변경되었다.

검증: 최신 Unity EditMode/PlayMode 집중 테스트 결과를 PR 본문과 별도 music-update 결과 XML로 기록한다. 과거 Results는 최초 커밋의 결과이며 최신 코드 통과를 의미하지 않는다.
최신 결과: Unity 2022.3.62f3 EditMode 24/24, PlayMode 10/10 통과. 튜토리얼 기록 화면은 .plan/assets/stage-boss-music/tutorial-record.png에 보존. git diff --cached --check 통과.
