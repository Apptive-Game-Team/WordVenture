# 2026-10-14 — 사용 이미지 전체를 화면 4px 픽셀 격자로 통일하고 안 쓰는 이미지 삭제

- Date: 2026-10-14
- GitHub Issue: #116
- Status: Draft

## Goal

- 빌드에서 쓰는 모든 이미지를 아트 1픽셀 = 1080p 화면 4px(480x270 가상 화면)로 보이게 하고, 1텍셀 = 1아트픽셀로 저장한다.
- 빌드에서 도달할 수 없는 이미지 35장과, 그 이미지만 가리키는 죽은 에셋 5개를 삭제한다.
- SpriteRenderer 스프라이트의 투명 여백을 잘라내고 pivot을 보정한다.
- 격자 없는 AI 이미지는 Codex image_gen으로 다시 그린다.
- 이미지를 만드는 스크립트도 같은 기준으로 출력하게 한다.

## Non-goals

- 그림의 내용·화풍 변경, 게임 로직·UI 배치 변경. 화면 크기를 유지하기 위한 수치 보정만 한다.
- 폰트, TextMeshPro 아틀라스, WebGL 템플릿 아이콘.
- #115(`fix/pixel-perfect-backgrounds`)를 합치는 것. 그 브랜치는 결과물만 재사용하고 PR은 닫는다.

## Context / Constraints

- 기준 브랜치는 develop `4c31200`. 이전 브랜치는 10/09 main(`b7c2ed0`) 기준이라 develop의 새 에셋을 다루지 못했다.
- 모든 빌드 씬의 카메라는 orthographic size 5(화면 1080px에 10월드 유닛, 108px/unit), Canvas Scaler는 1920x1080 기준이다.
- 대화 초상화는 화면 높이 63% 영역에 `preserveAspect`로 그린다 → 170x170 아트 픽셀. 여백을 자르면 화면 크기가 바뀌므로 trim 대상에서 뺀다.
- 2부 슬라임은 `docs/design/act-two-art/build_slimes.py`가 128칸 격자를 4배로 키워 512x512로 쓰고, `ActTwoCompleteDataTests`가 프레임을 manifest와 비교한다. PNG만 바꾸면 스크립트를 다시 돌릴 때 되돌아간다.
- develop `8b1d7ad`의 `regrid_backgrounds.py`는 배경을 미리 확대한 PNG(예: 224x126 격자 x8 = 1792x1008)로 저장한다. 이 기준과 충돌한다.
- b7c2ed0 이후 develop에서 바뀌지 않은 에셋(대화 초상화 8장, 로고, 타이틀 머리, NewCardTitle, 평원 전투 배경, 스테이지 배경 3종)은 #115의 결과를 재사용할 수 있다. 쓰임새가 바뀌었을 수 있으므로 측정 결과와 대조한 뒤 쓴다.
- Unity 2022.3.62f3가 Windows에 설치되어 있다(`/mnt/c/Program Files/Unity/Hub/Editor/2022.3.62f3`). add-slime skill 절차대로 `C:\temp` 사본에서 batchmode로 돌린다.

## Approach (Checklist)

- [ ] **Step 0: Recon**
  - [x] `audit_textures.py`: 빌드 씬·Resources·스크립트 경로 문자열·ProjectSettings에서 GUID로 도달하는 이미지를 계산 → 344장 중 309장 사용, 35장 미사용
  - [ ] 런타임 코드에서 스프라이트 크기·bounds를 쓰는 곳 목록(`sprite.bounds`, `sprite.rect`, `SetNativeSize`, `transform.localScale` 직접 계산 등). trim과 PPU 변경의 영향 범위를 정한다.
