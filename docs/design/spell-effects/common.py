"""마법 이펙트 스프라이트 시트를 그리는 공용 규칙과 도구.

원소별 스크립트(fire.py, ice.py, rock.py, lightning.py, holy.py)가 이 모듈을
import 해서 같은 프레임 크기, 외곽선, 명암 단계, 시트 배치를 사용한다.

픽셀 크기: 슬라임 스프라이트의 한 픽셀은 화면에서 약 0.04 unit 이다.
Shoot·Drop·Hit 프레임은 128px 를 PPU 128 로 가져와 prefab scale 5 에서 5 unit,
Explode 프레임은 256px 를 PPU 256 으로 가져와 prefab scale 10 에서 10 unit 이 된다.
두 경우 모두 한 픽셀이 0.039 unit 이라 슬라임과 픽셀 크기가 같다.
"""
from __future__ import annotations

import math
import random
from pathlib import Path

from PIL import Image, ImageDraw

REPO_ROOT = Path(__file__).resolve().parents[3]
ART_ROOT = REPO_ROOT / "Assets" / "Art" / "Combat" / "Spells"
PREVIEW_ROOT = Path(__file__).resolve().parent / "previews"

# 시트 종류별 프레임 크기와 프레임 수. build_unity_assets.py 도 이 값을 읽는다.
SHEET_SPECS = {
    # 오른쪽(+x)으로 날아가는 발사체. 반복 재생.
    "Shoot": {"frame": 128, "count": 4, "fps": 12, "loop": True},
    # 아래(-y)로 떨어지는 발사체. 반복 재생. 회전하지 않고 떨어지는 모양으로 그린다.
    "Drop": {"frame": 128, "count": 4, "fps": 12, "loop": True},
    # Shoot·Drop 이 적에게 닿은 자리에서 터지는 모양. 0.5 초 뒤 오브젝트가 사라진다.
    "Hit": {"frame": 128, "count": 6, "fps": 12, "loop": False},
    # 적 발밑에서 솟는 광역 마법. 0.5 초 뒤 오브젝트가 사라진다.
    "Explode": {"frame": 256, "count": 6, "fps": 12, "loop": False},
}

# Explode 프레임에서 지면이 놓이는 행. 프레임 중심이 월드 y=0 에 놓이고 적은
# y≈-3.3 에 서 있으므로, 위에서 218 행(월드 y≈-3.5)을 지면으로 쓴다.
EXPLODE_GROUND_ROW = 218

# 원소별 색. 순서: 외곽선, 어두운 면, 중간 면, 밝은 면, 하이라이트.
# 상태 이상 오버레이(Assets/Resources/Combat/ElementalStatusOverlay.png)와 같은 색 계열이다.
PALETTES = {
    "Fire": [(74, 22, 18), (196, 52, 34), (243, 122, 44), (255, 196, 76), (255, 244, 196)],
    "Ice": [(28, 52, 92), (62, 132, 196), (124, 204, 236), (196, 240, 252), (255, 255, 255)],
    "Rock": [(52, 36, 28), (112, 78, 54), (164, 120, 80), (206, 168, 118), (238, 218, 178)],
    "Lightning": [(58, 34, 84), (214, 150, 28), (252, 214, 54), (255, 244, 148), (255, 255, 236)],
    "Holy": [(108, 78, 40), (226, 178, 84), (250, 224, 140), (255, 246, 208), (255, 255, 255)],
}
OUTLINE, DARK, MID, LIGHT, HIGHLIGHT = range(5)


def rgba(color, alpha=255):
    return tuple(color[:3]) + (alpha,)


def new_frame(size: int) -> Image.Image:
    return Image.new("RGBA", (size, size), (0, 0, 0, 0))


def pen(image: Image.Image) -> ImageDraw.ImageDraw:
    """안티에일리어싱 없는 그리기 도구. 픽셀 경계를 그대로 유지한다."""
    draw = ImageDraw.Draw(image)
    draw.fontmode = "1"
    return draw


def put(image: Image.Image, x: int, y: int, color, alpha=255) -> None:
    if 0 <= x < image.width and 0 <= y < image.height:
        image.putpixel((int(x), int(y)), rgba(color, alpha))


def disc(image: Image.Image, cx: float, cy: float, radius: float, color, alpha=255) -> None:
    """정수 격자에서 반지름 안의 픽셀을 칠한다. 작은 원도 대칭으로 나온다."""
    r = max(radius, 0.5)
    for y in range(int(cy - r) - 1, int(cy + r) + 2):
        for x in range(int(cx - r) - 1, int(cx + r) + 2):
            if (x + 0.5 - cx) ** 2 + (y + 0.5 - cy) ** 2 <= r * r:
                put(image, x, y, color, alpha)


