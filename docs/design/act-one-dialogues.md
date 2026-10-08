# 1부 지역 대화

복수에 집중하던 워드가 피난민을 만나며 타인의 안전을 살피게 되는 과정을 담는다. 마법의 신이라는 호칭은 2부의 도착점으로 남기고, 현재 1부 엔딩은 복수의 마무리와 앞으로의 선택에 집중한다.

## 진행

| 지역 완료 | 대화의 역할 |
| --- | --- |
| 평원 | 워드도 주민도 돌아갈 집을 잃었다는 공통점 |
| 해안 | 워드의 이름을 알려 주고 피난민의 안전을 먼저 묻는 작은 변화 |
| 고원 | 뒤처진 주민을 챙기고, 워드에게도 돌아와 쉬라는 초대 |
| 빗길 | 복수와 함께 더 이상의 피해를 막고 싶다는 동기 |
| 마왕 | 주민의 감사와 귀향. 워드도 돌아갈 관계가 생겼음을 보여 줌 |

주민 초상화는 피난민 일행의 대표인 같은 인물이다. 해안에서 같은 피난 경로를 따라왔음을 밝히고, 고원과 빗길에서는 다른 피난민을 돕는다. 최종전 뒤에는 길 아래에서 기다리던 주민이 워드를 맞이한다. 호위나 구조 전투 규칙을 추가하지 않고 전후 대사로 표현한다.

일반 지역은 기존 카드 보상 화면을 본 뒤 맵으로 나가기 전에 대화한다. 보스는 지역 대화 뒤 귀향 엔딩으로 넘어간다. 튜토리얼 종료 대화가 먼저 끝나야 진행하며, 클릭이나 아무 키로 타이핑 완료/다음 대사를 구분한다. 마지막 입력은 한 프레임 소비해서 다음 화면으로 넘어가지 않는다.

읽음 기록은 `ActOneDialogueSeen` 비트 마스크로 저장한다. 같은 지역의 읽은 대화는 반복하지 않고 새 게임에서 초기화한다. 대화를 끝내기 전에 나가면 읽음 처리하지 않는다. 기존 세이브에는 키가 없어도 정상 동작하며, 이전에 지나간 지역의 대화를 자동으로 몰아서 재생하지 않는다.

## 수정 위치

- 대사·초상화·배경·폰트: `Assets/Resources/Story/ActOneDialogues.asset`
- 대화 화면: `Assets/Scripts/Story/StageDialogueView.cs`
- 공통 창 디자인·본문 폰트: `Assets/Scripts/Story/DialogueWindowPresentation.cs`, `Assets/Resources/Story/DialogueWindowStyle.asset`
- 클리어 후 연결: `Assets/Scripts/Scenes/GameClearController.cs`
- 읽음 저장/초기화: `Assets/Scripts/Core/SaveLoadController.cs`
- 귀향 엔딩: `Assets/ScriptableObjects/EndingScript.asset`
- 초상화: `Assets/Art/Story/Portraits/WordDialogue.png`, `VillagerDialogue.png`

생성 이미지는 기존 전투 워드의 붉은 단발, 노란 리본, 보라색 눈, 짙은 망토를 참조했다. 두 초상화를 내장 image_gen 도구로 개별 생성하고 투명 알파를 보존해 저장했다. Unity에서는 Sprite, Point 필터, 무압축으로 사용한다. 화자 초상화는 밝게, 듣는 사람은 어둡게 표시한다.

이 디자인을 기준으로 스토리·엔딩·튜토리얼도 [공통 대화창](shared-dialogue-window.md)을 사용한다.

## 생성 프롬프트

### 워드

Use case: stylized-concept. Generate a single transparent dialogue portrait for the existing 2D pixel-art fantasy game WordVenture. The supplied image is identity and pixel style reference, not an edit target. Character: Word, the exact same young female mage, red/coral bob haircut with straight bangs, pale skin, purple eyes, bright pale yellow ribbon on top, dark charcoal-purple cloak over magenta clothing. Preserve recognizable identity and costume. Waist-up three-quarter portrait facing slightly right, composed and quietly determined expression with a hint of sadness, staff with round dark blue head beside her shoulder. Crisp deliberately chunky retro pixel art, limited palette, clean pixel clusters, no painterly detail, no smoothing. Full head ribbon and shoulders visible, cropped neatly at waist, centered with small transparent padding. No text, no frame, no background, no floor, no other characters. Square canvas. Actual transparent alpha.

### 주민

Use case: stylized-concept. Generate one NEW transparent dialogue portrait, matching the pixel art style, chunky pixel clusters, shading, framing and scale of the supplied mage portrait. Reference is only for STYLE; create a DIFFERENT character: a kindly middle-aged female village resident, chestnut brown hair tied in a low loose bun, a few strands by her face, warm brown eyes, muted sage green medieval tunic over cream blouse, worn brown shawl, simple leather shoulder satchel strap. No weapons, no mage staff, no ribbon, no modern items. Waist-up three-quarter portrait facing slightly LEFT to talk to the mage who will be at left of the screen. Expression worried but relieved, modest rural fantasy villager with grounded human warmth. All head and shoulders visible, centered small transparent padding, cropped neatly at waist. Crisp limited-palette retro pixel art consistent with reference. Square canvas. Actual transparent alpha. No text, no border, no scenery, no other people.

## 검증

`StageDialoguePlayTests`는 실제 GameClearScene에서 대화 진입, 두 초상화 연결, 입력 잠금, 대화 완료 후 맵 전환, 읽음 저장 및 재도전 생략, 보스 대화 후 EndingScene 전환, 새 게임 초기화를 확인한다. 렌더링 미리보기는 `docs/design/act-one-dialogue-preview.png`에 저장한다.

Unity 2022.3.34f1의 검증용 프로젝트 복사본에서 위 PlayMode 테스트 3개가 모두 통과했다. 두 PNG는 1254×1254이며 모서리 알파가 0인 것을 확인했다. 실제 렌더링에서 초상화, 한글 폰트, 대화창 배치를 확인했다.

기존 `TutorialPlayTests` 회귀 테스트 4개도 모두 통과했다. 검증 결과는 `Logs/StageDialogue/playmode-results.xml`, `tutorial-results.xml`에 보관한다.
