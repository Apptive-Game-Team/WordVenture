#!/usr/bin/env python3
"""Cut the transparent margin of every sprite in a sprite sheet (spriteMode 2) and repack it.

Each sprite rect is shrunk to its visible pixels, the rects are stacked bottom to top in
their original order in a new, smaller texture, and each sprite's pivot is moved by its
crop offset so every visible pixel stays at the same world position. Sprite names, ids
and import settings stay, so animations and prefabs that point at the sprites keep working.

Usage (from the repository root):
    trim_sheet.py SHEET.png [SHEET2.png ...]

Do not use it on a sheet whose sprite size a script reads (sprite.bounds, sprite.rect),
or whose sprites have a custom outline or physics shape in the .meta.
"""
import re
import sys
from pathlib import Path

import numpy as np
from PIL import Image

SPRITE = re.compile(
    r"(    - serializedVersion: 2\n      name: (?P<name>[^\n]+)\n      rect:\n        serializedVersion: 2\n"
    r"        x: (?P<x>[-\d.]+)\n        y: (?P<y>[-\d.]+)\n        width: (?P<w>[-\d.]+)\n        height: (?P<h>[-\d.]+)\n"
    r"      alignment: (?P<align>\d+)\n      pivot: \{x: (?P<px>[-\d.e]+), y: (?P<py>[-\d.e]+)\}\n)")
ALIGN = {0: (.5, .5), 1: (0, 1), 2: (.5, 1), 3: (1, 1), 4: (0, .5), 5: (1, .5), 6: (0, 0), 7: (.5, 0), 8: (1, 0)}


def trim_sheet(path):
    meta = Path(str(path) + ".meta")
    s = meta.read_text(encoding="utf-8")
    if "spriteMode: 2" not in s:
        sys.exit(f"{path}: not a sprite sheet")
    sprites = list(SPRITE.finditer(s))
    if any(re.search(r"\n      (outline|physicsShape):\n\s+- ", s[m.end():m.end() + 400]) for m in sprites):
        sys.exit(f"{path}: custom outline or physics shape, not trimmed")
    a = np.asarray(Image.open(path).convert("RGBA"))
    H = a.shape[0]
    parts = []
    for m in sprites:
        x, y, w, h = (int(float(m[k])) for k in "xywh")
        align = int(m["align"])
        px, py = (float(m["px"]), float(m["py"])) if align == 9 else ALIGN[align]
        cell = a[H - (y + h):H - y, x:x + w]  # image rows run top-down, Unity y bottom-up
        ys, xs = np.nonzero(cell[..., 3] > 0)
        if len(xs):
            cx0, cx1, cy0, cy1 = xs.min(), xs.max() + 1, h - (ys.max() + 1), h - ys.min()  # cy from the bottom
        else:
            cx0, cx1, cy0, cy1 = 0, 1, 0, 1
        crop = cell[h - cy1:h - cy0, cx0:cx1]
        pivot = ((px * w - cx0) / (cx1 - cx0), (py * h - cy0) / (cy1 - cy0))
        parts.append((m, y, crop, pivot))
    width = max(p[2].shape[1] for p in parts)
    height = sum(p[2].shape[0] for p in parts)
    out = np.zeros((height, width, 4), np.uint8)
    edits, ny = [], 0
    for m, _, crop, pivot in sorted(parts, key=lambda p: p[1]):  # keep the original bottom-to-top order
        ch, cw = crop.shape[:2]
        out[height - (ny + ch):height - ny, :cw] = crop
        block = (f"    - serializedVersion: 2\n      name: {m['name']}\n      rect:\n        serializedVersion: 2\n"
                 f"        x: 0\n        y: {ny}\n        width: {cw}\n        height: {ch}\n"
                 f"      alignment: 9\n      pivot: {{x: {pivot[0]:.6g}, y: {pivot[1]:.6g}}}\n")
        edits.append((m.start(), m.end(), block))
        ny += ch
    for start, end, block in sorted(edits, reverse=True):
        s = s[:start] + block + s[end:]
    Image.fromarray(out).save(path, optimize=True)
    meta.write_text(s, encoding="utf-8", newline="")
    print(f"{path}: {a.shape[1]}x{H} -> {width}x{height}, {len(parts)} sprites")


if __name__ == "__main__":
    if len(sys.argv) < 2:
        sys.exit(__doc__)
    for p in sys.argv[1:]:
        trim_sheet(p)