def ellipse(image: Image.Image, cx: float, cy: float, rx: float, ry: float, color, alpha=255) -> None:
    rx, ry = max(rx, 0.5), max(ry, 0.5)
    for y in range(int(cy - ry) - 1, int(cy + ry) + 2):
        for x in range(int(cx - rx) - 1, int(cx + rx) + 2):
            if ((x + 0.5 - cx) / rx) ** 2 + ((y + 0.5 - cy) / ry) ** 2 <= 1:
                put(image, x, y, color, alpha)


def polygon(image: Image.Image, points, color, alpha=255) -> None:
    pen(image).polygon([(round(x), round(y)) for x, y in points], fill=rgba(color, alpha))


def line(image: Image.Image, points, color, width=1, alpha=255) -> None:
    pen(image).line([(round(x), round(y)) for x, y in points], fill=rgba(color, alpha), width=width)


def add_outline(image: Image.Image, color, alpha_threshold=1) -> Image.Image:
    """불투명 픽셀을 4방향으로 1px 둘러싸는 외곽선을 더한다.

    슬라임과 카드 그림처럼 모든 덩어리에 어두운 외곽선을 두르기 위한 함수다.
    외곽선을 두르지 않을 작은 불씨·반짝임은 이 함수를 부른 뒤에 그린다.
    """
    source = image.load()
    result = image.copy()
    target = result.load()
    width, height = image.size
    for y in range(height):
        for x in range(width):
            if source[x, y][3] >= alpha_threshold:
                continue
            for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                nx, ny = x + dx, y + dy
                if 0 <= nx < width and 0 <= ny < height and source[nx, ny][3] >= alpha_threshold:
                    target[x, y] = rgba(color)
                    break
    return result


def shade_bands(image: Image.Image, palette, light_from=(-1, -1)) -> Image.Image:
    """한 색으로 칠한 덩어리에 빛 방향 기준 명암 3단을 입힌다.

    light_from 은 빛이 오는 방향이다. 기본값 (-1, -1) 은 왼쪽 위.
    덩어리 경계에서 빛 반대쪽 2px 은 DARK, 빛 쪽 1px 은 LIGHT, 나머지는 MID 다.
    HIGHLIGHT 는 각 원소 스크립트가 직접 찍는다. 이미 칠한 색과 상관없이
    불투명 픽셀 전체를 덩어리로 본다.
    """
    lx, ly = light_from
    source = image.load()
    result = image.copy()
    target = result.load()
    width, height = image.size

    def solid(x, y):
        return 0 <= x < width and 0 <= y < height and source[x, y][3] > 0

    for y in range(height):
        for x in range(width):
            if not solid(x, y):
                continue
            alpha = source[x, y][3]
            color = palette[MID]
            if not solid(x - lx, y - ly) or not solid(x - lx * 2, y - ly * 2):
                color = palette[DARK]
            if not solid(x + lx, y + ly):
                color = palette[LIGHT]
            target[x, y] = rgba(color, alpha)
    return result


def sparkle(image: Image.Image, x: int, y: int, color, size=1) -> None:
    """십자 모양 반짝임. size=1 이면 5px, size=2 이면 9px."""
    put(image, x, y, color)
    for step in range(1, size + 1):
        for dx, dy in ((step, 0), (-step, 0), (0, step), (0, -step)):
            put(image, x + dx, y + dy, color)


def ease_out(t: float) -> float:
    return 1 - (1 - t) ** 2


def ease_in(t: float) -> float:
    return t * t


def seeded(name: str) -> random.Random:
    """원소와 시트 이름으로 고정한 난수. 다시 실행해도 같은 그림이 나온다."""
    return random.Random(name)


def save_sheet(frames: list[Image.Image], element: str, kind: str) -> Path:
    """프레임을 위에서 아래로 쌓아 Assets/Art/Combat/Spells/<원소>/<원소><종류>.png 로 저장한다."""
    spec = SHEET_SPECS[kind]
    size, count = spec["frame"], spec["count"]
    if len(frames) != count:
        raise ValueError(f"{element}{kind}: 프레임 {count}개가 필요한데 {len(frames)}개를 받았다")
    sheet = Image.new("RGBA", (size, size * count), (0, 0, 0, 0))
    for index, frame in enumerate(frames):
        if frame.size != (size, size):
            raise ValueError(f"{element}{kind}: 프레임 크기는 {size}x{size} 여야 한다")
        sheet.alpha_composite(frame, (0, index * size))
    path = ART_ROOT / element / f"{element}{kind}.png"
    path.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(path)
    return path


