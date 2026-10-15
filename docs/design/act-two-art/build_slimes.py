"""2부 적 슬라임 6종의 프레임을 기존 보라 원거리 슬라임 프레임 8장에서 만든다.

사용법: python3 docs/design/act-two-art/build_slimes.py [미리보기 PNG 경로]
저장소 루트에서 실행한다. 미리보기 경로를 주면 에셋은 건드리지 않고 비교용 그림만 만든다.

기존 보스 슬라임(악마, 꽃, 선인장)처럼 몸은 기준 슬라임 그대로 두고 색 4개만 바꾼 뒤 장식을 얹는다.
그래서 몸 모양, 눈 위치, 프레임마다의 움직임이 기존 슬라임과 같다. 기존 그림은 4픽셀 덩어리라서
128x128 칸 위에서 그리고 4배로 키운다. 최종 보스(Surge)는 고유한 그림을 쓰므로 이 스크립트가 만들거나 덮어쓰지 않는다.
"""
import json
import sys
import uuid
from pathlib import Path

from PIL import Image

ROOT = Path.cwd()
MANIFEST = json.loads((ROOT / "docs/design/act-two-art/manifest.json").read_text(encoding="utf-8"))
BASE = ROOT / "Assets/ThirdParty/ArtResource/JinWook/Slime Resource/Ranged_Purple/KakaoTalk_20240702_171522097_0{}.png"
CELL = 4
GRID = 128
FRAME_SIZE = 512
PART_SCALE = 2

# 기준 슬라임의 색 4개: 몸, 테두리와 눈, 아래 띠, 반사광
BODY, RIM, BAND, SHINE = (86, 30, 114), (220, 107, 232), (121, 51, 156), (250, 245, 245)

# 프레임 순서는 SlimeAnimator와 같다: 대기 2장, 이동·근접 공격 2장, 피격, 쓰러짐, 원거리 공격 2장
IDLE_A, IDLE_B, MOVE_A, MOVE_B, HIT, DEATH, RANGE_A, RANGE_B = range(8)


def grid(rows):
    return [list(row) for row in rows]


# 장식 그림. 글자 하나가 128x128 칸의 한 칸이고 '.'은 투명이다. 몸은 54x30칸이고 눈은 4x7칸이다.
HORN = grid([
    "............oo",
    "..........ooco",
    ".........occo.",
    "........occo..",
    ".......occso..",
    "......occsso..",
    ".....occsso...",
    "....occssso...",
    "...occsssoo...",
    "..ocsssoo.....",
    ".osssoo.......",
])
FUSE = grid([
    "....y...",
    ".y.yry..",
    "..yrRry.",
    ".y.yry.y",
    "....f...",
    "...ff...",
    "...f....",
    "..ff....",
    "..f.....",
    ".ff.....",
])
FUSE_BIG = grid([
    "y...y...y.",
    ".y.yyy.y..",
    "..yrRRry..",
    "yyrRWWRryy",
    "..yrRRry..",
    ".y.yry.y..",
    "y...f...y.",
    "...ff.....",
    "...f......",
    "..ff......",
    "..f.......",
    ".ff.......",
])
LEAF = grid([
    ".......llll.",
    ".....lLLLLll",
    ".lll.lLLLll.",
    "lLLLl.lll...",
    ".lLLLls.....",
    "..lll.s.....",
    "......s.....",
    ".....ss.....",
])
CANNON = grid([
    ".....kkkkkk.",
    "....kGGGGgkk",
    "...kGgggggk.",
    "..kGgggggk..",
    "..kGggggk...",
    ".kGggggk....",
    "kkggggkk....",
    "kgggggggk...",
    "kkkkkkkkk...",
])
SHIELD = grid([
    "....kkkkkk....",
    "..kkGGGGGGkk..",
    ".kGGggggggGGk.",
    ".kGggggggggGk.",
    "kGgggwwwwgggGk",
    "kGggwGGGGwggGk",
    "kGggwGkkGwggGk",
    "kGggwGkkGwggGk",
    "kGggwGGGGwggGk",
    "kGgggwwwwgggGk",
    ".kGggggggggGk.",
    ".kGGggggggGGk.",
    "..kkGGGGGGkk..",
    "....kkkkkk....",
])
CRYSTALS = grid([
    ".....c........c.....",
    "....cCc......cCc....",
    "....cCc..c...cCc....",
    "...cCwCc.cc.cCwCc...",
    "...cCCCc.cCccCCCc...",
    "..cCCCCcccCcccCCCc..",
    "..cCCCCCcCCCcCCCCc..",
])
DUST = grid([
    "..dd.....",
    ".dddd..d.",
    "dddddd...",
    ".dddd..d.",
])

