# Handoff — 2026-10-14 픽셀 크기 통일 후속: Codex 재생성

Codex에 이 파일 경로를 주고 "이 핸드오프대로 이어서 작업해"라고 하면 된다.

## 작업 위치

- 브랜치: `fix/pixel-perfect-backgrounds` (원격에 푸시됨). 여기서 계속 커밋한다.
- PR 대상: `develop`. 아직 PR 없음.
- 먼저 읽을 것: `AGENTS.md`, `.agents/skills/pixel-art/SKILL.md`,
  `.agents/skills/pixel-art/regen-candidates.md`

## 지금까지 한 일

1. AI로 생성한 이미지 20장이 픽셀아트처럼 보이기만 하고 격자가 없었다(안티앨리어싱, 색 12만~25만 개).
   이것을 실제 픽셀 격자로 변환했다.
2. 게임 전체의 픽셀 크기를 통일했다. 기준은 **아트 1픽셀 = 1080p 화면 4px(480x270 가상 화면)**이다.
   - 사용 중인 텍스처 214개를 1텍셀 = 1아트픽셀로 다시 샘플링했다.
   - 대상 크기는 씬, 프리팹, 애니메이션, 런타임 코드를 분석해 정했다.
   - SpriteRenderer는 `PPU = 기존 PPU x 텍셀당 화면 px / 4`로 바꿔 월드 크기와 충돌 범위를 유지했다.
   - 스프라이트 시트 rect와 outline도 같은 비율로 조정했다. Point 필터와 무압축으로 통일했다.
3. `.agents/skills/pixel-art/`를 추가했다. 기준, 생성 프롬프트 틀, 변환 도구 `scripts/pixelate.py`가 들어 있다.
4. 워드 전투 스프라이트 7장과 워드 대화 초상화에는 `--keep-lines 2`를 적용했다(외곽선 끊김 보정).

## 남은 일

### 1. Codex image_gen으로 재생성(핵심)

대상, 크기, 옵션은 `.agents/skills/pixel-art/regen-candidates.md`에 있다.
**우선순위**는 다음과 같다.

1. 대화 초상화 8장, 특히 `WordDialogue.png`
2. 스토리·타이틀 배경 6장
3. 로고
4. 나머지

에셋마다 아래 순서로 처리한다.

1. 참고 이미지를 준비한다.
   - 현재 PNG: 구도와 색 기준
   - AI 원본: `git show b7c2ed0:<경로> > /tmp/ref.png`. 대화 초상화와 스토리 배경이 해당한다.
   - 대화 초상화는 같은 캐릭터의 전투 스프라이트도 참고로 준다. 워드는 `Assets/Art/Combat/Word/WordIdle1.png`이다.
2. 아래 프롬프트 틀로 생성한다. 출력은 리포 밖 임시 폴더(`/tmp/regen/` 등)에 저장한다.
3. 격자에 맞춰 대상 경로에 덮어쓴다.

   ```bash
   python3 .agents/skills/pixel-art/scripts/pixelate.py /tmp/regen/<name>.png <대상경로> --target <W>x<H> [옵션]
   ```

   `.meta`는 이미 그 크기에 맞춰져 있으니 `--update-meta`는 쓰지 않는다.
4. 결과를 4배 nearest로 확대해서 원본과 나란히 놓고 확인한다. 얼굴, 글자, 외곽선이 깨졌으면 다시 생성한다.
5. 통과하면 `regen-candidates.md`에서 해당 줄을 "처리 완료"로 옮긴다.
   생성에 쓴 최종 프롬프트는 `background-generation.md`에 기존 형식으로 기록한다.

프롬프트 틀은 다음과 같다. 대괄호를 채운다.

```text
Redraw the attached reference image as true pixel art for the game WordVenture.
[무엇을 그린 이미지인지 한 줄, 예: "Dialogue portrait of Word, a young red-haired
mage girl with a yellow ribbon, black cloak and a staff with a blue orb, bust shot".]
Keep the same composition, character design, pose and colours as the reference.
Draw it on an exact [W]x[H] pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about [24-48] colours, clean 1-pixel dark outline
[characters only]. [Transparent background.] No text, UI or logos [except the
logo's own lettering]. Match the palette and density of the other attached game art.
```

에셋별 주의사항은 다음과 같다.
- **대화 초상화:** 8장이 같은 크기 비율로 보이도록 머리 크기와 프레이밍을 맞춘다. 상반신이고 투명 배경이다.
- **산골 집 배경 4장(평상시, 습격, 폐허, 추모):** 집 위치와 산 실루엣이 같아야 한다. 평상시 버전을 먼저 만들고, 나머지 3장은 그 결과를 편집 대상으로 넘긴다.
- **맵 배경 5장(DefaultBG, Stage1~4BG):** 길과 지형 좌표가 같아야 한다. 맵 노드 위치가 고정이기 때문이다.
- **게임오버 3장, 타이틀 머리 2장:** 애니메이션 프레임이다. 프레임 사이의 차이만 유지한다.
- **로고, NewCardTitle, 카드:** 글자 모양을 원본과 같게 유지한다. 대상 크기가 작아서, 글자는 픽셀 폰트 느낌으로 또렷하게 그리라고 지시한다.

### 2. Unity에서 확인(이 환경들에는 Unity가 없어 아직 미검증)

Unity 2022.3.34f1에서 TitleScene부터 플레이한다.
- 모든 씬에서 오브젝트 크기와 위치가 이전과 같은지
- 대화 초상화(화면 높이 63%), 튜토리얼 손(72px), 원소 상태 오버레이의 크기.
  런타임 코드에서 크기를 정해서 정적 분석으로 추정한 값이다.
- 카드: 런타임 스케일이 `(0.379, 0.42)`로 균일하지 않아 픽셀이 가로 4px x 세로 약 4.4px다.
  `CardManager.RoundAlignment` 호출부의 스케일을 균일하게 바꿀지 판단한다(카드 위 텍스트 배치에 영향 있음).
- 콘솔에 스프라이트 임포트 경고가 없는지

### 3. PR

- `develop` 대상 **draft** PR로 연다. 제목과 본문은 한국어이고, `.agents/docs/pull-request.md` 템플릿을 따른다.
- 본문의 Example에는 전후 비교 이미지를 넣는다. 이미지는 `.plan/assets/pixel-art/`에 커밋하고, 커밋 해시로 고정한 raw 링크를 쓴다.
- Validation에는 Unity 확인을 했는지 안 했는지 분명히 적는다.

## 하지 말 것

- 2배·4배·8배로 미리 키운 PNG를 커밋하지 않는다. 1텍셀 = 1아트픽셀로 저장한다.
- PNG 크기를 바꾸면서 `.meta`를 그대로 두지 않는다. 크기를 바꿀 때는 `--update-meta`를 쓰거나 PPU를 계산한다.
- `Assets/ThirdParty/TextMeshPro`, 폰트, WebGL 템플릿 아이콘은 건드리지 않는다.
- 커밋 메시지는 Conventional Commits에 한국어 요약으로 쓴다(`AGENTS.md` 참고).
