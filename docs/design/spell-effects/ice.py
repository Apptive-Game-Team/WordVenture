"""얼음 마법 시트 4장(IceShoot, IceDrop, IceHit, IceExplode)을 그린다.

실행: python3 docs/design/spell-effects/ice.py
"""
from __future__ import annotations

import math
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent))

import common  # noqa: E402
from common import DARK, HIGHLIGHT, LIGHT, MID, OUTLINE  # noqa: E402

ELEMENT = "Ice"
PAL = common.PALETTES[ELEMENT]
EXPLODE_SIZE = 256


# ---- 작은 도구 ----

def solid(mass, dark_when=None):
    """MID 한 색으로 칠한 덩어리에 3단 명암과 외곽선을 입힌다.

    dark_when(x, y) 가 참인 MID 픽셀은 DARK 로 바꿔 그늘진 면을 만든다.
    """
    shaded = common.shade_bands(mass, PAL)
    if dark_when:
        px = shaded.load()
        for y in range(shaded.height):
            for x in range(shaded.width):
                if px[x, y][:3] == PAL[MID] and dark_when(x, y):
                    px[x, y] = common.rgba(PAL[DARK])
    return common.add_outline(shaded, PAL[OUTLINE])


def layer(size, paint):
    img = common.new_frame(size)
    paint(img)
    return img


def square(img, x, y, size, color):
    """size×size 네모 점. 외곽선 밖 반짝임과 가루에 쓴다."""
    for dy in range(size):
        for dx in range(size):
            common.put(img, x + dx, y + dy, color)


def twinkle(img, x, y, big=False):
    """굵은 십자 반짝임. 가운데 2×2, 팔 2px 폭."""
    square(img, x, y, 2, PAL[HIGHLIGHT])
    arm = 4 if big else 2
    for i in range(1, arm + 1):
        for ox, oy in ((-i, 0), (i + 1, 0), (0, -i), (0, i + 1)):
            common.put(img, x + ox, y + oy, PAL[LIGHT] if i == arm else PAL[HIGHLIGHT])
            if ox:
                common.put(img, x + ox, y + 1, PAL[LIGHT] if i == arm else PAL[HIGHLIGHT])
            else:
                common.put(img, x + 1, y + oy, PAL[LIGHT] if i == arm else PAL[HIGHLIGHT])


def crystal_points(cx, base_y, width, height, lean=0):
    """땅에 박힌 얼음 결정 가시 한 개의 윤곽. 아래가 넓고 끝이 뾰족하다."""
    half = width / 2
    return [
        (cx - half, base_y),
        (cx - half * 0.9 + lean * 0.3, base_y - height * 0.45),
        (cx - half * 0.35 + lean * 0.7, base_y - height * 0.8),
        (cx + lean, base_y - height),
        (cx + half * 0.45 + lean * 0.7, base_y - height * 0.72),
        (cx + half * 0.95 + lean * 0.3, base_y - height * 0.4),
        (cx + half, base_y),
    ]


def rotated(points, cx, cy, angle):
    c, s = math.cos(angle), math.sin(angle)
    return [(cx + (x - cx) * c - (y - cy) * s, cy + (x - cx) * s + (y - cy) * c) for x, y in points]


# ---- Shoot: 오른쪽으로 날아가는 얼음 창 ----

