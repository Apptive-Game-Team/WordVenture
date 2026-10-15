# 워드 몸짓 초상화

워드는 세계수 결말 전까지 말하지 않는다(이슈 97). 지역 대화에서 워드가 하려던 말은 나레이션의 몸짓으로 나오고, 그 줄에서는
워드 초상화가 몸짓 그림으로 바뀐다(이슈 106).

| 몸짓 (`WordGesture`) | 표정 | 에셋 (`Assets/Art/Story/Portraits/`) | 나레이션 줄 |
| --- | --- | --- | --- |
| `Nod` | 부드럽게 미소 | `WordNodDialogue.png` | 4 |
| `NodFirm` | 굳게 결심 | `WordNodFirmDialogue.png` | 2 |
| `ShakeHead` | 슬픔 | `WordShakeHeadDialogue.png` | 1 |
| `ShakeHeadFirm` | 적에게 단호하게 | `WordShakeHeadFirmDialogue.png` | 1 |
| `TouchThroat` | 슬픔, 목을 짚는다 | `WordTouchThroatDialogue.png` | 1 |
| `ReadBook` | 차분하고 조금 슬픔, 기록을 내려다보며 읽는다 | `WordReadBookDialogue.png` | 튜토리얼 전체, 안내 |
| `Point` | 진지함 | `WordPointDialogue.png` | 10 |
| `PointWorried` | 걱정 | `WordPointWorriedDialogue.png` | 2 |
| `Tilt` | 궁금함 | `WordTiltDialogue.png` | 3 |
| `ShowBook` | 진지함, 펼친 마법서 | `WordShowBookDialogue.png` | 2 |
| `ShowCover` | 조금 망설임, 마법서 표지 | `WordShowCoverDialogue.png` | 2 |
| `RaiseCard` | 결심 | `WordRaiseCardDialogue.png` | 3 |
| `PalmOut` | 차분함 | `WordPalmOutDialogue.png` | 2 |

몸짓 그림과 GUID 의 연결은 `Assets/Resources/Story/ActOneDialogues.asset` 의 `gesturePortraits` 에 있다. 줄마다 쓰는 몸짓은
같은 파일의 `gesture` 필드다. 무릎 꿇기, 새끼손가락 약속, 마법서 끌어안기처럼 한 번만 나오는 몸짓과 장면 묘사 줄은
`None` 이라 평소 초상화를 어둡게 보여 준다.

## 만드는 방법

1. Codex 의 내장 이미지 생성 도구를 편집 모드로 쓴다. 기존 워드 초상화(`WordDialogue.png`)를 바탕 그림으로 두고
   팔, 손, 소품, 고개 각도, 눈꺼풀, 눈썹, 입만 바꾸게 한다. 프롬프트 원문은 `prompt.txt` 다. 생성 규격은 1024×1024
   마젠타 배경이다.
   처음에는 바탕 그림 없이 새로 그리게 했는데, 얼굴이 원래 초상화보다 둥글고 어려 보였다. 그리고 몸짓마다 그림이
   1장뿐이라 걱정하며 가리키는 줄과 결심하고 가리키는 줄이 같은 표정이었다. 그래서 편집 모드로 바꾸고, 표정이 크게
   갈리는 몸짓은 그림을 나눴다. 마법서도 펼친 속지를 보여 주는 줄과 '워드에게' 가 적힌 표지를 보여 주는 줄이 있어서
   두 장으로 나눴다.
2. `import_gestures.py` 가 마젠타 배경을 지우고 1254×1254 로 키운 뒤 `.meta` 를 만든다. 저장소 루트에서 실행한다.

```bash
python3 docs/design/word-gesture-portraits/import_gestures.py <생성 폴더>
```

워드의 눈이 분홍빛이라 넓은 색 기준으로 그림 전체를 지우면 눈까지 투명해진다. 그래서 그림 가장자리와 순수한 마젠타
픽셀(지팡이와 머리카락 사이에 갇힌 배경)에서 시작해, 거기에 이어진 분홍빛 번짐까지만 지운다.

## 기록을 읽는 그림 (`ReadBook`)

튜토리얼과 안내는 워드가 할아버지의 기록을 읽는 장면이다. 처음에는 `ShowBook` 을 썼지만, 그 그림은 펼친 책을 보는 사람 쪽으로
들어 보여 주는 자세라 읽는 장면으로는 어색했다. 그래서 책을 자기 쪽으로 펼쳐 든 그림을 따로 만들었다.

1. `WordDialogue.png` 를 바탕으로, `WordShowBookDialogue.png` 를 책 모양 참고로 넘겨 편집 모드로 생성했다. 프롬프트는 `prompt.txt` 의
   "WordReadBook" 부분이다. 책 방향은 맞았지만 눈이 앞을 보고 있어서 읽는 모습으로 보이지 않았다.
2. 1번 결과를 바탕으로 눈꺼풀과 시선을 내리게 다시 편집했는데("WordReadBook 시선" 부분), 눈이 반쯤 감겨 졸리고 기괴해 보였다.
   그래서 1번 결과에서 원래 초상화의 눈 모양과 크기는 그대로 두고 눈동자만 아래로 내리게 다시 편집해 이것을 썼다
   ("WordReadBook 눈 다시 그리기 (최종)" 부분).
3. 다른 몸짓과 같이 `import_gestures.py` 의 `remove_background` 로 마젠타 배경을 지우고 1254×1254 로 키웠다. import 설정은
   `WordDialogue.png.meta` 를 복사했다.

