# Codex 재생성 후보

4px 기준으로 맞춘 뒤에도 보기 좋지 않은 에셋. 대상 크기(아트 픽셀)로 Codex에서
다시 생성한 뒤 `pixelate.py --target`으로 맞춘다. 재생성하면 이 목록에서 지운다.

## 원본에 4px 디테일이 없어 블록이 크게 남는 것

| 에셋 | 대상 크기 | 현재 상태 |
|---|---|---|
| `Assets/Art/Story/*.png`, `Assets/Art/Title/MountainValley.png` | 480x270 | AI 원본 격자가 약 6.5px라 블록 크기가 1~2텍셀로 고르지 않음 |
| `Assets/Art/Title/WordVentureLogo.png` | 275x91 | 원본 가짜 픽셀이 12~19px로 제각각 |
| `Assets/Art/UI/NewCardTitle.png` | 267x45 | 기존 6.5px 블록을 늘림 |
| `Assets/Art/Battle/Backgrounds/PlainBackground.png` | 1024x512 (전투 배경 화면 크기 기준) | 256x128 원본을 4배로 붙여 둠, 실제 블록 16px |
| 스테이지 배경 4종: `Art/Tutorial/PlainBackground.png`, `ThirdParty/.../Desert/HIghland.png`, `.../Ocean/SeaBackground.png`, `.../WorldMap/KakaoTalk_20240701_202830558.png` | 512x256 | 256x128 원본을 2배로 붙여 둠, 실제 블록 8px |
| 게임오버 배경 3장 `ThirdParty/.../GameOverBackground/*.png` | 512x256 | 실제 블록 8px |
| 맵 배경 `ThirdParty/.../WorldMap/DefaultBG.png`, `Stage1~4BG.png` | 494x329 | 384x256 원본을 1.29배로 늘려 외곽선 두께가 고르지 않음 |

## 줄이면서 디테일이 뭉개진 것

| 에셋 | 대상 크기 | 현재 상태 |
|---|---|---|
| `Assets/Art/Title/WordHeadIdle1.png`, `WordHeadIdle2.png` | 35x35 | 64x64를 줄여 얼굴이 뭉개짐 |
| `Assets/Art/Cards/MagicCard*.png`, `TypeCard*.png` | 52x52 | "Magic", "Type" 글자 일부가 깨짐 |
