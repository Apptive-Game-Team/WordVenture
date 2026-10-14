# Codex 재생성 후보

4px 기준(아트 1픽셀 = 1080p 화면 4px)에는 맞췄지만, AI 이미지를 줄여서 만들어 다시 그리면 더 나아질 에셋.
대상 크기와 `.meta`(PPU, rect)는 이미 맞춰져 있으므로 같은 크기의 PNG로 교체하면 되고 `.meta`는 건드리지 않는다.

```bash
python3 .agents/skills/pixel-art/scripts/pixelate.py <codex 출력>.png <대상>.png --target <W>x<H> [옵션]
```

애니메이션 프레임은 `resample_set.py`로 함께 줄여야 프레임마다 색과 칸 경계가 같다.
재생성해서 교체하면 이 목록에서 지운다.

지금은 남은 후보가 없다. 새로 생긴 에셋이 기준에 맞지 않으면 아래 표 형식으로 더한다.

| 에셋 | 대상 | 현재 상태 |
|---|---|---|

## 예외(바꾸지 않음)

- 주문 이펙트 시트 20장(`Assets/Art/Combat/Spells/`): 프리팹 스케일 5 또는 10에서 1텍셀 = 4.22px. 5% 차이라 그대로 둔다.
- `SplitSlimeSmall`: 프리팹 스케일 0.3으로 `SplitSlime`(0.5)의 프레임을 다시 써서 2.4px로 보인다.
- `AllySlime`: 프리팹 스케일 0.4로 `Ranged_Light`(0.5)의 프레임을 다시 써서 3.2px로 보인다.
- `AffinityMark`, `ReactionBurst`, `ElementalStatusOverlay`: 적의 몸 폭에 맞춰 런타임에 크기가 정해진다. 일반 슬라임에서 약 4px, 보스에서는 더 크다.
- `Assets/Art/Icon.png`: 앱 아이콘이라 게임 화면에 그려지지 않는다.

## 처리 완료(#116)

- 대화 초상화 25장: Codex로 170x170에 다시 그림(`docs/design/pixel-grid/portrait-prompts.md`).
- 스토리·타이틀·엔딩 배경 6장, 전투·대화 배경 10장, 게임오버·게임클리어 배경: Codex로 다시 그림.
- 월드맵 2장: Codex 로 다시 그린 그림은 외곽선이 자글자글해서, 원래 지도를 화면 비율로 늘린 뒤 `pixelate.py --target 495x276 --keep-lines 2` 로 줄였다.
- 1부 월드맵 진행 4장: `docs/design/pixel-grid/map_stages.py`로 새 WorldMapDefault에서 만듦.
- 게임오버 1·2프레임: `gameover_frames.py`가 3프레임의 눈물 줄기를 짧게 잘라 만듦. 비 오는 배경 5프레임: `rain_frames.py`로 빗줄기만 옮김.
- 타이틀 머리: 워드 전투 대기 프레임의 머리 부분을 잘라 35x30으로 만듦.
- 슬라임 24종, 워드 전투 7프레임, 적 투사체 7색, 타이틀 머리, NEW CARD·GAME OVER 글자, 튜토리얼 손, 상태 이상 오버레이: 줄이면 선이 자글자글해서 공통 캐릭터 스타일로 Codex 로 다시 그림(`char_sheet.py`, `docs/design/pixel-grid/character-prompts.md`).
- 로고, 타이틀 머리, NEW CARD·GAME OVER 글자, 박격포 표적: 줄이거나 예전 결과를 씀. 타이틀 머리와 글자 두 장은 투명 여백도 잘라냄(로고는 UI Image 라 자르지 않음).
- 투명 여백: 슬라임 22종 프레임, 주문 이펙트 시트 20장(`trim_sheet.py`), 워드·투사체·타이틀 머리·글자 두 장을 잘라냄. 대화 초상화, 로고, 튜토리얼 손, 조합 버튼, `Resources/Combat` 아틀라스는 UI 배치나 코드가 크기를 읽어서 자르지 않음.