def ice_shoot(frame):
    img = common.new_frame(128)
    wobble = frame % 2  # 꼬리 가시가 길어졌다 짧아진다
    tail = 16 + wobble * 8

    def body(m):
        # 창날 본체: 머리(98,64)가 오른쪽
        common.polygon(m, [(100, 64), (80, 50), (50, 51), (34, 64), (50, 77), (80, 78)], PAL[MID])
        # 앞으로 뻗은 뾰족한 끝
        common.polygon(m, [(104, 64), (86, 56), (86, 72)], PAL[MID])
        # 뒤로 갈라진 꼬리 가시 세 개
        common.polygon(m, [(44, 54), (34 - tail * 0.4, 44), (30, 60)], PAL[MID])
        common.polygon(m, [(44, 74), (34 - tail * 0.4, 84), (30, 68)], PAL[MID])
        common.polygon(m, [(38, 57), (22 - tail * 0.6, 64), (38, 71)], PAL[MID])

    spear = solid(layer(128, body), dark_when=lambda x, y: y >= 65 - max(0, (x - 70)) * 0.5)
    # 창날 가운데 능선과 반사광
    common.line(spear, [(48, 64), (96, 64)], PAL[LIGHT], 1)
    common.line(spear, [(60, 58), (84, 58)], PAL[HIGHLIGHT], 2)
    square(spear, 90, 61, 2, PAL[HIGHLIGHT])
    img.alpha_composite(spear)

    # 서리 가루: 4프레임에 정확히 한 바퀴 돌아 처음 프레임으로 이어진다
    for phase, y, big in ((0, 52, 3), (11, 74, 2), (22, 63, 3), (6, 44, 2), (17, 84, 3), (28, 58, 2)):
        x = 30 - (phase + frame * 8) % 32
        if x < 2:
            continue
        shade = PAL[HIGHLIGHT] if (phase // 11) % 2 == 0 else PAL[LIGHT]
        size = 3 if x > 14 and big == 3 else 2
        square(img, x, y, size, shade)
    # 창날 위를 도는 반짝임
    spots = [(66, 52), (88, 58), (52, 66), (76, 70)]
    sx, sy = spots[frame]
    twinkle(img, sx, sy, big=frame % 2 == 0)
    return img


# ---- Drop: 아래를 향해 떨어지는 고드름 ----

def ice_drop(frame):
    img = common.new_frame(128)
    crown = 6 + (frame % 2) * 6  # 윗부분 가시가 번갈아 솟아 반짝인다

    def body(m):
        # 고드름 본체: 머리(끝)가 아래 (64,100)
        common.polygon(m, [(64, 100), (46, 62), (44, 38), (84, 38), (82, 62)], PAL[MID])
        # 위쪽 왕관 가시 세 개
        common.polygon(m, [(44, 40), (50, 28 - crown * 0.3), (58, 40)], PAL[MID])
        common.polygon(m, [(56, 40), (64, 22 - crown), (72, 40)], PAL[MID])
        common.polygon(m, [(70, 40), (78, 28 - crown * 0.3), (84, 40)], PAL[MID])
        # 옆에 따라 떨어지는 작은 조각
        common.polygon(m, [(34, 84 - frame % 2 * 4), (28, 62), (40, 66)], PAL[MID])
        common.polygon(m, [(92, 76 + frame % 2 * 4), (98, 56), (86, 60)], PAL[MID])

    icicle = solid(layer(128, body), dark_when=lambda x, y: x >= 66 and y > 44 or y > 76)
    common.line(icicle, [(64, 46), (64, 90)], PAL[LIGHT], 1)
    common.line(icicle, [(53, 44), (53, 62)], PAL[HIGHLIGHT], 2)
    square(icicle, 57, 70, 2, PAL[HIGHLIGHT])
    img.alpha_composite(icicle)

    # 눈가루: 떨어지는 물체 기준으로 위쪽으로 흘러간다. 4프레임에 한 바퀴.
    for phase, x in ((0, 50), (7, 76), (14, 62), (3, 40), (10, 88), (18, 68), (21, 56)):
        y = 4 + (phase + frame * 6) % 24
        if y > 24:
            continue
        # 위로 갈수록 작아져 사라진다 -- 아래(물체 쪽)는 큼직하다
        square(img, x, y, 3 if y > 12 else 2, PAL[HIGHLIGHT] if phase % 2 == 0 else PAL[LIGHT])
    # 꼬리 줄기: 굵은 눈가루 선
    for x, length in ((60, 10), (70, 14), (64, 8)):
        top = 8 + ((x + frame * 6) % 12)
        for dy in range(length):
            common.put(img, x, top + dy, PAL[LIGHT])
            common.put(img, x + 1, top + dy, PAL[MID])
    spots = [(52, 50), (70, 66), (74, 82), (56, 74)]
    twinkle(img, *spots[frame])
    return img


# ---- Hit: 얼음 조각이 사방으로 깨져 흩어진다 ----

SHARD_COUNT = 7


def ice_hit(frame):
    img = common.new_frame(128)
    cx = cy = 64
    t = frame / 5
    if frame == 0:
        # 충돌 순간: 하얀 충격 별
        def flash(m):
            for k in range(8):
                a = k * math.pi / 4
                reach = 30 if k % 2 == 0 else 18
                common.polygon(m, [(cx + math.cos(a - 0.35) * 8, cy + math.sin(a - 0.35) * 8),
                                   (cx + math.cos(a) * reach, cy + math.sin(a) * reach),
                                   (cx + math.cos(a + 0.35) * 8, cy + math.sin(a + 0.35) * 8)], PAL[MID])
            common.disc(m, cx, cy, 12, PAL[MID])
        burst = solid(layer(128, flash))
        common.disc(burst, cx - 3, cy - 3, 6, PAL[HIGHLIGHT])
        img.alpha_composite(burst)
        return img

    # 조각 하나하나가 따로 외곽선을 가진 얼음 파편이다
    rng = common.seeded("Ice-Hit")
    angles = [k * 2 * math.pi / SHARD_COUNT + rng.uniform(-0.2, 0.2) for k in range(SHARD_COUNT)]
    sizes = [rng.randint(14, 19) for _ in range(SHARD_COUNT)]
    spin = [rng.uniform(-1.2, 1.2) for _ in range(SHARD_COUNT)]
    reach = common.ease_out(t) * 24 + 18
    scale = 1.0 - t * 0.6

    def shards(m):
        for k in range(SHARD_COUNT):
            a = angles[k]
            sx, sy = cx + math.cos(a) * reach, cy + math.sin(a) * reach + t * t * 6
            s = max(3.5, sizes[k] * scale)
            base = [(sx + s * 1.1, sy), (sx - s * 0.5, sy - s * 0.7), (sx - s * 0.2, sy + s * 0.8)]
            common.polygon(m, rotated(base, sx, sy, a + spin[k] * frame), PAL[MID])
        # 중앙에 남은 얼음 가루 덩이: 점점 작아진다
        if frame < 4:
            common.disc(m, cx, cy, 11 - frame * 3, PAL[MID])

    broken = solid(layer(128, shards))
    img.alpha_composite(broken)
    for k in range(SHARD_COUNT):  # 파편마다 반사광
        a = angles[k]
        sx, sy = cx + math.cos(a) * reach, cy + math.sin(a) * reach + t * t * 6
        if scale > 0.65:
            square(img, round(sx) - 2, round(sy) - 2, 2, PAL[HIGHLIGHT])
    # 바깥으로 튀는 가루
    for k in range(6):
        a = angles[k] + 0.4
        d = reach + 6 + rng.randint(0, 6) + frame
        square(img, round(cx + math.cos(a) * d), round(cy + math.sin(a) * d), 2, PAL[LIGHT])
    if frame >= 3:
        twinkle(img, 40 + frame * 3, 36 + frame * 2)
    return img


# ---- Explode: 얼음 결정 가시가 솟고 금이 간 뒤 부서진다 ----

GROUND = common.EXPLODE_GROUND_ROW
CX = 128
# (x 오프셋, 최대 높이, 폭, 기울기, 솟기 시작 프레임)
SPIKES = [
    (-72, 56, 34, -6, 1), (-44, 92, 40, -5, 0), (-18, 118, 44, -3, 0),
    (10, 142, 50, 0, 0), (38, 108, 42, 3, 0), (64, 84, 38, 5, 0), (88, 52, 32, 6, 1),
]


def frost_patch(img, frame):
    """지면에 퍼진 서리 얼룩. 마지막에 걷힌다."""
    fade = [0.55, 1.0, 1.0, 1.0, 0.85, 0.4][frame]
    rx = 96 * fade

    def paint(m):
        common.ellipse(m, CX, GROUND - 3, rx, 8, PAL[MID])
        common.ellipse(m, CX, GROUND - 1, rx * 0.6, 5, PAL[MID])

    patch = solid(layer(EXPLODE_SIZE, paint), dark_when=lambda x, y: y > GROUND + 1)
    img.alpha_composite(patch)


def spike_layer(offset, height, width, lean, cracked, shattered):
    """가시 한 개. 외곽선을 따로 둘러 겹쳐도 덩어리가 구분된다."""
    cx = CX + offset

    def paint(m):
        common.polygon(m, crystal_points(cx, GROUND, width, height, lean), PAL[MID])

    spike = solid(layer(EXPLODE_SIZE, paint), dark_when=lambda x, y: x > cx + lean * (GROUND - y) / max(height, 1) + 1)
    # 가운데 능선과 반사광
    top = GROUND - height
    common.line(spike, [(cx + lean * 0.5, GROUND - 6), (cx + lean * 0.9, top + 14)], PAL[LIGHT], 1)
    common.line(spike, [(cx - width * 0.28, GROUND - height * 0.2), (cx - width * 0.22 + lean * 0.4, GROUND - height * 0.5)], PAL[HIGHLIGHT], 2)
    if cracked:
        # 금: 불투명한 영역 안에만 외곽선 색으로 긋는다
        rng = common.seeded(f"Ice-Explode-crack-{offset}")
        mask = spike.split()[3]
        crack = common.new_frame(EXPLODE_SIZE)
        x, y = cx + rng.randint(-4, 4), GROUND - height * 0.75
        for _ in range(5):
            nx, ny = x + rng.choice((-6, -4, 4, 6)), y + rng.randint(10, 18)
            common.line(crack, [(x, y), (nx, ny)], PAL[OUTLINE], 2)
            x, y = nx, ny
        px, src = crack.load(), mask.load()
        for yy in range(EXPLODE_SIZE):
            for xx in range(EXPLODE_SIZE):
                if px[xx, yy][3] and not src[xx, yy]:
                    px[xx, yy] = (0, 0, 0, 0)
        spike.alpha_composite(crack)
    return spike


def ice_explode(frame):
    img = common.new_frame(EXPLODE_SIZE)
    frost_patch(img, frame)
    # 솟는 정도: 0, 0.45, 0.85, 1.0 (정지), 1.0(깨짐), 사라짐
    grow = [0.18, 0.5, 0.88, 1.0, 1.0, 0.0][frame]
    if frame == 5:
        return finish_explode(img, frame)
    # 뒤쪽(키 큰 것)부터 그려 앞쪽 작은 가시가 가리게 한다
    order = sorted(range(len(SPIKES)), key=lambda i: -SPIKES[i][1])
    for i in order:
        offset, full, width, lean, start = SPIKES[i]
        height = full * grow if frame >= start else 0
        if frame == 0:
            height = full * 0.3 if start == 0 else 0
        if height < 8:
            continue
        w = width * (0.7 + 0.3 * min(grow * 1.4, 1))
        if frame == 4:
            # 부서지는 중: 아랫동강만 남기고 윗동강은 따로 날린다
            keep = height * 0.38
            img.alpha_composite(spike_layer(offset, keep, w, lean * 0.4, False, True))
            continue
        img.alpha_composite(spike_layer(offset, height, w, lean, frame == 3, False))
    if frame == 4:
        flying_shards(img, frame)
    if frame in (2, 3):
        # 솟아오르며 튀는 서리 가루
        rng = common.seeded(f"Ice-Explode-{frame}")
        for _ in range(10):
            square(img, CX + rng.randint(-100, 96), GROUND - rng.randint(30, 150), 3, PAL[HIGHLIGHT if rng.random() < 0.5 else LIGHT])
    return finish_explode(img, frame)


def flying_shards(img, frame):
    """부서진 윗동강: 위·바깥으로 날아가는 조각들."""
    rng = common.seeded("Ice-Explode-shards")
    pieces = common.new_frame(EXPLODE_SIZE)
    for offset, full, width, lean, _ in SPIKES:
        for part in range(3):
            a = math.radians(rng.uniform(210, 330))
            if offset < 0:
                a = math.radians(rng.uniform(200, 290))
            elif offset > 0:
                a = math.radians(rng.uniform(250, 340))
            d = full * 0.2 + part * 14 + rng.uniform(4, 18)
            sx = CX + offset + math.cos(a) * d * 0.95
            sy = GROUND - full * (0.55 + part * 0.14) + math.sin(a) * d * 0.5
            s = width * (0.34 - part * 0.06)
            tri = [(sx + s, sy), (sx - s * 0.6, sy - s * 0.8), (sx - s * 0.4, sy + s * 0.9)]
            common.polygon(pieces, rotated(tri, sx, sy, a * 2 + part), PAL[MID])
    pieces = solid(pieces)
    img.alpha_composite(pieces)


def finish_explode(img, frame):
    """마지막 프레임에서 걷히는 얼음 가루와 반짝임."""
    rng = common.seeded(f"Ice-Explode-dust-{frame}")
    if frame == 4:
        for _ in range(14):
            square(img, CX + rng.randint(-100, 98), GROUND - rng.randint(20, 150), 3, PAL[LIGHT if rng.random() < 0.5 else HIGHLIGHT])
    if frame == 5:
        for k in range(7):
            twinkle(img, CX + rng.randint(-90, 86), GROUND - rng.randint(14, 70) - k * 6)
        for _ in range(8):
            square(img, CX + rng.randint(-100, 96), GROUND - rng.randint(6, 40), 2, PAL[LIGHT])
    return img


def main():
    sheets = {
        "Shoot": [ice_shoot(f) for f in range(4)],
        "Drop": [ice_drop(f) for f in range(4)],
        "Hit": [ice_hit(f) for f in range(6)],
        "Explode": [ice_explode(f) for f in range(6)],
    }
    for kind, frames in sheets.items():
        print(common.save_sheet(frames, ELEMENT, kind))
    print(common.write_preview(ELEMENT))


if __name__ == "__main__":
    main()
