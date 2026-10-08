"""불 마법 시트 4장(FireShoot, FireDrop, FireHit, FireExplode)을 그린다."""
from __future__ import annotations

import math
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent))

import common  # noqa: E402
from common import DARK, HIGHLIGHT, LIGHT, MID, OUTLINE  # noqa: E402

PALETTE = common.PALETTES["Fire"]
# 불꽃 속 심지: 한 단계 밝은 색으로 같은 3단 명암을 입힌다.
CORE_PALETTE = [PALETTE[OUTLINE], PALETTE[MID], PALETTE[LIGHT], PALETTE[HIGHLIGHT], PALETTE[HIGHLIGHT]]
FLAT = PALETTE[MID]


# ---------- 공용 도구 ----------

def mass(size, draw_fn, palette=PALETTE):
    """한 색으로 덩어리를 그리고 3단 명암을 입힌 층을 돌려준다."""
    layer = common.new_frame(size)
    draw_fn(layer)
    return common.shade_bands(layer, palette)


def finish(outer, core=None):
    """바깥 불꽃 위에 심지를 얹고 전체에 외곽선을 두른다."""
    if core is not None:
        outer.alpha_composite(core)
    return common.add_outline(outer, PALETTE[OUTLINE])


def clip_below(layer, row):
    """row 아래 픽셀을 지운다. 지면 밑으로 번지는 불꽃을 외곽선 전에 잘라낸다."""
    for y in range(row, layer.height):
        for x in range(layer.width):
            layer.putpixel((x, y), (0, 0, 0, 0))
    return layer


def keep_margin(frame, margin=2):
    """프레임 가장자리 margin px 안쪽에만 그리도록 바깥을 지운다."""
    w, h = frame.size
    for y in range(h):
        for x in range(w):
            if x < margin or y < margin or x >= w - margin or y >= h - margin:
                frame.putpixel((x, y), (0, 0, 0, 0))
    return frame


def ember(frame, x, y, size=2, color=LIGHT):
    """외곽선 밖 불씨. 2x2 이상 네모로 찍는다."""
    for dy in range(size):
        for dx in range(size):
            common.put(frame, round(x) + dx, round(y) + dy, PALETTE[color])


def flame_chain(layer, points, color=FLAT):
    """(x, y, 반지름) 원을 이어 붙여 굵은 불꽃 줄기를 만든다."""
    for x, y, r in points:
        common.disc(layer, x, y, r, color)


def smoke(frame, x, y, r, color=DARK):
    """울퉁불퉁한 연기 덩어리: 원 세 개를 합쳐 한 번에 반투명으로 칠한다."""
    for dx, dy, k in ((0, 0, 1.0), (-r * 0.7, r * 0.3, 0.7), (r * 0.7, r * 0.2, 0.75)):
        common.disc(frame, x + dx, y + dy, r * k, PALETTE[color], 130)


# ---------- Shoot ----------

