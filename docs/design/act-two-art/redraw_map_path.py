"""2부 맵의 길을 스테이지 순서대로 한 줄로 이어지게 고친다.

사용법: python3 docs/design/act-two-art/redraw_map_path.py
저장소 루트에서 실행한다. regrid_backgrounds.py 로 384×256 격자를 맞춘 ActTwoMap.png 를 고친다.

생성한 맵은 길이 잿빛 유적(7)에서 뒤틀린 숲(8)과 세계수(9)로 갈라지고 숲과 세계수 사이에는 길이 없다.
유적→세계수 길을 지우고 숲→세계수 길을 그려서 서리 마을→협곡→유적→숲→세계수가 한 줄이 되게 한다.
regrid_backgrounds.py 를 실행한 직후의 그림에 한 번만 실행한다.
"""
import math
from collections import Counter
from pathlib import Path

from PIL import Image

MAP = Path.cwd() / "Assets/Art/Map/ActTwoMap.png"
GRID = (384, 256)

OUTLINE = (2, 3, 2)
ROAD = (155, 98, 34)
ROAD_LIGHT = (177, 140, 33)
ROAD_HIGHLIGHT = (203, 146, 29)
ROAD_SEAM = (85, 50, 28)
ROAD_COLORS = {ROAD, ROAD_LIGHT, ROAD_HIGHLIGHT, ROAD_SEAM, (98, 88, 37)}

# 지울 유적→세계수 길이 들어 있는 범위(격자 좌표, 끝 포함). 아래쪽 유적 기둥과 마른 나무는 회색·갈색이라 남는다.
ERASE_BOX = (293, 95, 316, 117)
# 세계수 섬 바닥 테두리 줄. 이 줄의 길 색은 풀 대신 같은 줄 양옆의 테두리 색으로 메운다.
PLATFORM_EDGE_ROWS = (95, 96)
# 새 길의 중심선을 지나는 점. 숲 섬 안쪽에서 시작해 풀밭을 완만하게 휘어 지나고 세계수 섬 안쪽에서 끝난다.
NEW_PATH = [(231, 83), (244, 86), (260, 88), (276, 88), (289, 86), (298, 85)]
ROAD_HALF_WIDTH = 3.2
# 타일 길이를 조금씩 바꿔 자로 그은 무늬가 되지 않게 한다.
TILE_LENGTHS = (5, 4, 6, 5, 4, 5)


def is_green(color):
    r, g, b = color
    return g > 120 and g > r and g > b


def is_stone(color):
    r, g, b = color
    return abs(r - g) < 15 and abs(g - b) < 15 and r > 60


def erase_branch(px):
    x0, y0, x1, y1 = ERASE_BOX
    road = {(x, y) for y in range(y0, y1 + 1) for x in range(x0, x1 + 1) if px[x, y] in ROAD_COLORS}
    erased = set()
    for x, y in road:
        if y not in PLATFORM_EDGE_ROWS:
            erased.add((x, y))
            continue
        # 테두리 줄은 왼쪽·오른쪽으로 가장 가까운 길 아닌 칸의 색을 이어 쓴다.
        for step in range(1, x1 - x0 + 2):
            side = [(x - step, y), (x + step, y)]
            colors = [px[p] for p in side if p not in road]
            if colors:
                px[x, y] = colors[0]
                break
    # 길 윤곽선만 지운다. 길 색 칸에 붙어 있지 않은 검은 칸(섬 테두리)과 회색 기둥에 붙은 검은 칸은 남긴다.
    for y in range(max(y0, PLATFORM_EDGE_ROWS[-1] + 1), y1 + 1):
        for x in range(x0, x1 + 1):
            if px[x, y] != OUTLINE:
                continue
            around = [(x + dx, y + dy) for dx in (-1, 0, 1) for dy in (-1, 0, 1)]
            if any(p in road for p in around) and not any(is_stone(px[p]) for p in around):
                erased.add((x, y))
    # 지운 칸은 둘레의 풀색으로 바깥부터 채운다.
    while erased:
        filled = {}
        for x, y in erased:
            around = [px[x + dx, y + dy] for dx in (-1, 0, 1) for dy in (-1, 0, 1)
                      if (x + dx, y + dy) not in erased and is_green(px[x + dx, y + dy])]
            if around:
                filled[(x, y)] = Counter(around).most_common(1)[0][0]
        if not filled:
            break
        for point, color in filled.items():
            px[point] = color
            erased.discard(point)


