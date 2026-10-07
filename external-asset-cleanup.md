# 외부 에셋 정리

- 참조 없는 외부 에셋 685개와 해당 .meta 제거 (약 273.44MB, .meta 제외).
- AnimatedPixelArtBackgrounds: 519개. ArtResource: 166개.
- 프로젝트 에셋과 ProjectSettings에서 시작해 GUID 의존성을 재귀 추적. Resources, 코드, 셰이더, DOTween, TextMeshPro, 라이선스와 패키지 문서는 보존.
- 스토리와 타이틀의 미사용 배경 Animator 및 타이틀의 기존 보너스 배경 GameObject 제거.
- 남은 파일에서 삭제된 GUID 참조 0개 확인.
- Unity 배치 테스트를 시도했지만 LicensingClient 연결 오류 뒤 결과가 생성되지 않아, 실행과 화면 검증은 미확인.
