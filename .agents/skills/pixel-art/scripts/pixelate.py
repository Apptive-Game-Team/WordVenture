#!/usr/bin/env python3
"""Snap an image to the WordVenture pixel grid and store it 1 texel = 1 art pixel.

Project rule: at the 1920x1080 reference resolution one art pixel is UNIT (4)
screen pixels, i.e. a 480x270 virtual screen. The camera shows 10 world units
over 1080 px (108 px/unit), so a SpriteRenderer at scale 1 needs PPU 27.

Size the output with exactly one of:
  --display WxH         on-screen size at 1080p (W or H may be 0 to keep aspect)
  --screen-px-per-texel how many 1080p screen px one source texel covers today
  --target WxH          logical size in art pixels

Examples:
  pixelate.py gen.png Assets/Art/Story/New.png --display 1920x1080
  pixelate.py old.png old.png --screen-px-per-texel 0.54 --update-meta
"""
import argparse, os, re, sys
import numpy as np
from PIL import Image

UNIT = 4
PX_PER_WORLD_UNIT = 108  # orthographic size 5 at 1080 px


def exact_block(a):
    """Largest b such that the image is made of uniform b x b blocks (real, upscaled pixel art)."""
    h, w = a.shape[:2]
    for b in (32, 24, 20, 16, 12, 10, 8, 6, 5, 4, 3, 2):
        if w % b or h % b:
            continue
        c = a.reshape(h // b, b, w // b, b, a.shape[2])
        if (c == c[:, :1, :, :1]).all(axis=(1, 3, 4)).mean() > 0.995:
            return b
    return 1


def palette_indices(img_rgba, colors, alpha_threshold):
    a = np.asarray(img_rgba)
    opaque = a[..., 3] >= alpha_threshold
    rgb = Image.fromarray(np.ascontiguousarray(a[..., :3]))
    uniq = np.unique(a[opaque][:, :3].reshape(-1, 3), axis=0) if opaque.any() else np.zeros((1, 3), np.uint8)
    if len(uniq) <= colors:
        # few colours already (real pixel art): keep them exactly
        pal = uniq.astype(np.uint8)
        keys = pal.astype(np.int64) @ [65536, 256, 1]  # np.unique output is sorted
        idx = np.clip(np.searchsorted(keys, a[..., :3].astype(np.int64) @ [65536, 256, 1]), 0, len(pal) - 1)
        idx[~opaque] = len(pal)
        return idx, pal
    sample = Image.fromarray(np.where(opaque[..., None], a[..., :3], a[opaque][:1, :3] if opaque.any() else 0).astype(np.uint8))
    q = sample.quantize(colors=colors, method=Image.Quantize.MEDIANCUT, kmeans=3, dither=Image.Dither.NONE)
    pal = np.array(q.getpalette()[:colors * 3], np.uint8).reshape(-1, 3)
    idx = np.asarray(rgb.quantize(palette=q, dither=Image.Dither.NONE)).astype(np.int64)
    idx[~opaque] = len(pal)
    return idx, pal


def mode_pool(idx, K, LW, LH, phase=True, weights=None):
    """Each output cell takes the most frequent palette index of the source pixels it covers.

    weights (per index) lets thin features such as outlines win cells they only partly cover."""
    h, w = idx.shape
    sx, sy = w / LW, h / LH
    steps = [i / 4 for i in range(4)] if phase and min(sx, sy) >= 2 else [0.0]
    best = None
    for fx in steps:
        for fy in steps:
            cx = np.clip(((np.arange(w) + fx * sx) // sx).astype(int), 0, LW - 1)
            cy = np.clip(((np.arange(h) + fy * sy) // sy).astype(int), 0, LH - 1)
            cell = cy[:, None] * LW + cx[None, :]
            c = np.bincount((cell * K + idx).ravel(), minlength=LW * LH * K).reshape(-1, K)
            if weights is not None:
                c = c * weights
            score = c.max(1).sum()
            if best is None or score > best[0]:
                best = (score, c.argmax(1).reshape(LH, LW), cell)
    return best[1], best[2]


def outline_index(idx, K):
    """Palette index drawn most often along the sprite silhouette (its outline colour)."""
    clear = np.pad(idx == K - 1, 1, constant_values=True)
    edge = (idx != K - 1) & (clear[:-2, 1:-1] | clear[2:, 1:-1] | clear[1:-1, :-2] | clear[1:-1, 2:])
    return int(np.bincount(idx[edge], minlength=K).argmax()) if edge.any() else None


def pixelate(src, LW, LH, colors=64, alpha_threshold=128, alpha_levels=0, despeckle=False, keep_lines=0.0):
    im = src.convert('RGBA')
    a = np.asarray(im)
    idx, pal = palette_indices(im, colors, alpha_threshold)
    K = len(pal) + 1  # last class = transparent
    weights = None
    line = outline_index(idx, K) if keep_lines else None
    if line is not None:
        # outline and colours as dark as it are lines: let them win cells they half cover
        lum = pal.astype(float) @ [0.299, 0.587, 0.114]
        weights = np.ones(K)
        weights[:-1][lum <= lum[line] + 24] = keep_lines
    mode, cell = mode_pool(idx, K, LW, LH, weights=weights)
    if line is not None:
        # close the silhouette: every opaque pixel touching transparency becomes outline
        clear = np.pad(mode == K - 1, 1, constant_values=True)
        edge = (mode != K - 1) & (clear[:-2, 1:-1] | clear[2:, 1:-1] | clear[1:-1, :-2] | clear[1:-1, 2:])
        mode = np.where(edge, line, mode)
    if despeckle:
        m = np.pad(mode, 1, mode='edge')
        up, dn, lf, rt = m[:-2, 1:-1], m[2:, 1:-1], m[1:-1, :-2], m[1:-1, 2:]
        lone = (up == dn) & (dn == lf) & (lf == rt) & (up != mode)
        mode = np.where(lone, up, mode)
    out = np.zeros((LH, LW, 4), np.uint8)
    solid = mode < len(pal)
    out[solid, :3] = pal[mode[solid]]
    out[solid, 3] = 255
    if alpha_levels:
        cnt = np.bincount(cell.ravel(), minlength=LW * LH)
        mean_a = (np.bincount(cell.ravel(), weights=a[..., 3].ravel().astype(float), minlength=LW * LH) / np.maximum(cnt, 1)).reshape(LH, LW)
        q = np.round(mean_a / 255 * alpha_levels) / alpha_levels * 255
        vis = a[..., 3] >= 24
        cc = np.bincount(cell[vis] * K + idx[vis], minlength=LW * LH * K).reshape(-1, K)[:, :-1]
        has = cc.sum(1).reshape(LH, LW) > 0
        col = pal[np.clip(cc.argmax(1), 0, len(pal) - 1)].reshape(LH, LW, 3)
        out[..., :3] = np.where(has[..., None], col, 0)
        out[..., 3] = np.where(has, q, 0).astype(np.uint8)
    img = Image.fromarray(out, 'RGBA')
    return img if a[..., 3].min() < 255 else img.convert('RGB')


def plan_size(w, h, args, a):
    """Logical output size, snapping real pixel art to an integer multiple of its native grid when close."""
    if args.target:
        return tuple(map(int, args.target.lower().split('x'))), 'target'
    if args.display:
        dw, dh = map(float, args.display.lower().split('x'))
        if not dw: dw = dh * w / h
        if not dh: dh = dw * h / w
        return (max(1, round(dw / UNIT)), max(1, round(dh / UNIT))), 'display'
    t = args.screen_px_per_texel
    LW, LH = max(1, round(w * t / UNIT)), max(1, round(h * t / UNIT))
    b = exact_block(a)
    few_colors = len(np.unique(a.reshape(-1, a.shape[2]), axis=0)) <= 256  # hand-made pixel art, not an AI image
    if b > 1 or few_colors:
        nw, nh = w // b, h // b
        r = LW / nw
        k = max(1, round(r))
        if abs(r - k) / r <= args.snap:
            return (nw * k, nh * k), f'snap x{k} (native {nw}x{nh}, ratio {r:.2f})'
    return (LW, LH), 'resample'


def scale_meta(meta_path, old_wh, new_wh, ppu=None):
    s = open(meta_path, encoding='utf-8').read()
    fx, fy = new_wh[0] / old_wh[0], new_wh[1] / old_wh[1]

    def rect(m):
        ind = m.group(2)
        x, y, w, h = (float(m.group(i)) for i in (3, 4, 5, 6))
        x2, y2 = round((x + w) * fx), round((y + h) * fy)
        x, y = round(x * fx), round(y * fy)
        return f'{m.group(1)}{ind}x: {x}\n{ind}y: {y}\n{ind}width: {x2 - x}\n{ind}height: {y2 - y}'
    s = re.sub(r'(rect:\n\s+serializedVersion: 2\n)(\s+)x: ([-\d.]+)\n\s+y: ([-\d.]+)\n\s+width: ([-\d.]+)\n\s+height: ([-\d.]+)', rect, s)
    s = re.sub(r'border: \{x: ([-\d.]+), y: ([-\d.]+), z: ([-\d.]+), w: ([-\d.]+)\}',
               lambda m: 'border: {x: %d, y: %d, z: %d, w: %d}' % (round(float(m.group(1)) * fx), round(float(m.group(2)) * fy), round(float(m.group(3)) * fx), round(float(m.group(4)) * fy)), s)
    s = re.sub(r'- \{x: ([-\d.e]+), y: ([-\d.e]+)\}', lambda m: '- {x: %g, y: %g}' % (round(float(m.group(1)) * fx, 3), round(float(m.group(2)) * fy, 3)), s)
    if ppu is not None:
        s = re.sub(r'spritePixelsToUnits: [\d.]+', f'spritePixelsToUnits: {ppu:.8g}', s)
    s = re.sub(r'filterMode: \d', 'filterMode: 0', s)
    s = re.sub(r'enableMipMap: \d', 'enableMipMap: 0', s)
    s = re.sub(r'textureCompression: \d', 'textureCompression: 0', s)
    open(meta_path, 'w', encoding='utf-8', newline='').write(s)


def main():
    p = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument('src'); p.add_argument('dst')
    g = p.add_mutually_exclusive_group(required=True)
    g.add_argument('--display'); g.add_argument('--screen-px-per-texel', type=float); g.add_argument('--target')
    p.add_argument('--colors', type=int, default=64)
    p.add_argument('--alpha-threshold', type=int, default=128)
    p.add_argument('--alpha-levels', type=int, default=0, help='keep soft alpha as N uniform steps (VFX glow)')
    p.add_argument('--despeckle', action='store_true', help='drop lone pixels (backgrounds from soft AI gradients)')
    p.add_argument('--keep-lines', type=float, default=0, metavar='W',
                   help='sprites with dark outlines: weight outline-dark colours W (try 2.5) and redraw the silhouette outline')
    p.add_argument('--snap', type=float, default=0.08, help='max relative size change to keep real pixel art at an integer multiple')
    p.add_argument('--update-meta', action='store_true', help="rewrite dst.meta: PPU so 1 texel = UNIT px, sprite rects, point filter")
    p.add_argument('--keep-ppu', action='store_true', help='with --update-meta: leave PPU (UI-only images)')
    args = p.parse_args()
    src = Image.open(args.src)
    a = np.asarray(src.convert('RGBA'))
    (LW, LH), how = plan_size(src.width, src.height, args, a)
    out = pixelate(src, LW, LH, args.colors, args.alpha_threshold, args.alpha_levels, args.despeckle, args.keep_lines)
    os.makedirs(os.path.dirname(os.path.abspath(args.dst)), exist_ok=True)
    out.save(args.dst, optimize=True)
    msg = f'{args.src} {src.width}x{src.height} -> {args.dst} {LW}x{LH} ({how})'
    if args.update_meta:
        meta = args.dst + '.meta'
        if not os.path.exists(meta):
            sys.exit(msg + '\nno .meta yet: open Unity once to import, then rerun with --update-meta')
        old_ppu = float(re.search(r'spritePixelsToUnits: ([\d.]+)', open(meta).read()).group(1))
        if args.keep_ppu:
            ppu = None
        elif args.screen_px_per_texel:
            # world size of one old texel stays the same in screen px; one new texel must be UNIT px
            ppu = old_ppu * args.screen_px_per_texel / UNIT
        else:
            ppu = PX_PER_WORLD_UNIT / UNIT
        scale_meta(meta, (src.width, src.height), (LW, LH), ppu)
        msg += f', PPU {old_ppu:g} -> {ppu:.8g}' if ppu else ''
    print(msg)


if __name__ == '__main__':
    main()