def smooth_path(points, steps=12):
    """Catmull-Rom 곡선으로 점 사이를 촘촘한 점으로 채운다."""
    padded = [points[0]] + points + [points[-1]]
    result = []
    for i in range(1, len(padded) - 2):
        p0, p1, p2, p3 = padded[i - 1:i + 3]
        for step in range(steps):
            t = step / steps
            result.append(tuple(
                0.5 * (2 * p1[k] + (-p0[k] + p2[k]) * t + (2 * p0[k] - 5 * p1[k] + 4 * p2[k] - p3[k]) * t * t
                       + (-p0[k] + 3 * p1[k] - 3 * p2[k] + p3[k]) * t * t * t)
                for k in (0, 1)))
    result.append(points[-1])
    return result


def nearest_on_path(path, x, y):
    """중심선에서 가장 가까운 점까지의 거리, 그 점까지 따라간 길이, 중심선 위(-)·아래(+) 쪽을 돌려준다."""
    best = (math.inf, 0.0, 0.0)
    travelled = 0.0
    for (ax, ay), (bx, by) in zip(path, path[1:]):
        dx, dy = bx - ax, by - ay
        length = math.hypot(dx, dy)
        t = max(0.0, min(1.0, ((x - ax) * dx + (y - ay) * dy) / (length * length)))
        cx, cy = ax + t * dx, ay + t * dy
        distance = math.hypot(x - cx, y - cy)
        if distance < best[0]:
            side = (x - ax) * -dy / length + (y - ay) * dx / length
            best = (distance, travelled + t * length, side)
        travelled += length
    return best


def tile_position(along):
    """타일 경계에서 얼마나 떨어졌는지와 그 타일의 길이를 돌려준다."""
    index = 0
    while along >= TILE_LENGTHS[index % len(TILE_LENGTHS)]:
        along -= TILE_LENGTHS[index % len(TILE_LENGTHS)]
        index += 1
    return along, TILE_LENGTHS[index % len(TILE_LENGTHS)]


def draw_road(px):
    path = smooth_path(NEW_PATH)
    xs = [p[0] for p in NEW_PATH]
    ys = [p[1] for p in NEW_PATH]
    for y in range(int(min(ys) - ROAD_HALF_WIDTH - 2), int(max(ys) + ROAD_HALF_WIDTH + 3)):
        for x in range(min(xs), max(xs) + 1):
            distance, along, side = nearest_on_path(path, x + 0.5, y + 0.5)
            if distance > ROAD_HALF_WIDTH + 1:
                continue
            if distance > ROAD_HALF_WIDTH:
                # 섬 위로 들어간 양 끝은 윤곽선을 그리지 않아 길이 섬 안으로 이어져 보이게 한다.
                if is_green(px[x, y]):
                    px[x, y] = OUTLINE
                continue
            in_tile, length = tile_position(along)
            if in_tile < 1:
                px[x, y] = ROAD_SEAM
            elif side < -1.6 and 1.5 <= in_tile < length - 1:
                px[x, y] = ROAD_HIGHLIGHT
            elif side < -0.8:
                px[x, y] = ROAD_LIGHT
            else:
                px[x, y] = ROAD


def main():
    image = Image.open(MAP).convert("RGB")
    scale = image.width // GRID[0]
    small = image.resize(GRID, Image.NEAREST)
    px = small.load()
    erase_branch(px)
    draw_road(px)
    small.resize((GRID[0] * scale, GRID[1] * scale), Image.NEAREST).save(MAP)


if __name__ == "__main__":
    main()
