# 기여 가이드

WordVenture를 함께 개발할 때 따르는 협업 규칙입니다.
게임 소개와 플레이 방법은 [README](README.md)를 확인해 주세요.

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

## 브랜치 흐름

1. 기능별 브랜치를 만들어 작업합니다.
2. 작업을 마치면 본인 브랜치에 `develop`을 먼저 병합하고 충돌과 오류를 해결합니다.
3. 검증을 마친 변경을 `develop`에 병합합니다.
4. 안정화된 버전을 `main`에 병합합니다.

## 코드와 리소스 정리

- 코드와 리소스는 해당 기능의 폴더에 정리하고, C# 코드에 네임스페이스를 적용합니다.
- 이름은 [C# 명명 지침](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-classes-structs-and-interfaces)을 참고합니다.
- 새 에셋을 추가하거나 이동할 때 Unity의 `.meta` 파일도 함께 관리합니다.

## 커밋과 문서 작성

- 커밋, PR, 릴리스 노트는 한국어로 작성하며, 코드 식별자와 API 이름은 원문을 유지합니다.
- 커밋은 Conventional Commits 형식을 사용합니다.

```text
<type>: <한글 변경 사항>
```

예:

```text
fix: 카드 조합 후 대상 선택 오류 수정
feat: 타이틀 화면 크레딧 추가
docs: 게임 실행 안내 보강
```

## 관련 지침

- [AGENTS.md](AGENTS.md): 저장소의 에이전트 작업 지침
- [릴리스 가이드](.agents/docs/release.md): 버전 관리와 릴리스 발행 절차
