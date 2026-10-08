"""번개 마법 시트 4장(Shoot, Drop, Hit, Explode)을 그린다.

실행: python3 docs/design/spell-effects/lightning.py
번개는 신성과 구분하려고 날카롭게 꺾인 굵은 지그재그와 뾰족한 별 모양만 쓴다.
"""
from __future__ import annotations

import math
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent))
import common  # noqa: E402
from common import DARK, HIGHLIGHT, LIGHT, MID, OUTLINE, PALETTES  # noqa: E402

PAL = PALETTES["Lightning"]
ELEMENT = "Lightning"


# ---------- 작은 도구 ----------

def thick(image, points, width, color):
    """꺾은선을 굵게 찍는다. 꺾이는 곳이 뭉툭하지 않도록 디스크를 촘촘히 놓는다."""
    for (x0, y0), (x1, y1) in zip(points, points[1:]):
        steps = max(1, int(max(abs(x1 - x0), abs(y1 - y0))))
        for i in range(steps + 1):
            t = i / steps
            common.disc(image, x0 + (x1 - x0) * t, y0 + (y1 - y0) * t, width / 2, color)


def thick_taper(image, points, w_start, w_end, color):
    """꺾은선을 굵게 찍되 처음 점의 굵기에서 끝 점의 굵기로 뾰족하게 줄인다."""
    total = sum(math.hypot(b[0] - a[0], b[1] - a[1]) for a, b in zip(points, points[1:])) or 1
    done = 0.0
    for (x0, y0), (x1, y1) in zip(points, points[1:]):
        seg = math.hypot(x1 - x0, y1 - y0)
        steps = max(1, int(seg))
        for i in range(steps + 1):
            t = i / steps
            w = w_start + (w_end - w_start) * (done + seg * t) / total
            common.disc(image, x0 + (x1 - x0) * t, y0 + (y1 - y0) * t, w / 2, color)
        done += seg


def zigzag(rng, start, end, segments, amp):
    """start 에서 end 로 가는 날카로운 지그재그 점 목록. 좌우를 번갈아 크게 꺾는다."""
    (x0, y0), (x1, y1) = start, end
    dx, dy = x1 - x0, y1 - y0
    length = math.hypot(dx, dy) or 1
    nx, ny = -dy / length, dx / length
    points = [start]
    for i in range(1, segments):
        t = i / segments
        side = 1 if i % 2 else -1
        off = side * amp * (0.7 + 0.5 * rng.random())
        points.append((x0 + dx * t + nx * off, y0 + dy * t + ny * off))
    points.append(end)
    return points


def finish(mass, cores=(), core_width=2):
    """덩어리에 3단 명암, 중심 하이라이트, 외곽선을 입힌다."""
    layer = common.shade_bands(mass, PAL)
    for path in cores:
        thick(layer, path, core_width, PAL[HIGHLIGHT])
    return common.add_outline(layer, PAL[OUTLINE])


def star_points(cx, cy, outer, inner, spikes, rot):
    """뾰족한 별 꼭짓점."""
    pts = []
    for i in range(spikes * 2):
        r = outer if i % 2 == 0 else inner
        a = rot + math.pi * i / spikes
        pts.append((cx + math.cos(a) * r, cy + math.sin(a) * r))
    return pts


def spark(image, x, y, size=2, color=None):
    """외곽선 밖에 찍는 2x2 이상 불꽃 조각. 다른 그림과 겹치면 찍지 않는다."""
    color = color or PAL[LIGHT]
    for yy in range(y - 2, y + size + 2):
        for xx in range(x - 2, x + size + 2):
            if 0 <= xx < image.width and 0 <= yy < image.height and image.getpixel((xx, yy))[3]:
                return
    common.pen(image).rectangle([x - 1, y - 1, x + size, y + size], fill=common.rgba(PAL[OUTLINE]))
    common.pen(image).rectangle([x, y, x + size - 1, y + size - 1], fill=common.rgba(color))


# ---------- Shoot ----------

def lightning_shoot():
    frames = []
    for f in range(4):
        rng = common.seeded(f"LightningShoot{f}")
        cx, cy, r = 80, 64, 17
        mass = common.new_frame(128)
        # 둥근 구체(지름 34px)와 둘레의 작은 뾰족 가시. 길이가 프레임마다 달라 지직거린다.
        common.disc(mass, cx, cy, r, PAL[MID])
        for k in range(9):
            a = math.pi * 2 * k / 9 + rng.uniform(-0.2, 0.2) + f * 0.35
            length = r + rng.randint(5, 10)
            w = 0.28
            common.polygon(mass, [(cx + math.cos(a - w) * (r - 3), cy + math.sin(a - w) * (r - 3)),
                                  (cx + math.cos(a + w) * (r - 3), cy + math.sin(a + w) * (r - 3)),
                                  (cx + math.cos(a) * length, cy + math.sin(a) * length)], PAL[MID])
        # 왼쪽으로 흐르는 굵은 지그재그 꼬리 한 줄
        end = (10 + rng.randint(0, 8), cy + (-10, 12, -6, 8)[f])
        trail = zigzag(rng, (cx - 8, cy), end, 4, 9)
        thick(mass, trail, 11, PAL[MID])
        layer = finish(mass, [trail], 3)
        # 구체 안쪽: LIGHT 고리와 하얗게 달아오른 중심
        common.disc(layer, cx, cy, 12, PAL[LIGHT])
        common.disc(layer, cx - 1, cy - 1, 7, PAL[HIGHLIGHT])
        for _ in range(3):
            spark(layer, rng.randint(6, 50), rng.randint(14, 108), 3)
        frames.append(layer)
    return frames


