#!/usr/bin/env python3
"""Resample a set of same-size frames to an N x N art grid together and set an exact PPU.

The frames are laid side by side in one sheet and snapped with pixelate.py in one pass,
so every frame of an animation gets the same palette and the same cell boundaries
(a per-frame run can pick different colours or a different phase and make it flicker).

Usage (from the repository root):
    resample_set.py --target 69 --ppu 13.5 [--keep-lines 2] FRAME_01.png FRAME_02.png ... [X.gif]

--ppu: pick 27 x the world scale the sprite is drawn at (a slime prefab at scale 0.5 -> 13.5),
       so one new texel is exactly 4 screen px at 1080p. The pivot stays where it was
       (normalised), so the sprite keeps its place; its world size changes only by the
       rounding of N.
A .gif keeps its format (Unity reads its first frame); only its first frame is kept.
"""
import argparse
import re
import sys
from pathlib import Path

import numpy as np
from PIL import Image

sys.path.insert(0, str(Path(__file__).parent))
from pixelate import pixelate, scale_meta  # noqa: E402


def load(path):
    with Image.open(path) as im:
        im.seek(0)
        return im.convert("RGBA")


def save(img, path):
    if path.suffix.lower() == ".gif":
        rgba = np.asarray(img.convert("RGBA"))
        p = Image.fromarray(np.ascontiguousarray(rgba[..., :3])).quantize(colors=255, dither=Image.Dither.NONE)
        idx = np.asarray(p).copy()
        idx[rgba[..., 3] < 128] = 255
        out = Image.fromarray(idx, "P")
        out.putpalette(p.getpalette()[:255 * 3] + [0, 0, 0])
        out.save(path, transparency=255)
    else:
        img.save(path, optimize=True)


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--target", type=int, required=True, help="frame size in art px (square frames)")
    ap.add_argument("--ppu", type=float, required=True)
    ap.add_argument("--keep-lines", type=float, default=0)
    ap.add_argument("--colors", type=int, default=64)
    ap.add_argument("images", nargs="+")
    args = ap.parse_args()
    paths = [Path(p) for p in args.images]
    frames = [load(p) for p in paths]
    W, H = frames[0].size
    if any(f.size != (W, H) for f in frames):
        sys.exit("all frames must have the same size")
    sheet = Image.new("RGBA", (W * len(frames), H))
    for i, f in enumerate(frames):
        sheet.paste(f, (i * W, 0))
    N = args.target
    small = pixelate(sheet, N * len(frames), N * H // W, args.colors, keep_lines=args.keep_lines).convert("RGBA")
    for i, p in enumerate(paths):
        save(small.crop((i * N, 0, (i + 1) * N, N * H // W)), p)
        meta = str(p) + ".meta"
        old_ppu = float(re.search(r"spritePixelsToUnits: ([\d.]+)", Path(meta).read_text()).group(1))
        scale_meta(meta, (W, H), (N, N * H // W), args.ppu)
        print(f"{p}: {W}x{H} -> {N}x{N * H // W}, PPU {old_ppu:g} -> {args.ppu:g}, world {W / old_ppu:.3f} -> {N / args.ppu:.3f}")


if __name__ == "__main__":
    main()
