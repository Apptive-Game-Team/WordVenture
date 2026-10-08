"""이미지 생성으로 만든 원소 atlas 를 게임용 스프라이트 시트 4장으로 바꾼다.

입력 (모두 1536x1024, 분홍(#FF00FF) 배경, 4x4 px 덩어리 하나가 픽셀 하나):
  generated/<원소>.png         256x256 칸 6열 x 4행.
                               1행 Shoot(1~4열), 2행 Drop(1~4열), 3행 Hit(1~6열).
                               4행은 작게 그린 첫 Explode 라서 쓰지 않는다.
  generated/<원소>Explode.png  512x512 칸 3열 x 2행. 왼쪽 위부터 Explode 6프레임.

처리:
1. 4x4 덩어리마다 가운데 2x2 의 중앙값 색을 골라 칸을 픽셀 그림으로 줄인다.
   256 칸은 64x64, 512 칸은 128x128 이 된다.
   한 픽셀이 게임에서 0.039 unit 으로, 슬라임 스프라이트의 픽셀과 크기가 같다.
2. 분홍 배경과 분홍이 섞인 가장자리 픽셀을 투명으로 바꾼다.
3. 원소마다 색을 최대 MAX_COLORS 개로 줄인다.
4. 행 전체 프레임을 한 덩어리로 보고 기준점을 맞춰 시트 프레임에 놓는다.
   Shoot: 오른쪽 끝(머리)을 프레임 중심 오른쪽 HEAD_OFFSET 에.
   Drop: 아래쪽 끝(머리)을 프레임 중심 아래 HEAD_OFFSET 에.
   Hit: 가운데를 프레임 중심에.
   Explode: 아래쪽 끝을 지면 행(common.EXPLODE_GROUND_ROW)에.
   같은 행의 프레임은 같은 만큼 옮기므로 생성 이미지 안에서의 움직임이 그대로 남는다.
"""
from __future__ import annotations

import sys
from pathlib import Path

import numpy as np
from PIL import Image

sys.path.insert(0, str(Path(__file__).parent))
import common  # noqa: E402

GENERATED_ROOT = Path(__file__).resolve().parent / "generated"
BLOCK = 4
# 덩어리 경계가 4의 배수에 있다고 보고 각 덩어리의 가운데 2x2(오프셋 1, 2)를 읽는다.
GRID_PHASE = 0
MAX_COLORS = 24
HEAD_OFFSET = 10
# 종류 → (atlas 파일 이름 뒤에 붙는 말, 칸 크기, 프레임 순서의 (행, 열))
SOURCES = {
    "Shoot": ("", 256, [(0, col) for col in range(4)]),
    "Drop": ("", 256, [(1, col) for col in range(4)]),
    "Hit": ("", 256, [(2, col) for col in range(6)]),
    "Explode": ("Explode", 512, [(row, col) for row in range(2) for col in range(3)]),
}


def is_background(rgb: np.ndarray) -> np.ndarray:
    """분홍 배경과 분홍이 섞인 가장자리. 빨강·파랑이 초록보다 훨씬 크면 배경으로 본다."""
    r, g, b = rgb[..., 0].astype(int), rgb[..., 1].astype(int), rgb[..., 2].astype(int)
    return (b - g > 60) & (r - g > 60) & (b > 120)


def downsample(cell: np.ndarray) -> Image.Image:
    """칸 하나를 BLOCK 분의 1 크기 픽셀 그림으로 줄인다."""
    size = cell.shape[0]
    logical = size // BLOCK
    out = np.zeros((logical, logical, 4), dtype=np.uint8)
    for y in range(logical):
        for x in range(logical):
            y0 = min(y * BLOCK + 1 + GRID_PHASE, size - 2)
            x0 = min(x * BLOCK + 1 + GRID_PHASE, size - 2)
            center = cell[y0:y0 + 2, x0:x0 + 2].reshape(-1, 3)
            keep = center[~is_background(center)]
            if len(keep) < 3:
                continue
            out[y, x, :3] = np.median(keep, axis=0)
            out[y, x, 3] = 255
    return Image.fromarray(out, "RGBA")


def reduce_colors(frames: list[Image.Image]) -> list[Image.Image]:
    """같은 원소의 모든 프레임이 같은 색 MAX_COLORS 개를 쓰도록 줄인다."""
    pixels = np.concatenate([
        np.asarray(f.convert("RGB")).reshape(-1, 3)[np.asarray(f)[..., 3].ravel() > 0] for f in frames
    ])
    sample = Image.fromarray(pixels.reshape(1, -1, 3).astype(np.uint8), "RGB")
    palette_image = sample.quantize(colors=MAX_COLORS, method=Image.Quantize.MEDIANCUT)
    result = []
    for frame in frames:
        rgb = frame.convert("RGB").quantize(palette=palette_image, dither=Image.Dither.NONE).convert("RGB")
        rgba = rgb.convert("RGBA")
        rgba.putalpha(frame.getchannel("A"))
        result.append(rgba)
    return result


def union_bbox(frames: list[Image.Image]) -> tuple[int, int, int, int]:
    boxes = [f.getchannel("A").getbbox() for f in frames]
    boxes = [b for b in boxes if b]
    return (min(b[0] for b in boxes), min(b[1] for b in boxes), max(b[2] for b in boxes), max(b[3] for b in boxes))


def anchor_offset(kind: str, box: tuple[int, int, int, int], size: int) -> tuple[int, int]:
    """칸 좌표를 시트 프레임 좌표로 옮기는 거리."""
    left, top, right, bottom = box
    center = size // 2
    middle_x, middle_y = (left + right) // 2, (top + bottom) // 2
    if kind == "Shoot":
        return center + HEAD_OFFSET - right, center - middle_y
    if kind == "Drop":
        return center - middle_x, center + HEAD_OFFSET - bottom
    if kind == "Hit":
        return center - middle_x, center - middle_y
    return center - middle_x, common.EXPLODE_GROUND_ROW - bottom


def read_frames(element: str, kind: str) -> list[Image.Image]:
    suffix, cell, order = SOURCES[kind]
    atlas = np.asarray(Image.open(GENERATED_ROOT / f"{element}{suffix}.png").convert("RGB"))
    return [downsample(atlas[row * cell:(row + 1) * cell, col * cell:(col + 1) * cell]) for row, col in order]


def build_element(element: str) -> None:
    cells = {kind: read_frames(element, kind) for kind in SOURCES}
    ordered = [frame for kind in SOURCES for frame in cells[kind]]
    reduced = iter(reduce_colors(ordered))
    for kind in SOURCES:
        frames = [next(reduced) for _ in cells[kind]]
        size = common.SHEET_SPECS[kind]["frame"]
        dx, dy = anchor_offset(kind, union_bbox(frames), size)
        placed = []
        for frame in frames:
            canvas = common.new_frame(size)
            canvas.paste(frame, (dx, dy), frame)
            placed.append(canvas)
        print(common.save_sheet(placed, element, kind))
    print(common.write_preview(element))


def main() -> None:
    elements = sys.argv[1:] or ["Fire", "Ice", "Rock", "Lightning", "Holy"]
    for element in elements:
        build_element(element)


if __name__ == "__main__":
    main()
