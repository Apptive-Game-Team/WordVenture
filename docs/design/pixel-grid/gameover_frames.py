"""게임오버 애니메이션 1·2프레임을 눈물이 다 흐른 3프레임에서 만든다(#116).

사용법: python3 docs/design/pixel-grid/gameover_frames.py
저장소 루트에서 실행한다. GameOverFrame3.png(480x240)는 Codex 로 그린 그림이고, 눈물이 뺨 아래까지 흐른다.
예전 애니메이션처럼 눈물이 점점 길어지도록, 1프레임은 눈물 줄기마다 위쪽 1/3, 2프레임은 2/3만 남긴다.
지운 눈물 픽셀은 같은 줄에서 가장 가까운 눈물이 아닌 픽셀 색으로 채운다.
"""
import numpy as np
from PIL import Image

DIR = "Assets/Art/GameOver/"
FACE = (150, 60, 280, 150)  # 얼굴 영역(x0, y0, x1, y1). 아래쪽 파란 옷과 지팡이 구슬은 빠진다.

full = np.asarray(Image.open(DIR + "GameOverFrame3.png").convert("RGB")).astype(int)
r, g, b = full[..., 0], full[..., 1], full[..., 2]
tear = (b > r + 40) & (b > g)
mask = np.zeros_like(tear)
x0, y0, x1, y1 = FACE
mask[y0:y1, x0:x1] = True
tear &= mask


def components(m):
    lab, n = np.zeros(m.shape, int), 0
    for y, x in zip(*np.nonzero(m)):
        if lab[y, x]:
            continue
        n += 1
        lab[y, x], stack = n, [(y, x)]
        while stack:
            cy, cx = stack.pop()
            for ny in (cy - 1, cy, cy + 1):
                for nx in (cx - 1, cx, cx + 1):
                    if m[ny, nx] and not lab[ny, nx]:
                        lab[ny, nx] = n
                        stack.append((ny, nx))
    return lab, n


lab, n = components(tear)
for frame, frac in ((1, 1 / 3), (2, 2 / 3)):
    out = full.copy()
    removed = 0
    for k in range(1, n + 1):
        ys, xs = np.nonzero(lab == k)
        cut = ys.min() + max(1, round((ys.max() - ys.min() + 1) * frac))
        for y, x in zip(ys, xs):
            if y < cut:
                continue
            row = np.nonzero(~tear[y])[0]
            out[y, x] = full[y, row[np.abs(row - x).argmin()]]
            removed += 1
    Image.fromarray(out.astype(np.uint8)).save(DIR + f"GameOverFrame{frame}.png", optimize=True)
    print(f"GameOverFrame{frame}.png: {n} tear streams, {removed} px removed")