SLIMES = {
    # 돌진: 붉은 갈색 몸, 머리 앞쪽의 뿔
    "Charge": {
        "colors": {BODY: (112, 40, 32), RIM: (236, 132, 92), BAND: (150, 64, 46)},
        "parts": [(HORN, "top", 0.62, 4, 2, {"o": (82, 42, 28), "c": (244, 226, 186), "s": (196, 164, 120)})],
        "windup": "crouch",
    },
    # 폭발: 연두 몸, 머리 위의 심지와 불꽃
    "Explode": {
        "colors": {BODY: (112, 160, 22), RIM: (228, 250, 104), BAND: (90, 130, 16)},
        "parts": [(FUSE, "top", 0.42, 1, 1, {"f": (120, 82, 44), "y": (255, 214, 64), "r": (255, 140, 40), "R": (240, 64, 32), "W": (255, 250, 220)})],
        "windup": "fuse",
    },
    # 치유: 분홍 몸, 머리 위의 새싹
    "Heal": {
        "colors": {BODY: (210, 104, 156), RIM: (255, 214, 232), BAND: (182, 84, 132)},
        "parts": [(LEAF, "top", 0.45, 2, 1, {"l": (50, 128, 56), "L": (120, 210, 96), "s": (110, 76, 44)})],
    },
    # 곡사: 회보라 몸, 등의 포신
    "Mortar": {
        "colors": {BODY: (64, 58, 92), RIM: (176, 166, 216), BAND: (88, 80, 124)},
        "parts": [(CANNON, "top", 0.3, 0, 4, {"k": (36, 34, 48), "g": (104, 104, 124), "G": (168, 168, 188)})],
    },
    # 방패: 강철색 몸, 앞에 든 둥근 방패. 눈을 가리지 않도록 몸 앞 끝 밖으로 내민다.
    "Shield": {
        "colors": {BODY: (92, 100, 116), RIM: (214, 222, 232), BAND: (118, 128, 144)},
        "parts": [(SHIELD, "front", 1.0, 18, 1, {"k": (40, 42, 50), "g": (120, 108, 92), "G": (176, 156, 112), "w": (236, 214, 140)})],
    },
    # 분열: 용암 몸, 몸을 가르는 노란 금
    "Split": {
        "colors": {BODY: (150, 32, 20), RIM: (255, 136, 44), BAND: (192, 64, 22)},
        "crack": (255, 220, 64),
    },
    # 최종 보스(Surge)는 이전의 소용돌이 그림을 그대로 쓰므로 여기서 만들지 않는다.
}

# 원거리 공격 프레임의 입 안 색. 몸과 테두리 사이 색으로 바꾼다.
MOUTH = (199, 61, 215)


def load_base(index):
    image = Image.open(str(BASE).format(index + 1)).convert("RGBA")
    return image.resize((GRID, GRID), Image.NEAREST)


def body_mask(image):
    """반사광과 눈을 포함한 몸 칸. 피격·쓰러짐 프레임의 튄 방울은 가장 큰 덩어리가 아니므로 뺀다."""
    pixels = image.load()
    filled = {(x, y) for y in range(GRID) for x in range(GRID) if pixels[x, y][3] > 0}
    best = set()
    seen = set()
    for start in filled:
        if start in seen:
            continue
        group, stack = set(), [start]
        while stack:
            point = stack.pop()
            if point in seen or point not in filled:
                continue
            seen.add(point)
            group.add(point)
            x, y = point
            stack += [(x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)]
        if len(group) > len(best):
            best = group
    return best


def recolor(image, colors):
    pixels = image.load()
    for y in range(GRID):
        for x in range(GRID):
            r, g, b, a = pixels[x, y]
            if a and (r, g, b) in colors:
                pixels[x, y] = colors[(r, g, b)] + (255,)


