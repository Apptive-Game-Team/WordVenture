# Issue 79: 2부 대화 배경, Spawn 카드 획득, 2부 대사 수정

- Issue: https://github.com/Apptive-Game-Team/WordVenture/issues/79
- Branch: `feat/act-two-dialogue-polish` (`feat/map-chapter-switch` f00eb66 위)

## 배경

- 2부 입장·클리어 대화 10개에 지역 전투 배경 그림 5장(`Assets/Art/Battle/Backgrounds/ActTwo/`)을 지정했다.
  1부 대화가 지역 배경 그림을 까는 방식과 같다.
- wave 대화 3개는 1부처럼 배경을 비워 두어 전투 화면 위에 뜬다.

## 픽셀 격자

기존 원본 배경(고원, 해안, 빗길, 마왕 성, 1부 맵)과 2부 전투 배경은 4픽셀 격자에 색이 25~58개다.
아래 그림 8장은 격자가 없고 색이 859~25만 개여서 픽셀 크기가 제각각이었다.
`docs/design/pixel-grid/regrid_backgrounds.py`로 원본 크기에서 색을 줄인 뒤, 격자 한 칸마다 가장 많이 쓰인
색을 골라 다시 그렸다.

| 그림 | 격자 | 확대 | 맞춘 기준 |
| --- | --- | --- | --- |
| `Art/Tutorial/PlainBackground.png` | 256×128 | 4배 | 다른 전투 배경 |
| `Art/Map/ActTwoMap.png` | 384×256 | 4배 | 1부 맵 `Stage1BG` |
| `Art/Story/` 5장, `Art/Title/MountainValley.png` | 224×126 | 8배 | 전투 배경이 화면 높이에 보이는 칸 수(128) |

- 스토리·타이틀·엔딩 그림은 1672×941에서 1792×1008로 바뀌었다. `spritePixelsToUnits`를 348.33에서 373.33으로
  고쳐 화면에 그려지는 크기(4.8×2.7 unit)를 유지했다.
- 대화 배경 창은 2:1 전투 배경을 16:9 화면에 늘려 픽셀이 세로로 길어졌다. `StageDialogueView`에서
  `AspectRatioFitter`(`EnvelopeParent`)로 비율을 지킨 채 화면을 덮게 했다. 넘치는 좌우는 잘린다.

## 2부 맵 길

생성한 2부 맵은 길이 잿빛 유적(7)에서 뒤틀린 숲(8)과 세계수(9)로 갈라지고, 숲과 세계수 사이에는 길이 없었다.
`docs/design/act-two-art/redraw_map_path.py`로 유적→세계수 길을 풀로 메우고, 숲 오른쪽에서 세계수 왼쪽까지
기존 길과 같은 색·폭·타일 무늬의 길을 그렸다. 이제 서리 마을→협곡→유적→숲→세계수가 한 줄로 이어진다.
스테이지 지점 좌표(`ActTwoMap.asset`)는 그대로다.

## Spawn 카드

- 서리 마을(스테이지 5)을 처음 클리어하면 클리어 대화 → New Card 창(Spawn) → 맵 순서로 진행한다.
  `GameClearController.HasCardReward`와 `ShowGettedCard`에 `SpawnRewardStageID`(5)를 더했다.
- Spawn 카드는 서리 마을을 클리어한 뒤 스테이지 6부터 손패에 섞인다(`CardManager.SpawnUnlockStage` 5 → 6).
- 서리 마을 입장 대화에서 Spawn 사용법 설명 두 줄을 뺐다. 대신 클리어 대화에서 서리 마을 주민이
  할아버지 샌즈가 맡기고 간 카드를 건넨다.

## 대사

- 스테이지 5~9 대화 13개와 2부 에필로그 6줄을 다시 썼다. 장면 순서, 화자, 대화 시점, 읽음 기록 비트는
  그대로다. 스테이지 5 클리어와 스테이지 9 입장 장면 제목을 바꿨다.
- 2부에서 슬라임이 다시 사람을 덮치는 이유(세계수에서 넘친 마력)를 스테이지 5 입장과 스테이지 9 입장에서
  이어지게 했다.

## 검증

- Unity 2022.3.34f1 batchmode로 EditMode·PlayMode 테스트 실행

## 위험

- 다시 그린 그림은 원본보다 작은 장식(울타리 사이 틈, 얇은 윤곽선 일부)이 뭉개졌다.
- 1부 맵과 전투 배경은 원본 그대로라서 맵의 픽셀(화면 높이에 256칸)이 전투 배경(128칸)보다 작다.

- Spawn 사용법(아군 슬라임이 턴 종료 때 맨 앞 적을 공격하고 공격을 대신 받는다) 설명이 대사에서 빠졌다.
  카드에는 설명이 없으므로 플레이어가 직접 써 보면서 알게 된다.
- 서리 마을 첫 전투는 Spawn 없이 치른다. 웨이브 구성은 바꾸지 않았다.
- `feat/map-chapter-switch`가 `GameClearController.HasCardReward`의 첫 클리어 조건을 `IsFirstClear`로
  바꾸고 있으므로 merge 할 때 같은 줄이 충돌한다.
