# 워드 몸짓 초상화

워드는 세계수 결말 전까지 말하지 않는다(이슈 97). 지역 대화에서 워드가 하려던 말은 나레이션의 몸짓으로 나오고, 그 줄에서는
워드 초상화가 몸짓 그림으로 바뀐다(이슈 106).

| 몸짓 (`WordGesture`) | 에셋 | 나레이션 줄 |
| --- | --- | --- |
| `Nod` | `Assets/Art/Story/Portraits/WordNodDialogue.png` | 6 |
| `ShakeHead` | `WordShakeHeadDialogue.png` | 3 |
| `Point` | `WordPointDialogue.png` | 12 |
| `Tilt` | `WordTiltDialogue.png` | 3 |
| `ShowBook` | `WordShowBookDialogue.png` | 4 |
| `RaiseCard` | `WordRaiseCardDialogue.png` | 3 |
| `PalmOut` | `WordPalmOutDialogue.png` | 2 |

몸짓 그림과 GUID 의 연결은 `Assets/Resources/Story/ActOneDialogues.asset` 의 `gesturePortraits` 에 있다. 줄마다 쓰는 몸짓은
같은 파일의 `gesture` 필드다. 무릎 꿇기, 새끼손가락 약속, 마법서 끌어안기처럼 한 번만 나오는 몸짓과 장면 묘사 줄은
`None` 이라 평소 초상화를 어둡게 보여 준다.

## 만드는 방법

1. Codex 의 내장 이미지 생성 도구로 만든다. 프롬프트 원문은 `prompt.txt` 이고, 기존 워드 초상화(`WordDialogue.png`)와
   어부 초상화(`FishermanDialogue.png`)를 화풍 참고 이미지로 첨부했다. 생성 규격은 1024×1024 마젠타 배경이다.
   처음 만든 마법서 그림은 책을 위에서 내려다본 각도라서 표지가 상자처럼 꺾여 보였다. 그래서 펼친 책을 똑바로 세워
   두 쪽이 정면을 향하게 하는 프롬프트(`prompt-book.txt`)로 다시 만들었다. 시안 2장 중 표정이 더 맞는 쪽을 골라
   `WordShowBook.png` 로 썼다.
2. `import_gestures.py` 가 마젠타 배경을 지우고 1254×1254 로 키운 뒤 `.meta` 를 만든다. 저장소 루트에서 실행한다.

```bash
python3 docs/design/word-gesture-portraits/import_gestures.py <생성 폴더>
```

워드의 눈이 분홍빛이라 넓은 색 기준으로 그림 전체를 지우면 눈까지 투명해진다. 그래서 그림 가장자리와 순수한 마젠타
픽셀(지팡이와 머리카락 사이에 갇힌 배경)에서 시작해, 거기에 이어진 분홍빛 번짐까지만 지운다.
