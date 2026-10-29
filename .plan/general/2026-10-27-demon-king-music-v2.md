# 2026-10-27 — 마왕 등장 BGM 위압감 강화

- Date: 2026-10-27
- GitHub Issue: None
- Status: Listening draft ready

## Goal
사용자가 선택한 '등장 순간부터 강렬한 위압감'에 맞춰 액트 1 마왕 전용곡 시안을 만든다.

## Non-goals
세계수 보스곡 및 일반 지역곡 변경, 청취 전 기존 곡 덮어쓰기, 배포.

## Context / Constraints
기존 v1은 절제된 낮은 건반 중심이며 opening flourish를 금지했다. 새 시안은 첫 박부터 저음 브라스, 묵직한 베이스와 타악기로 압박감을 준다. 기존 로컬 ACE Step 생성기를 사용한다.

## Approach (Checklist)
- [x] **Step 0: Recon** — 연결된 보스곡과 기존 생성기 확인.
- [x] **Step 1: Implementation** — 독립 v2 원본, 요청 및 생성 메타데이터 보존.
- [x] **Step 2: Tests** — 길이, 피크, 무음 확인 및 청취용 파일 제공.
- [ ] **Step 3: Rollout / Rollback** — 청취 후 적용 여부 결정. 기존 v1 유지.

## Validation
- **Commands to run:** ffprobe, ffmpeg volumedetect/silencedetect.
- **Expected output:** 정상 디코딩되는 음원과 요청 메타데이터. 음악적 적합성은 사용자 청취로 확인.
- **Results:** 32초, 48kHz 스테레오 WAV, peak -3.0dB, mean -19.1dB. 28.73초부터 끝 무음이 있어 별도 28.9초 OGG 청취본을 만들었다. 원본 보존, 게임 곡 미교체.
- **Resource settings:** 사용자 요청에 따라 PyTorch GPU 할당 상한 55%, CPU/DiT 오프로딩, int8_weight_only, 보조 LM 비활성화, 배치 1, 32초. 생성 성공 후 GPU 전체 사용량 약 623MiB 확인. 임시 생성 컨테이너 종료 완료. 최초 4B LM 시도는 GPU OOM, 이후 WSL 연결 중단으로 재시도했다.

## Risks & Rollback
- **Risks:** 모델이 원하는 첫 박과 편곡을 정확히 따르지 않을 수 있음.
- **Rollback steps:** 기존 곡은 변경하지 않으므로 새 초안을 사용하지 않으면 됨.

## Open Questions
- 시안 청취 평가.
