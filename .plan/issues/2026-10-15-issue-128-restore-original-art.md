# 2026-10-15 — 픽셀 격자 맞추기(#117) 되돌리기와 픽셀 아트 규칙 삭제

- Date: 2026-10-15
- GitHub Issue: #128
- Status: In Progress

## Goal

- #117 에서 4px 격자에 맞춰 줄이거나 다시 그린 그림을 #117 이전 원본으로 돌린다.
- 앞으로 그림을 격자에 맞추지 않도록 픽셀 아트 규칙과 skill 을 지운다.

## Non-goals

- 원본 그림의 투명 여백 다시 자르기
- 코드로 그리는 작은 연출(#119 의 카드 소각, 적 공격력 상승 아이콘, 버리기 칸) 크기 변경

## Decisions

- #117 merge 를 통째로 `git revert` 하지 않았다. #117 에는 그림을 줄인 것 말고도 지킬 정리가 섞여 있다.
  - 유지: 쓰지 않는 그림 35장과 죽은 prefab, 애니메이션 삭제(`3fb796d`), `KakaoTalk_*` 이미지 139장의 이름 변경(`f09673f`).
  - 되돌림: 초상화, 슬라임, 투사체, 효과, 워드 전투 스프라이트, 배경, 월드맵의 그림 내용과 import 설정, prefab 과 씬의 크기 값.
- 이름이 바뀐 이미지는 #117 이전 그림을 새 이름으로 쓴다. `.meta` 도 #117 이전 내용을 새 이름에 쓰므로 GUID 는 그대로다.
- 투명 여백 자르기(`2183709`)는 줄인 그림에 했던 것이라 원본과 함께 되돌린다. 폭발 효과 크기를 정하는 `BlastVfx`, `ExplodeEnemy` 코드는
  원본에서도 한 칸이 1 unit 이라 그대로 둔다.
- #117 이 지운 2부 슬라임, 맵 길, 효과 생성 스크립트는 #117 이전으로 되돌린다. `docs/design/pixel-grid/` 는 기록으로 남긴다.
- #117 merge 이후 #117 이 바꾼 파일을 다시 고친 변경은 없어서 충돌이 없다.

## Approach (Checklist)
- [x] #117 이 바꾼 Assets 파일 579개를 #117 이전 내용으로 복원(이름 변경과 삭제는 유지)
- [x] `AGENTS.md`, add-slime skill 의 규칙 삭제, pixel-art skill 삭제
- [x] EditMode, PlayMode 전체
- [x] 전후 비교 이미지

## Validation

- EditMode 217개 통과
- PlayMode 67개 중 66개 통과, 실패 0, 건너뜀 1(밸런스 시뮬레이션)
- 전후 비교: `.plan/assets/2026-10-15-issue-128-restore-original-art/pixel-before-after.png` (왼쪽 #117, 오른쪽 복원)

## Risks & Rollback

- 그림마다 픽셀 크기가 다른 #116 이전 상태로 돌아간다.
- #119(#120), #125, #126 브랜치는 #117 이 들어간 develop 을 합친 상태라, 이 pull request 가 병합된 뒤 develop 을 다시 합쳐야 원본 그림이 들어간다.
- 되돌리기: 이 commit 을 `git revert`
