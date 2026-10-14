"""게임오버 애니메이션 2·3프레임을 새 1프레임에서 만든다(#116).

사용법: python3 docs/design/pixel-grid/gameover_frames.py <예전 Frame1> <예전 Frame2> <예전 Frame3>
저장소 루트에서 실행한다. 예전 프레임끼리는 눈물 부분만 다르다. 새 1프레임(480x240)에 예전 프레임의 눈물 픽셀만
옮겨 2·3프레임을 만든다. 새 그림은 캐릭터 위치가 1~2칸 다를 수 있어, 얼굴 부분을 맞춰 보며 어긋난 만큼 옮긴다.
색은 새 1프레임 팔레트에서 가장 가까운 색을 쓴다.
"""
import sys

import numpy as np
from PIL import Image, ImageChops

DIR = "Assets/Art/GameOver/"
old = [Image.open(p).convert("RGB") for p in sys.argv[1:4]]
new1 = np.asarray(Image.open(DIR + "GameOverFrame1.png").convert("RGB")).astype(int)
H, W = new1.shape[:2]
sx, sy = W / old[0].width, H / old[0].height
small = [np.asarray(o.resize((W, H), Image.NEAREST)).astype(int) for o in old]

# 눈물이 바뀌는 곳 주변(예전 좌표 400~520, 220~330)으로 새 그림과 예전 그림이 가장 잘 맞는 어긋남을 찾는다.
x0, x1, y0, y1 = int(380 * sx), int(540 * sx), int(200 * sy), int(340 * sy)
best = min(((np.abs(np.roll(np.roll(small[0], dy, 0), dx, 1)[y0:y1, x0:x1] - new1[y0:y1, x0:x1]).sum(), dx, dy)
            for dx in range(-4, 5) for dy in range(-4, 5)))
_, dx, dy = best
palette = np.unique(new1.reshape(-1, 3), axis=0)
for n in (2, 3):
    diff = np.asarray(ImageChops.difference(old[0], old[n - 1]).convert("L").resize((W, H), Image.BOX)) > 8
    diff = np.roll(np.roll(diff, dy, 0), dx, 1)
    src = np.roll(np.roll(small[n - 1], dy, 0), dx, 1)
    out = new1.copy()
    near = palette[((src[diff][:, None, :] - palette[None]) ** 2).sum(-1).argmin(1)]
    out[diff] = near
    Image.fromarray(out.astype(np.uint8)).save(DIR + f"GameOverFrame{n}.png")
    print(f"GameOverFrame{n}.png: offset ({dx},{dy}), {diff.sum()} px from the old frame")
