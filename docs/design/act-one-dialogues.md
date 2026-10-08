# 1부 지역 대화

복수에 집중하던 워드가 피난민을 만나며 타인의 안전을 살피게 되는 과정을 담는다. 마법의 신이라는 호칭은 2부의 도착점으로 남기고, 현재 1부 엔딩은 복수의 마무리와 앞으로의 선택에 집중한다.

## 진행

| 시점 | 상대 | 대화의 역할 |
| --- | --- | --- |
| 평원 완료 | 평원 마을 주민 | 할아버지의 작별 뒤 워드가 "…할아버지." 하고, 한참 뒤 숨어 있던 주민이 말을 건다. 둘 다 돌아갈 집을 잃었다는 공통점. 주민은 남쪽의 안전한 마을로 피한다 |
| 해안 완료 | 해안 어부 | 워드가 이름을 알려 주고 피난민의 안전을 먼저 묻는 작은 변화 |
| 고원 완료 | 고원 양치기 소년 | 놓친 동생을 찾아 주고, 남쪽 마을에 들러 쉬라는 초대를 받음 |
| 빗길 완료 | 빗길 경비병 | 복수와 함께 더 이상의 피해를 막고 싶다는 동기 |
| 마왕 성 입장 | 워드 혼자 | "드디어"로 시작하는 각오. 할아버지와 길에서 만난 사람들을 떠올림 |
| 마왕 등장 직전 (wave 2) | 슬라임 마왕 | 샌즈에게 당한 동족의 복수라는 마왕의 명분과, 아무도 집을 잃지 않게 하겠다는 워드의 답 |
| 언데드 부활 직전 (wave 3) | 언데드 슬라임 마왕 | 원한 때문에 죽어서도 일어난 마왕. 워드는 원한과 복수를 함께 끝내자고 한다 |
| 마왕 처치 후 | 평원 마을 주민 | 마왕이 쓰러지고 워드가 왔던 길을 되짚어 남쪽 마을로 내려가는 나레이션 뒤, 그곳에서 다시 만나 감사와 귀향. 워드도 돌아갈 관계가 생겼음을 보여 줌 |

지역마다 다른 피난민을 만나고, 평원에서 처음 만난 주민만 마지막에 한 번 다시 만난다. 마지막 대화에서 어부와 양치기 소년도 남쪽 마을에 와 있다고 알려 준다. 호위나 구조 전투 규칙을 추가하지 않고 전후 대사로 표현한다.

클리어 화면에 들어오면 먼저 지역 대화를 보여 주고, 대화가 끝난 뒤 키 입력으로 새 카드를 보여 준 다음 맵으로 나간다. 첫 클리어(평원)에서는 할아버지의 작별 인사와 새 카드 안내 튜토리얼이 먼저 끝난 뒤 지역 대화를 시작한다. 튜토리얼 대사 사이에도 입력 잠금이 잠깐 풀리므로 잠금 대신 튜토리얼 종료 기록을 기다린다. 마왕 성은 맵에서 들어갈 때 대화한 뒤 전투 씬으로 넘어간다. 마왕 전투에서는 `BossBattleWaveData` 의 wave 2(`SlimeKing1`)와 wave 3(`SlimeKing2`)를 시작하기 직전에 대화하고, 대화가 끝나야 적이 나온다. 전투 중 대화는 배경 그림 없이 반투명 막만 깔아 전투 화면이 비쳐 보이게 한다. 보스는 클리어 대화 뒤 귀향 엔딩으로 넘어간다. 클릭이나 아무 키로 타이핑 완료/다음 대사를 구분한다. 마지막 입력은 한 프레임 소비해서 다음 화면으로 넘어가지 않는다.

대화마다 `stageID`, `moment`(`Clear`·`Enter`·`Wave`), `wave` 로 재생 시점을 정하고, `speakerName`·`speakerPortrait` 로 상대를 정한다. 대사 줄의 `narration` 을 켜면 이름과 초상화 없이 나레이션으로 보여 준다. 상대 초상화는 상대가 처음 말할 때부터 보여 준다.

대화 뒤에 다른 씬으로 넘어가는 경우(마왕 성 입장 → 전투, 마왕 처치 후 → 엔딩)에는 대화창을 닫지 않고 씬이 바뀔 때까지 마지막 대사를 덮어 둔다. 대화창을 먼저 닫으면 그 사이 한두 프레임 동안 맵이나 클리어 화면이 깜박인다.

한글은 띄어쓰기 단위로 줄바꿈한다(`TMP Settings.asset` 의 `m_UseModernHangulLineBreakingRules`).

읽음 기록은 `ActOneDialogueSeen` 비트 마스크로 저장한다. 클리어 대화는 0~4번, 입장 대화는 5~9번, wave 대화는 10~29번 비트(10 + stageID × 4 + wave)를 쓴다. 클리어 비트는 이전 버전과 같아서 기존 세이브의 읽음 기록이 유지된다. 같은 대화는 반복하지 않고 새 게임에서 초기화한다. 대화를 끝내기 전에 나가면 읽음 처리하지 않는다. 기존 세이브에는 키가 없어도 정상 동작하며, 이전에 지나간 지역의 대화를 자동으로 몰아서 재생하지 않는다.

## 수정 위치

