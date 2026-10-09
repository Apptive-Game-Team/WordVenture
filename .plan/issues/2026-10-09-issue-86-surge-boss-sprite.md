# Issue 86: 2부 최종 보스 슬라임 그림을 이전 소용돌이 그림으로 복원

- Issue: https://github.com/Apptive-Game-Team/WordVenture/issues/86
- Branch: `fix/surge-boss-sprite`

## 배경

`cddf472`가 2부 슬라임 7종을 `build_slimes.py`로 다시 그리면서 최종 보스 `SurgeSlime_01.png` ~ `_08.png`도
보라 슬라임 몸에 결정을 얹은 그림으로 바뀌었다. 최종 보스는 초록·보라·노랑 소용돌이와 결정이 있는 이전 그림을 유지한다.

## 변경

- `SurgeSlime_01.png` ~ `_08.png`를 `cddf472^` 내용으로 되돌린다. 이전과 지금 모두 512x512이고 `.meta`(GUID, pixelsPerUnit 100, pivot, filter)는 차이가 없어 그대로 둔다.
- `SurgeBoss.prefab`과 `SurgeBossEnemy`는 바꾸지 않는다. 그림 크기가 같으므로 화면 크기도 `cddf472` 이전과 같다.
- `build_slimes.py`의 대상에서 Surge를 빼서 다시 실행해도 Surge 프레임을 덮어쓰지 않는다. `README.md`의 설명을 맞춘다. `manifest.json`의 Surge 항목(경로, GUID)은 그대로다.

## 검증

- EditMode `ActTwoCompleteDataTests`는 manifest의 Surge 프레임 8장과 prefab 프레임 GUID를 비교하므로 수정 없이 통과해야 한다.
- 다른 여섯 슬라임(Charge, Explode, Heal, Mortar, Shield, Split)의 그림은 바꾸지 않는다.
