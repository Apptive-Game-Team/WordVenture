# 2026-10-27 — 두 보스곡의 깨끗하고 압도적인 시안

- Date: 2026-10-27
- GitHub Issue: None
- Status: Listening drafts ready

## Goal
마왕과 세계수 전용곡을 모두 압도적인 분위기와 깨끗한 음색으로 다시 생성한다.

## Non-goals
병렬 생성, 기존 음원 덮어쓰기, 게임 적용 또는 배포.

## Context / Constraints
마왕 v2에서 소리가 깨지는 느낌이라는 사용자 피드백. 출력 WAV/OGG 피크는 약 -3dB로 최종 출력 클리핑은 미확인이나 음원 내부 왜곡 가능성이 있다.
사용자가 두 곡을 반드시 직렬 생성하도록 지시했다. GPU 메모리 할당 상한 55%, CPU 오프로딩과 int8, 보조 LM 없음, 32초·배치 1을 유지한다.

## Approach (Checklist)
- [x] **Step 0: Recon** — 기존 생성 설정과 원본/OGG 피크 확인.
- [x] **Step 1: Implementation** — 마왕 v3 생성·저장 후 세계수 v2 생성·저장. 거친 저음 대신 자연스러운 호른·더블베이스·팀파니, 피크 -6dB.
- [x] **Step 2: Tests** — 길이/피크/무음 검사. 청취본을 HTML에 추가.
- [ ] **Step 3: Rollout / Rollback** — 원본 보존. 청취 확인 전 게임 참조는 유지.

## Validation
- **Commands to run:** ffprobe, ffmpeg volumedetect/silencedetect, build-sound-library.ps1.
- **Expected output:** 독립된 두 WAV와 요청/실제 생성 정보, 청취본 2개. 음색 적합성은 청취로 확인한다.
- **Results:** 두 원본 모두 32초/48kHz/스테레오, peak -6.0dB. 마왕 mean -24.2dB, 세계수 mean -22.9dB. 각 끝 무음 3.01초/2.73초를 정리한 무손실 WAV 청취본 29.15초/29.45초 제공. HTML 29개 참조 파일 모두 존재 확인. 두 곡은 완료 및 호스트 저장을 확인한 뒤 순서대로 생성했고, 완료 후 컨테이너 종료.

## Risks & Rollback
- **Risks:** 생성된 음색의 거친 질감은 피크 검사만으로 판단할 수 없다. GPU 메모리 제한은 연산 사용률 제한과 다르다.
- **Rollback steps:** 새 시안을 사용하지 않으면 됨.

## Open Questions
- 두 시안의 청취 평가.
