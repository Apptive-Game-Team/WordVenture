# Issue 92: 2부 대사 85줄과 2부 화면 문구에 영어 번역 추가

- Issue: https://github.com/Apptive-Game-Team/WordVenture/issues/92
- Branch: `feat/act-two-english` (`fix/act-two-grandfather-records` 위)

## 배경

1부 대사와 화면 문구는 `Assets/Resources/Localization/English.json` 에 모두 번역돼 있지만, 2부(스테이지 5~9 지역 대화와
2부 에필로그)는 #87 에서 바꾼 6줄만 번역돼 있었다. 영어로 설정해도 2부 대사는 한국어로 나왔다.

## 확인 방법

`LocalizationTests` 의 세 테스트를 Python 으로 옮겨 실행했다.

- 대사 에셋: `Assets` 아래 ScriptableObject(`Assets/ThirdParty/` 제외)의 `script` 의 `name`·`text`, `chapters` 의
  `title`·`speakerName`·`lines.text` 중 한글이 있는 문자열
- 빌드 씬: `EditorBuildSettings` 의 7개 씬에서 `m_text` 의 한글 문구(`\uXXXX` escape 해제)
- 폰트: `NeoDunggeunmoPro-Regular SDF.asset` 의 `m_CharacterTable` 에 있는 글자만 쓰는지

1부 문자열 중 빠진 것은 없었다.

## 추가한 번역 75개

| 구분 | 개수 |
| --- | --- |
| 2부 지역 대화 줄(`ActOneDialogues` 의 `lines.text`) | 51 |
| 2부 지역 대화 제목(`title`) | 12 |
| 2부 화자 이름(`speakerName`) | 5 |
| 2부 에필로그(`ActTwoEndingScript` 의 `text`) | 5 |
| 맵 장 전환 버튼 문구(`MapScene` 의 "2부 맵으로 (Tab)", `MapMove` 의 "1부 맵으로 (Tab)") | 2 |

#87 에서 번역한 6개(대화 줄 4, 제목 1, 에필로그 1)를 더하면 2부 문자열은 모두 번역된다. 중복 key 는 없다.

## 이름 정하기

기존 번역(Frost Village, Ashen Ruins, World Tree, Slime Lord, Miss Mage, Grandpa)을 그대로 따랐다.

| 한국어 | 영어 |
| --- | --- |
| 서리 마을 | Frost Village |
| 벼락 협곡 | Thunder Canyon |
| 잿빛 유적 | Ashen Ruins |
| 뒤틀린 숲 | Twisted Forest |
| 세계수의 심장 | Heart of the World Tree |
| 서리 마을 주민 | Frost Villager |
| 협곡 순찰대원 | Canyon Ranger |
| 유적 연구자 | Ruins Researcher |
| 숲 피난민 | Forest Refugee |
| 폭주한 마력의 형상 | Form of Runaway Magic |
| 마력 | magic |
| 언니 (숲 피난민이 워드를 부를 때) | Miss |

제목의 ` · ` 는 1부처럼 ` - ` 로, `…` 는 `...` 로 옮겼다. 폰트 atlas 에 em dash·곡선 따옴표·`…` 가 없다.

## 함께 고친 것

- `Assets/Scripts/Map/MapMove.cs`: 맵 장 전환 버튼 문구를 코드에서 다시 쓸 때 `Localization.Translate` 를 거치게 했다.
  씬을 열 때 영어로 바뀐 문구를 `Start` 에서 한국어로 덮어쓰고 있었다.

## 검증

- `English.json` JSON 파싱 확인, 항목 133개 → 208개
- Python 으로 옮긴 누락 검사: 대사 에셋·빌드 씬 모두 0개
- 폰트 글자 검사: atlas 에 없는 글자 0개
- Unity 테스트는 실행하지 않았다.
