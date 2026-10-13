# 2026-10-13 — 화면별 UI 색상과 폰트를 나무색 픽셀 스타일로 통일

- Date: 2026-10-13
- GitHub Issue: #105
- Status: Complete

## Goal

- 버튼은 밝은 나무, 메뉴 판은 밝은 나무 판, 정보 패널은 어두운 나무 + 금색 테두리 + 크림색 글자로 통일한다.
- 런타임에 UI 를 만드는 스크립트가 같은 팔레트 정의(`Core.UiPalette`)를 쓴다.
- 프로젝트의 TMP 텍스트를 `NeoDunggeunmoPro` 로 통일한다.
- 카드, 조합창, HP 표시를 같은 스타일로 새로 그린다.

## Non-goals

- 캐릭터 아트와 전투 이펙트
- 중복된 `Button` prefab(`Prefabs/Common`, `Prefabs/UI`) 정리
- `docs/store/screenshots` 스토어 이미지 교체

## Decisions

- 화면별 디자인 시안(1920x1080)을 먼저 만들어 사용자와 확정했다: 구성 요소, 타이틀, 맵, 대화, 전투(조합창 열림·닫힘), 새 카드, 게임 오버, 설정, 튜토리얼, 카드 8종, HP 표시 3안. 시안은 `.plan/assets/2026-10-13-issue-105-unify-ui-style/mockups/` 에 있다.
- 버튼은 밝은 나무(누르는 것), 메뉴 판은 밝은 나무 판(버튼을 모아 두는 곳), 정보 패널은 어두운 나무 + 금색 테두리(읽는 것)로 나눈다.
- 기준 팔레트는 이미 어두운 나무색을 쓰던 `SlimeEncyclopediaView` 의 상수를 `Core.UiPalette` 로 옮긴다. 디자이너가 조정하는 값은 기존 ScriptableObject 인 `DialogueWindowStyle.asset` 에 남긴다.
- 원본 아트(`Assets/ThirdParty/ArtResource/JinWook`)는 덮어쓰지 않는다. 고친 사본을 `Assets/Art` 에 두고 참조를 바꾼다.
- 버튼은 좌우 대칭으로 새로 그린 62x30 픽셀 `WoodButton` 과 40x18 픽셀 `WoodButtonSmall` 을 9-slice 로 쓰고, 상태(마우스 올림·눌림·사용 불가)는 SpriteSwap 으로 보여 준다. 기존 Start 버튼 그림은 오른쪽·아래 테두리만 두꺼운 비대칭 모양이었다.
- UI 그림은 point filter, PPU 100 이고 `pixelsPerUnitMultiplier` 0.25 로 써서 1080p 에서 1 픽셀이 4 픽셀로 보인다.
- 글자가 그림에 들어 있던 버튼은 TMP 라벨을 올리고, 라벨은 한국어 원문으로 씬에 두어 `Localization` 이 씬을 열 때 번역하게 한다.
- TMP 기본 폰트는 `LiberationSans SDF` 로 둔다. `NeoDunggeunmoPro` SDF 는 정적 atlas 라 가운뎃점(U+00B7)이 없고, 기본 폰트가 그 글자의 대체 글꼴 역할을 한다. 기본 폰트를 바꿨더니 대화창 장 제목의 가운뎃점이 사라져서 되돌렸다.
- 카드는 34x50 픽셀 나무 틀 + 양피지 그림이다. 윗부분 띠 색으로 마법 카드(자주색)와 속성 카드(원소 색)를 구분하고 단어마다 12x12 아이콘을 넣는다. PPU 10.8 에 손패 배율 0.4 로 같게 맞춰 1 픽셀이 4 픽셀로 보이게 한다. 원래 배율 (0.379, 0.42) 은 가로세로가 달라서 픽셀이 찌그러진다.
- world space TextMeshPro 는 Linear color space 에서 색을 변환하지 않으므로, 카드 글자와 HP 외곽선 색은 `.linear` 값으로 저장했다.
- 조합창은 원래 위치에 두고 376x328 로 줄인다. 닫기 버튼은 두지 않고, 열린 동안 `조합` 버튼을 눌린 그림으로 보여 준다(지금 게임도 같은 버튼으로 여닫는다).
- HP 는 하트 + 숫자로 보여 주고, `HpLabelLayout` 이 숫자 폭이 바뀔 때마다 묶음 전체를 머리 중앙에 맞춘다.
- `튜토리얼 건너뛰기` 는 왼쪽 위 설정 메뉴 버튼 옆으로 옮긴다. 오른쪽 위에 두면 전투의 `턴 종료`, 맵의 도감 버튼과 겹친다.
- 맵 배경 5장과 게임 오버 프레임 3장은 글자를 지운 사본을 쓴다. 맵에서만 설정 메뉴 버튼을 내리던 `MapButtonPosition` 은 없앤다.
- Unity 는 WSL 경로의 프로젝트를 열지 않으므로, track 마다 Windows 임시 폴더에 프로젝트 사본을 두고 일회용 editor script 와 screen capture 용 PlayMode test 를 그 사본에서만 실행했다.

