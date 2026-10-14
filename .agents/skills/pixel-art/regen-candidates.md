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

## 처리 완료

- 워드 전투 스프라이트 7프레임(`Assets/Art/Combat/Word/`): `--keep-lines 2`로 다시 변환해 외곽선이 닫힘
- 워드 대화 초상화(`Assets/Art/Story/Portraits/WordDialogue.png`): Codex image_gen으로 재생성해 `--target 170x170 --keep-lines 2`로 변환. 나머지 초상화 7장의 머리 크기와 프레이밍 기준
- 대화 초상화 7장(Fisherman, Guard, Guide, Shepherd, Villager, SlimeKing, UndeadSlimeKing): 새 WordDialogue.png를 프레이밍 기준으로 함께 첨부해 재생성하고 `--target 170x170 --keep-lines 2`로 변환
- 스토리·타이틀 배경 6장(`MountainHome*.png` 4장, `WorldTreeEpilogue.png`, `MountainValley.png`): 재생성해 `--target 480x270 --despeckle`로 변환. 습격·폐허·추모 3장은 새 MountainHome을 편집 대상으로 넘겨 집과 산 위치를 맞춤. `MountainHomeAttack.png`는 기본 팔레트가 불덩이 중심의 크림색을 연두색으로 합쳐서, 생성 원본을 480x270으로 BOX 축소한 이미지에서 64색 팔레트를 뽑아 원본에 먼저 적용한 뒤 변환
- 타이틀 로고(`WordVentureLogo.png`): 재생성 원본을 내용 영역으로 자른 뒤 기존 로고의 내용 영역(215x58, 위치 31,18)에 맞춰 변환
- 전투 배경(`Assets/Art/Battle/Backgrounds/PlainBackground.png`, 1024x512)과 스테이지 배경 4종(512x256): 재생성해 `--target`으로 변환. 폐허 배경(`KakaoTalk_20240701_202830558.png`)은 `--despeckle` 추가
- 게임오버 3프레임: 1프레임을 재생성하고, 2·3프레임은 1프레임을 편집 대상으로 넘겨 생성. 캐릭터가 프레임마다 흔들리지 않도록 2·3프레임은 1프레임 위에 글자 영역(위 60줄)과 눈물 픽셀만 옮기고 1프레임 팔레트로 맞춤
- 맵 배경 5장: DefaultBG만 재생성(기존 대비 랜드마크 위치 차이 1~2아트픽셀). Stage1~4BG는 DefaultBG에서 깬 랜드마크(버섯 집, 야자 섬, 바위 폐허, 나무 바위 순서로 누적)를 회색으로 바꿔 만듦. 회색은 DefaultBG 팔레트에 있는 회색 중 밝기가 가장 가까운 것을 써서 64색 이하 유지
- 타이틀 머리(`WordHeadIdle1.png`, `WordHeadIdle2.png`): Codex가 약 85칸으로 그려 35칸으로 줄이면 입이 사라진다. 1프레임은 `--colors 12 --keep-lines 2`로 변환하고 입을 손으로 다시 그림. 2프레임은 같은 생성 원본을 턱 기준으로 5도 기울여 변환하고 1프레임 팔레트로 맞춤(기존 애니메이션도 고개를 기울이는 차이만 있음)
- 카드 4장(`MagicCard*.png`, `TypeCard*.png`): Codex 결과는 글자가 더 깨져서 쓰지 않음. 기존 테두리는 두고 "Magic", "Type" 글자만 높이 6픽셀(내림 2픽셀) 픽셀 폰트로 다시 그림