- [ ] **Step 1: 도구와 문서 이식** — `.agents/skills/pixel-art/`(SKILL.md, `pixelate.py`, `regen-candidates.md`, `audit_textures.py`), `AGENTS.md`의 pixel-art 안내 한 줄, `background-generation.md` 기록.
- [ ] **Step 2: 미사용 삭제** — 이미지 35장과 `.meta`, 죽은 에셋 5개(`BlackProjectile.prefab`, `DarkRedProjectile.prefab`, `RedProjectile.prefab`, `Spells/Creature/Player.prefab`, `GameOverBackground.anim`·`.controller`). 삭제 뒤 `audit_textures.py`로 깨진 참조가 없는지 확인.
- [ ] **Step 3: 측정** — batchmode Editor 스크립트 `PixelGridReport.cs`(skill `scripts/`에 두고 실행할 때만 사본의 `Assets/Editor/`로 복사)로 빌드 씬과 프리팹의 SpriteRenderer·Image·애니메이션 스프라이트마다 텍셀당 화면 px(가로·세로)를 JSON으로 출력. 런타임에 크기를 정하는 것은 코드에서 읽어 수동 항목으로 추가.
- [ ] **Step 4: 목표 크기 결정** — 이미지마다 목표 아트 크기 = 텍스처 크기 x 텍셀당 화면 px / 4. 여러 배율로 쓰이는 이미지, 균일하지 않은 스케일, 9-slice 테두리는 표로 따로 정리.
- [ ] **Step 5: 변환**
  - 진짜 픽셀아트: `pixelate.py --screen-px-per-texel <값> --update-meta`(정수배 근처면 스냅).
  - 생성 스크립트: `build_slimes.py`가 128x128 그대로 쓰고 PPU를 맞추도록 수정, manifest·`ActTwoCompleteDataTests` 갱신. `regrid_backgrounds.py`는 이 기준으로 바꾸거나 제거.
  - 격자 없는 이미지: Codex로 재생성. 새 대화 초상화 17장, 2부 배경, 맵, 게임오버, `SurgeSlime` 등은 측정 뒤 목록 확정. 재사용 가능한 #115 결과는 대조 후 가져옴.
- [ ] **Step 6: Trim** — SpriteRenderer에만 쓰이는 스프라이트의 투명 여백을 잘라내고 pivot을 보정(시트는 rect별). UI Image, `preserveAspect`, 9-slice, bounds를 읽는 코드가 있는 스프라이트는 제외하고 목록에 이유를 남긴다.
- [ ] **Step 7: 검증** — EditMode·PlayMode 테스트, 바뀌기 전후 씬 캡처 비교, 측정 보고서 재실행(모든 항목 4px 확인).

### 병렬 트랙(쓰기 범위가 겹치지 않는 것만)

- A. 새 대화 초상화 17장 Codex 재생성 — `Assets/Art/Story/Portraits/`의 해당 17장 PNG·`.meta`만 쓴다.
- B. 측정 스크립트와 보고서 — skill `scripts/PixelGridReport.cs`와 scratch 출력만 쓴다.
- C. 미사용 삭제와 도구 이식 — 삭제 대상과 `.agents/`, `AGENTS.md`만 쓴다.
- Step 4~6은 B의 결과에 의존하고 같은 PNG·`.meta`를 쓰므로 직렬로 진행한다.

## Validation

- **Commands to run:**
  - `python3 .agents/skills/pixel-art/scripts/audit_textures.py --summary`
  - Unity batchmode EditMode·PlayMode(`-runTests`, add-slime skill의 명령)
  - `PixelGridReport.cs` 재실행 → 모든 사용 항목의 텍셀당 화면 px가 4(허용 오차 2%)
- **Expected output:** 미사용 0장, 테스트 전부 통과, 측정 보고서 예외 목록이 문서와 일치.

## Risks & Rollback

- **Risks:**
  - 런타임에 크기를 정하는 오브젝트(카드, 상태 오버레이, 튜토리얼 손)는 측정 스크립트가 못 잡는다 → 코드에서 읽고 PlayMode 캡처로 확인.
  - trim이 `sprite.bounds`를 쓰는 코드의 결과를 바꿀 수 있다 → Step 0 목록에 걸린 스프라이트는 제외.
  - 미사용 판정이 놓친 로드 경로(문자열 조합 등) → 삭제 전 `Resources`·경로 문자열 재확인, 테스트 통과로 확인.
  - develop이 진행되며 같은 이미지를 고치면 충돌 → 작업 끝에 develop을 합치고 이 기준으로 해결.
- **Rollback steps:** PR revert. 삭제한 에셋은 git 이력에 남아 복구 가능.

## Open Questions

- 카드 런타임 스케일 `(0.379, 0.42)`처럼 균일하지 않은 스케일은 이번 범위에서 고칠지(현재 계획: 측정 보고서에 예외로 남기고 고치지 않음).
