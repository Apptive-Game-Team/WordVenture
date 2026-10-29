# 2026-10-27 — 세계수 마력 응축 효과음 비교안

- Date: 2026-10-27
- GitHub Issue: None
- Status: A applied locally by user selection

## Goal
마력 응축 소리에 대한 불만족 피드백에 따라 귀를 찌르는 질감을 피한 두 비교안을 제공한다.

## Non-goals
게임 효과음 교체, BGM 변경, GPU 생성.

## Context / Constraints
현재 CoreRise는 1.45초/22.05kHz/모노. GatherStartAt=1.05, GatherAt=1.7이므로 시작 0.65초 뒤 본체 등장에 맞춰 합성음이 가장 강해지게 한다.

## Approach (Checklist)
- [x] **Step 0: Recon** — 소스와 전환 연출의 재생 타이밍 확인.
- [x] **Step 1: Implementation** — 낮은 공명과 맑은 공명 두 CPU 합성 WAV, 원본 보존.
- [x] **Step 2: Tests** — 길이/피크 검사, HTML 참조 확인.
- [x] **Step 3: Rollout / Rollback** — 사용자 선택 A를 CoreRise.wav에 적용. 경로/meta 유지 및 원본 백업.

## Validation
- **Commands to run:** node generate-core-rise-variants.mjs; ffprobe; ffmpeg volumedetect; build-sound-library.ps1.
- **Expected output:** 1.45초/48kHz/모노, -9dB peak WAV 두 개. 파형 시작과 끝은 부드럽게 감쇠.
- **Results:** 두 파일 모두 1.45초/48kHz/모노/peak -9.0dB. 낮은 공명 mean -19.6dB, 맑은 공명 mean -19.9dB. HTML 목록 35개로 갱신. 음색 청취와 실제 게임 연출 검증은 미완료.

## Risks & Rollback
- **Risks:** 실제 연출과 BGM에 섞었을 때의 적합성은 청취 검증 필요.
- **Rollback steps:** 새 시안을 사용하지 않으면 됨.

## Open Questions
- 선호하는 공명 질감.
