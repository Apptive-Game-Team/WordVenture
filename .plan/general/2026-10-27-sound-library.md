# 2026-10-27 — 사운드 청취 HTML

- Date: 2026-10-27
- GitHub Issue: None
- Status: Complete locally

## Goal
프로젝트 사운드와 새 마왕 시안을 한 HTML에서 비교 청취한다.

## Non-goals
외부 게시, 게임 오디오 참조 변경.

## Context / Constraints
HTML은 MusicDrafts에 보존하며 Assets 및 MusicDrafts의 음원을 상대경로로 읽는다. HTML만 다른 폴더로 옮기면 음원 경로가 끊긴다.

## Approach (Checklist)
- [x] **Step 0: Recon** — Assets의 오디오 26개 확인.
- [x] **Step 1: Implementation** — 새 시안 포함 27개 목록, 검색, 분류, 반복, 볼륨, 다운로드, 동시 재생 방지.
- [x] **Step 2: Tests** — 브라우저에서 기존 두 보스곡/신규 시안 재생, 필터, 빈 검색, 전체 정지 확인.
- [x] **Step 3: Rollout / Rollback** — 로컬 파일 제공. 게임 파일에는 의존하지 않는 도구.

## Validation
- **Commands to run:** build-sound-library.ps1; node --check serve-sound-library.mjs; git diff --check.
- **Expected output:** 27곡 카탈로그와 정상 재생.
- **Results:** 생성 및 브라우저 검증 통과. 모든 참조 파일 존재 확인. 미리보기는 loopback 서버로 검증했고 파일 URL 직접 열기는 앱 브라우저 정책상 불가.

## Risks & Rollback
- **Risks:** HTML과 음원 폴더의 상대 위치 유지 필요.
- **Rollback steps:** 새 HTML과 빌더를 사용하지 않으면 됨.

## Open Questions
- 없음.
