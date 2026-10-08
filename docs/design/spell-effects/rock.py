"""바위 마법 시트 4장(RockShoot, RockDrop, RockHit, RockExplode)을 그린다."""
from __future__ import annotations

import math
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent))
import common  # noqa: E402
from PIL import Image  # noqa: E402
from common import DARK, HIGHLIGHT, LIGHT, MID, OUTLINE  # noqa: E402

PAL = common.PALETTES["Rock"]
ELEMENT = "Rock"


# ---------------------------------------------------------------- helpers
def shade(layer, dark=3, light=2):
    """왼쪽 위 빛 기준 3단 명암. 큰 덩어리용으로 common.shade_bands 보다 띠가 두껍다."""
    src = layer.load()
    out = layer.copy()
    dst = out.load()
    w, h = layer.size

    def solid(x, y):
        return 0 <= x < w and 0 <= y < h and src[x, y][3] > 0

    for y in range(h):
        for x in range(w):
            if not solid(x, y):
                continue
            color = PAL[MID]
            if any(not solid(x + k, y + k) for k in range(1, dark + 1)):
                color = PAL[DARK]
            elif any(not solid(x - k, y - k) for k in range(1, light + 1)):
                color = PAL[LIGHT]
            dst[x, y] = common.rgba(color, src[x, y][3])
    return out


def mass(frame, paint, detail=None, dark=3, light=2):
    """한 덩어리를 따로 칠해 명암과 외곽선을 입힌 뒤 프레임에 올린다."""
    layer = common.new_frame(frame.width)
    paint(layer)
    layer = shade(layer, dark, light)
    if detail:
        body = layer.copy()
        detail(layer)
        clear = common.new_frame(frame.width)
        layer = Image.composite(layer, clear, body.getchannel("A").point(lambda a: 255 if a else 0))
    frame.alpha_composite(common.add_outline(layer, PAL[OUTLINE]))


def rotate(points, cx, cy, angle):
    c, s = math.cos(angle), math.sin(angle)
    return [(cx + (x - cx) * c - (y - cy) * s, cy + (x - cx) * s + (y - cy) * c) for x, y in points]


def blob(cx, cy, radii, angle=0.0):
    """각도별 반지름 목록으로 만든 울퉁불퉁한 덩어리 꼭짓점."""
    n = len(radii)
    return [(cx + math.cos(angle + i * math.tau / n) * r, cy + math.sin(angle + i * math.tau / n) * r)
            for i, r in enumerate(radii)]


def dust_group(frame, puffs, alpha=190):
    """먼지 구름 여럿을 한 덩어리로 합쳐 바깥에만 MID 외곽선을 두른다. puffs 는 (x, y, r) 목록."""
    layer = common.new_frame(frame.width)
    lobes = ((0, 0, 1.0), (-.85, .3, .72), (.8, .3, .75), (.1, -.55, .6))
    for x, y, r in puffs:
        for dx, dy, k in lobes:
            common.disc(layer, x + dx * r, y + dy * r, r * k, PAL[LIGHT], alpha)
    for x, y, r in puffs:
        common.disc(layer, x - r * .4, y - r * .55, max(r * .35, 1.5), PAL[HIGHLIGHT], alpha)
    frame.alpha_composite(common.add_outline(layer, PAL[MID]))


def dust(frame, x, y, r, alpha=190):
    """부드러운 먼지 구름 한 개. LIGHT 면에 HIGHLIGHT 로브, MID 외곽선, 명암·금 없음."""
    dust_group(frame, [(x, y, r)], alpha)


def pebble(frame, x, y, size, angle=0.0):
    """외곽선 있는 작은 자갈."""
    pts = rotate([(x - size, y - size * .6), (x + size * .2, y - size), (x + size, y - size * .1),
                  (x + size * .5, y + size), (x - size * .8, y + size * .7)], x, y, angle)
    mass(frame, lambda l: common.polygon(l, pts, PAL[MID]), dark=1, light=1)


# ---------------------------------------------------------------- Shoot
# 180도 대칭 바위. 프레임마다 45도씩 돌아 4프레임에 한 바퀴 반(=대칭 한 주기)을 돈다.
BOULDER = [26, 23, 25, 27, 24, 22, 26, 23, 25, 27, 24, 22, 26, 23, 25, 27]
BOULDER = BOULDER[:8] + BOULDER[:8]
BOULDER = [26, 23, 25, 27, 24, 22, 25, 24] * 2


