# 공통 대화창

새 1부 지역 대화창의 디자인을 StoryScene, EndingScene, 튜토리얼에도 적용한다. 짙은 패널, 금색 테두리, 화자 이름, 본문, 하단 진행 안내, 초상화 배치를 동일한 표시 컴포넌트에서 구성한다.

## 공통 표시

- `DialogueWindowPresentation`: 패널·글자·초상화의 공통 레이아웃.
- `Assets/Resources/Story/DialogueWindowStyle.asset`: 폰트, 글자 크기, 색상, 기본 워드 초상화. 디자인 수정은 이 자산에서 시작한다.
- `ChatWindowController`: 공통 타이핑과 진행 안내. 한 번 누르면 대사를 펼치고, 그다음 입력으로 진행하는 기존 흐름을 유지한다.
- 나레이션: 이름과 초상화를 숨기고 본문을 넓게 표시한다.
- 튜토리얼: 안내 화자 한 명의 초상화를 표시하고, 행동을 기다리는 동안 창 전체를 숨긴다.
- 지역 대화: 두 초상화를 표시하고 현재 화자를 밝게 보여 준다.

CanvasScaler는 1920×1080 기준으로 통일한다. 숨김 상태에서 타이핑 코루틴과 진행 상태를 정리해 튜토리얼 창을 다시 열 때 이전 스트리밍이 남지 않게 한다.

기존 씬과 튜토리얼 프리팹의 컨트롤러 참조는 유지한다. 실행 시 기존 표시 자식을 숨기고 공통 창을 만들므로 씬별로 디자인을 따로 수정할 필요가 없다. 별도 프리팹을 복제해 세 가지 디자인을 관리하지 않고 하나의 표시 컴포넌트를 재사용한다.

## 유지하는 동작

StoryController의 배경 전환과 스토리 진행, 지역 클리어의 보상·읽음 기록·씬 전환, 튜토리얼의 행동 감지·강조 표시·손 시연·스킵 확인·입력 잠금을 유지한다. 튜토리얼의 스킵 버튼과 확인 창은 대화창 위에 표시한다.

지역마다 다른 주민을 만나도록 바꾼 대사와 초상화, 마왕 대화는 [1부 지역 대화](act-one-dialogues.md)에 정리한다.

## 검증

`SharedDialoguePlayTests`에서 StoryScene과 튜토리얼의 동일 폰트·본문 크기·패널 색, 나레이션과 단일 화자의 표시 차이, 스킵 버튼 표시, 행동 단계에서 창 숨김과 입력 해제, 재표시 후 스트리밍, EndingScene 연결을 확인한다. 기존 `StageDialoguePlayTests`, `TutorialPlayTests`도 함께 실행한다.

실제 Unity 렌더링 미리보기는 `shared-dialogue-story.png`, `shared-dialogue-tutorial.png`, `act-one-dialogue-preview.png`로 보관한다.
