# 2026-10-27 — 세계수 v5 적용과 처절한 마왕곡

- Date: 2026-10-27
- GitHub Issue: None
- Status: World tree v5 applied; demon v7 listening draft ready

## Goal
승인된 세계수 v5를 보스곡으로 적용한다. 마왕은 사용자 지정 '처절하고 압도적인 느낌'으로 재생성한다.

## Non-goals
마왕곡 자동 적용, 배포, 병렬 생성.

## Context / Constraints
세계수 v5 원본 끝 무음을 제외하고 0.25초 경계 크로스페이드로 반복용 OGG를 생성한다. 새 파일과 meta를 추가하고 WorldTreeHeartStage.bossMusic만 변경한다. 본체 전환 중 같은 곡을 재시작하지 않는 기존 처리 유지.
마왕 v7은 처절한 하행 단조 선율, 낮은 브라스와 묵직한 타악 중심. GPU 할당 55%, CPU 오프로딩, int8, batch 1, 32초, peak -6dB.

## Approach (Checklist)
- [x] **Step 0: Recon** — 세계수 에셋과 음악 전환, 테스트 확인.
- [x] **Step 1: Implementation** — 세계수 v5 새 에셋 연결 및 테스트 기대 경로 갱신.
- [x] **Step 2: Tests** — OGG 검증, 마왕곡 생성·파일 검사 및 HTML 갱신.
- [ ] **Step 3: Rollout / Rollback** — 세계수 로컬 적용, 마왕 청취 대기. 이전 세계수 GUID로 복원 가능.

## Validation
- **Commands to run:** ffprobe, ffmpeg, Get-FileHash, git diff --check.
- **Expected output:** 세계수 v5 OGG 26초 및 올바른 GUID 연결. 마왕 v7 별도 WAV.
- **Results:** 세계수 OGG 26초/48kHz/스테레오/peak -5.9dB, 0.5초 이상 무음 없음, GUID 연결 확인. 마왕 원본 32초/peak -6dB, mean -22.6dB, 끝 무음 제거 청취본 29.5초. HTML 39개 파일 참조 확인. 생성기 종료. Unity 테스트와 실제 게임 청취는 미실행.

## Risks & Rollback
- **Risks:** 실제 Unity 전투와 반복 경계 청취 검증 필요.
- **Rollback steps:** WorldTreeHeartStage.bossMusic을 기존 d6c0978783a7419d9119d229e8ffab73로 복원.

## Open Questions
- 마왕 v7 청취 평가.
