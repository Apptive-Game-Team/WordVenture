# 2026-10-27 — 마력 응축 A 적용과 두 보스곡 재생성

- Date: 2026-10-27
- GitHub Issue: None
- Status: A applied; two BGM drafts ready

## Goal
사용자가 선택한 마력 응축 A를 게임에 적용하고, 기존 시안이 별로라는 피드백에 따라 보스곡 두 개를 다시 만든다.

## Non-goals
BGM 자동 교체, 배포, 병렬 생성.

## Context / Constraints
CoreRise의 경로와 meta를 유지해 기존 연출 연결을 보존한다. 이전 파일은 MusicDrafts/audio-originals에 백업한다.
BGM은 반복적인 짧은 저음 패턴 중심에서 벗어나 명확한 테마와 응답 구절을 요청한다. 마왕은 전자 리듬, 세계수는 심포닉 전투곡. GPU 할당 55%, CPU 오프로딩/int8, 배치 1, 32초, peak -6dB, 직렬 저장 후 다음 곡 생성.

## Approach (Checklist)
- [x] **Step 0: Recon** — 효과음 경로와 기존 생성기 확인.
- [x] **Step 1: Implementation** — CoreRise A 복사 및 meta 보존 검증.
- [x] **Step 2: Tests** — 두 곡 직렬 생성 및 길이/피크/끝 무음 검사, HTML에 추가.
- [ ] **Step 3: Rollout / Rollback** — BGM은 청취 후 결정. 효과음은 백업으로 복원 가능.

## Validation
- **Commands to run:** Get-FileHash; ffprobe; ffmpeg; build-sound-library.ps1; git diff --check.
- **Expected output:** CoreRise와 A 파일 해시 동일, meta 불변. 새 BGM 두 곡과 보존된 원본.
- **Results:** CoreRise A 해시 일치/meta 불변/1.45초 확인. 두 BGM 원본 모두 32초/48kHz/스테레오/peak -6dB. 마왕 mean -22dB, 세계수 mean -20.4dB. 청취본은 끝 무음 정리 후 31.3초/26.45초. HTML 37개 참조 확인. 생성기 종료. Unity 연출 재생 및 BGM 주관적 적합성은 미검증.

## Risks & Rollback
- **Risks:** Unity 실제 연출 청취와 새 곡의 적합성은 별도 확인 필요.
- **Rollback steps:** MusicDrafts/audio-originals/CoreRise-before-A.wav를 CoreRise.wav로 복원.

## Open Questions
- 새 보스곡 청취 평가.