def _slime_at_game_scale() -> Image.Image:
    """MeleeRock 슬라임을 이펙트와 같은 픽셀 크기(0.039 unit)로 줄인 그림."""
    slime = Image.open(
        REPO_ROOT / "Assets" / "ThirdParty" / "ArtResource" / "JinWook" / "Slime Resource"
        / "Melee_Rock" / "KakaoTalk_20240702_171553497_01.png"
    ).convert("RGBA")
    # 512px, PPU 100, scale 0.5 → 2.56 unit → 약 66px.
    return slime.resize((66, 66), Image.NEAREST)


def _battle_scene_panel(element: str) -> Image.Image:
    """전투 배경 위에 슬라임과 각 시트의 가운데 프레임을 실제 게임 픽셀 크기로 배치한다.

    가로 300px(약 11.7 unit), 세로 256px(10 unit). 프레임 중심을 화면 중앙(월드 y=0)에
    두고 슬라임 발을 월드 y=-3.3 에 둔다.
    """
    width, height = 300, 256
    background = Image.open(REPO_ROOT / "Assets" / "Art" / "Battle" / "Backgrounds" / "PlainBackground.png").convert("RGBA")
    panel = background.resize((int(height * 2), height), Image.NEAREST).crop((60, 0, 60 + width, height))
    feet_row = height // 2 + round(3.3 / 0.039)
    slime = _slime_at_game_scale()
    slime_x = 200
    panel.alpha_composite(slime, (slime_x - 33, feet_row - 45))

    def middle(kind):
        path = ART_ROOT / element / f"{element}{kind}.png"
        if not path.exists():
            return None
        spec = SHEET_SPECS[kind]
        index = spec["count"] // 2
        sheet = Image.open(path).convert("RGBA")
        return sheet.crop((0, index * spec["frame"], spec["frame"], (index + 1) * spec["frame"]))

    explode = middle("Explode")
    if explode is not None:
        panel.alpha_composite(explode, (slime_x - 128, height // 2 - 128))
    shoot = middle("Shoot")
    if shoot is not None:
        panel.alpha_composite(shoot, (20, feet_row - 20 - 64))
    drop = middle("Drop")
    if drop is not None:
        panel.alpha_composite(drop, (slime_x - 64, feet_row - 150 - 64))
    hit = middle("Hit")
    if hit is not None:
        panel.alpha_composite(hit, (slime_x - 64 - 40, feet_row - 22 - 64))
    return panel


def write_preview(element: str) -> Path:
    """원소의 시트 4장을 확인하는 미리보기 이미지를 만든다.

    결과: docs/design/spell-effects/previews/<원소>.png
    왼쪽: Shoot, Drop, Hit, Explode 순서의 행. 프레임을 2배 확대했고 Explode 는
    128px 칸에 맞게 절반으로 줄였다. 오른쪽: 전투 배경과 슬라임 위에 각 시트의
    가운데 프레임을 게임과 같은 픽셀 크기로 올린 장면을 2배 확대했다.
    """
    zoom, cell, gap = 2, 128, 4
    kinds = ["Shoot", "Drop", "Hit", "Explode"]
    max_count = max(SHEET_SPECS[k]["count"] for k in kinds)
    grid_width = max_count * (cell * zoom + gap)
    row_height = cell * zoom + gap
    scene = _battle_scene_panel(element)
    scene = scene.resize((scene.width * zoom, scene.height * zoom), Image.NEAREST)
    canvas = Image.new("RGBA", (grid_width + scene.width + gap, max(row_height * len(kinds), scene.height)), (34, 38, 58, 255))
    for row, kind in enumerate(kinds):
        path = ART_ROOT / element / f"{element}{kind}.png"
        if not path.exists():
            continue
        spec = SHEET_SPECS[kind]
        sheet = Image.open(path).convert("RGBA")
        for index in range(spec["count"]):
            frame = sheet.crop((0, index * spec["frame"], spec["frame"], (index + 1) * spec["frame"]))
            if spec["frame"] != cell:
                frame = frame.resize((cell, cell), Image.NEAREST)
            frame = frame.resize((cell * zoom, cell * zoom), Image.NEAREST)
            canvas.alpha_composite(frame, (index * (cell * zoom + gap), row * row_height))
    canvas.alpha_composite(scene, (grid_width + gap, 0))
    PREVIEW_ROOT.mkdir(parents=True, exist_ok=True)
    out = PREVIEW_ROOT / f"{element}.png"
    canvas.convert("RGB").save(out)
    return out
