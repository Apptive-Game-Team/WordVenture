"""적이 주문 속성에 강한지 약한지를 머리 위에 알려 주는 작은 표시 2종의 그림을 만든다.

사용법: python3 docs/design/combat-effects/build_affinity_marks.py [미리보기 PNG 경로]
저장소 루트에서 실행한다. 미리보기 경로를 주면 에셋은 건드리지 않고 비교용 그림만 만든다.

결과는 Assets/Resources/Combat/AffinityMark.png 한 장이다. 행이 표시, 열이 프레임이다.
행 순서는 AffinityMarkVfx.Mark와 같다: 약점(위쪽 겹쳐진 화살표와 반짝임), 저항(아래쪽 겹쳐진 화살표).
글자는 쓰지 않는다. 방패 모양은 ReactionBurstVfx.Burst.Guard가 쓰므로 피한다.
16x16칸에 그리고 4배로 키운다. 그림 아래 가장자리는 아래에서 2번째 칸이고 가운데 가로 기준은 x=7.5다.
AffinityMarkVfx가 이 기준으로 적 머리 위에 맞춘다. 배경이 밝아도 보이도록 1칸 어두운 테두리를 두른다.
"""
import sys
from pathlib import Path

from PIL import Image

ROOT = Path.cwd()
TARGET = ROOT / "Assets/Resources/Combat/AffinityMark.png"
CELL = 16
SCALE = 4
FRAMES = 6

OUTLINE = (44, 28, 36)
# ReactionBurst 그림의 FIRE, BOLT 색과 같은 계열이다.
FIRE = [(255, 250, 214), (255, 214, 64), (255, 140, 40), (214, 64, 24)]
BOLT = [(255, 255, 214), (255, 232, 84), (206, 162, 32)]
# 저항은 눈에 덜 띄도록 채도를 뺀 회청색이다.
DULL = [(188, 202, 218), (132, 150, 176), (86, 102, 130)]


def chevron(layer, top, arms, direction, colors):
    """두께 2칸의 겹쳐진 화살표 한 개. direction이 -1이면 위쪽(^), 1이면 아래쪽(v)이다."""
    for k in range(arms + 1):
        row = top + k if direction < 0 else top + arms - k
        for x in (7 - k, 8 + k):
            layer[(x, row)] = colors[0]
            layer[(x, row + 1)] = colors[1]


def sparkle(layer, cx, cy, size, colors):
    """한 칸 굵기의 작은 십자 반짝임. size가 2면 팔이 두 칸이다."""
    layer[(cx, cy)] = colors[0]
    for step in range(1, size + 1):
        color = colors[1] if step == 1 else colors[2]
        for dx, dy in ((step, 0), (-step, 0), (0, step), (0, -step)):
            layer[(cx + dx, cy + dy)] = color


def render(layer, thin=0):
    """칸 단위 그림을 테두리와 함께 이미지로 옮긴다. thin이 1 이상이면 격자무늬로 칸을 덜어 낸다."""
    image = Image.new("RGBA", (CELL, CELL))
    pixels = image.load()
    kept = {}
    for (x, y), color in layer.items():
        if not (0 <= x < CELL and 0 <= y < CELL):
            continue
        if thin and (x + y) % 2 == 0 and (thin > 1 or x % 2 == 0):
            continue
        kept[(x, y)] = color
    for (x, y) in kept:
        for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            nx, ny = x + dx, y + dy
            if (nx, ny) not in kept and 0 <= nx < CELL and 0 <= ny < CELL:
                pixels[nx, ny] = OUTLINE + (255,)
    for (x, y), color in kept.items():
        pixels[x, y] = color + (255,)
    return image


def weak(frame):
    """약점: 노란 위쪽 화살표 두 개가 튀어나오듯 나타나고, 둘레에서 반짝임이 깜박인 뒤 떠오르며 흐려진다."""
    layer = {}
    arms = [1, 4, 3, 3, 3, 3][frame]
    gap = [3, 5, 5, 5, 5, 5][frame]
    rise = [0, 0, 0, 0, 0, 1][frame]
    bottom = 13 - rise
    top = bottom - arms - gap - 2
    chevron(layer, top + gap, arms, -1, [FIRE[1], FIRE[2]])
    chevron(layer, top, arms, -1, [BOLT[0], BOLT[1]])
    if frame >= 1:
        # 반짝임은 테두리까지 가운데 12칸 폭(x=2~13) 안에 머물도록 한 칸짜리 점과 작은 십자로 둔다.
        sparkles = [(3, 6), (12, 8), (3, 11), (12, 12)]
        if frame in (1, 3):
            sparkles = [(12, 5), (3, 8), (12, 11), (3, 12)]
        for index, (x, y) in enumerate(sparkles):
            if frame >= 4 and index % 2:
                continue
            layer[(x, y - rise)] = BOLT[0] if frame in (1, 2) else BOLT[1]
    return render(layer, thin=[0, 0, 0, 0, 1, 2][frame])


def resisted(frame):
    """저항: 작은 회청색 아래쪽 화살표 두 개가 조용히 나타나 한 칸 가라앉으며 흐려진다."""
    layer = {}
    arms = [1, 3, 3, 3, 3, 3][frame]
    sink = [0, 0, 0, 0, 0, 1][frame]
    bottom = 13 + sink
    gap = 6
    top = bottom - arms - gap - 2
    chevron(layer, top + gap, arms, 1, [DULL[1], DULL[2]])
    chevron(layer, top, arms, 1, [DULL[0], DULL[1]])
    return render(layer, thin=[0, 0, 0, 0, 1, 2][frame])


ROWS = [weak, resisted]


def check_margins(atlas):
    """모든 칸에서 투명하지 않은 칸이 칸 가장자리에 닿으면 이웃 칸과 이어져 보이므로 실패시킨다."""
    for row in range(len(ROWS)):
        for frame in range(FRAMES):
            cell = atlas.crop((frame * CELL, row * CELL, (frame + 1) * CELL, (row + 1) * CELL))
            left, top, right, bottom = cell.getchannel("A").getbbox()
            assert left >= 1 and top >= 1 and right <= CELL - 1 and bottom <= CELL - 1, \
                f"행 {row} 프레임 {frame}: 그림이 칸 가장자리에 닿는다 {(left, top, right, bottom)}"
            assert left >= 2 and right <= CELL - 2, f"행 {row} 프레임 {frame}: 12칸 폭을 넘는다"


def build_atlas():
    atlas = Image.new("RGBA", (CELL * FRAMES, CELL * len(ROWS)))
    for row, draw in enumerate(ROWS):
        for frame in range(FRAMES):
            atlas.alpha_composite(draw(frame), (frame * CELL, row * CELL))
    check_margins(atlas)
    return atlas.resize((atlas.width * SCALE, atlas.height * SCALE), Image.NEAREST)


def preview(path):
    """하늘색, 풀색, 어두운 배경 위에 표시를 올려 본다."""
    atlas = build_atlas()
    cell = CELL * SCALE
    backgrounds = [(126, 196, 238), (92, 160, 76), (58, 68, 80)]
    sheet = Image.new("RGBA", (cell * FRAMES, cell * len(ROWS) * len(backgrounds)))
    for index, color in enumerate(backgrounds):
        panel = Image.new("RGBA", atlas.size, color + (255,))
        panel.alpha_composite(atlas)
        sheet.paste(panel, (0, index * atlas.height))
    sheet.save(path)


if __name__ == "__main__":
    if len(sys.argv) > 1:
        preview(sys.argv[1])
    else:
        build_atlas().save(TARGET)
