# 2026-10-13 — 화면별 UI 색상과 폰트를 나무색 픽셀 스타일로 통일

- Date: 2026-10-13
- GitHub Issue: #105
- Status: In progress

## Goal

- 버튼은 타이틀의 밝은 나무색 스타일, 패널과 안내창은 어두운 나무색 패널 + 금색 테두리 + 크림색 글자로 통일한다.
- 런타임에 UI 를 만드는 스크립트가 같은 팔레트 정의(`Core.UiPalette`)를 쓴다.
- 프로젝트의 모든 TMP 텍스트와 TMP 기본 폰트를 `NeoDunggeunmoPro` 로 통일한다.

## Non-goals

- 배경 그림에 포함된 글자와 캐릭터 아트
- 중복된 `Button` prefab(`Prefabs/Common`, `Prefabs/UI`) 정리
- 화면 배치와 텍스트 크기 체계 재설계

## Decisions

- 기준 팔레트는 이미 어두운 나무색을 쓰는 `SlimeEncyclopediaView` 의 상수(Wood, Border, Cream, CellColor, SelectedCellColor)를 그대로 옮긴다. 새 색을 만들지 않는다.
- 팔레트는 static class 로 둔다. 디자이너가 조정하는 값은 기존 ScriptableObject 인 `DialogueWindowStyle.asset` 에 그대로 남긴다.
- 원본 아트(`Assets/ThirdParty/ArtResource/JinWook`)는 덮어쓰지 않는다. 원본을 다시 칠한 스프라이트를 `Assets/Art/UI/Buttons` 에 추가하고, `.meta` 는 원본의 import 설정과 sprite ID 를 복사하고 guid 만 새로 발급한다. 그러면 참조하는 쪽은 guid 만 바꾸면 된다.
- 조합 영역의 분홍(Magic)·노랑(Type) 칸은 카드 색과 짝을 이루므로 유지하고, 테두리와 바탕만 나무색과 양피지색으로 바꾼다.
- 글자가 그림에 들어 있던 버튼(타이틀의 Start·Continue·Exit, 전투의 `Turn End`)은 새로 그린 공용 배경 `WoodButton.png` 위에 TMP 라벨을 올린다. 기존 Start 버튼 그림은 오른쪽·아래 테두리만 두꺼운 비대칭 모양이라, 같은 팔레트로 좌우 대칭인 62×30 픽셀 그림을 새로 그렸다.
- `WoodButton.png` 는 9-slice(왼쪽·오른쪽·위 6, 아래 9 픽셀)와 point filter 로 import 하고, `pixelsPerUnitMultiplier` 0.25 로 1 픽셀을 4 단위로 보여서 버튼마다 픽셀 크기가 같다.
- 라벨은 한국어 원문(시작하기, 이어하기, 종료, 턴 종료)으로 씬에 두고 `English.json` 에 번역을 추가한다. 씬을 열 때 `Localization` 이 번역한다.

## Approach (Checklist)

쓰기 범위가 겹치지 않는 세 갈래로 나눠 병렬로 진행한다.

- [ ] **Track A — 런타임 UI 팔레트** (`Assets/Scripts/**`, `Assets/Resources/Story/DialogueWindowStyle.asset`)
  - `Core.UiPalette` 추가, `SlimeEncyclopediaView` 상수를 이 class 로 이동
  - `TutorialOverlay` 의 남색 안내창, 확인창, 버튼 색을 팔레트로 교체
  - `StageDialogueView` 장 제목 색, `DialogueWindowStyle` 기본값과 asset 의 패널·테두리·글자 색을 나무색으로 교체
- [ ] **Track B — 폰트** (`Assets/Prefabs/**` 중 `Prefabs/Common/Button.prefab`, `Prefabs/UI/**` 제외, `TurnBattleScene` 외 scene, `TMP Settings.asset` 2개)
  - `LiberationSans SDF` 를 참조하는 `m_fontAsset`, `m_sharedMaterial` 을 `NeoDunggeunmoPro-Regular SDF` 와 그 material 로 교체
  - TMP 기본 폰트를 `NeoDunggeunmoPro-Regular SDF` 로 교체
- [ ] **Track C — 스프라이트** (`Assets/Art/UI/Buttons/**`, `Assets/Scenes/TurnBattleScene.unity`, `Assets/Prefabs/Common/Button.prefab`, `Assets/Prefabs/UI/**`)
  - `CombineButton.png`, `CombineZone.png` 생성, 글자 없는 공용 버튼 `WoodButton.png` 새로 그림
  - 일회용 editor script 로 타이틀 버튼 3개와 `Turn End` 버튼에 `WoodButton.png` 와 TMP 라벨 적용
  - 전투 scene 과 버튼·조합 영역 prefab 의 sprite 참조와 폰트 참조 교체
- [ ] 통합 후 Unity 컴파일, EditMode·PlayMode test, 화면별 screen capture 비교

## Validation

- Unity 2022.3.34f1 batchmode 로 EditMode·PlayMode test 실행
- 타이틀, 맵, 대화, 전투, 튜토리얼, 새 카드 화면 screen capture 를 `docs/store/screenshots` 와 비교

## Risks & Rollback

- **Risks:** `NeoDunggeunmoPro` 는 `LiberationSans` 와 글자 폭이 달라서 카드 이름이나 적 HP 숫자가 칸을 넘칠 수 있다. screen capture 로 확인하고 필요하면 크기를 조정한다.
- **Rollback steps:** `git revert` 로 되돌린다. 원본 스프라이트는 그대로 남아 있다.