# ---------- Drop ----------

def lightning_drop():
    frames = []
    for f in range(4):
        rng = common.seeded(f"LightningDrop{f}")
        cx, head_y = 64, 78
        mass = common.new_frame(128)
        # 머리: 아래를 향한 굵은 화살촉
        common.polygon(mass, [(cx - 15, head_y - 18), (cx + 15, head_y - 18), (cx + 4, head_y - 2), (cx, head_y + 18), (cx - 4, head_y - 2)], PAL[MID])
        common.polygon(mass, [(cx - 15, head_y - 18), (cx + 15, head_y - 18), (cx, head_y + 18)], PAL[MID])
        # 꼬리: 위로 올라가는 굵은 지그재그와 곁가지
        cores = []
        path = zigzag(rng, (cx, head_y - 14), (cx + rng.randint(-4, 4), 12), 5, 10)
        thick_taper(mass, path, 10, 3, PAL[MID])
        cores.append(path)
        for side in (-1, 1):
            by = 44 + rng.randint(-3, 3) + (f % 2) * 10 * side
            bx, _ = path[2]
            branch = [(bx, by), (bx + side * 14, by - 8), (bx + side * 10, by - 16), (bx + side * 24, by - 20)]
            thick(mass, branch, 7, PAL[MID])
            cores.append(branch)
        layer = finish(mass, cores, 4)
        common.polygon(layer, [(cx - 7, head_y - 14), (cx, head_y - 14), (cx - 2, head_y + 4)], PAL[HIGHLIGHT])
        for _ in range(3):
            spark(layer, rng.randint(14, 106), rng.randint(10, 70), 2)
        frames.append(layer)
    return frames


# ---------- Hit ----------

def lightning_hit():
    frames = []
    # 곧은 가시 6개: (각도, 길이). 일부러 비대칭이다.
    spikes = [(0.15, 40), (1.15, 28), (2.0, 44), (3.3, 30), (4.2, 38), (5.3, 26)]
    # 짧은 지그재그 번개 3개: (각도, 길이, 꺾임 방향)
    bolts = [(0.75, 34, 1), (2.7, 28, -1), (4.8, 36, 1)]
    scales = [0.45, 0.85, 1.0, 0.8, 0.5, 0.0]
    for f, sc in enumerate(scales):
        rng = common.seeded(f"LightningHit{f}")
        cx = cy = 64
        mass = common.new_frame(128)
        cores = []
        if sc:
            tilt = f * 0.12
            for a, length in spikes:
                a += tilt
                w = 0.42
                ln = length * sc
                common.polygon(mass, [(cx + math.cos(a - w) * 11, cy + math.sin(a - w) * 11),
                                      (cx + math.cos(a + w) * 11, cy + math.sin(a + w) * 11),
                                      (cx + math.cos(a) * ln, cy + math.sin(a) * ln)], PAL[MID])
            common.disc(mass, cx, cy, 12 * sc + 3, PAL[MID])
            for a, length, side in bolts:
                a += tilt
                ln = length * sc
                mid = (cx + math.cos(a) * ln * 0.5 - math.sin(a) * 7 * side, cy + math.sin(a) * ln * 0.5 + math.cos(a) * 7 * side)
                end = (cx + math.cos(a) * ln, cy + math.sin(a) * ln)
                path = [(cx, cy), mid, end]
                thick(mass, path, 6 if sc > 0.6 else 4, PAL[MID])
        else:
            for k in range(7):
                a = k * 0.9 + 0.3
                r = 50 + rng.randint(0, 6)
                common.pen(mass).rectangle([round(cx + math.cos(a) * r), round(cy + math.sin(a) * r)] * 2, fill=common.rgba(PAL[MID]))
        # 마지막 단계 말고는 조각을 먼저 그려 외곽선을 같이 두른다
        if f >= 2 and sc:
            for k in range(7):
                a = k * 0.9 + 0.5 + f * 0.2
                r = 52 + rng.randint(0, 8) + (f - 2) * 3
                x, y = round(cx + math.cos(a) * r), round(cy + math.sin(a) * r)
                x, y = min(max(x, 5), 118), min(max(y, 5), 118)
                size = 5 if f < 4 else 4
                common.pen(mass).rectangle([x, y, x + size - 1, y + size - 1], fill=common.rgba(PAL[MID]))
        layer = finish(mass, [], 3) if sc else common.add_outline(common.shade_bands(mass, PAL), PAL[OUTLINE])
        if sc:
            common.disc(layer, cx - 1, cy - 1, max(5 * sc, 2.5), PAL[HIGHLIGHT])
        if f == 5:
            for k in range(7):
                a = k * 0.9 + 0.5 + 0.2 * f
                spark(layer, min(max(round(cx + math.cos(a) * (56 + k % 3 * 2)), 5), 118), min(max(round(cy + math.sin(a) * (56 + k % 3 * 2)), 5), 118), 4)
        frames.append(layer)
    return frames