def shoot_frame(i):
    f = common.new_frame(128)
    cx, cy = 80, 64
    angle = i * math.radians(45) / 1
    angle = i * math.pi / 4
    # 먼지 꼬리: 프레임마다 왼쪽으로 흐르며 크기가 변한다
    for k in range(3):
        x = cx - 28 - k * 13 - (i * 5) % 10
        y = cy + 12 - k * 2 + (4 if (k + i) % 2 else -3)
        dust(f, x, y, 9 - k * 1.5 + (i % 2))
    # 따라오는 자갈
    for k in range(2):
        pebble(f, cx - 34 - k * 18 - (i * 7) % 12, cy - 14 + k * 24 - (i % 2) * 3, 4, i + k)
    pts = blob(cx, cy, BOULDER, angle)

    def paint(l):
        common.polygon(l, pts, PAL[MID])

    def detail(l):
        # 같이 도는 금과 얼룩(180도 대칭)
        for sgn in (1, -1):
            p1 = rotate([(cx - 14 * sgn, cy - 6 * sgn)], cx, cy, angle)[0]
            p2 = rotate([(cx - 2 * sgn, cy + 4 * sgn)], cx, cy, angle)[0]
            p3 = rotate([(cx + 4 * sgn, cy + 12 * sgn)], cx, cy, angle)[0]
            common.line(l, [p1, p2, p3], PAL[DARK], 2)
        for sgn in (1, -1):
            sx, sy = rotate([(cx + 9 * sgn, cy - 12 * sgn)], cx, cy, angle)[0]
            common.disc(l, sx, sy, 3, PAL[DARK])
        common.disc(l, cx - 12, cy - 13, 3, PAL[HIGHLIGHT])  # 빛은 항상 왼쪽 위
        common.disc(l, cx - 7, cy - 17, 1.5, PAL[HIGHLIGHT])

    mass(f, paint, detail, dark=4, light=3)
    return f


def make_shoot():
    return [shoot_frame(i) for i in range(4)]


# ---------------------------------------------------------------- Drop
def drop_frame(i):
    f = common.new_frame(128)
    cx, cy = 64, 74
    # 위로 가늘어지는 먼지 줄기: 바위 바로 위가 가장 굵고, 프레임마다 좌우로 흔들린다
    puffs = []
    for k in range(6):
        t = k / 5
        drift = math.sin(i * 1.6 + k * 1.1) * (2 + 4 * t)
        puffs.append((cx + drift, 44 - t * 34, 13 - t * 9))
    dust_group(f, puffs)
    # 옆에서 같이 떨어지는 작은 자갈
    for k, px in enumerate((cx - 36, cx + 38, cx + 30)):
        pebble(f, px, 40 + ((i * 10 + k * 23) % 44), 4, i + k)
    # 각진 큰 바위: 윗면이 넓고 아래가 뭉툭하게 좁아진다
    wob = (0, 1, 0, -1)[i]
    pts = [(cx - 28, cy - 14), (cx - 14, cy - 26 + wob), (cx + 10, cy - 24), (cx + 27, cy - 12),
           (cx + 29, cy + 8), (cx + 14, cy + 28), (cx - 6, cy + 31 - wob), (cx - 24, cy + 16),
           (cx - 30, cy)]

    def detail(l):
        # 면 구분 선과 어두운 갈라진 면
        common.polygon(l, [(cx + 10, cy - 24), (cx + 27, cy - 12), (cx + 29, cy + 8), (cx + 14, cy + 28),
                           (cx + 6, cy + 4)], PAL[DARK])
        common.line(l, [(cx - 28, cy - 14), (cx - 8, cy - 6), (cx + 6, cy + 4), (cx - 6, cy + 31)], PAL[DARK], 2)
        common.line(l, [(cx - 8, cy - 6), (cx + 10, cy - 24)], PAL[DARK], 2)
        common.polygon(l, [(cx - 22, cy - 14), (cx - 12, cy - 22), (cx - 6, cy - 12), (cx - 18, cy - 8)], PAL[LIGHT])
        common.disc(l, cx - 15, cy - 16, 2.5, PAL[HIGHLIGHT])

    mass(f, lambda l: common.polygon(l, pts, PAL[MID]), detail, dark=3, light=2)
    return f


