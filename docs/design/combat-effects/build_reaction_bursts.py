"""적 머리 위 반응 글자를 대신하는 한 번 재생 효과 6종의 그림을 만든다.

사용법: python3 docs/design/combat-effects/build_reaction_bursts.py [미리보기 PNG 경로]
저장소 루트에서 실행한다. 미리보기 경로를 주면 에셋은 건드리지 않고 비교용 그림만 만든다.

결과는 Assets/Resources/Combat/ReactionBurst.png 한 장이다. 행이 효과, 열이 프레임이다.
행 순서는 ReactionBurstVfx.Row와 같다: 불꽃 방전, 쇄빙, 신경 마비, 용암 균열, 방패 보호, 회복.
상태 효과 그림(ElementalStatusOverlay.png)처럼 큰 픽셀 덩어리로 그리도록 48x40칸에 그리고 4배로 키운다.
몸은 가운데 28칸 폭이고 바닥선은 아래에서 6번째 칸이다. ReactionBurstVfx가 이 기준으로 몸에 맞춘다.
"""
import math
import random
import sys
from pathlib import Path

from PIL import Image

ROOT = Path.cwd()
TARGET = ROOT / "Assets/Resources/Combat/ReactionBurst.png"
WIDTH, HEIGHT = 48, 40
SCALE = 4
FRAMES = 6
CENTER_X, BOTTOM_Y = 24, 34
BODY_HALF_WIDTH, BODY_HEIGHT = 14, 16
CENTER_Y = BOTTOM_Y - BODY_HEIGHT // 2

FIRE = [(255, 250, 214), (255, 214, 64), (255, 140, 40), (214, 64, 24)]
ICE = [(244, 252, 255), (168, 232, 250), (86, 184, 232), (44, 112, 172)]
BOLT = [(255, 255, 214), (255, 232, 84), (206, 162, 32)]
LAVA = [(255, 232, 128), (255, 140, 40), (204, 62, 22), (112, 42, 22)]
SHIELD = [(232, 246, 255), (150, 206, 255), (74, 134, 224)]
HEAL = [(236, 255, 236), (136, 240, 146), (58, 178, 82)]


