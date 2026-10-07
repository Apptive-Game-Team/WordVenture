# WordVenture · 워드의 모험

**카드를 조합해 마법을 만들고, 나만의 전략으로 모험을 이어가는 2D 턴제 전투 게임.**

WordVenture는 **Team 6203**가 GIGDC 2024 출품을 위해 제작한 Unity 게임입니다.
주인공 워드와 함께 마법을 익히고, 스테이지를 진행하며 적들과 맞서 싸워 보세요.

[게임 다운로드](https://github.com/Apptive-Game-Team/WordVenture/releases) · [빌드 및 배포 현황](https://github.com/Apptive-Game-Team/WordVenture/actions) · [GIGDC 대회 소개](https://www.gigdc.or.kr/sub01/sub02.php)

## 게임 소개

마법 카드와 속성 카드를 조합하면 하나의 주문이 완성됩니다.
어떤 마법을 만들지, 누구에게 사용할지, 언제 턴을 마칠지가 전투의 핵심입니다.

- **카드 조합:** 마법 카드와 속성 카드를 조합해 주문을 시전합니다.
- **다양한 마법:** 발사, 낙하, 소환 방식의 마법을 사용할 수 있습니다.
- **속성 상성:** 불, 얼음, 바위, 번개, 신성, 언데드 속성의 상성을 고려해 싸웁니다.
- **턴제 전투:** 플레이어와 적이 번갈아 행동합니다. 내 턴이 시작되면 카드를 두 장 받습니다.
- **스테이지 모험:** 이야기와 맵, 전투를 거쳐 모험을 진행합니다.
- **튜토리얼:** 카드 조합과 대상 선택, 턴 종료를 직접 따라 하며 배울 수 있습니다. 건너뛰기도 지원합니다.
- **이어하기:** 저장된 스테이지 진행 위치에서 모험을 이어갈 수 있습니다.

## 플레이 방법

기본 조작은 **마우스 클릭과 드래그**입니다.

| 순서 | 행동 |
| --- | --- |
| 1 | 타이틀에서 새 게임을 시작하거나 저장된 진행을 이어갑니다. |
| 2 | 전투에서 조합 버튼을 눌러 조합창을 엽니다. |
| 3 | 마법 카드와 속성 카드를 각각 알맞은 칸에 드래그합니다. |
| 4 | 조합을 실행한 뒤 마법을 사용할 대상을 선택합니다. |
| 5 | 행동을 마치면 오른쪽 위의 **Turn End** 버튼을 누릅니다. |
| 6 | 적의 행동이 끝나면 다음 플레이어 턴을 진행합니다. |

새 게임을 시작하면 기존 스테이지 진행과 튜토리얼 완료 기록이 초기화됩니다.
이어하기는 저장 데이터가 있을 때 표시됩니다.

타이틀의 **크레딧** 버튼에서 제작진을 확인할 수 있으며, 이름을 누르면 GitHub 프로필이 열립니다.
크레딧은 닫기 버튼 또는 **Esc**로 닫을 수 있습니다.

## 다운로드와 실행

[Releases](https://github.com/Apptive-Game-Team/WordVenture/releases)에서 해당 버전의 다운로드 링크와 실행 안내를 확인해 주세요.
릴리스 빌드는 Windows, macOS, WebGL을 대상으로 구성되어 있습니다.
웹 플레이 링크와 플랫폼별 안내는 각 릴리스 본문에서 확인할 수 있습니다.

## Unity에서 프로젝트 열기

### 개발 환경

| 항목 | 구성 |
| --- | --- |
| 엔진 | Unity **2022.3.34f1** |
| 언어 | C# |
| UI | Unity UI, TextMesh Pro |
| 데이터 | ScriptableObject |
| 진행 저장 | PlayerPrefs |
| 테스트 | Unity Test Framework · EditMode / PlayMode |
| 자동 빌드 | GitHub Actions, GameCI |

### 실행 순서

1. Unity Hub에서 **Unity 2022.3.34f1**을 설치합니다. 직접 빌드할 플랫폼의 Build Support 모듈도 함께 설치합니다.
2. 저장소를 복제합니다.

   ```bash
   git clone https://github.com/Apptive-Game-Team/WordVenture.git
   ```

3. Unity Hub에서 복제한 `WordVenture` 폴더를 프로젝트로 추가하고 엽니다.
4. 패키지 설치와 에셋 임포트가 끝나면 `Assets/Scenes/TitleScene.unity`를 엽니다.
5. 에디터의 **Play** 버튼을 눌러 타이틀부터 실행합니다.

직접 빌드할 때는 **File → Build Settings**에서 대상 플랫폼을 선택합니다.
저장소의 빌드 설정에는 타이틀, 스토리, 맵, 전투, 게임 클리어, 게임 오버, 엔딩 씬이 등록되어 있습니다.

### 테스트

**Window → General → Test Runner**에서 EditMode와 PlayMode 테스트를 실행할 수 있습니다.
테스트 코드는 `Assets/Tests`에 있으며, 씬 빌드 설정, 프리팹 연결, 스테이지·적 데이터와 튜토리얼 동작을 확인합니다.

## 프로젝트 구조

```text
Assets/
├── Art/                 게임 아트, UI, 폰트, 애니메이션
├── Prefabs/             재사용 게임 오브젝트와 UI
├── Scenes/              타이틀, 스토리, 맵, 전투, 엔딩 등
├── ScriptableObjects/   게임 데이터와 대사
├── Scripts/
│   ├── Battle/          턴 진행
│   ├── Cards/           카드 데이터와 관리
│   ├── Combat/          마법, 적, 스테이지, 전투 UI
│   ├── Core/            저장·불러오기와 공통 기능
│   ├── Map/             맵 이동
│   ├── Scenes/          씬별 흐름과 타이틀 크레딧
│   ├── Story/           스토리와 대화
│   └── Tutorial/        튜토리얼 진행과 행동 안내
├── Tests/               EditMode / PlayMode 테스트
├── ThirdParty/          외부 리소스
└── WebGLTemplates/      웹 빌드 템플릿
```

## 제작진 · Team 6203

<table>
  <tr>
    <th align="center">문성필</th>
    <th align="center">김현진</th>
    <th align="center">정윤성</th>
    <th align="center">정진욱</th>
    <th align="center">황인섭</th>
  </tr>
  <tr>
    <td align="center"><a href="https://github.com/Monolong"><img src="https://avatars.githubusercontent.com/u/83206119?v=4" width="120" alt="문성필 프로필" /></a></td>
    <td align="center"><a href="https://github.com/Gimlocal"><img src="https://avatars.githubusercontent.com/u/127363458?v=4" width="120" alt="김현진 프로필" /></a></td>
    <td align="center"><a href="https://github.com/dev-yunseong"><img src="https://avatars.githubusercontent.com/u/88422717?v=4" width="120" alt="정윤성 프로필" /></a></td>
    <td align="center"><a href="https://github.com/Jinwook700"><img src="https://avatars.githubusercontent.com/u/127014921?v=4" width="120" alt="정진욱 프로필" /></a></td>
    <td align="center"><a href="https://github.com/hwanginseop"><img src="https://avatars.githubusercontent.com/u/163392234?v=4" width="120" alt="황인섭 프로필" /></a></td>
  </tr>
  <tr>
    <td align="center">개발자<br /><a href="https://github.com/Monolong">@Monolong</a></td>
    <td align="center">개발자<br /><a href="https://github.com/Gimlocal">@Gimlocal</a></td>
    <td align="center">개발자<br /><a href="https://github.com/dev-yunseong">@dev-yunseong</a></td>
    <td align="center">디자이너 / 개발자<br /><a href="https://github.com/Jinwook700">@Jinwook700</a></td>
    <td align="center">개발자<br /><a href="https://github.com/hwanginseop">@hwanginseop</a></td>
  </tr>
</table>

### 아트 사용 안내

**이 게임에는 AI로 생성된 아트가 사용되었습니다.**

## 함께 개발하기

브랜치 흐름, 코드 작성 규칙과 리소스 관리 방법은 [기여 가이드](CONTRIBUTING.md)를 확인해 주세요.