def make_drop():
    return [drop_frame(i) for i in range(4)]


# ---------------------------------------------------------------- Hit
def hit_frame(i):
    f = common.new_frame(128)
    c = 64
    rng = common.seeded("Rock-Hit-chunks")
    # 조각 7개의 방향·크기·속도는 모든 프레임에서 같고, 뒤쪽 조각부터 사라진다
    chunks = [(math.radians(-170 + k * 50 + rng.uniform(-10, 10)), rng.uniform(7, 11), rng.uniform(.8, 1.2))
              for k in range(7)]
    t = i / 5
    if i == 0:  # 충돌 순간: 바위 + 별 모양 섬광
        star = []
        for k in range(16):
            r = 36 if k % 2 == 0 else 16
            a = k * math.tau / 16 - math.pi / 2
            star.append((c + math.cos(a) * r, c + math.sin(a) * r))
        mass(f, lambda l: common.polygon(l, star, PAL[MID]),
             lambda l: common.disc(l, c - 3, c - 3, 8, PAL[HIGHLIGHT]), dark=2, light=2)
        mass(f, lambda l: common.polygon(l, blob(c, c, [22, 18, 24, 19, 23, 18, 24, 19], 0.2), PAL[MID]),
             lambda l: common.line(l, [(c - 6, c - 8), (c, c), (c + 4, c + 8)], PAL[DARK], 2))
        return f
    # 먼지 구름: 터져 나오며 커졌다가 작아지고 흩어진다
    grow = common.ease_out(min(i / 3, 1))
    shrink = 1 - max(i - 3, 0) * 0.22
    for k in range(7 - max(i - 3, 0) * 2):
        a = k * math.tau / 7 + 0.4
        d = (6 + 24 * grow) * (0.5 + 0.5 * (k % 2))
        dust(f, c + math.cos(a) * d * 1.2, c + math.sin(a) * d * 0.8 + 4 * (i - 1),
             (9 + 6 * (k % 3) / 2) * shrink * (0.6 + 0.4 * grow))
    dust(f, c, c + 2 * i, 14 * shrink)
    # 날아가는 어두운 각진 조각: 호를 그리며 떨어진다
    for a, size, speed in chunks[:max(7 - (i - 1), 3)]:
        dist = (6 + 46 * common.ease_out(t)) * speed
        x = min(max(c + math.cos(a) * dist, 12), 116)
        y = c + math.sin(a) * dist * 0.8 - 14 * math.sin(math.pi * min(t * 1.4, 1)) + 85 * t * t
        s_ = size * (1.0 - 0.35 * t)
        pts = [(x - s_, y - s_ * .7), (x + s_ * .3, y - s_), (x + s_, y - s_ * .1), (x + s_ * .4, y + s_), (x - s_ * .9, y + s_ * .6)]
        pts = rotate(pts, x, y, a + i * 0.9)
        mass(f, lambda l, p=pts: common.polygon(l, p, PAL[MID]), dark=3, light=1)
    return f


def make_hit():
    return [hit_frame(i) for i in range(6)]


# ---------------------------------------------------------------- Explode
G = common.EXPLODE_GROUND_ROW
# (x 오프셋, 밑너비, 최대 높이, 기울기)
SPIKES = [(-78, 30, 95, -6), (78, 30, 90, 7), (-42, 38, 140, -8), (44, 38, 135, 9), (0, 48, 190, 3)]
# 프레임별 높이 비율 (마지막은 무너진 잔해)
RISE = [0.0, 0.3, 0.7, 1.0, 0.85, 0.0]


def spike(f, x, base, h, lean, crumble=0.0):
    """땅에서 솟은 돌기둥 가시 한 개. crumble 이 크면 윗부분이 잘려 나간다."""
    if h < 6:
        return
    top_x = x + lean * h / 30
    pts = [(x - base / 2, G + 6), (x - base * .42, G - h * .55), (top_x - base * .12, G - h),
           (top_x + base * .1, G - h * .93), (x + base * .44, G - h * .5), (x + base / 2, G + 6)]
    if crumble:  # 부러진 윗면
        cut = G - h * (1 - crumble)
        pts = [(px, max(py, cut) if py < cut else py) for px, py in pts]

    def detail(l):
        common.polygon(l, [(x + base * .1, G + 6), (top_x + base * .1, G - h * .93), (x + base * .44, G - h * .5),
                           (x + base / 2, G + 6)], PAL[DARK])
        for k in (0.3, 0.6):  # 가로 금
            y = G - h * k
            common.line(l, [(x - base * .35, y), (x + base * .05, y + 4)], PAL[DARK], 2)
        common.line(l, [(x - base * .3, G - h * .25), (top_x - base * .22, G - h * .8)], PAL[LIGHT], 2)
        common.disc(l, top_x - base * .12, G - h * .78, 2.5, PAL[HIGHLIGHT])

    mass(f, lambda l: common.polygon(l, pts, PAL[MID]), detail, dark=4, light=3)


