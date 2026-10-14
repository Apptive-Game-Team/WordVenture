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

## 원본에 4px 디테일이 없어 블록이 크게 남는 것

| 에셋 | 대상 | 옵션 | 현재 상태 |
|---|---|---|---|
| `Assets/Art/UI/NewCardTitle.png` | 267x45 | 투명 배경 | 기존 6.5px 블록을 늘림. 2026-10-14에 재생성했지만 Codex가 약 14.6px 칸으로 그려 260칸으로 줄이면 1.95배라 가장자리가 깨졌다. 지금 PNG가 더 깨끗해서 유지함 |
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
- 대화 초상화 7장(Fisherman, Guard, Guide, Shepherd, Villager, SlimeKing, UndeadSlimeKing): 새 WordDialogue.png를 프레이밍 기준으로 함께 첨부해 재생성하고 `--target 170x170 --keep-lines 2`로 변환
- 스토리·타이틀 배경 6장(`MountainHome*.png` 4장, `WorldTreeEpilogue.png`, `MountainValley.png`): 재생성해 `--target 480x270 --despeckle`로 변환. 습격·폐허·추모 3장은 새 MountainHome을 편집 대상으로 넘겨 집과 산 위치를 맞춤. `MountainHomeAttack.png`는 기본 팔레트가 불덩이 중심의 크림색을 연두색으로 합쳐서, 생성 원본을 480x270으로 BOX 축소한 이미지에서 64색 팔레트를 뽑아 원본에 먼저 적용한 뒤 변환
- 타이틀 로고(`WordVentureLogo.png`): 재생성 원본을 내용 영역으로 자른 뒤 기존 로고의 내용 영역(215x58, 위치 31,18)에 맞춰 변환