# ---------- Explode ----------

def ground_ring(mass, cx, radius, f):
    """지면(218 행)에 붙은 낮고 굵은 지그재그 스파크 고리. 늦은 프레임은 조각을 빼서 줄인다."""
    gy = common.EXPLODE_GROUND_ROW
    n = 14
    keep = {4: 9, 5: 5}.get(f, n)
    for k in range(keep):
        # 고리를 반으로 가르듯 좌우 끝에서부터 남긴다
        idx = (k // 2) if k % 2 == 0 else n - 1 - k // 2
        a0 = math.pi * 2 * idx / n
        pts = []
        for step in range(3):
            a = a0 + step * math.pi * 2 / n / 2
            rr = radius + (7 if step % 2 else -2)
            pts.append((cx + math.cos(a) * rr, gy - 6 + math.sin(a) * rr * 0.16 - (step % 2) * 6))
        thick(mass, pts, 8, PAL[MID])


def clip_path(path, max_y):
    """max_y 아래로 내려간 부분을 잘라 번개가 위에서부터 내려오는 모양을 만든다."""
    out = [path[0]]
    for (x0, y0), (x1, y1) in zip(path, path[1:]):
        if y1 <= max_y:
            out.append((x1, y1))
        else:
            t = (max_y - y0) / (y1 - y0) if y1 != y0 else 0
            out.append((x0 + (x1 - x0) * t, max_y))
            break
    return out


def lightning_explode():
    frames = []
    gy = common.EXPLODE_GROUND_ROW
    # 프레임별 (번개 굵기, 번개가 내려온 비율, 고리 반지름, 고리를 그릴지)
    stages = [(12, 0.6, 0, False), (26, 1.0, 22, True), (22, 1.0, 46, True),
              (14, 1.0, 72, True), (0, 0, 74, True), (0, 0, 54, True)]
    for f, (width, reach, ring, show_ring) in enumerate(stages):
        rng = common.seeded(f"LightningExplode{f}")
        cx = 128
        mass = common.new_frame(256)
        cores = []
        if width:
            end = (cx, 4 + (gy - 4) * reach)
            path = zigzag(common.seeded("LightningExplodeBolt" + str(f % 2)), (cx + 6, 2), (cx, gy), 7, 26)
            path = clip_path(path, end[1])
            thick(mass, path, width, PAL[MID])
            cores.append(path)
            # 곁가지 번개
            for k in (2, 4, 6):
                if k < len(path) - 1:
                    bx, by = path[k]
                    side = 1 if k % 4 else -1
                    branch = [(bx, by), (bx + side * 26, by + 10), (bx + side * 20, by + 30), (bx + side * 44, by + 44)]
                    thick(mass, branch, max(8, width // 2), PAL[MID])
                    cores.append(branch)
        if show_ring:
            ground_ring(mass, cx, ring, f)
            if f < 4:
                common.polygon(mass, star_points(cx, gy - 8, (34 - f * 3) + 8, 12, 8, f * 0.2), PAL[MID])
            # 지면 위로 튀는 3x3 조각
            for k in range(6):
                a = k * math.pi / 3 + f * 0.5
                r = ring * 0.8 + 14 + rng.randint(0, 10)
                x = round(cx + math.cos(a) * r)
                y = round(gy - 18 - abs(math.sin(a)) * (24 + ring * 0.15) - (f - 3) * 4 * (f > 3))
                if f < 4 or k < (4 if f == 4 else 2):
                    common.pen(mass).rectangle([x, y, x + 2, y + 2], fill=common.rgba(PAL[MID]))
        layer = finish(mass, cores, max(3, width // 4) if width else 2)
        frames.append(layer)
    return frames


if __name__ == "__main__":
    common.save_sheet(lightning_shoot(), ELEMENT, "Shoot")
    common.save_sheet(lightning_drop(), ELEMENT, "Drop")
    common.save_sheet(lightning_hit(), ELEMENT, "Hit")
    common.save_sheet(lightning_explode(), ELEMENT, "Explode")
    common.write_preview(ELEMENT)
