# 기여 가이드

WordVenture를 함께 개발할 때 따르는 협업 규칙입니다.
개발 환경과 프로젝트 실행 방법은 [README](README.md#unity에서-프로젝트-열기)를 확인해 주세요.

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
