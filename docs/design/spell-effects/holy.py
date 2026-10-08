"""신성(Holy) 마법 시트 4장을 그린다: HolyShoot, HolyDrop, HolyHit, HolyExplode.

번개와 구분하기 위해 둥근 고리, 4갈래 십자 별, 빛줄기, 기둥 같은 대칭 형태를 쓴다.
"""
from __future__ import annotations

import math
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent))

import common  # noqa: E402
from common import DARK, HIGHLIGHT, LIGHT, MID, OUTLINE, PALETTES  # noqa: E402

PAL = PALETTES["Holy"]


# ---------- 덩어리 도구 ----------

def mass(size, paint):
    """paint(layer) 로 한 색 실루엣을 그리면 3단 명암과 외곽선을 입힌 덩어리 레이어를 돌려준다."""
    layer = common.new_frame(size)
    paint(layer)
    return common.add_outline(common.shade_bands(layer, PAL), PAL[OUTLINE])


def stack(size, *layers):
    frame = common.new_frame(size)
    for layer in layers:
        frame.alpha_composite(layer)
    return frame


def clear(image, x, y):
    if 0 <= x < image.width and 0 <= y < image.height:
        image.putpixel((x, y), (0, 0, 0, 0))


def ring_paint(cx, cy, rx, ry, thick, color=None):
    """속이 빈 타원 고리."""
    def paint(layer):
        common.ellipse(layer, cx, cy, rx, ry, PAL[MID] if color is None else color)
        inner_x, inner_y = rx - thick, ry - min(max(3, thick * 0.8), ry - 1)
        if inner_x > 0.5 and inner_y > 0.5:
            for y in range(int(cy - ry) - 1, int(cy + ry) + 2):
                for x in range(int(cx - rx) - 1, int(cx + rx) + 2):
                    if ((x + 0.5 - cx) / inner_x) ** 2 + ((y + 0.5 - cy) / inner_y) ** 2 <= 1:
                        clear(layer, x, y)
    return paint


def star_paint(cx, cy, arm_x, arm_y, waist, down=None):
    """4갈래 십자 별. waist 는 팔이 갈라지는 허리 폭, down 은 아래 팔 길이(생략하면 arm_y)."""
    bottom = arm_y if down is None else down

    def paint(layer):
        t = waist
        common.polygon(layer, [
            (cx, cy - arm_y), (cx + t, cy - t), (cx + arm_x, cy), (cx + t, cy + t),
            (cx, cy + bottom), (cx - t, cy + t), (cx - arm_x, cy), (cx - t, cy - t),
        ], PAL[MID])
    return paint