class Canvas:
    def __init__(self):
        self.image = Image.new("RGBA", (WIDTH, HEIGHT))
        self.pixels = self.image.load()

    def dot(self, x, y, color, size=1):
        x, y = round(x), round(y)
        for py in range(y, y + size):
            for px in range(x, x + size):
                if 0 <= px < WIDTH and 0 <= py < HEIGHT:
                    self.pixels[px, py] = color + (255,)

    def line(self, x0, y0, x1, y1, color, size=2):
        steps = max(abs(round(x1) - round(x0)), abs(round(y1) - round(y0)), 1)
        for step in range(steps + 1):
            t = step / steps
            self.dot(x0 + (x1 - x0) * t, y0 + (y1 - y0) * t, color, size)

    def diamond(self, cx, cy, radius, color, shine):
        for y in range(-radius, radius + 1):
            for x in range(-radius, radius + 1):
                if abs(x) + abs(y) <= radius:
                    self.dot(cx + x, cy + y, shine if x + y < -radius // 2 else color)

    def disc(self, cx, cy, radius, color):
        for y in range(math.floor(cy - radius), math.ceil(cy + radius) + 1):
            for x in range(math.floor(cx - radius), math.ceil(cx + radius) + 1):
                if (x - cx) ** 2 + (y - cy) ** 2 <= radius * radius:
                    self.dot(x, y, color)

    def plus(self, cx, cy, arm, color, core, size=2):
        """굵기 size의 십자. size가 2면 2칸 굵기다."""
        for offset in range(-arm, arm + 1):
            self.dot(cx + offset, cy, color, size)
            self.dot(cx, cy + offset, color, size)
        self.dot(cx, cy, core, size)

    def star(self, cx, cy, color, core):
        self.plus(cx, cy, 2, color, core)
        for dx, dy in ((-1, -1), (2, -1), (-1, 2), (2, 2)):
            self.dot(cx + dx, cy + dy, color)

    def bolt(self, x0, y0, angle, length, color, rng):
        """angle 방향으로 꺾이며 뻗는 번개 한 줄기."""
        x, y = x0, y0
        for _ in range(max(1, length // 3)):
            turn = angle + rng.uniform(-0.9, 0.9)
            nx, ny = x + math.cos(turn) * 3, y + math.sin(turn) * 3
            self.line(x, y, nx, ny, color)
            x, y = nx, ny


def ring_points(radius, count, rng, jitter=0.0):
    for index in range(count):
        angle = 2 * math.pi * index / count + rng.uniform(-jitter, jitter)
        yield CENTER_X + math.cos(angle) * radius, CENTER_Y + math.sin(angle) * radius * 0.8, angle


def overload(frame, rng):
    """불꽃 방전: 화상이 번개에 터진다. 가운데가 번쩍인 뒤 불꽃 고리와 번개가 퍼진다."""
    canvas = Canvas()
    if frame <= 2:
        canvas.disc(CENTER_X, CENTER_Y, [4, 6, 4][frame], FIRE[2])
        canvas.disc(CENTER_X, CENTER_Y, [3, 4, 2][frame], FIRE[1])
        canvas.disc(CENTER_X, CENTER_Y, [2, 2, 1][frame], FIRE[0])
    if frame <= 3:
        for _, _, angle in ring_points(1, 6, rng, 0.3):
            canvas.bolt(CENTER_X, CENTER_Y, angle, [6, 12, 15, 9][frame], BOLT[frame % 2], rng)
    if frame >= 1:
        radius = [0, 8, 12, 15, 17, 18][frame]
        color = FIRE[min(3, frame - 1)]
        for x, y, _ in ring_points(radius, 26, rng, 0.08):
            if rng.random() < [1, 0.95, 0.85, 0.6, 0.4, 0.2][frame]:
                canvas.dot(x, y, color, 2)
                canvas.dot(x, y - 1, FIRE[min(3, frame)])
    return canvas.image


def shatter(frame, rng):
    """쇄빙: 언 몸이 바위에 깨진다. 금이 간 뒤 얼음 조각이 사방으로 튄다."""
    canvas = Canvas()
    if frame <= 1:
        for _, _, angle in ring_points(1, 7, rng, 0.25):
            length = 7 + frame * 5
            canvas.line(CENTER_X, CENTER_Y, CENTER_X + math.cos(angle) * length,
                        CENTER_Y + math.sin(angle) * length * 0.8, ICE[frame])
        canvas.disc(CENTER_X, CENTER_Y, 2 - frame, ICE[0])
    if frame >= 1:
        for index in range(10):
            angle = 2 * math.pi * index / 10 + 0.3
            distance = 5 + (frame - 1) * 4.5
            x = CENTER_X + math.cos(angle) * distance
            y = CENTER_Y + math.sin(angle) * distance * 0.7 + 0.5 * (frame - 1) ** 2
            color = ICE[min(3, frame)]
            canvas.diamond(x, y, 2 if frame < 4 else 1, color, ICE[0] if frame < 4 else color)
    return canvas.image


def paralysis(frame, rng):
    """신경 마비: 감전된 몸이 얼음에 굳는다. 몸 둘레에 번개가 튀고 머리 위로 별이 돈다."""
    canvas = Canvas()
    if frame % 2 == 0 or frame < 3:
        for index in range(3):
            angle = math.pi + rng.uniform(0.15, math.pi - 0.15)
            x = CENTER_X + math.cos(angle) * BODY_HALF_WIDTH
            y = BOTTOM_Y + math.sin(angle) * BODY_HEIGHT
            canvas.bolt(x, y, angle + rng.uniform(-0.4, 0.4), 9, BOLT[(frame + index) % 2], rng)
            canvas.dot(x, y, ICE[1], 2)
    for star in range(3):
        angle = frame * math.pi / 3 + star * 2 * math.pi / 3
        x = CENTER_X + math.cos(angle) * 10
        y = BOTTOM_Y - BODY_HEIGHT - 5 + math.sin(angle) * 2
        front = math.sin(angle) > -0.2
        canvas.star(x, y, BOLT[1] if front else BOLT[2], BOLT[0])
    return canvas.image


def lava_crack(frame, rng):
    """용암 균열: 균열에 불이 붙는다. 바닥과 몸에 빛나는 금이 퍼지고 용암이 솟는다."""
    canvas = Canvas()
    glow = [LAVA[1], LAVA[0], LAVA[0], LAVA[1], LAVA[2], LAVA[3]][frame]
    reach = [6, 11, 15, 16, 16, 14][frame]
    branches = [(-1, 0.0), (1, 0.0), (-1, -0.5), (1, -0.6), (0, -1.0)]
    for side, rise in branches:
        x, y = CENTER_X, BOTTOM_Y
        for step in range(reach // 2):
            nx = x + (side if side else rng.choice((-1, 1))) * 2
            ny = y + rise * 2 + (rng.choice((0, 1)) if rise == 0 else 0)
            ny = min(BOTTOM_Y, ny)
            canvas.line(x, y, nx, ny, glow)
            x, y = nx, ny
    if 1 <= frame <= 4:
        for index in range(5):
            x = CENTER_X - 10 + index * 5
            height = [0, 5, 10, 12, 9][frame] - abs(index - 2) * 2
            if height <= 0:
                continue
            top = BOTTOM_Y - 1 - height
            canvas.line(x, BOTTOM_Y - 1, x, top + 2, LAVA[2])
            canvas.disc(x, top, 1, LAVA[1] if frame < 4 else LAVA[2])
            canvas.dot(x, top, LAVA[0])
    return canvas.image


def guard(frame, rng):
    """방패 보호: 앞의 방패 슬라임이 주문을 막는다. 몸 앞에 방패 모양 빛이 번쩍인다."""
    canvas = Canvas()
    if frame == 0:
        canvas.disc(CENTER_X + BODY_HALF_WIDTH + 3, CENTER_Y + 2, 2, SHIELD[0])
        return canvas.image
    color = [None, SHIELD[0], SHIELD[1], SHIELD[1], SHIELD[2], SHIELD[2]][frame]
    keep = [1, 1, 1, 0.9, 0.6, 0.3][frame]
    for row in range(-11, 12):
        y = CENTER_Y + 2 + row
        bulge = math.sqrt(max(0.0, 1 - (row / 12) ** 2)) * 4
        x = CENTER_X + BODY_HALF_WIDTH + 1 + bulge
        if y > BOTTOM_Y + 1 or rng.random() > keep:
            continue
        canvas.dot(x, y, SHIELD[2])
        canvas.dot(x + 1, y, color, 2)
        canvas.dot(x + 3, y, SHIELD[2] if frame >= 3 else color)
        if row % 4 == 0 and frame <= 3:
            canvas.line(x - 2, y, x, y, SHIELD[0], 1)
    if frame <= 2:
        canvas.disc(CENTER_X + BODY_HALF_WIDTH + 4, CENTER_Y + 2, 2 - frame + 1, SHIELD[0])
    return canvas.image


def heal(frame, rng):
    """회복: 치유 슬라임이 다친 적을 고친다. 초록 십자가 몸에서 떠오른다."""
    canvas = Canvas()
    starts = [(-9, 0), (-3, 2), (4, 1), (10, 3), (0, 4)]
    for index, (dx, delay) in enumerate(starts):
        age = frame - delay // 2
        if age < 0 or age > 4:
            continue
        x = CENTER_X + dx
        y = BOTTOM_Y - 3 - age * 4 - index % 2
        color = HEAL[1] if age < 3 else HEAL[2]
        canvas.plus(x, y, 2 if age < 3 else 1, color, HEAL[0], 2 if age < 3 else 1)
    if frame <= 1:
        for x, y, _ in ring_points(BODY_HALF_WIDTH + 1 + frame * 2, 18, rng):
            if y < BOTTOM_Y:
                canvas.dot(x, y, HEAL[frame + 1], 2)
    return canvas.image


ROWS = [overload, shatter, paralysis, lava_crack, guard, heal]


def build_atlas():
    atlas = Image.new("RGBA", (WIDTH * FRAMES, HEIGHT * len(ROWS)))
    for row, draw in enumerate(ROWS):
        for frame in range(FRAMES):
            # 프레임마다 같은 씨앗을 쓰지 않으면 다시 만들 때마다 그림이 바뀐다.
            atlas.alpha_composite(draw(frame, random.Random(row * 100 + frame)), (frame * WIDTH, row * HEIGHT))
    return atlas.resize((atlas.width * SCALE, atlas.height * SCALE), Image.NEAREST)


def preview(path):
    """기준 슬라임 첫 프레임 위에 효과를 겹쳐 본다."""
    slime = Image.open(ROOT / "Assets/ThirdParty/ArtResource/JinWook/Slime Resource/Ranged_Purple/"
                       "KakaoTalk_20240702_171522097_01.png").convert("RGBA")
    body = slime.crop(slime.getbbox())
    cell_w, cell_h = WIDTH * SCALE, HEIGHT * SCALE
    body = body.resize((BODY_HALF_WIDTH * 2 * SCALE, round(body.height * BODY_HALF_WIDTH * 2 * SCALE / body.width)),
                       Image.NEAREST)
    atlas = build_atlas()
    sheet = Image.new("RGBA", (cell_w * FRAMES, cell_h * len(ROWS)), (58, 68, 80, 255))
    for row in range(len(ROWS)):
        for frame in range(FRAMES):
            x, y = frame * cell_w, row * cell_h
            sheet.alpha_composite(body, (x + (CENTER_X - BODY_HALF_WIDTH) * SCALE, y + (BOTTOM_Y + 1) * SCALE - body.height))
            sheet.alpha_composite(atlas.crop((x, y, x + cell_w, y + cell_h)), (x, y))
    sheet.save(path)


if __name__ == "__main__":
    if len(sys.argv) > 1:
        preview(sys.argv[1])
    else:
        build_atlas().save(TARGET)
