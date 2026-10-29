# 2026-10-27 — 보스 음악 바리에이션 4곡

- Date: 2026-10-27
- GitHub Issue: None
- Status: Listening drafts ready

## Goal
기존 두 보스 시안에 대한 불만족 피드백에 따라, 각 보스에 서로 다른 방향의 시안 두 곡씩 제공한다.

## Non-goals
게임 적용, 기존 파일 덮어쓰기, 병렬 생성.

## Context / Constraints
GPU 할당 상한 55%, CPU 오프로딩/int8, 보조 LM 비활성화, 32초·배치 1, 피크 -6dB 유지.
사용자 지시대로 생성 완료와 호스트 저장을 확인한 뒤 다음 곡을 시작한다.

## Approach (Checklist)
- [x] **Step 0: Recon** — 기존 생성기와 카탈로그 확인.
- [x] **Step 1: Implementation** — 마왕 행진/추격, 세계수 의식/최종전 4곡 직렬 생성.
- [x] **Step 2: Tests** — 길이·피크·무음 검사 후 WAV 청취본 및 HTML 제공.
- [ ] **Step 3: Rollout / Rollback** — 청취 평가 대기. 기존 게임 곡 유지.

## Validation
- **Commands to run:** generate-boss-variations-serial.ps1, ffprobe, ffmpeg, build-sound-library.ps1.
- **Expected output:** 4개 원본과 생성 메타데이터, 비교 가능한 청취본.
- **Results:** 4개 원본 각 32초, peak -6.0dB. 끝 무음을 제거한 WAV 청취본은 행진 30.2초/추격 27.2초/의식 28.65초/최종전 29.1초. HTML 33개 파일 참조 확인. 4개 생성·호스트 저장을 순서대로 완료 후 생성 컨테이너 종료. 음악적 적합성은 청취 평가 대기.

## Risks & Rollback
- **Risks:** 원하는 장르/음색 준수 여부는 청취 평가가 필요하다.
- **Rollback steps:** 새 시안을 사용하지 않으면 됨.

## Open Questions
- 사용자 선호 시안.