def glint_paint(cx, cy, arm):
    """작은 반짝임. 팔 굵기 3px 이상이라 외곽선을 둘러도 보인다."""
    return star_paint(cx, cy, arm, arm, max(2, arm // 3))


def add_core(frame, cx, cy, r):
    """가장 밝은 중심."""
    common.disc(frame, cx, cy, r, PAL[LIGHT])
    common.disc(frame, cx - r * 0.25, cy - r * 0.25, max(r * 0.55, 1), PAL[HIGHLIGHT])


# ---------- Shoot ----------

def shoot_frame(f):
    size, cx, cy = 128, 82, 64
    layers = []
    # 꼬리 고리: 프레임마다 6px 씩 왼쪽으로 밀려나며 작아진다. 4프레임에 24px 간격이 한 바퀴.
    for k in range(4, -1, -1):
        dist = 22 + 24 * (k - 1) + 6 * f if k else 6 * f
        if k == 0:
            dist = 14 + 6 * f
        else:
            dist = 14 + 6 * f + 24 * k
        shrink = 1 - dist / 110
        ry = 5 + 14 * shrink
        rx = 3 + 5 * shrink
        x = cx - dist
        if x - rx - 1 < 3 or ry < 6:
            continue
        layers.append(mass(size, ring_paint(x, cy, rx + 1, ry, 4)))
    # 십자 빛: 길이가 프레임마다 맥동한다.
    arm = (26, 30, 26, 22)[f]
    layers.append(mass(size, star_paint(cx, cy, arm, arm, 5)))
    # 빛 구슬
    layers.append(mass(size, lambda l: common.disc(l, cx, cy, 15, PAL[MID])))
    frame = stack(size, *layers)
    add_core(frame, cx - 2, cy - 2, 9)
    return frame


def make_shoot():
    return [shoot_frame(f) for f in range(4)]


# ---------- Drop ----------

def drop_frame(f):
    size, cx, cy = 128, 64, 72
    pulse = (0, 3, 0, -3)[f]
    layers = []
    # 별 위의 부드러운 혜성 빛줄기: 반투명 LIGHT, MID 외곽선, 가운데 흰 줄.
    top = 8 + (0, 3, 6, 3)[f]
    beam = common.new_frame(size)
    common.polygon(beam, [(cx - 9, cy - 6), (cx + 9, cy - 6), (cx + 3, top), (cx - 3, top)], PAL[LIGHT], alpha=170)
    beam = common.add_outline(beam, PAL[MID])
    common.polygon(beam, [(cx - 2, top + 3), (cx + 1, top + 3), (cx + 1, cy - 8), (cx - 2, cy - 8)], PAL[HIGHLIGHT])
    layers.append(beam)
    # 가운데는 위로 갈수록 작아지는 구슬 줄. 4프레임에 8px 씩 내려간다.
    for k in range(5):
        y = 6 + (8 * f + 16 * k) % 80
        if y > cy - 12:
            continue
        r = 2.5 + 4.5 * y / cy
        layers.append(mass(size, lambda l, y=y, r=r: common.disc(l, cx, y, r, PAL[MID])))
    # 떨어지는 별: 아래 팔이 길다(머리가 아래).
    layers.append(mass(size, star_paint(cx, cy, 22 + pulse, 22 + pulse, 6, down=32 + pulse)))
    frame = stack(size, *layers)
    add_core(frame, cx - 2, cy - 2, 8)
    # 별 양옆의 작은 반짝임
    for dx, dy in ((-30, -2), (30, -2)):
        sx, sy = cx + dx, cy + dy - (f % 2) * 3
        frame.alpha_composite(mass(size, glint_paint(sx, sy, 5)))
    return frame


def make_drop():
    return [drop_frame(f) for f in range(4)]


# ---------- Hit ----------

def hit_frame(f):
    size, cx, cy = 128, 64, 64
    ring_r = (10, 22, 33, 43, 52, 57)[f]
    ring_t = (6, 6, 6, 6, 5, 5)[f]
    star_arm = (34, 46, 38, 28, 0, 0)[f]
    layers = []
    if f < 5:
        layers.append(mass(size, ring_paint(cx, cy, ring_r, ring_r, ring_t)))
    if f == 0:
        layers.append(mass(size, lambda l: common.disc(l, cx, cy, 16, PAL[MID])))
    if star_arm:
        layers.append(mass(size, star_paint(cx, cy, star_arm, star_arm, 7 - f)))
    # 대각선으로 퍼지는 반짝임
    spark = (0, 20, 32, 42, 50, 54)[f]
    arm = (0, 7, 7, 7, 7, 7)[f]
    if spark:
        for sx, sy in ((1, 1), (-1, 1), (1, -1), (-1, -1)):
            layers.append(mass(size, glint_paint(cx + sx * spark * 0.8, cy + sy * spark * 0.8, arm)))
    frame = stack(size, *layers)
    if f <= 3:
        add_core(frame, cx - 1, cy - 1, (12, 11, 7, 3)[f])
    return frame


def make_hit():
    return [hit_frame(f) for f in range(6)]


# ---------- Explode ----------

GROUND = common.EXPLODE_GROUND_ROW


def explode_frame(f):
    size, cx = 256, 128
    scale = (0.5, 1.0, 1.0, 0.85, 0.5, 0)[f]       # 빛줄기 굵기
    tip = (118, GROUND, GROUND, GROUND, GROUND, GROUND)[f]  # 내려오는 중이면 지면보다 위
    halo = (0, 50, 76, 88, 60, 0)[f]
    layers = []

    # 지면 후광: 두꺼운 LIGHT 띠, 안쪽 가장자리 HIGHLIGHT, 갈색 외곽선
    if halo:
        ry = max(14, halo * 0.18)
        thick = 10 if f < 4 else 7
        layers.append(mass(size, ring_paint(cx, GROUND, halo, ry, thick)))
        inner = common.new_frame(size)
        ring_paint(cx, GROUND, halo - thick + 3, ry - 5, 2, PAL[HIGHLIGHT])(inner)
        layers.append(inner)

    frame = stack(size, *layers)
    if scale:
        frame.alpha_composite(light_beam(size, cx, tip, scale, f))

    # 위로 오르는 십자 반짝임 (프레임마다 올라가며 작아진다)
    glints = [(-1, 0), (1, 1), (-1, 2)]
    if f >= 1:
        for side, k in glints:
            age = f - 1 + k * 0.5
            x = cx + side * (56 + 10 * k)
            y = GROUND - 26 - age * 34 - k * 14
            arm = (8, 8, 7, 6, 5)[f - 1]
            if f == 5 and k == 2:
                continue
            frame.alpha_composite(mass(size, glint_paint(round(x), round(y), arm)))
    return frame


def light_beam(size, cx, tip, scale, f):
    """위에서 내려오는 빛줄기. 바깥은 반투명 띠, 가운데는 불투명한 흰 줄기."""
    top = 2  # 프레임 가장자리 2px 안쪽에서 시작
    band = common.new_frame(size)
    core = common.new_frame(size)

    def half(y, top_w, bottom_w):
        w = (top_w + (bottom_w - top_w) * y / GROUND) * scale
        round_start = tip - w  # 내려오는 머리는 둥글게
        if tip < GROUND and y > round_start:
            w *= math.sqrt(max(0.0, 1 - ((y - round_start) / max(w, 1)) ** 2))
        return w

    for y in range(top, tip + 1):
        hw = half(y, 24, 36)
        for x in range(int(cx - hw), int(cx + hw) + 1):
            common.put(band, x, y, PAL[LIGHT], 200)
        cw = half(y, 12, 20)
        for x in range(int(cx - cw), int(cx + cw) + 1):
            common.put(core, x, y, PAL[HIGHLIGHT])
    # 바깥 띠의 세로 줄무늬: 프레임마다 가운데에서 바깥으로 밀려난다.
    for k in range(4):
        offset = ((k * 18 + f * 6) % 72) - 36
        for y in range(top, tip):
            hw = half(y, 24, 36)
            x = round(cx + offset * hw / 36)
            if abs(x - cx) < half(y, 12, 20) + 2 or abs(x - cx) > hw - 3:
                continue
            for dx in range(3):
                common.put(band, x + dx, y, PAL[DARK], 200)
    out = common.add_outline(band, PAL[OUTLINE])
    out.alpha_composite(core)
    return out


def make_explode():
    return [explode_frame(f) for f in range(6)]


def main():
    for kind, maker in (("Shoot", make_shoot), ("Drop", make_drop), ("Hit", make_hit), ("Explode", make_explode)):
        print(common.save_sheet(maker(), "Holy", kind))
    print(common.write_preview("Holy"))


if __name__ == "__main__":
    main()
