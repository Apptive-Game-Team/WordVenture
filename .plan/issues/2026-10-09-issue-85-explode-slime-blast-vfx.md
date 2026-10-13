# Issue 85: 폭발 슬라임이 터질 때 폭발 효과 추가

- Issue: https://github.com/Apptive-Game-Team/WordVenture/issues/85
- Branch: `fix/explode-slime-blast-vfx` (`develop` 기준)

## 원인

`ExplodeEnemy`는 터질 때 `Animator.RangeAttack()`과 `Kill()`을 같은 프레임에 부른다.
`Enemy.Death()`는 쓰러지는 그림을 보여 주고 0.25초 뒤 오브젝트를 끄므로 폭발 그림이 나올 틈이 없다.
예고한 슬라임을 먼저 쓰러뜨려 `DamageNearbyEnemies()`가 돌 때도 같다.

## 변경

- `BlastVfx`를 더한다. 슬라임과 따로 선 오브젝트에 `SpriteRenderer`를 붙여 그림 6장을 12 fps(0.5초)로
  한 번 재생하고 스스로 지운다. 슬라임의 자식이 아니라서 슬라임이 꺼져도 끝까지 재생된다.
- 그림은 기존 `Assets/Art/Combat/Spells/Fire/FireExplode.png`의 `FireExplode_0..5`를 쓴다.
  `SpellObj`가 붙은 Explode 주문 prefab은 피해를 주므로 쓰지 않는다.
- `ExplodeEnemy`에 `blastFrames` 필드를 더하고 `ExplodeSlime.prefab`에 6장을 연결한다.
- 스스로 터질 때와 예고한 슬라임이 쓰러져 주변 적이 피해를 받을 때 재생한다.
  예고하지 않은 슬라임이 쓰러지면 재생하지 않는다.
- 크기: 가로 5 unit(`BlastRadius` 2.5의 지름). 정렬 층은 슬라임과 같고 정렬 순서는 슬라임보다 10 높다.
- 피해량과 범위는 바꾸지 않는다.

## 검증

- PlayMode: 스스로 터지면 효과가 1개 생기고 부모가 없으며 슬라임이 꺼진 뒤에도 남았다가 0.5초 뒤 사라진다.
- PlayMode: 예고한 슬라임을 쓰러뜨리면 효과가 생기고, 예고하지 않은 슬라임을 쓰러뜨리면 생기지 않는다.
- Unity를 실행하지 못해 테스트는 돌리지 않았다.
