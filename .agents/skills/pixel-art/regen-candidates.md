# Codex 재생성 후보

4px 기준(아트 1픽셀 = 1080p 화면 4px)으로 맞춘 뒤에도 보기 좋지 않은 에셋.
대상 크기는 현재 PNG 크기와 같고, `.meta`(PPU, rect)는 이미 그 크기에 맞춰져
있다. 그러니 같은 크기의 PNG로 교체하면 되고 `.meta`는 건드리지 않는다.

```bash
python3 .agents/skills/pixel-art/scripts/pixelate.py <codex 출력>.png <대상>.png --target <W>x<H> [옵션]
```

참고 이미지 출처:
- 지금 프로젝트에 있는 PNG(구도 기준)
- AI로 생성했던 원본: `git show b7c2ed0:<경로> > ref.png`(해상도가 더 높음)

재생성해서 교체하면 이 목록에서 지운다.

## 대화 초상화: 128칸 격자로 그려진 원본을 170칸으로 늘려 선 두께가 고르지 않음

대상 170x170, 옵션 `--keep-lines 2`, 투명 배경. 화면에서는 680x680px로 보인다.

| 에셋 | 비고 |
|---|---|
| `Assets/Art/Story/Portraits/FishermanDialogue.png` | |
| `Assets/Art/Story/Portraits/GuardDialogue.png` | |
| `Assets/Art/Story/Portraits/GuideDialogue.png` | |
| `Assets/Art/Story/Portraits/ShepherdDialogue.png` | |
| `Assets/Art/Story/Portraits/VillagerDialogue.png` | |
| `Assets/Art/Story/Portraits/SlimeKingDialogue.png` | |
| `Assets/Art/Story/Portraits/UndeadSlimeKingDialogue.png` | |

## 원본에 4px 디테일이 없어 블록이 크게 남는 것

| 에셋 | 대상 | 옵션 | 현재 상태 |
|---|---|---|---|
| `Assets/Art/Story/MountainHome.png`, `MountainHomeAttack.png`, `MountainHomeRuins.png`, `MountainHomeMemorial.png`, `WorldTreeEpilogue.png`, `Assets/Art/Title/MountainValley.png` | 480x270 | `--despeckle` | AI 원본 격자가 약 6.5px라 블록이 1~2텍셀로 고르지 않음. 같은 집이 나오는 4장은 구도와 집 모양을 서로 맞출 것 |
| `Assets/Art/Title/WordVentureLogo.png` | 275x91 | 투명 배경 | 원본 가짜 픽셀이 12~19px로 제각각 |
| `Assets/Art/UI/NewCardTitle.png` | 267x45 | 투명 배경 | 기존 6.5px 블록을 늘림 |
| `Assets/Art/Battle/Backgrounds/PlainBackground.png` | 1024x512 | | 256x128 원본을 4배로 붙여 둠, 실제 블록 16px |
| `Assets/Art/Tutorial/PlainBackground.png`, `Assets/ThirdParty/ArtResource/JinWook/Desert/HIghland.png`, `.../Ocean/SeaBackground.png`, `.../WorldMap/KakaoTalk_20240701_202830558.png` (스테이지 배경 4종) | 512x256 | | 256x128 원본을 2배로 붙여 둠, 실제 블록 8px |
| `Assets/ThirdParty/ArtResource/JinWook/GameOverBackground/*.png` (3장, 애니메이션 프레임) | 512x256 | | 실제 블록 8px. 세 장의 차이를 유지할 것 |
| `Assets/ThirdParty/ArtResource/JinWook/WorldMap/DefaultBG.png`, `Stage1BG.png`~`Stage4BG.png` | 494x329 | | 384x256 원본을 1.29배로 늘려 외곽선 두께가 고르지 않음. 5장의 길·지형 위치가 같아야 함(맵 노드 좌표가 고정) |

## 줄이면서 디테일이 뭉개진 것

| 에셋 | 대상 | 옵션 | 현재 상태 |
|---|---|---|---|
| `Assets/Art/Title/WordHeadIdle1.png`, `WordHeadIdle2.png` | 35x35 | `--keep-lines 2`, 투명 | 64x64를 줄여 얼굴이 뭉개짐. 두 프레임은 애니메이션 |
| `Assets/Art/Cards/MagicCard.png`, `MagicCard1.png`, `TypeCard.png`, `TypeCard1.png` | 52x52 | 투명 | "Magic", "Type" 글자 일부가 깨짐 |

## 처리 완료

- 워드 전투 스프라이트 7프레임(`Assets/Art/Combat/Word/`): `--keep-lines 2`로 다시 변환해 외곽선이 닫힘
- 워드 대화 초상화(`Assets/Art/Story/Portraits/WordDialogue.png`): Codex image_gen으로 재생성해 `--target 170x170 --keep-lines 2`로 변환. 나머지 초상화 7장의 머리 크기와 프레이밍 기준
