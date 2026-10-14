---
name: pixel-art
description: >
  Generate or fix game images so they are real pixel art on the WordVenture pixel grid
  (1 art pixel = 4 screen px at 1920x1080, stored 1 texel = 1 art pixel). Use whenever
  an image is generated (Codex image_gen or any AI tool), added, or replaced under Assets/,
  or when the user mentions 픽셀, 픽셀아트, 픽셀 크기, 이미지 생성, 배경/초상화/아이콘 생성,
  "pixel art", "pixel size", or asks to regenerate an asset.
---

# pixel-art

Every image in the game must look like it was drawn on one shared grid: a 480x270
virtual screen, scaled 4x to 1920x1080. AI image generators only *imitate* pixel
art (anti-aliased edges, uneven block sizes, 100k+ colours), so their output is
never used directly. It is always snapped with `scripts/pixelate.py`.

## The rule

| Item | Value |
|---|---|
| Unit | 1 art pixel = **4** screen px at 1080p (480x270 virtual screen) |
| Storage | texture size = size in art pixels (no pre-upscaled 2x/4x/8x files) |
| Camera | orthographic size 5 -> 108 screen px per world unit |
| PPU (SpriteRenderer at scale 1) | **27** (= 108 / 4) |
| PPU (scaled SpriteRenderer) | `27 x world scale` |
| UI Image | texture size = RectTransform size (ref 1920x1080) / 4 |
| Import | Point filter, no mipmaps, no compression |
| Colours | at most 64 per image (fewer for small sprites) |

Logical sizes of current assets, for matching new ones:

| Asset kind | Art px |
|---|---|
| Full-screen background (story, title, ending) | 480x270 |
| Dialogue portrait (`DialogueWindowPresentation`, 63% of screen height) | 170x170 |
| Title logo | 275x91 |
| Word / normal enemies | 69x69 frame |
| Bosses | 97x97 frame |
| Cards in hand | 52x52 |
| Projectiles | 28x28 |

## Workflow: new image

1. **Decide the on-screen size** at 1920x1080 (for example a full-screen background is
   1920x1080 and a portrait is 680x680). Divide by 4 to get the art size.
2. **Generate** with Codex (`codex`, built-in image_gen). This sandbox may have no
   OpenAI credentials, so if `codex` is missing or unauthenticated, ask the user to run
   it locally. Use this prompt frame and fill in the brackets:

   ```text
   [subject and composition]. Pixel art drawn on an exact [W]x[H] pixel grid
   (one art pixel = one square cell), then enlarged by an integer factor with
   nearest-neighbour so every cell is a crisp, equal-sized square. Hard edges,
   no anti-aliasing, no gradients, no dithering noise, no blur, flat colour
   clusters, limited palette of about [24-48] colours, 1-pixel dark outline on
   characters. Match the palette and density of the supplied game art.
   [No text, UI or logos.] [Transparent background for sprites.]
   ```

   Ask for a size close to `[W]x[H]` times an integer. Pass existing assets of the same kind
   as style references. Record the final prompt in `background-generation.md` (or the
   matching doc) as the existing entries do.
3. **Snap to the grid**:

   ```bash
   python3 .agents/skills/pixel-art/scripts/pixelate.py gen.png Assets/Art/<Path>/<Name>.png --display 1920x1080
   # or an exact art size:  --target 480x270
   # sprites with soft glow:  --alpha-levels 4 --alpha-threshold 96
   # backgrounds with AI gradient speckle:  --despeckle
   # characters/portraits with dark outlines:  --keep-lines 2
   ```

   `--keep-lines` gives outline-dark colours extra weight and redraws the silhouette
   outline, so 1-pixel lines do not break into dashes when shrinking. Values above 2
   thicken lines too much.

4. **Import settings**: once Unity has created the `.meta`, rerun the same command with
   `--update-meta`. It sets PPU 27, Point filter, no mipmaps, and no compression. Add
   `--keep-ppu` for UI-only images. On a scaled SpriteRenderer, set PPU to `27 x scale`
   by hand.
5. **Review** the result enlarged 4x with nearest-neighbour next to the source. If faces,
   text, or thin lines broke, regenerate at the target grid (step 2) instead of tweaking
   colours. Commit the PNG together with its `.meta`.

## Workflow: existing image that does not match the grid

1. Measure how many 1080p screen px one texel covers today (`texpx`):
   - SpriteRenderer: `world scale x 108 / PPU`
   - UI Image: `displayed rect width / sprite rect width`
   - Anything that sets scale at runtime: read the code (cards use
     `CardManager.RoundAlignment` scale; status overlay scales to the enemy body).
2. Resample and fix the meta in place:

   ```bash
   python3 .agents/skills/pixel-art/scripts/pixelate.py X.png X.png --screen-px-per-texel <texpx> --update-meta
   ```

   PPU becomes `old PPU x texpx / 4`, so world size and colliders stay the same. Sprite-sheet
   rects, borders and outlines are scaled with the texture. Real pixel art within 8% of an
   integer ratio is snapped to that multiple instead of being resampled.
3. Review as in step 5 above. If the result looks wrong (detail lost when shrinking, or
   blocks larger than 4px because the source had no finer detail), regenerate it with
   Codex at the target art size. Then run "new image" steps 3-5. Keep the list in
   [`regen-candidates.md`](regen-candidates.md) up to date.

## Workflow: find unused images

```bash
python3 .agents/skills/pixel-art/scripts/audit_textures.py --summary   # counts and UNUSED lines
python3 .agents/skills/pixel-art/scripts/audit_textures.py > audit.json  # size, colours, margins, PPU per image
```

An image is used when GUID references reach it from an enabled build scene, from
anything under `Assets/Resources`, from an `"Assets/....ext"` path literal in a script or
test, or from `ProjectSettings` (app icons). Before deleting an UNUSED image, also delete
the unreachable prefabs, animations and controllers that point only at it, and grep the
repository for its path. Delete the `.meta` with the file, and the folder `.meta` when a
folder becomes empty.

## Pitfalls

- Never save an upscaled copy (2x/4x/8x) to the project. Upscaling is the job of PPU and
  the Point filter.
- Non-uniform transform scale (for example card scale `(0.379, 0.42)`) makes pixels
  non-square. Prefer uniform scales for new objects.
- Changing a texture size without updating the PPU changes the in-game size. Always use
  `--update-meta`, or compute the PPU by hand.
- Do not run the tool on fonts, TextMeshPro sprite atlases or WebGL template icons.