def stamp(image, art, palette, left, bottom, scale=1):
    pixels = image.load()
    height = len(art) * scale
    for row, line in enumerate(art):
        for column, key in enumerate(line):
            if key not in palette:
                continue
            for sy in range(scale):
                for sx in range(scale):
                    x = left + column * scale + sx
                    y = bottom - (height - 1) + row * scale + sy
                    if 0 <= x < GRID and 0 <= y < GRID:
                        pixels[x, y] = palette[key] + (255,)


def place(image, mask, art, palette, anchor, fraction, dx, dy):
    """장식 한 글자를 2x2칸으로 그린다. 기존 보스의 장식처럼 몸과 같은 굵기의 픽셀이 된다."""
    xs = [x for x, _ in mask]
    left, right = min(xs), max(xs)
    width = len(art[0]) * PART_SCALE
    if anchor == "top":
        column = left + round((right - left) * fraction)
        surface = min(y for x, y in mask if x == column)
        stamp(image, art, palette, column - width // 2 + dx, surface + dy, PART_SCALE)
    else:
        bottom = max(y for _, y in mask)
        stamp(image, art, palette, right - width + 1 + dx, bottom + dy, PART_SCALE)


def components(points):
    points, groups = set(points), []
    while points:
        stack, group = [points.pop()], set()
        while stack:
            x, y = stack.pop()
            group.add((x, y))
            for near in ((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)):
                if near in points:
                    points.remove(near)
                    stack.append(near)
        groups.append(group)
    return groups


def eyes(image, mask, rim):
    """몸 안의 테두리색 덩어리 중 눈 크기(4x7칸 안팎)인 것을 찾는다."""
    pixels = image.load()
    rim_points = [(x, y) for x, y in mask if pixels[x, y][:3] == rim]
    return [group for group in components(rim_points) if 10 <= len(group) <= 40]


def angry_brows(image, mask, rim):
    found = eyes(image, mask, rim)
    if len(found) != 2:
        return
    pixels = image.load()
    found.sort(key=lambda group: min(x for x, _ in group))
    for side, group in zip((-1, 1), found):
        top = min(y for _, y in group)
        left = min(x for x, _ in group)
        right = max(x for x, _ in group)
        # 안쪽 끝이 내려가는 눈썹. 왼쪽 눈은 오른쪽 끝이, 오른쪽 눈은 왼쪽 끝이 안쪽이다.
        xs = range(left - 1, right + 2)
        for step, x in enumerate(xs if side < 0 else reversed(xs)):
            y = top - 3 + step // 2
            pixels[x, y] = rim + (255,)
            pixels[x, y - 1] = rim + (255,)


def crack(image, mask, color):
    pixels = image.load()
    xs = [x for x, _ in mask]
    left, right = min(xs), max(xs)
    column = left + round((right - left) * 0.32)
    rows = sorted({y for _, y in mask})
    for index, y in enumerate(rows):
        x = column + (1 if (index // 3) % 2 else 0)
        for cx in (x, x + 1):
            if (cx, y) in mask and pixels[cx, y][:3] != SHINE:
                pixels[cx, y] = color + (255,)


def resize_body(image, width_scale, height_scale, dx):
    """몸을 바닥선에 붙인 채 가로·세로로 늘리거나 줄인다. 칸 단위로 늘려 픽셀 덩어리가 유지된다."""
    box = image.getbbox()
    body = image.crop(box)
    size = (round(body.width * width_scale), round(body.height * height_scale))
    body = body.resize(size, Image.NEAREST)
    result = Image.new("RGBA", image.size)
    center = (box[0] + box[2]) // 2
    result.alpha_composite(body, (center - size[0] // 2 + dx, box[3] - size[1]))
    return result


def build_frame(spec, index, windup=None):
    if windup == "crouch":
        # 이동 프레임은 눈 바로 위 테두리가 패여 있어 눈썹이 묻힌다. 대기 프레임에 눈썹을 그린 뒤 누른다.
        source_index = IDLE_A
    elif windup == "fuse":
        source_index = RANGE_A
    else:
        source_index = index
    image = load_base(source_index)
    mask = body_mask(image)
    colors = dict(spec["colors"])
    rim, body = colors[RIM], colors[BODY]
    colors[MOUTH] = tuple((a + b) // 2 for a, b in zip(rim, body))
    if windup == "fuse" and index == 1:
        # 터지기 직전에 몸이 밝게 번쩍인다. 눈은 그대로 테두리색이라 보인다.
        colors[BODY] = tuple((a * 2 + b) // 3 for a, b in zip(rim, body))
        colors[BAND] = body
    recolor(image, colors)
    if windup == "crouch":
        angry_brows(image, mask, rim)
    if "crack" in spec:
        crack(image, mask, spec["crack"])
    for art, anchor, fraction, dx, dy, palette in spec.get("parts", []):
        if windup == "fuse":
            art = FUSE_BIG
        place(image, mask, art, palette, anchor, fraction, dx, dy)
    if windup == "crouch" and index == 0:
        image = resize_body(image, 1.06, 0.86, -1)
    if windup == "crouch" and index == 1:
        # 두 번째 장은 더 납작하게 웅크리며 뒤로 물러나고, 뒤쪽에 먼지가 인다.
        image = resize_body(image, 1.12, 0.76, -3)
        box = image.getbbox()
        stamp(image, DUST, {"d": (214, 200, 170)}, box[0] - 18, box[3] - 1, PART_SCALE)
    if windup == "fuse" and index == 1:
        image = resize_body(image, 1.1, 1.1, 0)
    return image


def upscale(image, scale):
    big = image.resize((GRID * scale, GRID * scale), Image.NEAREST)
    if scale == CELL:
        return big
    # 캔버스는 512로 같게 두고, 바닥선을 기준 슬라임과 맞춘다.
    base_bottom = 75 * CELL
    bottom = max(y for y in range(big.height) for x in range(0, big.width, scale) if big.getpixel((x, y))[3])
    left = (big.width - FRAME_SIZE) // 2
    top = bottom - base_bottom
    return big.crop((left, top, left + FRAME_SIZE, top + FRAME_SIZE))


def frames_for(name):
    spec = SLIMES[name]
    scale = spec.get("scale", CELL)
    frames = [upscale(build_frame(spec, i), scale) for i in range(8)]
    if "windup" in spec:
        frames += [upscale(build_frame(spec, i, spec["windup"]), scale) for i in range(2)]
    return frames


def preview(path):
    names = list(SLIMES)
    reference = [Image.open(str(BASE).format(i + 1)).convert("RGBA") for i in range(8)]
    cell = 200
    sheet = Image.new("RGBA", (cell * 10, cell * (len(names) + 1)), (58, 68, 80, 255))
    rows = [reference] + [frames_for(name) for name in names]
    for r, frames in enumerate(rows):
        for c, frame in enumerate(frames):
            crop = frame.crop((56, 56, 456, 456)).resize((cell, cell), Image.NEAREST)
            sheet.alpha_composite(crop, (c * cell, r * cell))
    sheet.save(path)


def write_assets():
    """프레임 PNG를 manifest 경로에 덮어쓴다. 예고 프레임(9, 10번째)은 처음이면 manifest와 .meta를 만든다."""
    for name in SLIMES:
        frames = frames_for(name)
        entries = MANIFEST["enemies"][name]["frames"]
        first = ROOT / entries[0]["path"]
        for number in range(len(entries) + 1, len(frames) + 1):
            path = first.with_name(first.name.replace("_01", f"_{number:02d}"))
            entries.append({"path": str(path.relative_to(ROOT)), "guid": uuid.uuid4().hex})
        template = first.with_name(first.name + ".meta").read_text(encoding="utf-8")
        template_guid = next(line.split()[1] for line in template.splitlines() if line.startswith("guid:"))
        for frame, entry in zip(frames, entries):
            path = ROOT / entry["path"]
            frame.save(path)
            meta = path.with_name(path.name + ".meta")
            if not meta.exists():
                meta.write_text(template.replace(template_guid, entry["guid"]), encoding="utf-8")
    (ROOT / "docs/design/act-two-art/manifest.json").write_text(
        json.dumps(MANIFEST, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


if __name__ == "__main__":
    if len(sys.argv) > 1:
        preview(sys.argv[1])
    else:
        write_assets()
