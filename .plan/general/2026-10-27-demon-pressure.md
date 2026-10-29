# 2026-10-27 — 마왕 v8 절박한 긴장감

- Date: 2026-10-27
- GitHub Issue: None
- Status: Complete (청취 시안)

## Goal
v7이 웅장하기만 하다는 피드백을 반영해 작은 편성의 절박한 전투곡을 만든다. 사용자가 떠올린 진격의 거인 BGM은 감정 방향 참고로만 사용하고 원곡 선율은 사용하지 않는다.

## Non-goals
원곡 복제, 게임 자동 적용, 승인된 세계수 v5 변경.

## Context / Constraints
날카로운 현악 반복, 애절하고 불안정한 단조 선율, 짧은 타격과 틈으로 긴장감을 만든다. 팡파르/합창/대형 북/오케스트라 과잉을 배제한다.
GPU 할당 상한 55%, CPU 오프로딩, int8, batch 1, 32초/peak -6dB 유지.

## Approach (Checklist)
- [x] **Step 0: Recon** — 직전 시안과 사용자 피드백 확인.
- [x] **Step 1: Implementation** — 독립 마왕 v8 생성과 원본/메타데이터 보존.
- [x] **Step 2: Tests** — 길이/피크/무음 검사, WAV 청취본 및 HTML 갱신.
- [x] **Step 3: Rollout / Rollback** — 청취 페이지에 제공. 게임 미적용.

## Validation
- **Commands to run:** generate-demon-pressure-serial.ps1, ffmpeg, ffprobe, build-sound-library.ps1.
- **Expected output:** 별도 시안과 정상 디코딩 가능한 청취본.
- **Result:** 원본 32초 / 48kHz stereo / peak -6.0dB. 29.938542초 이후 무음. 청취본 30.1초와 마지막 0.2초 페이드. 생성 종료 후 컨테이너 중지 완료.

## Risks & Rollback
- **Risks:** 요청한 편성과 감정의 구현 정도는 청취 평가 필요.
- **Rollback steps:** 새 시안을 사용하지 않으면 됨.

## Open Questions
- 사용자 청취 평가.
