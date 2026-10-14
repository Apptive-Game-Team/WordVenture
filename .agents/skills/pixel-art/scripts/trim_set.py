#!/usr/bin/env python3
"""Cut the transparent margin off a set of single-sprite frames without moving them on screen.

All frames of one animation are cropped to the union of their visible boxes, so they
keep the same size and the same pivot, and the pivot is moved by the crop offset so
every visible pixel stays at the same world position.

Usage (from the repository root):
    trim_set.py FRAME_01.png FRAME_02.png ...

Only for sprites drawn by a SpriteRenderer whose size no code reads. Do not trim:
UI Image sprites (preserveAspect or a rect sized to the sprite changes what is shown),
sprite sheets (spriteMode 2), sprites with a custom outline or physics shape in the
.meta, and sprites whose bounds a script uses (BlastVfx frames, cards in the tutorial
highlight, battle backgrounds in TitleStageView).
"""
import re
import sys
from pathlib import Path

import numpy as np
from PIL import Image


def main():
    paths = [Path(p) for p in sys.argv[1:]]
    if not paths:
        sys.exit(__doc__)
    imgs = [np.asarray(Image.open(p).convert("RGBA")) for p in paths]
    H, W = imgs[0].shape[:2]
    if any(a.shape[:2] != (H, W) for a in imgs):
        sys.exit("all frames must have the same size")
    alpha = np.any(np.stack([a[..., 3] > 0 for a in imgs]), axis=0)
    ys, xs = np.nonzero(alpha)
    x0, x1, y0, y1 = xs.min(), xs.max() + 1, ys.min(), ys.max() + 1
    if (x0, y0, x1, y1) == (0, 0, W, H):
        print("no transparent margin")
        return
    for p, a in zip(paths, imgs):
        meta = Path(str(p) + ".meta")
        s = meta.read_text(encoding="utf-8")
        if re.search(r"spriteMode: 2", s) or re.search(r"\n\s+- \{x: ", s):
            sys.exit(f"{p}: sprite sheet or custom shape in .meta, not trimmed")
        px, py = (float(v) for v in re.search(r"spritePivot: \{x: ([-\d.e]+), y: ([-\d.e]+)\}", s).groups())
        # pivot in texels from the bottom-left, then relative to the cropped box
        nw, nh = x1 - x0, y1 - y0
        npx = (px * W - x0) / nw
        npy = (py * H - (H - y1)) / nh
        Image.fromarray(np.ascontiguousarray(a[y0:y1, x0:x1])).save(p, optimize=True)
        s = re.sub(r"spritePivot: \{x: [-\d.e]+, y: [-\d.e]+\}", f"spritePivot: {{x: {npx:.6g}, y: {npy:.6g}}}", s)
        s = re.sub(r"(\n  alignment:) \d+", r"\1 9", s)
        meta.write_text(s, encoding="utf-8", newline="")
        print(f"{p}: {W}x{H} -> {nw}x{nh}, pivot {px:g},{py:g} -> {npx:.4f},{npy:.4f}")


if __name__ == "__main__":
    main()
