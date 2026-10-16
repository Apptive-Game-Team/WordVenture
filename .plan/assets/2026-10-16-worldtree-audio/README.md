# 세계수 등장 효과음

직접 합성한 22,050 Hz 모노 PCM16 WAV 4개다. 외부 음원이나 생성 API를 사용하지 않았고 `synthesize.py`의 고정 난수 시드로 재현한다. 게임 에셋은 `Assets/Resources/Combat/WorldTreeAudio/`에 있다. 음원을 합성할 때 사용한 Python의 numpy 외에 실행 중 추가 의존성은 없다.

| 파일 | 등장 연출 | 외피 처치 후 본체 등장 |
| --- | --- | --- |
| Flash.wav | 0.85초 반짝임의 짧은 종소리 | 1.7초 본체 출현 |
| Approach.wav | 1.05~2.15초, 비행 진행에 따라 음량과 좌우 위치 증가 | 사용하지 않음 |
| Landing.wav | 2.15초 착지 충격과 낮은 잔향 | 0.3초 외피 붕괴, 낮은 음량 |
| CoreRise.wav | 사용하지 않음 | 1.05초 응집 시작부터 본체가 커지는 구간 |

`entrance-preview.wav`와 `core-preview.wav`는 동일 파일과 실제 시점/음량으로 만든 모노 미리듣기다. 게임 녹음은 아니며 실제 BGM과 좌우 패닝은 포함하지 않는다. 녹화 GIF에는 음성이 들어가지 않는다.

오디오는 연출 호스트의 자식으로만 재생한다. 일시정지 때 Pause/UnPause, 시간 배율에 따라 pitch 조절, 스킵/호스트 비활성화/코루틴 취소/씬 종료 때 즉시 Stop 후 정리한다. 큰 프레임 지연으로 이미 지나간 반짝임/비행이 착지음과 함께 몰아서 재생되지 않는다. 음량은 기존 전투 소리와 같이 AudioListener의 전역 음량을 따르고 별도 설정 UI는 추가하지 않았다.

`signal-validation.json`에는 길이/피크/RMS를 기록했다. 클리핑 없이 시작과 끝을 6ms 페이드 처리했다. 신호 분석은 기기별 청감 평가를 대신하지 않는다. 실제 BGM 대비 음량과 저음 체감은 Draft를 Ready로 바꾸기 전에 플레이하며 확인한다.

PR #137이 작업 중 병합되어 최신 develop에서 별도 후속 브랜치를 만들었다. 이슈 #138.


## 검증

- 실제 WAV 6개(게임 4개, 미리듣기 2개) 헤더/무클리핑/끝점 0 확인.
- Unity 2022.3.62f3 전체 EditMode 308 통과/1 GPU 생략, 관련 PlayMode 13개 통과.
- `-batchmode -runTests -testPlatform PlayMode -testFilter WordVenture.Tests.WorldTreeEntrancePlayTests;WordVenture.Tests.TwoPhaseBossPlayTests`, EditMode는 `-nographics -testPlatform EditMode`.
- 첫 PlayMode 실행은 resumedFrame 입력 보호를 fixture가 무시해 1개 실패했다. 검사에서 메뉴를 닫은 다음 프레임에 Skip을 호출하도록 고친 뒤 13개를 모두 재실행해 통과했다.
- 전체 PlayMode/플랫폼별 빌드와 장치별 청감 확인은 실행하지 않았다. 원시 요약은 `unity-validation.json`과 `wav-validation.json` 참고.
