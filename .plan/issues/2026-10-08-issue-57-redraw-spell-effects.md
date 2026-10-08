# 마법 이펙트 15종을 직접 그린 픽셀 스프라이트로 교체

- Date: 2026-10-08
- GitHub issue: #57

## Goal

Shoot·Drop·Explode × 불·얼음·바위·번개·신성 15개 prefab 이 외부 에셋 `PixelArtRPGVFX` 대신 저장소에서 직접 그린 스프라이트를 쓴다. Drop 은 회전한 Shoot 이 아니라 떨어지는 모양으로 따로 그린다.

## Approach

1. `docs/design/spell-effects/common.py` 에 프레임 크기, 원소 팔레트, 외곽선·명암 도구, 미리보기를 둔다.
2. 원소별 스크립트 5개가 Shoot·Drop·Hit·Explode 시트 4장씩, 총 20장을 그린다. 원소마다 subagent 1개가 맡고, 미리보기를 보고 고친다.
3. `build_unity_assets.py` 가 시트의 `.meta`, `.anim`, `.controller` 를 만들고 prefab 15개의 첫 스프라이트, controller, 회전을 바꾼다.
4. 참조가 0개가 된 `Assets/ThirdParty/ArtResource/Pixel Art` 를 지운다.

## Decisions

- 한 픽셀을 0.039 unit 으로 맞춘다. 슬라임 스프라이트의 픽셀 크기와 같다. Shoot·Drop·Hit 은 128px/PPU 128, Explode 는 256px/PPU 256 이라 prefab scale 과 collider 를 바꾸지 않는다.
- 12fps. Hit 과 Explode 는 6프레임(0.5초)으로 `SpellObj` 의 0.5초 파괴 시점과 맞춘다.
- Explode controller 에도 `Hit` parameter 를 둔다. `SpellObj` 가 충돌 시 `SetTrigger("Hit")` 를 부르므로, 없는 parameter 경고를 막는다.
- Shoot·Drop prefab 의 회전을 0 으로 바꾼다. 새 시트는 이동 방향대로 그렸다.

## Risks

- `.meta` 와 `.anim` 을 손으로 만든 YAML 로 쓴다. Unity 에서 가져오기 오류가 없는지 확인해야 한다.
- 원소별 담당이 달라 그림체가 어긋날 수 있다. 공용 팔레트와 외곽선 도구로 맞추고, 미리보기를 나란히 비교한다.

## Validation

- `python3 docs/design/spell-effects/generate_all.py` 실행 후 다시 실행해도 diff 가 없는지 확인한다.
- 남은 파일에서 지운 GUID 참조 0개를 확인한다.
- Unity 2022.3.34f1 에서 가져오기 오류가 없는지, 전투에서 15종 시전과 적중 애니메이션을 확인한다.

## Result

- `generate_all.py` 를 두 번 실행해 생성 파일 md5 가 같음을 확인했다.
- 시트 20장의 모든 프레임이 가장자리 2px 여백 안에 있다. 예외: 하늘에서 내려오는 번개·신성 Explode 는 프레임 위쪽 끝에 닿는다.
- 지운 47개 GUID 의 남은 참조 0개.
- Unity 2022.3.34f1 batchmode 로 프로젝트 사본을 가져와 prefab 15개를 검사했다. 스프라이트, PPU(128/256), Point 필터, RGBA32, clip 프레임 수(4/6)와 반복 설정, 회전 0 이 모두 맞았다. Unity 가 생성한 `.meta`·`.anim`·`.controller` 를 고쳐 쓰지 않았다.
- 실제 전투 플레이 확인은 하지 않았다.
