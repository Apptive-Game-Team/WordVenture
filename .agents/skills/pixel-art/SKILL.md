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

Logical sizes of current assets (measured on develop, #116), for matching new ones:

| Asset kind | Art px | PPU |
|---|---|---|
| Story, title and ending background (SpriteRenderer, world width 4.8) | 480x270 | 100 |
| Battle and dialogue background (battle object scale 2, `StageDialogueView` envelope) | 552x276 | 53.90625 |
| Game over and game clear background | 480x240 | 46.875 |
| World map (MapScene background scale 1) | 495x276 | 27 |
| Dialogue portrait (`DialogueWindowPresentation`, 63% of screen height) | 170x170 | UI |
| Word in battle and normal slimes (prefab scale 0.5) | 69x69 frame | 13.5 |
| Bosses (prefab scale 0.7) / Shield (0.65) / Surge (0.91) | 97 / 90 / 126 | 18.9 / 17.55 / 24.57 |
| Enemy projectiles (prefab scale 0.2) | 28x28 before trim | 5.4 |
| Spell effects (prefab scale 5 or 10, PPU = frame width) | 128 or 256 frames | 4.22 px per texel, kept |
| Cards in hand (`Card.prefab` root and runtime scale 0.4) | 34x50 | 10.8 |

Known exceptions, kept on purpose: spell effect sheets (4.22 px per texel, 5% off);
`SplitSlimeSmall` (prefab scale 0.3 reuses the 0.5 frames, 2.4 px); `AllySlime` (scale 0.4
reuses the `Ranged_Light` frames, 3.2 px); `ElementalStatusOverlay`,
`AffinityMark` and `ReactionBurst` are sized by the enemy's body at runtime, about 4 px on a
normal slime and larger on bosses; `Assets/Art/Icon.png` is the app icon, not drawn in game.

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

## Workflow: measure the whole game

`scripts/PixelGridReport.cs` measures, for every SpriteRenderer, UI Image, serialized
sprite field and animation sprite in the build scenes and in every prefab, how many 1080p
screen px one texel covers (`texpx`). A prefab is measured at its own root scale, as if
instantiated with no parent (slimes 0.5, spells 5 or 10). It cannot see sizes set by code:
read the script for anything assigned or scaled at runtime (`StageManager` backgrounds,
`MapMove` maps, VFX sized by the enemy body, cards). `scripts/PixelGridCapture.cs` renders
every build scene at 1920x1080 for a before and after comparison (edit mode, so UI built at
runtime is missing).

Both are editor scripts that are not kept in `Assets/`. Copy the project and run them there:

```bash
mkdir -p /mnt/c/temp/wv-pixel && git archive HEAD Assets Packages ProjectSettings docs | tar -x -C /mnt/c/temp/wv-pixel
mkdir -p /mnt/c/temp/wv-pixel/Assets/Editor/PixelGrid && cp .agents/skills/pixel-art/scripts/PixelGrid*.cs /mnt/c/temp/wv-pixel/Assets/Editor/PixelGrid/
"/mnt/c/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe" -batchmode -nographics -projectPath 'C:\temp\wv-pixel' -executeMethod PixelGridReport.Run -quit -logFile 'C:\temp\wv-pixel\report.log'
"/mnt/c/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe" -batchmode -projectPath 'C:\temp\wv-pixel' -executeMethod PixelGridCapture.Run -quit -logFile 'C:\temp\wv-pixel\capture.log'
```

The report is `C:\temp\wv-pixel\pixel-report.json`, the captures `C:\temp\wv-pixel\captures\`.
Target art size of a texture = texture size x texpx / 4.

## Workflow: character animation (slimes, Word, projectiles)

Characters are drawn in one shared style (#116): a 1-pixel outline in a darker shade of
each colour (a slime's rim is its outline), flat 2 to 3 tone shading lit from the top left,
small clean eyes that stay identical across frames unless the expression changes, no
scattered single pixels. Do not shrink character art drawn on a finer grid: the lines
break into jagged steps. Redraw it at the target grid with Codex as one sheet:

1. Frame size: 512 x prefab scale x 1.08 / 4 art px for a 512 frame (69 at scale 0.5),
   or the size of an existing trimmed frame of the same character.
2. `char_sheet.py ref ref.png W H <frames>` (4 columns, one frame per cell).
3. Run Codex with the sheet attached, plus `Assets/Art/Story/Portraits/WordDialogue.png`
   and `Assets/Art/Tutorial/PlainBackground.png` enlarged 4x and 2x as style references:

   ```text
   Attached: (1) a sprite sheet of [N] animation frames of one game character, laid out left
   to right, top to bottom in a 4 x [rows] grid, each cell one frame; (2) and (3) style
   references: an approved dialogue portrait and an approved background of the same game.
   Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a
   clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading
   with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small
   clean eyes. [Character, colours, decoration, frame order.]
   Keep the same grid layout and frame order. In every frame keep the character at the same
   position inside its cell (same baseline, same left and right extent), the same pose and
   action, the same colours and decorations. Keep each frame's expression. Keep areas clean
   and flat: no scattered single pixels. The eyes are the same two small blocks in every frame
   unless the expression changes. Each cell is an exact [W]x[H] pixel grid, so the whole sheet
   is [4W]x[rows*H] pixels: every art pixel is one equal square cell, then enlarge with
   nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no
   dithering noise, no blur, at most about 16 colours. Transparent background. No text, no
   grid lines.
   ```
4. `char_sheet.py post raw.png W H <frame paths>`, then compare with the reference at 2x
   screen size (eyes in every frame, baseline, decoration); regenerate the sheet if a frame breaks.
5. Set PPU 27 x prefab scale in each `.meta`, then run `trim_set.py` on the frames.

## Workflow: animation sets and margins

```bash
# shrink all frames of one animation together (same palette and cell boundaries), exact PPU
python3 .agents/skills/pixel-art/scripts/resample_set.py --target 69 --ppu 13.5 --keep-lines 2 FRAME_*.png
# cut the transparent margin of a set without moving it on screen (pivot follows the crop)
python3 .agents/skills/pixel-art/scripts/trim_set.py FRAME_*.png
```

Set `--ppu` to 27 x the scale the sprite is drawn at.

```bash
# sprite sheets (spriteMode 2): cut every sprite to its content and repack the sheet
python3 .agents/skills/pixel-art/scripts/trim_sheet.py SHEET.png
```

Both trim scripts move the pivot by the crop offset, so a sprite drawn by a SpriteRenderer
stays exactly where it was. Do not trim:
- UI Image sprites whose rect does not follow the sprite: the title logo and the dialogue
  portraits (`preserveAspect` in a fixed slot) would grow, and portrait heads would jump
  between speakers. The tutorial hand's pivot is set in code.
- Sprites whose size a script reads. `BlastVfx` takes the explosion frame width as a
  constant for this reason; `TitleStageView` covers the screen with a background's bounds;
  the tutorial highlight wraps cards by their bounds.
- Atlases that code cuts into a grid (`Resources/Combat`).

Slime frames are trimmed; the encyclopedia shows them with `preserveAspect`, so a trimmed
slime fills its cell instead of sitting small in a mostly empty frame.

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
- Non-uniform transform scale makes pixels non-square. The battle background was (1.8, 2)
  and the map (1.19, 1) until #116; use uniform scales, or draw the art at the on-screen
  aspect and give the object scale 1.
- A prefab's root scale counts. Slime prefabs are 0.5, spell prefabs 5 or 10: measure with
  the root scale, never as if the root were 1.
- Changing a texture size without updating the PPU changes the in-game size. Always use
  `--update-meta`, or compute the PPU by hand.
- Do not run the tool on fonts, TextMeshPro sprite atlases or WebGL template icons.
