"""마법 이펙트 시트의 공용 규칙: 프레임 크기, 시트 저장, 미리보기.

픽셀 크기: 슬라임 스프라이트의 한 픽셀은 화면에서 약 0.039 unit 이다.
Shoot·Drop·Hit 프레임은 128px 를 PPU 128 로 가져와 prefab scale 5 에서 5 unit,
Explode 프레임은 256px 를 PPU 256 으로 가져와 prefab scale 10 에서 10 unit 이 된다.
두 경우 모두 한 픽셀이 0.039 unit 이라 슬라임과 픽셀 크기가 같다.
"""
from __future__ import annotations

from pathlib import Path

from PIL import Image

REPO_ROOT = Path(__file__).resolve().parents[3]
ART_ROOT = REPO_ROOT / "Assets" / "Art" / "Combat" / "Spells"
PREVIEW_ROOT = Path(__file__).resolve().parent / "previews"

# 시트 종류별 프레임 크기와 프레임 수. build_unity_assets.py 도 이 값을 읽는다.
SHEET_SPECS = {
    # 오른쪽(+x)으로 날아가는 발사체. 반복 재생.
    "Shoot": {"frame": 128, "count": 4, "fps": 12, "loop": True},
    # 아래(-y)로 떨어지는 발사체. 반복 재생.
    "Drop": {"frame": 128, "count": 4, "fps": 12, "loop": True},
    # Shoot·Drop 이 적에게 닿은 자리에서 터지는 모양. 0.5 초 뒤 오브젝트가 사라진다.
    "Hit": {"frame": 128, "count": 6, "fps": 12, "loop": False},
    # 적 발밑에서 솟는 광역 마법. 0.5 초 뒤 오브젝트가 사라진다.
    "Explode": {"frame": 256, "count": 6, "fps": 12, "loop": False},
}

# Explode 프레임에서 지면이 놓이는 행. 프레임 중심이 월드 y=0 에 놓이고 적은
# y≈-3.3 에 서 있으므로, 위에서 218 행(월드 y≈-3.5)을 지면으로 쓴다.
EXPLODE_GROUND_ROW = 218

# 미리보기용 전투 화면: 1280x720, orthographic size 5 라서 1 unit 이 72px 이다.
COMBAT_SCREENSHOT = REPO_ROOT / "docs" / "images" / "combat.jpg"
SCREEN_PIXELS_PER_UNIT = 72
SPRITE_UNITS = {"Shoot": 5, "Drop": 5, "Hit": 5, "Explode": 10}


def new_frame(size: int) -> Image.Image:
    return Image.new("RGBA", (size, size), (0, 0, 0, 0))


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


def sheet_frame(element: str, kind: str, index: int) -> Image.Image:
    size = SHEET_SPECS[kind]["frame"]
    sheet = Image.open(ART_ROOT / element / f"{element}{kind}.png").convert("RGBA")
    return sheet.crop((0, index * size, size, (index + 1) * size))


def _combat_scene(element: str) -> Image.Image:
    """전투 스크린샷 위에 각 시트의 가운데 프레임을 게임과 같은 크기로 올린다.

    Shoot 은 플레이어 앞, Hit 은 가운데 지면, Drop 은 공중, Explode 는 슬라임 위치(x=7)다.
    """
    scene = Image.open(COMBAT_SCREENSHOT).convert("RGBA")
    placements = {"Shoot": (-3.0, -3.0), "Hit": (1.5, -2.9), "Drop": (4.5, -0.5), "Explode": (6.95, 0.0)}
    for kind, (world_x, world_y) in placements.items():
        frame = sheet_frame(element, kind, SHEET_SPECS[kind]["count"] // 2)
        size = SPRITE_UNITS[kind] * SCREEN_PIXELS_PER_UNIT
        frame = frame.resize((size, size), Image.NEAREST)
        left = round(scene.width / 2 + world_x * SCREEN_PIXELS_PER_UNIT - size / 2)
        top = round(scene.height / 2 - world_y * SCREEN_PIXELS_PER_UNIT - size / 2)
        scene.alpha_composite(frame, (left, top))
    return scene


def write_preview(element: str) -> Path:
    """원소의 시트 4장을 확인하는 미리보기 이미지를 만든다.

    결과: docs/design/spell-effects/previews/<원소>.png
    위: 전투 화면에 가운데 프레임을 게임과 같은 크기로 올린 장면.
    아래: Shoot, Drop, Hit, Explode 순서의 행. 프레임을 2배 확대했고 Explode 는
    128px 칸에 맞게 절반으로 줄였다.
    """
    zoom, cell, gap = 2, 128, 4
    kinds = ["Shoot", "Drop", "Hit", "Explode"]
    max_count = max(SHEET_SPECS[k]["count"] for k in kinds)
    grid_width = max_count * (cell * zoom + gap)
    row_height = cell * zoom + gap
    scene = _combat_scene(element)
    width = max(grid_width, scene.width)
    canvas = Image.new("RGBA", (width, scene.height + row_height * len(kinds)), (34, 38, 58, 255))
    canvas.alpha_composite(scene, (0, 0))
    for row, kind in enumerate(kinds):
        spec = SHEET_SPECS[kind]
        for index in range(spec["count"]):
            frame = sheet_frame(element, kind, index)
            if spec["frame"] != cell:
                frame = frame.resize((cell, cell), Image.NEAREST)
            frame = frame.resize((cell * zoom, cell * zoom), Image.NEAREST)
            canvas.alpha_composite(frame, (index * (cell * zoom + gap), scene.height + row * row_height))
    PREVIEW_ROOT.mkdir(parents=True, exist_ok=True)
    out = PREVIEW_ROOT / f"{element}.png"
    canvas.convert("RGB").save(out)
    return out