## Approach (Checklist)

쓰기 범위가 겹치지 않는 track 으로 나눠 병렬로 진행했다.

- [x] **1차 Track A — 런타임 UI 팔레트**: `Core.UiPalette`, `TutorialOverlay`, `StageDialogueView`, `DialogueWindowStyle`
- [x] **1차 Track B — 폰트**: 카드·적·플레이어·아군 슬라임 TMP 를 `NeoDunggeunmoPro` 로 교체
- [x] **1차 Track C — 버튼**: `WoodButton` 과 TMP 라벨(타이틀 3개, `턴 종료`)
- [x] **2차 Track A — 카드**: `Card.cs` 단어별 그림, 카드 prefab 3종, 손패 배율, 새 카드 화면, 튜토리얼 끌기 시연
- [x] **2차 Track B — 조합창**: `CombineZoneLegacy`, 조합 버튼 눌림 상태, 버튼 SpriteSwap
- [x] **2차 Track C — HP**: `HpLabelLayout`, 적 24종·워드·아군 슬라임
- [x] **2차 Track D — 메뉴·맵·튜토리얼·게임 오버**: 설정·크레딧·계정 창, 맵 배경과 버튼, 슬라임 도감 버튼, 튜토리얼 버튼 위치, 게임 오버 제목
- [x] 통합 후 EditMode·PlayMode test, 화면별 screen capture 확인

## Validation

- Unity 2022.3.34f1 batchmode, 통합 상태: EditMode 207개 모두 통과, PlayMode 59개 중 58개 통과, `BalanceSimulation` 1개는 원래 건너뛴다.
- 테스트가 찍는 `docs/design` 미리보기 13장을 새 UI 로 다시 찍었다.
- 통합 전투 화면(조합창 닫힘·열림, 카드 2장 넣음)과 track 별 화면은 `.plan/assets/2026-10-13-issue-105-unify-ui-style/captures/` 에 있다.

## Risks & Rollback

- **Risks:**
  - batchmode 화면이 640x480 이라 `Screen` 크기로 배치하는 크레딧·계정 창과 튜토리얼 건너뛰기 버튼은 1920x1080 screen capture 로 확인하지 못했다. 계산으로만 확인했다.
  - 실제 마우스로 카드를 끌어 조합 칸에 놓는 동작과 마우스 올림 그림은 에디터에서 직접 확인해야 한다.
  - HP 위치는 각 prefab 의 기본 그림에서 계산한 값이라, 애니메이션 프레임이 모양을 바꾸면 중심이 조금 어긋날 수 있다.
  - 여러 Unity 사본이 같은 Windows PlayerPrefs 를 써서, 동시에 테스트하면 저장 진행에 기대는 PlayMode test 가 서로 덮어써 실패할 수 있다. 통합 검증은 사본 하나로 다시 돌렸다.
- **Rollback steps:** commit 단위로 `git revert` 한다. 원본 아트는 그대로 남아 있다.
