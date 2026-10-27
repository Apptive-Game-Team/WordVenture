# 보스 가장자리 정리

지역 보스 4종과 세계수의 외피/정수, 총 6종 48개 PNG 프레임을 정리했다. 게임에서 사용하는 최종 파일은 `Assets/Art/Combat/Enemies/ActTwo/`의 SnowBoss, ThunderBoss, FurnaceBoss, GraftBoss, WorldTreeBoss, WorldTreeCore 폴더다.

## 비교 방법

각 `*-before-after.png`는 위 두 줄이 원본, 아래 두 줄이 수정본이다. 왼쪽 두 열은 어두운 배경, 오른쪽 두 열은 밝은 배경이다. 8개 애니메이션 프레임의 눈, 장식, 마법과 실루엣을 함께 비교했다. 비교 배경색은 이미지에 저장된 배경이 아니다.

## 편집과 보존

- 내장 imagegen의 background-extraction 편집과 실제 투명 배경 출력을 사용했다. 입력은 `*-reference.png`, 결과 원본은 `*-generated.png`, 프롬프트는 `prompts.json`에 보관했다.
- 생성 결과의 내부 명암/색 변경은 게임에 적용하지 않았다. 원본 f9e0d79의 RGB를 유지하고, 프레임마다 누끼를 정렬해 외곽 1 art pixel 안의 투명 경계만 적용했다. 떨어진 작은 마력/눈발/불티 입자는 보존했다.
- 1차 정리에서 48개 프레임의 경계 픽셀 10,329개를 투명 처리했다. 남아 있는 불투명 픽셀의 RGB 변경 0개, 새 불투명 픽셀 0개다. 프레임 크기, 피벗/PPU/필터/GUID와 프리팹 연결은 그대로다.
- 알파는 기존처럼 0/255만 사용한다. 프레임당 최대 64색이며 기존 픽셀 격자를 유지했다.

`apply-generated-mattes.cjs`는 결과를 보수적으로 통합하는 재현 스크립트다. Node와 sharp가 필요하며 저장소 루트에서 실행한다. 기본 실행은 Logs 아래에 미리보기만 쓰고, `--apply`를 붙이면 에셋에 반영한다. `WORDVENTURE_SHARP`로 설치된 sharp 모듈 경로를 지정할 수 있다. 원본은 고정된 Git 커밋 f9e0d79에서 읽는다.

`validation.json`은 프레임별 정렬/삭제 수, `independent-validation.json`은 별도의 원본 대조 검사다. 전후 비교는 수동 검토했으며 원본 이미지의 배경 잔여물과 의도된 윤곽선을 완벽하게 자동 구분한다는 의미는 아니다. 여러 스테이지의 수동 완주는 실행하지 않았다.


## Unity 검증

Unity 2022.3.62f3 검증 사본에서 전체 EditMode 308 통과/1 GPU 검사 생략, PlayMode 실제 전투 캡처 2개 통과. `-batchmode -nographics -runTests -testPlatform EditMode`, 캡처는 그래픽을 켜고 `-testPlatform PlayMode -testFilter WordVenture.Tests.WorldTreeEntranceCaptures.세계수_먼_반짝임부터_착지까지_실제_전장_캡처;WordVenture.Tests.WorldTreeEntranceCaptures.지역_보스_예고와_세계수_모방_상태_캡처`로 실행했다.

`battle-*.png` 6장을 수동 확인했다. 게임 코드 변경이 없어 전체 PlayMode 행동 검사는 이번에 다시 실행하지 않았다. 결과 요약은 `unity-validation.json`에 있다. Draft를 Ready로 바꾸기 전에 실제 플레이에서 8프레임의 경계가 자연스러운지 최종 확인한다.

## 세계수의 붉은 잎 경계 후속

사용자 확인에서 남은 붉은 주황색 선이 발견돼 세계수 8프레임에 2,359픽셀을 추가 정리했다. 현재 세계수 전후 비교는 이 후속 결과까지 포함한다. 상세 비교와 프롬프트/원본 대조는 `worldtree-red-fringe/`에 있다. 재현할 때는 1차 스크립트 다음에 해당 폴더의 `refine.cjs --apply`를 실행한다.
