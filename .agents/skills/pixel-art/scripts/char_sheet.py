#!/usr/bin/env python3
"""Redraw a character animation with Codex as one sprite sheet, so the frames stay consistent.

Frames go in a 4-column grid, one frame per cell. Each cell is the frame's final size in
art pixels (W x H), so the redraw lands on the 4-screen-px grid directly. Codex is given
the reference sheet plus the style references and returns one sheet; `post` snaps it and
splits it back into frames.

Usage (from the repository root):
    char_sheet.py ref  OUT.png W H FRAME [FRAME ...]   # reference: frames cropped to their union box
    char_sheet.py post RAW.png W H DST [DST ...]       # snap Codex's sheet, write one PNG per frame

FRAME can be a path or "<git ref>:<path>" (for example origin/develop:Assets/...png) to use
an older version. The prompt frame and the style references are in SKILL.md
("Workflow: character animation"). After `post`, set the .meta PPU to 27 x prefab scale
and run trim_set.py on the frames.
"""
import io
import math
import subprocess
import sys
from pathlib import Path

from PIL import Image

sys.path.insert(0, str(Path(__file__).parent))
from pixelate import pixelate  # noqa: E402
from resample_set import save  # noqa: E402

COLS = 4


def load(spec):
    if ":" in spec and not Path(spec).exists():
        data = subprocess.run(["git", "show", spec], capture_output=True, check=True).stdout
        im = Image.open(io.BytesIO(data))
    else:
        im = Image.open(spec)
    im.seek(0)
    return im.convert("RGBA")


def ref(out, W, H, frames):
    imgs = [load(f) for f in frames]
    boxes = [im.getchannel("A").getbbox() for im in imgs]
    box = (min(b[0] for b in boxes), min(b[1] for b in boxes), max(b[2] for b in boxes), max(b[3] for b in boxes))
    rows = math.ceil(len(imgs) / COLS)
    cw, ch = W * 8, H * 8
    sheet = Image.new("RGBA", (COLS * cw, rows * ch), (0, 0, 0, 0))
    for i, im in enumerate(imgs):
        sheet.alpha_composite(im.crop(box).resize((cw, ch), Image.NEAREST), ((i % COLS) * cw, (i // COLS) * ch))
    sheet.save(out)
    print(f"{out}: {COLS}x{rows} cells of {W}x{H}")


def post(raw, W, H, dsts):
    rows = math.ceil(len(dsts) / COLS)
    small = pixelate(Image.open(raw), COLS * W, rows * H, keep_lines=2, despeckle=True).convert("RGBA")
    for i, d in enumerate(dsts):
        x, y = (i % COLS) * W, (i // COLS) * H
        save(small.crop((x, y, x + W, y + H)), Path(d))
    print(f"wrote {len(dsts)} frames of {W}x{H}")


if __name__ == "__main__":
    if len(sys.argv) < 6 or sys.argv[1] not in ("ref", "post"):
        sys.exit(__doc__)
    cmd, path, W, H, rest = sys.argv[1], sys.argv[2], int(sys.argv[3]), int(sys.argv[4]), sys.argv[5:]
    (ref if cmd == "ref" else post)(path, W, H, rest)
