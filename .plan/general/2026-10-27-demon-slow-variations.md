# 2026-10-27 — 마왕의 느리고 처절한 BGM 3종

- Date: 2026-10-27
- GitHub Issue: None
- Status: Complete (청취 시안)

## Goal
v8의 경쾌함을 줄이고 공포(v9/64 BPM), 비탄(v10/72 BPM), 처절한 저항(v11/80 BPM)의 독립 시안 3개 비교.

## Non-goals
게임 적용, 배포, 기존 시안 덮어쓰기.

## Context / Constraints
GPU 할당 55%, CPU offload, int8, batch 1. 반드시 직렬 생성하고 매 곡 호스트 저장 후 다음 곡 진행. 각 32초, peak -6dB.

## Approach (Checklist)
- [x] **Step 0: Recon** — 이전 설정과 사용자 피드백 확인.
- [x] **Step 1: Implementation** — 세 시안 생성 및 메타데이터 보존.
- [x] **Step 2: Tests** — 디코딩, 피크, 무음 검사 및 청취본 제작.
- [x] **Step 3: Rollout / Rollback** — HTML 청취 목록 추가, 생성기 종료.

## Validation
- **Commands to run:** generate-demon-slow-variations-serial.ps1, ffprobe, ffmpeg, build-sound-library.ps1, git diff --check.
- **Expected output:** 정상 디코딩되는 새 시안 3개와 유효한 HTML 경로.

## Risks & Rollback
- **Risks:** 편성과 분위기는 생성 요청이며 실제 만족도는 사용자 청취로 평가.
- **Rollback steps:** 기존 게임 음원 그대로 유지, 새 시안 미선택.

## Open Questions
- 청취 후 선호 시안.

## Verification results
세 곡 원본 32초, 48kHz stereo, peak -6dB. 청취본 31.48/28.92/28.82초 디코딩 성공. 전체 43개 경로 및 새 시안 3개 확인. 생성기 정상 종료. 게임 파일 변경 없음.
