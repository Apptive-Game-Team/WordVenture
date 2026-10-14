"""비 오는 전투 배경 5프레임(JinWook/Rainy)을 새 바탕 그림 위에 빗줄기를 그려 만든다(#116).

사용법: python3 docs/design/pixel-grid/rain_frames.py <비 없는 바탕 552x276 PNG> <예전 프레임 5개(RainyBackground_01~04.png, _05.gif)>
저장소 루트에서 실행한다. 예전 프레임은 256x128 격자를 4배로 키운 그림이다. 5프레임의 칸마다 가장 흔한 색을
비 없는 바탕으로 보고, 바탕과 다른 칸을 빗줄기로 본다. 빗줄기 칸은 새 격자(552x276)에서 가로 1칸, 세로는
같은 비율로 늘려 그리고, 색은 예전 빗줄기 색을 그대로 쓴다(새 바탕 팔레트에는 빗줄기의 어두운 보라가 없다). 결과는 예전 파일 자리에 쓴다.
"""
import sys
from pathlib import Path

import numpy as np
from PIL import Image

sys.path.insert(0, ".agents/skills/pixel-art/scripts")
from resample_set import save  # noqa: E402

base_path, *frame_paths = sys.argv[1:]
base = np.asarray(Image.open(base_path).convert("RGB")).astype(int)
H, W = base.shape[:2]


def grid(path):
    with Image.open(path) as im:
        im.seek(0)
        return np.asarray(im.convert("RGB"))[::4, ::4].astype(int)  # 256x128 cells


cells = np.stack([grid(p) for p in frame_paths])
keys = cells[..., 0] * 65536 + cells[..., 1] * 256 + cells[..., 2]
# 칸마다 5프레임 중 가장 많은 프레임이 같은 값을 바탕으로 본다.
votes = (keys[:, None] == keys[None, :]).sum(1)
background = np.take_along_axis(keys, votes.argmax(0)[None], 0)[0]
gh, gw = background.shape
for path, key, cell in zip(frame_paths, keys, cells):
    out = base.copy()
    for y, x in zip(*np.nonzero(key != background)):
        colour = cell[y, x]
        nx = round((x + 0.5) * W / gw - 0.5)
        out[round(y * H / gh):round((y + 1) * H / gh), nx] = colour
    save(Image.fromarray(out.astype(np.uint8)).convert("RGBA"), Path(path))
    print(path, int((key != background).sum()), "rain cells")
