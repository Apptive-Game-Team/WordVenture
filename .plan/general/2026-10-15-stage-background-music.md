# 2026-10-15 — ACE Step 스테이지 배경음악 제작

- Date: 2026-10-15
- GitHub Issue: None
- Status: 첫 10곡은 너무 밝고 난잡하다는 사용자 피드백. 평원 v2 생성 완료, 사용자 청취 대기.

## Goal

ACE Step으로 스테이지 0~9에 어울리는 무보컬 배경음악을 각각 제작한다. 곡마다 지역의 개성을 살리되 픽셀 판타지 RPG의 음악적 분위기를 공유한다.

### 사용자 피드백에 따른 방향 수정

첫 초안은 밝고 요소가 많아 난잡하다는 평가를 받았다. 다른 지역의 재생성에 앞서 평원 한 곡을 84 BPM·D minor·64초로 다시 제작한다. 중저음 건반의 짧은 모티프, 단순한 베이스, 약한 림 타격 세 파트로 제한하고 여백과 일정한 편곡을 요청한다. 기존 음원을 보존하며 `MusicDrafts/ace-step-plain-v2/`에 별도 저장한다. 실제 편곡 준수 여부는 사용자 청취로 확인한다.

v2 결과는 64초·48kHz 스테레오이며 피크 -3.0dB, 평균 음량 -22.2dB다. 원본과 생성 설정, 실제 메타데이터를 저장했다. 이 수치는 편곡이나 분위기의 적합성을 검증하지 않는다.

## Non-goals

타이틀·맵·엔딩·효과음 교체, 전투 로직 변경, 음악 생성 모델의 학습.

## Context / Constraints

- StageData.music과 StageManager의 재생 경로가 이미 있다.
- 기존 작업 파일이 다수 수정된 상태다. 음악과 무관한 파일은 수정하지 않는다.
- ACE Step: WSL `/home/yunseong/workspace/ace-step`, Compose 포트 7860. 이미지 `ghcr.io/ace-step/ace-step-1.5:0.1.8`, DiT turbo와 1.7B LM을 사용했다.
- 첫 제작안은 곡당 약 64~96초. 실제 생성 결과를 확인한 뒤 반복 구간을 편집한다. 프롬프트만으로 매끄러운 반복을 보장하지 않는다.
- 생성 설정, 모델 버전, seed, 원본 파일, 편집 내역을 함께 보존한다.
- 게임 적용 단계에서는 별도 이슈·브랜치와 Unity .meta 파일이 필요하다.

## Approach (Checklist)
- [x] **Step 0: Recon** — 10개 StageData와 음악 연결 경로 확인.
- [ ] **Step 1: Implementation** — 연결 확인, 지역별 80초 원본 10곡 생성, 요청 BPM 기준 16마디 루프 초안 10곡 편집 완료. 청취 검수와 StageData.music 연결은 남았다.
- [ ] **Step 2: Tests** — 원본·편집본의 길이, 포맷, 피크, 2초 이상 무음, 재생목록 경로 검사 완료. 음악적 반복 경계와 Unity 재생 검수는 미실시.
- [ ] **Step 3: Rollout / Rollback** — 기존 곡을 보존하고 새 곡 참조만 연결한다. 문제가 있으면 해당 music 참조를 복원한다.

### 공통 생성 방향

공통 프롬프트에 아래 지역별 프롬프트를 덧붙인다. 실제 파라미터 이름은 연결된 ACE Step 버전의 API 문서를 확인한 뒤 사용한다.

> Instrumental fantasy pixel RPG turn-based battle background music, cohesive retro JRPG sound palette, warm synthesized instruments and subtle chiptune accents, memorable restrained melody, steady rhythmic pulse, room for game sound effects, consistent energy, no vocals, no spoken words, no cinematic intro, no dramatic ending.

