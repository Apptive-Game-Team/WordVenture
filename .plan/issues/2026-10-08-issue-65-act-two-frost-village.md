# Issue 65: 2부 맵 진입 구조와 서리 마을

- Issue: https://github.com/Apptive-Game-Team/WordVenture/issues/65
- Branch: `feat/act-two-frost-village` (PR #60 `feat/spawn-ally-slime` 위)

## 범위

1부를 끝낸 세이브로 2부 맵에 들어가 서리 마을(스테이지 5)을 끝까지 플레이할 수 있게 한다.
스테이지 6~9, 적 슬라임 6종, 2부 최종 에필로그는 넣지 않는다.

## 진행과 저장

- `StagePosition`은 그대로 쓴다. 0~4는 1부, 5~9는 2부다. 마왕을 쓰러뜨리면 이미 5가 되므로
  1부를 끝낸 기존 세이브는 데이터를 바꾸지 않고 2부로 이어진다.
- 2부 대화 읽음 기록은 새 키 `ActTwoDialogueSeen`에 둔다. `StageDialogueChapter.SeenBit`은
  1부에 0~29, 2부에 32~61을 돌려주고, `SaveLoadController`가 32 이상을 새 키에 기록한다.
  PlayerPrefs의 int는 31비트까지 쓰고 1부가 0~29를 이미 쓰고 있다.

## 맵

- `MapChapter`(ScriptableObject)에 장의 첫 스테이지 번호, 배경, 지점 좌표 5개, 들어갈 수 있는
  마지막 스테이지 번호를 둔다. `Assets/ScriptableObjects/Map/ActTwoMap.asset`이 2부 맵이다.
- `MapMove`는 `StagePosition`이 5 이상이면 2부 맵을 쓴다. 지점 오브젝트 5개는 스프라이트가
  없는 위치 표시라서, 장의 좌표로 옮기고 나머지 이동·클릭 로직은 그대로 쓴다.
- 전투 데이터가 없는 지점(6~9)은 보이지만 들어갈 수 없고, 상단 표시에 "준비 중"이 붙는다.
- 2부 맵 그림은 Codex 이미지 생성으로 만들었다. 프롬프트와 좌표 계산은
  `docs/design/act-two-map/README.md`에 있다.

## 서리 마을 전투

- 적 pool은 웨이브가 요청할 때 적을 만든다. 적 id와 스테이지 번호를 묶던 계산을 없앴다.
- 적 데이터 3종(id 16~18): 얼음 원거리(HP 25), 바위 근접(HP 40), 번개 정예(HP 45). 외형은 기존
  prefab을 쓴다.
- 웨이브 3개, 배경과 음악은 고원 스테이지 것을 임시로 쓴다. `StageData.music`이 비어 있으면
  기존처럼 스테이지 순서 음악을 쓴다.
- 손패 비율은 스테이지 4와 같고, Spawn 카드가 섞인다.
- 입장 대화에서 Spawn을 소개하고, 클리어 대화에서 벼락 협곡을 예고한다.
- 클리어 화면은 새 카드가 없으면 "New Card" 안내 없이 맵으로 돌아간다.

## 검증

- EditMode: 2부 맵 연결과 지점 범위, 대화 읽음 비트 중복, 2부 비트가 1부 저장값을 건드리지
  않는지, 서리 마을 데이터 연결
- PlayMode: `StagePosition` 2·5·6으로 맵 진입, 서리 마을 전투 시작
