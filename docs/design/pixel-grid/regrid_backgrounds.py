"""격자가 없는 배경 그림을 기존 픽셀 그림과 같은 격자로 다시 그린다.

사용법: python3 docs/design/pixel-grid/regrid_backgrounds.py
저장소 루트에서 실행한다. 그림 파일을 덮어쓰고, 크기가 바뀌는 그림은 .meta 의
spritePixelsToUnits 를 고쳐서 화면에 그려지는 크기를 유지한다.
"""
import re
from collections import Counter
from pathlib import Path

from PIL import Image

ROOT = Path.cwd()

# (경로, 격자 해상도, 확대 배율, 색 수)
# - 전투 배경은 원본(고원, 해안, 빗길)과 같이 256×128 격자를 4배로 키운다.
# - 맵은 1부 맵(Stage1BG)과 같이 384×256 격자를 4배로 키운다.
# - 화면을 채우는 스토리·타이틀·엔딩 배경은 화면 높이에 격자 126칸이 오게 한다.
#   전투 배경이 화면 높이에 128칸 남짓 보이는 것과 픽셀 크기를 맞춘다.
TARGETS = [
    ("Assets/Art/Tutorial/PlainBackground.png", (256, 128), 4, 32),
    ("Assets/Art/Map/ActTwoMap.png", (384, 256), 4, 64),
    ("Assets/Art/Story/MountainHome.png", (224, 126), 8, 64),
    ("Assets/Art/Story/MountainHomeAttack.png", (224, 126), 8, 64),
    ("Assets/Art/Story/MountainHomeMemorial.png", (224, 126), 8, 64),
    ("Assets/Art/Story/MountainHomeRuins.png", (224, 126), 8, 64),
    ("Assets/Art/Story/WorldTreeEpilogue.png", (224, 126), 8, 64),
    ("Assets/Art/Title/MountainValley.png", (224, 126), 8, 64),
]


def regrid(path: Path, grid: tuple, scale: int, colors: int) -> None:
    source = Image.open(path).convert("RGB")
    old_width = source.width
    # 평균색으로 줄이면 윤곽선과 작은 강조색이 주변색에 섞여 탁해진다. 원본 크기에서 먼저 색을 줄이고,
    # 격자 한 칸마다 가장 많이 쓰인 색을 고른다.
    indexed = source.quantize(colors=colors, method=Image.Quantize.FASTOCTREE, dither=Image.Dither.NONE)
    palette = indexed.getpalette()
    pixels = indexed.load()
    small = Image.new("RGB", grid)
    for gy in range(grid[1]):
        y0, y1 = gy * source.height // grid[1], (gy + 1) * source.height // grid[1]
        for gx in range(grid[0]):
            x0, x1 = gx * source.width // grid[0], (gx + 1) * source.width // grid[0]
            counts = Counter(pixels[x, y] for y in range(y0, y1) for x in range(x0, x1))
            index = counts.most_common(1)[0][0]
            small.putpixel((gx, gy), tuple(palette[index * 3:index * 3 + 3]))
    result = small.resize((grid[0] * scale, grid[1] * scale), Image.NEAREST)
    result.save(path)

    if result.width != old_width:
        meta = path.with_name(path.name + ".meta")
        text = meta.read_text(encoding="utf-8")
        old_ppu = float(re.search(r"spritePixelsToUnits: ([\d.]+)", text).group(1))
        new_ppu = old_ppu * result.width / old_width
        text = re.sub(r"spritePixelsToUnits: [\d.]+", "spritePixelsToUnits: %.8g" % new_ppu, text)
        meta.write_text(text, encoding="utf-8")


def main() -> None:
    for relative, grid, scale, colors in TARGETS:
        regrid(ROOT / relative, grid, scale, colors)
        print(relative, grid, "x%d" % scale)


if __name__ == "__main__":
    main()