| ID | 지역 | 템포 초안 | 지역별 프롬프트 |
|---|---|---|---|
| 0 | 평원 | 112 BPM | Sunlit grasslands, cheerful first adventure, nimble flute melody, pizzicato strings, warm acoustic guitar, light snare and hand percussion, C major, playful and hopeful with gentle battle momentum. |
| 1 | 해안 | 108 BPM | Sparkling seaside adventure, flowing marimba and plucked strings, airy flute, rounded bass and syncopated hand percussion, G major, breezy and buoyant with a confident battle groove. |
| 2 | 고원 | 116 BPM | Windswept highland journey, broad open intervals, wooden flute and warm horn phrases, driving low strings and frame drums, D dorian, spacious and resolute. |
| 3 | 빗길 | 100 BPM | Rain-soaked road at dusk, repeating soft piano figures, muted mallets, gently pulsing synth bass, restrained percussion, A minor, wistful yet alert, persistent battle rhythm. |
| 4 | 마왕 성 | 138 BPM | Demon king castle battle, urgent baroque organ motifs, sharp staccato strings, punchy retro synth bass and timpani, D minor, imposing and energetic, clear melodic phrasing. |
| 5 | 서리 마을 | 96 BPM | Frost-covered village under magical threat, delicate celesta and glassy bells, soft pizzicato strings, muted low percussion and warm bass, E minor, wintry tenderness with quiet battle tension. |
| 6 | 벼락 협곡 | 144 BPM | Lightning canyon battle, crackling arpeggiated synths, brisk tom patterns and tight snare, angular string ostinato, B minor, restless electric momentum and bold short motifs. |
| 7 | 잿빛 유적 | 104 BPM | Ancient ash-covered ruins, sparse dulcimer fragments, low bowed strings, dusty mallet tones and measured frame drums, D phrygian, solemn mystery with a steady determined battle pulse. |
| 8 | 뒤틀린 숲 | 120 BPM | Enchanted twisted forest battle, winding woodwind motif, dissonant plucked strings, shimmering synthetic textures, uneven accents over a steady four-four groove, F-sharp minor, uncanny and stealthy. |
| 9 | 세계수의 심장 | 132 BPM | World tree heart final battle, luminous harp arpeggios, soaring strings and warm brass, powerful toms and sparkling retro synth countermelody, E minor opening into hopeful G major phrases, urgent and majestic. |

## Validation
- **Commands to run:** 연결된 ACE Step의 health/model 조회 및 생성 상태 확인. 음원 생성 후 ffprobe/ffmpeg로 파일과 피크 확인. 프로젝트 지침에 따라 Unity에서 음악 연결 확인.
- **Expected output:** 서로 다른 지역 음악 10곡, 재생 가능한 파일, 기록된 생성 설정, 반복 경계 청취 확인, 스테이지별 올바른 재생.

## Risks & Rollback
- **Risks:** 생성 환경 미확인, 모델별 무보컬 지시 준수 차이, 끝부분 무음·반복 경계 불연속, 곡간 음량 차이.
- **Rollback steps:** 원본 생성물은 보존하고 편집본을 별도로 만든다. 기존 음악 파일을 삭제하지 않고 StageData의 음악 참조를 복원한다.

## Open Questions
- 청취 후 지역별 음악 분위기와 음성 혼입 여부, 반복 경계 자연스러움 확인.

## Results

- 결과: `MusicDrafts/ace-step-stage-music/README.md`, 원본 WAV 10곡, `loops/` WAV 10곡, `stage-loops.m3u`.
- 원본은 모두 80초·48kHz 스테레오. 원본과 생성 메타데이터를 Windows 프로젝트 폴더에 복사해 보존했다.
- 루프 초안은 요청 BPM 기준으로 2마디 이후부터 17마디를 자르고 양 끝 1마디를 선형 크로스페이드해 16마디로 편집했다. 실제 비트 검출이나 음악적 청취 검수는 하지 않았다.
- 세부 검사 결과와 편집 위치는 `validation.json`에 기록했다.
- 게임 데이터와 기존 음악 참조는 수정하지 않았다. 이번 결과는 검토용 음악 제작물이며 적용 작업은 후속 단계다.

## References

- [ACE Step 1.5 공식 API 문서](https://ace-step.github.io/ACE-Step-1.5/en/API)
