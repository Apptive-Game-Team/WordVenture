"""1부 맵 진행 그림(WorldMapStage1~4)을 새 WorldMapDefault 에서 만든다(#116).

사용법: python3 docs/design/pixel-grid/map_stages.py <예전 WorldMapDefault.png> <예전 Stage1..4 PNG 4개>
저장소 루트에서 실행한다. 예전 그림끼리 바뀐 픽셀(깬 랜드마크를 회색으로 칠한 곳)을 새 격자 크기로 줄여
마스크로 쓰고, 새 WorldMapDefault 의 그 자리를 같은 방식으로 회색으로 바꾼다. 새 그림은 랜드마크 위치가
1~2칸 어긋날 수 있어 마스크를 1칸 넓히고, 넓힌 테두리에서는 풀·바다 색은 그대로 둔다.
회색은 맵 팔레트에 이미 있는 회색 중 밝기가 가장 가까운 것을 쓴다(64색 이하 유지).
"""
import colorsys
import sys

import numpy as np
from PIL import Image, ImageChops, ImageFilter

MAP = "Assets/Art/Map/"
old_default, *old_stages = sys.argv[1:6]
new = np.asarray(Image.open(MAP + "WorldMapDefault.png").convert("RGB"))
H, W = new.shape[:2]
hsv = np.array([[colorsys.rgb_to_hsv(*(c / 255)) for c in row] for row in new.astype(float)])
h, s, v = hsv[..., 0], hsv[..., 1], hsv[..., 2]
open_ground = ((h > 0.17) & (h < 0.42) & (s > 0.3) & (v > 0.35)) | ((h > 0.45) & (h < 0.6) & (s > 0.4))
palette = np.unique(new.reshape(-1, 3), axis=0).astype(float)
neutral = palette[(palette.max(1) - palette.min(1)) < 24]
L = new.astype(float) @ [0.299, 0.587, 0.114]
grey = neutral[np.abs(L[..., None] - neutral @ [0.299, 0.587, 0.114]).argmin(-1)]
base = Image.open(old_default).convert("RGB")
for i, path in enumerate(old_stages, 1):
    diff = ImageChops.difference(base, Image.open(path).convert("RGB")).convert("L").point(lambda x: 255 if x > 30 else 0)
    core = diff.resize((W, H), Image.BOX).point(lambda x: 255 if x > 96 else 0)
    wide = np.asarray(core.filter(ImageFilter.MaxFilter(3))) > 0
    core = np.asarray(core) > 0
    mask = core | (wide & ~open_ground)
    out = new.astype(float)
    out[mask] = grey[mask]
    Image.fromarray(out.astype(np.uint8)).save(MAP + f"WorldMapStage{i}.png")
    print(f"WorldMapStage{i}.png: {mask.sum()} px grey")