- 대사·초상화·배경·폰트: `Assets/Resources/Story/ActOneDialogues.asset`
- 대화 화면: `Assets/Scripts/Story/StageDialogueView.cs`
- 공통 창 디자인·본문 폰트: `Assets/Scripts/Story/DialogueWindowPresentation.cs`, `Assets/Resources/Story/DialogueWindowStyle.asset`
- 클리어 후 연결: `Assets/Scripts/Scenes/GameClearController.cs`
- 마왕 성 입장 연결: `Assets/Scripts/Map/MapMove.cs` 의 `SelectStage`
- 마왕 wave 연결: `Assets/Scripts/Combat/Enemies/BattleWaveController.cs` 의 `WaveEndSensor`
- 읽음 저장/초기화: `Assets/Scripts/Core/SaveLoadController.cs`
- 귀향 엔딩: `Assets/ScriptableObjects/EndingScript.asset`
- 초상화(`Assets/Art/Story/Portraits/`): 워드 `WordDialogue.png`, 평원 마을 주민 `VillagerDialogue.png`, 해안 어부 `FishermanDialogue.png`, 고원 양치기 소년 `ShepherdDialogue.png`, 빗길 경비병 `GuardDialogue.png`, 슬라임 마왕 `SlimeKingDialogue.png`, 언데드 슬라임 마왕 `UndeadSlimeKingDialogue.png`, 튜토리얼 할아버지 `GuideDialogue.png`

생성 이미지는 기존 전투 워드의 붉은 단발, 노란 리본, 보라색 눈, 짙은 망토를 참조했다. 워드와 평원 주민 초상화를 내장 image_gen 도구로 개별 생성하고 투명 알파를 보존해 저장했다. 나머지 초상화는 이 두 장을 그림체 참조로 삼아 agy(Gemini)로 마젠타 배경에 생성한 뒤 배경을 투명하게 지우고 1254×1254 캔버스 아래쪽에 맞췄다. 마왕 두 장은 전투 스프라이트 `Boss_Devil`, `Boss_Undead` 를 모습 참조로 썼다. Unity에서는 Sprite, Point 필터, 무압축으로 사용한다. 화자 초상화는 밝게, 듣는 사람은 어둡게 표시한다.

이 디자인을 기준으로 스토리·엔딩·튜토리얼도 [공통 대화창](shared-dialogue-window.md)을 사용한다.

## 생성 프롬프트

### 워드

Use case: stylized-concept. Generate a single transparent dialogue portrait for the existing 2D pixel-art fantasy game WordVenture. The supplied image is identity and pixel style reference, not an edit target. Character: Word, the exact same young female mage, red/coral bob haircut with straight bangs, pale skin, purple eyes, bright pale yellow ribbon on top, dark charcoal-purple cloak over magenta clothing. Preserve recognizable identity and costume. Waist-up three-quarter portrait facing slightly right, composed and quietly determined expression with a hint of sadness, staff with round dark blue head beside her shoulder. Crisp deliberately chunky retro pixel art, limited palette, clean pixel clusters, no painterly detail, no smoothing. Full head ribbon and shoulders visible, cropped neatly at waist, centered with small transparent padding. No text, no frame, no background, no floor, no other characters. Square canvas. Actual transparent alpha.

### 주민

Use case: stylized-concept. Generate one NEW transparent dialogue portrait, matching the pixel art style, chunky pixel clusters, shading, framing and scale of the supplied mage portrait. Reference is only for STYLE; create a DIFFERENT character: a kindly middle-aged female village resident, chestnut brown hair tied in a low loose bun, a few strands by her face, warm brown eyes, muted sage green medieval tunic over cream blouse, worn brown shawl, simple leather shoulder satchel strap. No weapons, no mage staff, no ribbon, no modern items. Waist-up three-quarter portrait facing slightly LEFT to talk to the mage who will be at left of the screen. Expression worried but relieved, modest rural fantasy villager with grounded human warmth. All head and shoulders visible, centered small transparent padding, cropped neatly at waist. Crisp limited-palette retro pixel art consistent with reference. Square canvas. Actual transparent alpha. No text, no border, no scenery, no other people.

## 검증

`StageDialoguePlayTests` 는 다음을 확인한다.

- 실제 GameClearScene 에서 새 카드보다 먼저 대화 진입, 두 초상화 연결, 입력 잠금, 대화 완료 후 새 카드 화면 유지, 읽음 저장 및 재도전 생략, 보스 대화 후 EndingScene 전환, 새 게임 초기화
- 평원·해안·고원·빗길의 화자 이름과 초상화가 모두 다르고, 마왕 처치 후 대화만 평원 주민을 다시 쓰는지
- MapScene 에서 마왕 성을 고르면 대화를 먼저 보여 주고, 다 본 뒤 TurnBattleScene 으로 넘어가는지
- 실제 마왕 전투에서 wave 2·3 직전에 마왕 대화가 나오고, 대화가 끝나기 전에는 적이 나오지 않으며, 배경 그림이 전투 화면을 가리지 않는지

렌더링 미리보기는 `act-one-dialogue-preview.png`(해안 어부), `act-one-castle-dialogue-preview.png`(마왕 성 입장), `act-one-boss-dialogue-preview.png`(슬라임 마왕 등장)에 저장한다. 미리보기는 대화창 canvas 를 카메라 모드로 바꿔 찍으므로 가장 위 sorting layer 에 둔다. 실제 게임에서는 Overlay canvas 라서 전투 카드보다 위에 그려진다.

2026-10-08 Unity 2022.3.34f1 batchmode 에서 `SharedDialoguePlayTests`, `StageDialoguePlayTests`, `TutorialPlayTests` PlayMode 13개와 EditMode 43개가 모두 통과했다.