def explode_frame(i):
    f = common.new_frame(256)
    cx = 128
    rng = common.seeded(f"Rock-Explode-{i}")
    rise = RISE[i]
    # 땅 갈라짐 (모든 프레임)
    if i == 0:
        for dx, ln in ((-58, 14), (-30, 22), (0, 30), (30, 22), (58, 14)):  # 지면 갈라짐
            pts = [(cx + dx, G + 2), (cx + dx + ln * .3, G - ln * .4), (cx + dx - ln * .1, G - ln * .8), (cx + dx + ln * .25, G - ln * 1.2)]
            common.line(f, pts, PAL[OUTLINE], 3)
            common.line(f, [(cx + dx - 2, G + 6), (cx + dx + ln * .6, G + 10)], PAL[OUTLINE], 2)
        for dx, r in ((-36, 9), (0, 13), (36, 9), (-14, 8), (16, 8)):
            dust(f, cx + dx, G - 6, r)
    for n, (dx, base, hmax, lean) in enumerate(sorted(SPIKES, key=lambda s: abs(s[0]) * -1)):
        local = max(min(rise * 1.15 - n * 0.0, 1.0), 0)
        if i == 5:
            continue
        spike(f, cx + dx, base, hmax * local, lean, crumble=0.22 if i == 4 and n % 2 else 0)
    # 앞쪽 흙 턱
    if 0 < i < 5:
        for dx in (-70, -30, 28, 66):
            mass(f, lambda l, dx=dx: common.ellipse(l, cx + dx, G + 4, 22, 9, PAL[MID]), dark=3, light=2)
    # 튀는 파편
    if 1 <= i <= 4:
        for k in range(6 + i):
            a = rng.uniform(-2.7, -0.45)
            speed = rng.uniform(40, 95) * (i / 3)
            x = min(max(cx + math.cos(a) * speed * 1.4, 14), 242)
            y = G - 20 + math.sin(a) * speed + 18 * i * i * rng.uniform(.6, 1.2)
            if y < G - 4:
                pebble(f, x, y, rng.choice((4, 5, 6)), k + i)
    # 먼지 구름
    if i >= 1:
        n = 5 + i
        for k in range(n):
            x = cx + (k / (n - 1) - 0.5) * min(130 + i * 28, 190) + rng.uniform(-6, 6)
            r = rng.uniform(8, 13) * (1 + (i - 1) * 0.12)
            dust(f, x, G - rng.uniform(0, 14 + i * 5), r)
    if i == 5:  # 무너진 잔해 더미
        for dx, w, hgt in ((-70, 26, 14), (-34, 30, 20), (6, 40, 26), (46, 30, 18), (78, 24, 12)):
            pts = [(cx + dx - w, G + 6), (cx + dx - w * .7, G - hgt), (cx + dx, G - hgt - 5),
                   (cx + dx + w * .8, G - hgt * .7), (cx + dx + w, G + 6)]
            mass(f, lambda l, p=pts: common.polygon(l, p, PAL[MID]),
                 lambda l, dx=dx, hgt=hgt: common.disc(l, cx + dx - 6, G - hgt + 2, 2, PAL[HIGHLIGHT]), dark=3, light=2)
        for k in range(4):
            pebble(f, cx + rng.uniform(-90, 90), G + rng.uniform(-2, 8), 5, k)
    return f


def make_explode():
    return [explode_frame(i) for i in range(6)]


if __name__ == "__main__":
    for kind, make in (("Shoot", make_shoot), ("Drop", make_drop), ("Hit", make_hit), ("Explode", make_explode)):
        print(common.save_sheet(make(), ELEMENT, kind))
    print(common.write_preview(ELEMENT))