def shoot_frame(i):
    phase = 2 * math.pi * i / 4
    cx, cy = 82, 64  # 머리 중심. 꼬리는 왼쪽으로 뻗는다.

    # 꼬리 불꽃 혀 (세로 오프셋, 길이, 시작 반지름). 프레임마다 길이와 높이가 달라진다.
    tongues = ((0, 62, 14), (-10, 44, 9), (10, 40, 9))

    def tongue(layer, n, dy, length, r0):
        wob = 4 * math.sin(phase + n * 2.0)
        length = length * (1 + 0.18 * math.sin(phase * 1 + n * 1.7))
        steps = int(length // 6)
        for k in range(steps):
            u = k / steps
            y = cy + dy * (0.3 + u) + wob * u + 3 * math.sin(phase - k * 0.8 + n)
            common.disc(layer, cx - 12 - k * 6, y, r0 * (1 - 0.85 * u), FLAT)

    def outer(layer):
        common.disc(layer, cx, cy, 21, FLAT)
        for n, (dy, length, r0) in enumerate(tongues):
            tongue(layer, n, dy, length, r0)

    def core(layer):
        common.disc(layer, cx + 3, cy + 1, 12, FLAT)
        tongue(layer, 0, 0, 34, 7)

    frame = finish(mass(128, outer), mass(128, core, CORE_PALETTE))
    common.disc(frame, cx + 4, cy - 3, 4, PALETTE[HIGHLIGHT])
    # 뒤로 흩날리는 불씨. 4프레임 주기로 같은 자리에 돌아온다.
    rng = common.seeded("FireShoot")
    for n in range(6):
        x0, y0 = rng.randrange(0, 32), rng.randrange(42, 84)
        x = cx - 50 - ((x0 + 8 * i) % 32) * 1.2
        ember(frame, x, y0 + 3 * math.sin(phase + n), 2 + n % 2, LIGHT if n % 2 else MID)
    return frame


# ---------- Drop ----------

def drop_frame(i):
    phase = 2 * math.pi * i / 4
    cx, cy = 64, 82  # 머리 중심. 꼬리는 위로 뻗는다.

    def outer(layer):
        common.disc(layer, cx, cy, 19, FLAT)
        # 위로 갈라지는 불꼬리 세 줄: 가운데가 가장 길고 좌우로 흔들린다.
        for n, (dx, length, width) in enumerate(((0, 64, 9), (-14, 46, 7), (14, 42, 7))):
            pts = []
            steps = length // 6
            for k in range(steps):
                sway = 4 * math.sin(phase + n * 1.7 - k * 0.9)
                pts.append((cx + dx + sway * (k / steps + 0.2), cy - 8 - k * 6, width - k * width / (steps + 1) * 0.9))
            flame_chain(layer, pts)

    def core(layer):
        common.disc(layer, cx + 1, cy + 2, 10, FLAT)
        flame_chain(layer, [(cx + 3 * math.sin(phase - k), cy - 6 - k * 6, 7 - k * 0.9) for k in range(0, 6)])

    frame = finish(mass(128, outer), mass(128, core, CORE_PALETTE))
    common.disc(frame, cx - 5, cy + 3, 3, PALETTE[HIGHLIGHT])
    # 위로 흩어지는 불씨.
    rng = common.seeded("FireDrop")
    for n in range(6):
        x0, y0 = rng.randrange(30, 94), rng.randrange(0, 40)
        ember(frame, x0, 6 + (y0 + 10 * i) % 40, 2 + n % 2, LIGHT if n % 2 else MID)
    return frame


# ---------- Hit ----------

def hit_frame(i):
    t = i / 5
    cx = cy = 64
    frame = common.new_frame(128)
    petals = 8
    # 꽃잎 길이: 0 프레임은 번쩍임, 1~2 프레임에 최대, 이후 줄어든다.
    reach = (14, 38, 48, 42, 30, 16)[i]
    thick = (10, 13, 13, 10, 7, 4)[i]

    def outer(layer):
        common.disc(layer, cx, cy, max(reach * 0.45, 7), FLAT)
        if i >= 1:
            for n in range(petals):
                a = 2 * math.pi * n / petals + 0.2
                length = reach * (1.0 if n % 2 == 0 else 0.75)
                # 꽃잎: 바깥으로 갈수록 가늘어지는 원 세 개.
                for step, (frac, rad) in enumerate(((0.35, thick * 0.9), (0.65, thick * 0.65), (1.0, thick * 0.35))):
                    common.disc(layer, cx + math.cos(a) * length * frac, cy + math.sin(a) * length * frac, rad, FLAT)

    def core(layer):
        common.disc(layer, cx, cy, max(reach * 0.28, 4), FLAT)

    if i < 5:
        frame = finish(mass(128, outer), mass(128, core, CORE_PALETTE))
        common.disc(frame, cx - 2, cy - 2, max(2, 4 - i // 3), PALETTE[HIGHLIGHT])
    # 사방으로 흩어지는 불씨. 마지막 프레임에는 작은 불씨만 남는다.
    rng = common.seeded("FireHit")
    for n in range(12 if i < 5 else 3):
        a = rng.uniform(0, 2 * math.pi)
        speed = rng.uniform(26, 50)
        d = 14 + speed * common.ease_out(t) + (4 if i == 0 else 0)
        size = max(2, (4 if n % 3 == 0 else 3) - i // 3)
        ember(frame, cx + math.cos(a) * d - size / 2, cy + math.sin(a) * d - size / 2, size, (LIGHT, MID, HIGHLIGHT)[n % 3])
    return frame


# ---------- Explode ----------

GROUND = common.EXPLODE_GROUND_ROW


def scorch(frame, strength):
    """지면이 그을린 자국. strength 가 클수록 넓고 진하다."""
    rx = 30 + 38 * strength
    common.ellipse(frame, 128, GROUND, rx, 7, PALETTE[OUTLINE], 200)
    common.ellipse(frame, 127, GROUND - 1, rx - 6, 4, PALETTE[DARK], 200)


def pillar_points(height, width, phase, wobble=6):
    """지면에서 위로 올라가며 좁아지는 불기둥 (x, y, 반지름) 목록."""
    pts = []
    steps = max(int(height // 7), 1)
    for k in range(steps + 1):
        u = k / steps
        # 가장자리에 2~3 군데 불룩한 굴곡을 준다.
        r = width * (1 - 0.6 * u ** 1.4) + 5 * math.sin(k * 1.9 + phase) * (1 - u)
        x = 128 + wobble * math.sin(phase + k * 0.7) * u
        pts.append((x, GROUND - 4 - k * 7, r))
    return pts


def explode_frame(i):
    phase = 1.3 * i
    frame = common.new_frame(256)
    rng = common.seeded(f"FireExplode{i}")

    # (불기둥 높이, 폭, 심지 비율, 바닥 불꽃 폭) 프레임별 값.
    heights = (0, 50, 100, 128, 100, 44)
    widths = (0, 26, 40, 44, 32, 16)
    flare = (0, 50, 68, 78, 56, 28)
    scorch(frame, (0.35, 0.8, 1.0, 1.0, 0.9, 0.55)[i])

    if i == 0:
        # 지면이 그을리며 작은 불꽃이 올라오기 시작한다.
        def outer(layer):
            for x in (92, 110, 128, 146, 164):
                common.polygon(layer, [(x - 8, GROUND - 2), (x + rng.randrange(-3, 4), GROUND - 34 - rng.randrange(0, 14)), (x + 8, GROUND - 2)], FLAT)
            common.ellipse(layer, 128, GROUND - 6, 46, 9, FLAT)

        def core(layer):
            for x in (118, 138):
                common.polygon(layer, [(x - 4, GROUND - 2), (x, GROUND - 14), (x + 4, GROUND - 2)], FLAT)

        frame.alpha_composite(finish(clip_below(mass(256, outer), GROUND + 14), clip_below(mass(256, core, CORE_PALETTE), GROUND + 14)))
        for n in range(5):
            ember(frame, 96 + rng.randrange(0, 64), GROUND - 28 - rng.randrange(0, 18), 2 + n % 2, LIGHT)
        return frame

    def outer(layer):
        common.ellipse(layer, 128, GROUND - 8, flare[i], 14 + 2 * i, FLAT)
        flame_chain(layer, pillar_points(heights[i], widths[i], phase))
        if 1 <= i <= 4:
            # 기둥 옆으로 휘어 올라가는 불꽃 혀: 밑동은 굵고 끝은 바깥 위로 말려 뾰족하다.
            for side in (-1, 1):
                for n in range(3):
                    x0 = 128 + side * widths[i] * (0.75 - n * 0.1)
                    base = GROUND - 16 - n * 22
                    reach = (34 - n * 5 + 6 * math.sin(phase + side * 1.3 + n * 2)) * (0.6 if i == 4 else 1)
                    rise = 30 + n * 6 + 6 * math.sin(phase * 1.3 + n + side)
                    for k in range(9):
                        u = k / 8
                        common.disc(layer, x0 + side * reach * math.sin(u * math.pi / 2), base - rise * u * u, 9 * (1 - u) + 1.5, FLAT)
        # 기둥 꼭대기는 두세 갈래로 갈라진다.
        if i >= 1:
            top = GROUND - 4 - heights[i]
            for n, dx in enumerate((-12, 0, 12)):
                h = (10 if n == 1 else 5) + 3 * math.sin(phase * 1.5 + n * 2.2) + heights[i] * 0.1
                for k in range(5):
                    common.disc(layer, 128 + dx * (1 + k * 0.12), top + 6 - k * h / 4, 7 - k * 1.2, FLAT)

    def core(layer):
        flame_chain(layer, [(x, y, r * 0.55) for x, y, r in pillar_points(heights[i] * 0.7, widths[i], phase)])
        common.ellipse(layer, 128, GROUND - 6, flare[i] * 0.5, 5, FLAT)

    frame.alpha_composite(finish(clip_below(mass(256, outer), GROUND + 14), clip_below(mass(256, core, CORE_PALETTE), GROUND + 14)))
    # 밑동이 가장 뜨겁다: LIGHT 불덩이 안에 HIGHLIGHT 심지.
    heat = (0.5, 0.8, 1.0, 1.0, 0.8, 0.5)[i]
    common.ellipse(frame, 128, GROUND - 12, 20 * heat, 12 * heat, PALETTE[LIGHT])
    common.ellipse(frame, 127, GROUND - 12, 11 * heat, 7 * heat, PALETTE[HIGHLIGHT])

    # 연기: 뒤 프레임일수록 위로 올라가고 커진다.
    if i >= 3:
        for n in range(2 + (i - 2)):
            x = 128 + rng.randrange(-16, 17) + (i - 3) * 6
            y = GROUND - heights[i] - 10 - rng.randrange(0, 18) - (i - 3) * 12
            smoke(frame, x, y, 10 + (i - 3) * 2 + rng.randrange(0, 3), OUTLINE)
    # 위로 튀는 불씨. 마지막 프레임에는 불씨와 연기만 남는다.
    count = (0, 6, 10, 12, 10, 8)[i]
    for n in range(count):
        x = 128 + rng.randrange(-60, 61)
        y = GROUND - 30 - rng.randrange(0, 20 + heights[i] + 40)
        ember(frame, x, y, 2 + (n % 3 == 0), (LIGHT, MID, HIGHLIGHT)[n % 3])
    return frame


def main():
    sheets = {
        "Shoot": [shoot_frame(i) for i in range(4)],
        "Drop": [drop_frame(i) for i in range(4)],
        "Hit": [hit_frame(i) for i in range(6)],
        "Explode": [explode_frame(i) for i in range(6)],
    }
    for kind, frames in sheets.items():
        print(common.save_sheet([keep_margin(f) for f in frames], "Fire", kind))
    print(common.write_preview("Fire"))


if __name__ == "__main__":
    main()
