# Issue 106: 지역 대화 나레이션 줄의 초상화 표시 수정과 워드 몸짓 초상화 12장 추가

- Issue: https://github.com/Apptive-Game-Team/WordVenture/issues/106
- Branch: `feat/word-gesture-portraits` (`develop` 위)

## 배경

이슈 97에서 세계수 결말 전까지 워드가 말하지 않도록 바꾸고, 워드가 하려던 말을 나레이션 66줄의 몸짓으로 옮겼다.
그런데 나레이션 줄에서는 두 초상화가 모두 사라졌다. `StageDialogueView.ShowLine` 이 초상화를 설정한 직후
`ChatWindowController.UpdateChatStream` 이 `DialogueWindowPresentation.SetSpeaker("")` 를 호출하고, 이 메서드가 이름이
비어 있으면 초상화 2장을 지운다. 그리고 몸짓은 글로만 나와서 "고개를 끄덕였다" 줄에서도 초상화는 평소 자세였다.

## 정한 방향

- `StageDialogueView` 는 대사를 넣은 뒤에 초상화를 설정한다. `SetSpeaker` 는 그대로 두어 프롤로그, 엔딩, 튜토리얼의
  나레이션 표시는 바뀌지 않는다.
- `DialogueWindowPresentation.SetPortraits` 에 두 초상화의 밝기를 따로 정하는 overload 를 추가했다. 기존 3인자 메서드는
  이 overload 를 호출한다.
- 줄마다 밝기:

  | 줄 | 워드 | 상대 |
  | --- | --- | --- |
  | 워드 대사 | 밝게 | 어둡게 |
  | 상대 대사 | 어둡게 | 밝게 |
  | 몸짓 나레이션 | 몸짓 그림, 밝게 | 어둡게 |
  | 그 밖의 나레이션 | 어둡게 | 어둡게 |

  상대 초상화는 기존처럼 상대가 처음 말한 뒤부터 보인다.
- `StageDialogueLine` 에 `WordGesture gesture` 필드를, `StageDialogueData` 에 `gesturePortraits` 와
  `WordPortraitFor(WordGesture)` 를 추가했다. 몸짓 그림이 없으면 평소 초상화를 쓴다.
- 몸짓 그림은 12장이다. 처음에는 3번 이상 나오는 몸짓 7가지를 1장씩 만들었는데, 리뷰에서 두 가지가 지적됐다.
  - 얼굴 인상이 원래 초상화와 달랐다. 바탕 그림 없이 새로 그리게 해서 얼굴이 둥글고 어려 보였다. 그래서 원래 초상화를
    바탕 그림으로 두고 팔, 손, 소품, 고개, 눈꺼풀, 눈썹, 입만 바꾸는 편집 모드로 다시 만들었다.
  - 같은 몸짓도 상황마다 감정이 다르고, "마법서 표지를 내밀었다" 줄에 펼친 속지 그림이 나왔다. 그래서 감정이 크게
    갈리는 몸짓의 그림을 나눴다: 끄덕이기(미소/결심), 고개 젓기(슬픔/단호함), 목 짚기, 가리키기(진지함/걱정),
    마법서(펼친 속지/표지).
- `WordGesture` 값은 에셋에 숫자로 저장되므로 기존 7개(1~7)의 숫자는 그대로 두고 새 5개를 8~12로 붙였다.
- 무릎 꿇기, 새끼손가락 약속, 마법서 끌어안기, 미소처럼 한 번만 나오는 몸짓과 장면 묘사 줄은 `None` 이다. 한 줄에
  몸짓이 둘이면(예: 끄덕이고 가리키기) 대답의 내용을 전하는 쪽을 골랐다.

## 바뀐 것

| 항목 | 값 |
| --- | --- |
| 나레이션 줄 | 66 |
| 몸짓을 지정한 줄 | 33 (가리키기 10+2, 끄덕이기 4+2, 마법서 2+2, 고개 젓기 1+1, 목 짚기 1, 갸웃하기 3, 카드 3, 손바닥 2) |
| 새 초상화 | 12장, `Assets/Art/Story/Portraits/Word*Dialogue.png`, 1254×1254 |

그림 제작 방법과 프롬프트는 `docs/design/word-gesture-portraits/` 에 있다. 대사 문장과 `English.json` 은 바꾸지 않았다.

## 검증

- PlayMode `StageDialoguePlayTests.NarrationKeepsBothPortraitsAndShowsWordGestures`: 21개 장 130줄을 모두 재생하며 줄마다
  워드 초상화 그림과 밝기, 상대 초상화 표시 여부와 밝기를 확인한다.
- `StageDialoguePlayTests.EveryWordGestureHasItsOwnPortrait`: 몸짓 12가지가 모두 서로 다른 그림을 가진다.
