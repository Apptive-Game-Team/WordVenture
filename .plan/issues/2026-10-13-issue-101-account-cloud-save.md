# Issue 101: 타이틀 씬에 계정 서버 저장·불러오기, 피드백 전송, 저장 stage 표시 추가

- Issue: https://github.com/Apptive-Game-Team/WordVenture/issues/101
- Branch: `feat/account-cloud-save`
- 서버: `https://account.theevilent.com`, Key-Value system `wordventure` (관리자가 `/admin/systems` 에서 등록)
- 관련 서버 변경: Apptive-Game-Team/AccountServer#99 (`key_value.value` 를 31자에서 1000자로 확장)

## 변경

- `Assets/Scripts/Account/`: 계정 서버와 통신하는 코루틴 API(`AccountService`)를 추가한다.
  - 이메일 가입, 로그인, 로그아웃, refresh token 으로 로그인 상태 복원
  - refresh token 은 PlayerPrefs `AccountRefreshToken` 에 두고, access token 은 메모리에만 둔다.
  - access token 이 만료 1분 전이면 요청 전에 refresh 한다. 서버는 만료된 요청에 401 대신 `/login` 으로 302 를
    보내는데, WebGL 에서는 브라우저가 302 를 따라가 버려서 만료 응답을 알아챌 수 없기 때문이다.
    데스크톱에서는 302·401 응답을 받아도 refresh 를 한 번 하고 요청을 한 번 다시 보낸다.
  - 진행 상황(`CloudSave`)은 key `save` 에 `1|stage|tutorial|act1(16진)|act2(16진)|map` 형식으로 저장한다.
    최대 28자라서 서버 migration 이 배포되기 전에도 31자 제한 안에 들어간다.
  - 피드백은 GyeMong 과 같은 형식이다: `feedback-id` 를 1 올리고 `feedback-{id}` 에 본문을 저장한다. 최대 500자.
- 타이틀 씬
  - `TitleStageView`: 타이틀 배경을 저장된 stage 의 전투 배경으로 바꾼다. 지역 이름 라벨은 두지 않는다.
    저장이 없으면 기존 배경을 쓴다. 2부까지 끝냈으면 세계수의 심장 배경을 보여 준다.
    - 전투 배경은 모두 1024×512(2:1)이고 타이틀 배경은 16:9 라서, 카메라 화면을 덮는 같은 배율을 계산한다. 좌우가 약 11%씩 잘린다.
    - 빗길(stage 3)은 전투 씬과 같은 `RainyBackgroundLegacy` prefab 을 띄워 비가 내린다. 전투 씬은 이 prefab 을
      가로·세로 다른 배율로 늘리지만, 타이틀에서는 같은 배율로 덮는다.
  - `계정` 버튼과 계정 창(`AccountPanel`, `LoginForm`, `MemberView`, `FeedbackPanel`)을 추가한다.
    회원 화면은 이 기기와 서버의 진행 stage 를 나란히 보여 주고, 서버에서 불러오기 전에 한 번 더 묻는다.
  - `AccountRequests`: 계정 요청을 하나씩 순서대로 보낸다. refresh token 은 호출마다 바뀌므로 두 요청이 동시에
    refresh 하면 한쪽이 거절된다.
- stage 0~4 의 `stageName` 을 `ActOneDialogues` 의 장 제목에서 가져와 채운다: 평원, 해안, 고원, 빗길, 마왕 성.
- 새 UI 문구와 지역 이름의 영어 번역을 `English.json` 에 추가한다.

## 검증

- EditMode: `CloudSave` 직렬화 왕복, 31자 이하, 잘못된 문자열 거절, PlayerPrefs 읽기·쓰기
- EditMode: 서버 응답(상태 코드와 본문)을 사용자 문구로 바꾸는 분류
- 기존 EditMode 테스트(BuildSettings, PrefabWiring, SceneCanvasOrder, Localization)
- 에디터 렌더링으로 타이틀과 계정 창 화면 확인

## 남은 위험

- 실제 서버와의 가입·저장·불러오기·피드백 흐름은 Play Mode 에서 확인해야 한다.
- Unity WebGL 은 한글 IME 입력을 지원하지 않아서, WebGL 빌드에서는 피드백을 한글로 입력하기 어렵다.
- GitHub Pages 에서 WebGL 빌드를 쓰려면 계정 서버의 `CORS_ALLOWED_ORIGINS` 에 그 origin 을 추가해야 한다.
  itch.io(`https://html-classic.itch.zone`)는 이미 허용되어 있다.
