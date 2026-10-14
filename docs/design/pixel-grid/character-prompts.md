# 캐릭터 재생성 프롬프트

#116 에서 줄여서 만든 캐릭터가 자글자글해서, 대화 초상화·배경과 같은 공통 스타일로 Codex image_gen 으로 다시 그렸다.
애니메이션 한 벌을 4열 시트 한 장으로 그리게 해서 프레임끼리 모양이 맞게 했다. 각 작업에 첨부한 그림은 세 장이다:
(1) develop 의 원래 프레임을 보이는 칸의 합집합으로 잘라 4열로 놓은 참고 시트(`char_sheet.py ref`),
(2) `Assets/Art/Story/Portraits/WordDialogue.png` 4배 확대본, (3) `Assets/Art/Tutorial/PlainBackground.png` 2배 확대본.
결과는 `char_sheet.py post`(외곽선 보정, 외톨이 픽셀 제거)로 칸 크기에 맞춰 나눴다. 칸 크기는 이미 맞춰 둔 프레임 크기다.
투사체는 본체와 떨어진 작은 점을 지웠고, 타이틀 머리는 프레임마다 그림을 칸에 맞춰 넣었다.

## Boss_Cactus

```text
Attached: (1) a sprite sheet of 8 animation frames of one game character, laid out left to right, top to bottom in a 4 x 2 grid (empty cells are unused), each cell one frame; (2) and (3) style references: an approved dialogue portrait and an approved background of the same game.
Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small clean eyes, the same palette treatment and density as the references.
A boss slime from the reference; keep its body colours and its decoration (a few short spines) exactly. Frames in order: idle, idle, move, move, hit, death, ranged attack, ranged attack. The rim colour is the slime's outline; give it no extra black line outside the rim.
Keep the same grid layout and frame order. In every frame keep the character at the same position inside its cell (same baseline, same left and right extent), the same pose and action, the same colours and decorations, so the frames still animate together and the character is recognisable. Keep each frame's expression (for example a squeezed >_< face when hit). 
Keep areas clean and flat: no scattered single pixels, no specks or noise inside the body; texture is shown with a few clear marks only. Each cell is an exact 50x42 pixel grid, so the whole sheet is 200x84 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, at most about 16 colours. Transparent background. No text, no grid lines.
```

## Boss_Devil

```text
Attached: (1) a sprite sheet of 8 animation frames of one game character, laid out left to right, top to bottom in a 4 x 2 grid (empty cells are unused), each cell one frame; (2) and (3) style references: an approved dialogue portrait and an approved background of the same game.
Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small clean eyes, the same palette treatment and density as the references.
A boss slime from the reference; keep its body colours and its decoration (horns, crown, ears, legs, shield, crystals and so on) exactly. Frames in order: idle, idle, move, move, hit, death, ranged attack, ranged attack. The rim colour is the slime's outline; give it no extra black line outside the rim.
Keep the same grid layout and frame order. In every frame keep the character at the same position inside its cell (same baseline, same left and right extent), the same pose and action, the same colours and decorations, so the frames still animate together and the character is recognisable. Keep each frame's expression (for example a squeezed >_< face when hit). 
Each cell is an exact 72x54 pixel grid, so the whole sheet is 288x108 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, at most about 16 colours. Transparent background. No text, no grid lines.
```

## Boss_Fish

```text
Attached: (1) a sprite sheet of 8 animation frames of one game character, laid out left to right, top to bottom in a 4 x 2 grid (empty cells are unused), each cell one frame; (2) and (3) style references: an approved dialogue portrait and an approved background of the same game.
Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small clean eyes, the same palette treatment and density as the references.
A boss slime from the reference; keep its body colours and its decoration (horns, crown, ears, legs, shield, crystals and so on) exactly. Frames in order: idle, idle, move, move, hit, death, ranged attack, ranged attack. The rim colour is the slime's outline; give it no extra black line outside the rim.
Keep the same grid layout and frame order. In every frame keep the character at the same position inside its cell (same baseline, same left and right extent), the same pose and action, the same colours and decorations, so the frames still animate together and the character is recognisable. Keep each frame's expression (for example a squeezed >_< face when hit). 
Each cell is an exact 59x43 pixel grid, so the whole sheet is 236x86 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, at most about 16 colours. Transparent background. No text, no grid lines.
```

## Boss_Flower

```text
Attached: (1) a sprite sheet of 8 animation frames of one game character, laid out left to right, top to bottom in a 4 x 2 grid (empty cells are unused), each cell one frame; (2) and (3) style references: an approved dialogue portrait and an approved background of the same game.
Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small clean eyes, the same palette treatment and density as the references.
A boss slime from the reference; keep its body colours and its decoration (horns, crown, ears, legs, shield, crystals and so on) exactly. Frames in order: idle, idle, move, move, hit, death, ranged attack, ranged attack. The rim colour is the slime's outline; give it no extra black line outside the rim.
Keep the same grid layout and frame order. In every frame keep the character at the same position inside its cell (same baseline, same left and right extent), the same pose and action, the same colours and decorations, so the frames still animate together and the character is recognisable. Keep each frame's expression (for example a squeezed >_< face when hit). 
Each cell is an exact 50x43 pixel grid, so the whole sheet is 200x86 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, at most about 16 colours. Transparent background. No text, no grid lines.
```

## Boss_Rainy

```text
Attached: (1) a sprite sheet of 8 animation frames of one game character, laid out left to right, top to bottom in a 4 x 2 grid (empty cells are unused), each cell one frame; (2) and (3) style references: an approved dialogue portrait and an approved background of the same game.
Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small clean eyes, the same palette treatment and density as the references.
A boss slime from the reference; keep its body colours and its decoration (horns, crown, fuse, sprout, cannon, legs and so on) exactly. Frames in order: idle, idle, move, move, hit, death, ranged attack, ranged attack. The rim colour is the slime's outline; give it no extra black line outside the rim.
Keep the same grid layout and frame order. In every frame keep the character at the same position inside its cell (same baseline, same left and right extent), the same pose and action, the same colours and decorations, so the frames still animate together and the character is recognisable. Keep each frame's expression (for example a squeezed >_< face when hit). 
Each cell is an exact 49x42 pixel grid, so the whole sheet is 196x84 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, at most about 16 colours. Transparent background. No text, no grid lines.
```

## Melee_Crab

```text
Attached: (1) a sprite sheet of 6 animation frames of one game character, laid out left to right, top to bottom in a 4 x 2 grid (empty cells are unused), each cell one frame; (2) and (3) style references: an approved dialogue portrait and an approved background of the same game.
Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small clean eyes, the same palette treatment and density as the references.
A slime enemy from the reference; keep its body colours and its decoration (horns, crown, ears, legs, shield, crystals and so on) exactly. Frames in order: idle, idle, move and melee attack, move and melee attack, hit, death. The rim colour is the slime's outline; give it no extra black line outside the rim.
Keep the same grid layout and frame order. In every frame keep the character at the same position inside its cell (same baseline, same left and right extent), the same pose and action, the same colours and decorations, so the frames still animate together and the character is recognisable. Keep each frame's expression (for example a squeezed >_< face when hit). 
Each cell is an exact 40x30 pixel grid, so the whole sheet is 160x60 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, at most about 16 colours. Transparent background. No text, no grid lines.
```

## Melee_Dark

```text
Attached: (1) a sprite sheet of 6 animation frames of one game character, laid out left to right, top to bottom in a 4 x 2 grid (empty cells are unused), each cell one frame; (2) and (3) style references: an approved dialogue portrait and an approved background of the same game.
Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small clean eyes, the same palette treatment and density as the references.
A slime enemy from the reference; keep its body colours exactly. Frames in order: idle, idle, move and melee attack, move and melee attack, hit, death. The rim colour is the slime's outline; give it no extra black line outside the rim.
Keep the same grid layout and frame order. In every frame keep the character at the same position inside its cell (same baseline, same left and right extent), the same pose and action, the same colours and decorations, so the frames still animate together and the character is recognisable. Keep each frame's expression (for example a squeezed >_< face when hit). 
Keep areas clean and flat: no scattered single pixels, no specks or noise inside the body; texture is shown with a few clear marks only. Each cell is an exact 35x30 pixel grid, so the whole sheet is 140x60 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, at most about 16 colours. Transparent background. No text, no grid lines.
```

## Melee_Neon

```text
Attached: (1) a sprite sheet of 6 animation frames of one game character, laid out left to right, top to bottom in a 4 x 2 grid (empty cells are unused), each cell one frame; (2) and (3) style references: an approved dialogue portrait and an approved background of the same game.
Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small clean eyes, the same palette treatment and density as the references.
A slime enemy from the reference; keep its body colours and its decoration (horns, crown, fuse, sprout, cannon, legs and so on) exactly. Frames in order: idle, idle, move and melee attack, move and melee attack, hit, death. The rim colour is the slime's outline; give it no extra black line outside the rim.
Keep the same grid layout and frame order. In every frame keep the character at the same position inside its cell (same baseline, same left and right extent), the same pose and action, the same colours and decorations, so the frames still animate together and the character is recognisable. Keep each frame's expression (for example a squeezed >_< face when hit). 
Each cell is an exact 35x30 pixel grid, so the whole sheet is 140x60 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, at most about 16 colours. Transparent background. No text, no grid lines.
```

## Melee_Rock

```text
Attached: (1) a sprite sheet of 6 animation frames of one game character, laid out left to right, top to bottom in a 4 x 2 grid (empty cells are unused), each cell one frame; (2) and (3) style references: an approved dialogue portrait and an approved background of the same game.
Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small clean eyes, the same palette treatment and density as the references.
A slime enemy from the reference; keep its body colours and its decoration (horns, crown, fuse, sprout, cannon, legs and so on) exactly. Frames in order: idle, idle, move and melee attack, move and melee attack, hit, death. The rim colour is the slime's outline; give it no extra black line outside the rim.
Keep the same grid layout and frame order. In every frame keep the character at the same position inside its cell (same baseline, same left and right extent), the same pose and action, the same colours and decorations, so the frames still animate together and the character is recognisable. Keep each frame's expression (for example a squeezed >_< face when hit). 
Each cell is an exact 35x32 pixel grid, so the whole sheet is 140x64 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, at most about 16 colours. Transparent background. No text, no grid lines.
```

## Melee_Stone

```text
Attached: (1) a sprite sheet of 6 animation frames of one game character, laid out left to right, top to bottom in a 4 x 2 grid (empty cells are unused), each cell one frame; (2) and (3) style references: an approved dialogue portrait and an approved background of the same game.
Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small clean eyes, the same palette treatment and density as the references.
A slime enemy from the reference; keep its body colours and its decoration (horns, crown, fuse, sprout, cannon, legs and so on) exactly. Frames in order: idle, idle, move and melee attack, move and melee attack, hit, death. The rim colour is the slime's outline; give it no extra black line outside the rim.
Keep the same grid layout and frame order. In every frame keep the character at the same position inside its cell (same baseline, same left and right extent), the same pose and action, the same colours and decorations, so the frames still animate together and the character is recognisable. Keep each frame's expression (for example a squeezed >_< face when hit). 
Each cell is an exact 35x30 pixel grid, so the whole sheet is 140x60 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, at most about 16 colours. Transparent background. No text, no grid lines.
```

## Ranged_Aqua

```text
Attached: (1) a sprite sheet of 8 animation frames of one game character, laid out left to right, top to bottom in a 4 x 2 grid (empty cells are unused), each cell one frame; (2) and (3) style references: an approved dialogue portrait and an approved background of the same game.
Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small clean eyes, the same palette treatment and density as the references.
A slime enemy from the reference; keep its body colours exactly. Frames in order: idle, idle, move, move, hit, death, ranged attack, ranged attack. The rim colour is the slime's outline; give it no extra black line outside the rim.
Keep the same grid layout and frame order. In every frame keep the character at the same position inside its cell (same baseline, same left and right extent), the same pose and action, the same colours and decorations, so the frames still animate together and the character is recognisable. Keep each frame's expression (for example a squeezed >_< face when hit). 
Keep areas clean and flat: no scattered single pixels, no specks or noise inside the body; texture is shown with a few clear marks only. Each cell is an exact 35x30 pixel grid, so the whole sheet is 140x60 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, at most about 16 colours. Transparent background. No text, no grid lines.
```

## Ranged_Cat

```text
Attached: (1) a sprite sheet of 8 animation frames of one game character, laid out left to right, top to bottom in a 4 x 2 grid (empty cells are unused), each cell one frame; (2) and (3) style references: an approved dialogue portrait and an approved background of the same game.
Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small clean eyes, the same palette treatment and density as the references.
A slime enemy from the reference; keep its body colours and its decoration (cat ears and a thin tail) exactly. Frames in order: idle, idle, move, move, hit, death, ranged attack, ranged attack. The rim colour is the slime's outline; give it no extra black line outside the rim.
Keep the same grid layout and frame order. In every frame keep the character at the same position inside its cell (same baseline, same left and right extent), the same pose and action, the same colours and decorations, so the frames still animate together and the character is recognisable. Keep each frame's expression (for example a squeezed >_< face when hit). 
Keep areas clean and flat: no scattered single pixels, no specks or noise inside the body; texture is shown with a few clear marks only. Each cell is an exact 43x33 pixel grid, so the whole sheet is 172x66 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, at most about 16 colours. Transparent background. No text, no grid lines.
```

## Ranged_Ground

```text
Attached: (1) a sprite sheet of 8 animation frames of one game character, laid out left to right, top to bottom in a 4 x 2 grid (empty cells are unused), each cell one frame; (2) and (3) style references: an approved dialogue portrait and an approved background of the same game.
Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small clean eyes, the same palette treatment and density as the references.
A slime enemy from the reference; keep its body colours and its decoration (horns, crown, fuse, sprout, cannon, legs and so on) exactly. Frames in order: idle, idle, move, move, hit, death, ranged attack, ranged attack. The rim colour is the slime's outline; give it no extra black line outside the rim.
Keep the same grid layout and frame order. In every frame keep the character at the same position inside its cell (same baseline, same left and right extent), the same pose and action, the same colours and decorations, so the frames still animate together and the character is recognisable. Keep each frame's expression (for example a squeezed >_< face when hit). 
Each cell is an exact 35x30 pixel grid, so the whole sheet is 140x60 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, at most about 16 colours. Transparent background. No text, no grid lines.
```

## Ranged_Light

```text
Attached: (1) a sprite sheet of 8 animation frames of one game character, laid out left to right, top to bottom in a 4 x 2 grid (empty cells are unused), each cell one frame; (2) and (3) style references: an approved dialogue portrait and an approved background of the same game.
Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small clean eyes, the same palette treatment and density as the references.
A slime enemy from the reference; keep its body colours exactly. Frames in order: idle, idle, move, move, hit, death, ranged attack, ranged attack. The rim colour is the slime's outline; give it no extra black line outside the rim.
Keep the same grid layout and frame order. In every frame keep the character at the same position inside its cell (same baseline, same left and right extent), the same pose and action, the same colours and decorations, so the frames still animate together and the character is recognisable. Keep each frame's expression (for example a squeezed >_< face when hit). 
Keep areas clean and flat: no scattered single pixels, no specks or noise inside the body; texture is shown with a few clear marks only. Each cell is an exact 35x30 pixel grid, so the whole sheet is 140x60 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, at most about 16 colours. Transparent background. No text, no grid lines.
```

## Ranged_Purple

```text
Attached: (1) a sprite sheet of 8 animation frames of one game character, laid out left to right, top to bottom in a 4 x 2 grid (empty cells are unused), each cell one frame; (2) and (3) style references: an approved dialogue portrait and an approved background of the same game.
Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small clean eyes, the same palette treatment and density as the references.
A slime enemy from the reference; keep its body colours and its decoration (horns, crown, ears, legs, shield, crystals and so on) exactly. Frames in order: idle, idle, move, move, hit, death, ranged attack, ranged attack. The rim colour is the slime's outline; give it no extra black line outside the rim.
Keep the same grid layout and frame order. In every frame keep the character at the same position inside its cell (same baseline, same left and right extent), the same pose and action, the same colours and decorations, so the frames still animate together and the character is recognisable. Keep each frame's expression (for example a squeezed >_< face when hit). 
Each cell is an exact 35x30 pixel grid, so the whole sheet is 140x60 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, at most about 16 colours. Transparent background. No text, no grid lines.
```

## Boss_Undead

```text
Attached: (1) a sprite sheet of 8 animation frames of one game character, laid out left to right, top to bottom in a 4 x 2 grid (empty cells are unused), each cell one frame; (2) and (3) style references: an approved dialogue portrait and an approved background of the same game.
Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small clean eyes, the same palette treatment and density as the references.
A boss slime from the reference; keep its body colours and its decoration (horns, crown, ears, legs, shield, crystals and so on) exactly. Frames in order: idle, idle, move, move, hit, death, ranged attack, ranged attack. The rim colour is the slime's outline; give it no extra black line outside the rim.
Keep the same grid layout and frame order. In every frame keep the character at the same position inside its cell (same baseline, same left and right extent), the same pose and action, the same colours and decorations, so the frames still animate together and the character is recognisable. Keep each frame's expression (for example a squeezed >_< face when hit). 
Each cell is an exact 72x54 pixel grid, so the whole sheet is 288x108 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, at most about 16 colours. Transparent background. No text, no grid lines.
```

## Shield

```text
Attached: (1) a sprite sheet of 8 animation frames of one game character, laid out left to right, top to bottom in a 4 x 2 grid (empty cells are unused), each cell one frame; (2) and (3) style references: an approved dialogue portrait and an approved background of the same game.
Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small clean eyes, the same palette treatment and density as the references.
A slime enemy from the reference; keep its body colours and its decoration (horns, crown, ears, legs, shield, crystals and so on) exactly. Frames in order: idle, idle, move, move, hit, death, ranged attack, ranged attack. The rim colour is the slime's outline; give it no extra black line outside the rim.
Keep the same grid layout and frame order. In every frame keep the character at the same position inside its cell (same baseline, same left and right extent), the same pose and action, the same colours and decorations, so the frames still animate together and the character is recognisable. Keep each frame's expression (for example a squeezed >_< face when hit). 
Each cell is an exact 58x38 pixel grid, so the whole sheet is 232x76 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, at most about 16 colours. Transparent background. No text, no grid lines.
```

## Surge

```text
Attached: (1) a sprite sheet of 8 animation frames of one game character, laid out left to right, top to bottom in a 4 x 2 grid (empty cells are unused), each cell one frame; (2) and (3) style references: an approved dialogue portrait and an approved background of the same game.
Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small clean eyes, the same palette treatment and density as the references.
A boss slime from the reference; keep its body colours and its decoration (horns, crown, ears, legs, shield, crystals and so on) exactly. Frames in order: idle, idle, move, move, hit, death, ranged attack, ranged attack. The rim colour is the slime's outline; give it no extra black line outside the rim.
Keep the same grid layout and frame order. In every frame keep the character at the same position inside its cell (same baseline, same left and right extent), the same pose and action, the same colours and decorations, so the frames still animate together and the character is recognisable. Keep each frame's expression (for example a squeezed >_< face when hit). 
Each cell is an exact 78x80 pixel grid, so the whole sheet is 312x160 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, at most about 16 colours. Transparent background. No text, no grid lines.
```

## Charge

```text
Attached: (1) a sprite sheet of 10 animation frames of one game character, laid out left to right, top to bottom in a 4 x 3 grid (empty cells are unused), each cell one frame; (2) and (3) style references: an approved dialogue portrait and an approved background of the same game.
Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small clean eyes, the same palette treatment and density as the references.
A slime enemy from the reference; keep its body colours and its decoration (horns, crown, fuse, sprout, cannon, legs and so on) exactly. Frames in order: idle, idle, move, move, hit, death, ranged attack, ranged attack, then two warning frames: crouching low with angry brows before charging. The rim colour is the slime's outline; give it no extra black line outside the rim.
Keep the same grid layout and frame order. In every frame keep the character at the same position inside its cell (same baseline, same left and right extent), the same pose and action, the same colours and decorations, so the frames still animate together and the character is recognisable. Keep each frame's expression (for example a squeezed >_< face when hit). 
Each cell is an exact 45x39 pixel grid, so the whole sheet is 180x117 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, at most about 16 colours. Transparent background. No text, no grid lines.
```

## Explode

```text
Attached: (1) a sprite sheet of 10 animation frames of one game character, laid out left to right, top to bottom in a 4 x 3 grid (empty cells are unused), each cell one frame; (2) and (3) style references: an approved dialogue portrait and an approved background of the same game.
Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small clean eyes, the same palette treatment and density as the references.
A slime enemy from the reference; keep its body colours and its decoration (horns, crown, fuse, sprout, cannon, legs and so on) exactly. Frames in order: idle, idle, move, move, hit, death, ranged attack, ranged attack, then two warning frames: the fuse on top burns brighter and the body flashes and swells before exploding. The rim colour is the slime's outline; give it no extra black line outside the rim.
Keep the same grid layout and frame order. In every frame keep the character at the same position inside its cell (same baseline, same left and right extent), the same pose and action, the same colours and decorations, so the frames still animate together and the character is recognisable. Keep each frame's expression (for example a squeezed >_< face when hit). 
Each cell is an exact 35x38 pixel grid, so the whole sheet is 140x114 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, at most about 16 colours. Transparent background. No text, no grid lines.
```

## Heal

```text
Attached: (1) a sprite sheet of 8 animation frames of one game character, laid out left to right, top to bottom in a 4 x 2 grid (empty cells are unused), each cell one frame; (2) and (3) style references: an approved dialogue portrait and an approved background of the same game.
Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small clean eyes, the same palette treatment and density as the references.
A slime enemy from the reference; keep its body colours and its decoration (horns, crown, fuse, sprout, cannon, legs and so on) exactly. Frames in order: idle, idle, move, move, hit, death, ranged attack, ranged attack. The rim colour is the slime's outline; give it no extra black line outside the rim.
Keep the same grid layout and frame order. In every frame keep the character at the same position inside its cell (same baseline, same left and right extent), the same pose and action, the same colours and decorations, so the frames still animate together and the character is recognisable. Keep each frame's expression (for example a squeezed >_< face when hit). 
Each cell is an exact 35x37 pixel grid, so the whole sheet is 140x74 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, at most about 16 colours. Transparent background. No text, no grid lines.
```

## Mortar

```text
Attached: (1) a sprite sheet of 8 animation frames of one game character, laid out left to right, top to bottom in a 4 x 2 grid (empty cells are unused), each cell one frame; (2) and (3) style references: an approved dialogue portrait and an approved background of the same game.
Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small clean eyes, the same palette treatment and density as the references.
A slime enemy from the reference; keep its body colours and its decoration (horns, crown, fuse, sprout, cannon, legs and so on) exactly. Frames in order: idle, idle, move, move, hit, death, ranged attack, ranged attack. The rim colour is the slime's outline; give it no extra black line outside the rim.
Keep the same grid layout and frame order. In every frame keep the character at the same position inside its cell (same baseline, same left and right extent), the same pose and action, the same colours and decorations, so the frames still animate together and the character is recognisable. Keep each frame's expression (for example a squeezed >_< face when hit). 
Each cell is an exact 35x35 pixel grid, so the whole sheet is 140x70 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, at most about 16 colours. Transparent background. No text, no grid lines.
```

## Split

```text
Attached: (1) a sprite sheet of 8 animation frames of one game character, laid out left to right, top to bottom in a 4 x 2 grid (empty cells are unused), each cell one frame; (2) and (3) style references: an approved dialogue portrait and an approved background of the same game.
Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small clean eyes, the same palette treatment and density as the references.
A slime enemy from the reference; keep its body colours and its decoration (horns, crown, fuse, sprout, cannon, legs and so on) exactly. Frames in order: idle, idle, move, move, hit, death, ranged attack, ranged attack. The rim colour is the slime's outline; give it no extra black line outside the rim.
Keep the same grid layout and frame order. In every frame keep the character at the same position inside its cell (same baseline, same left and right extent), the same pose and action, the same colours and decorations, so the frames still animate together and the character is recognisable. Keep each frame's expression (for example a squeezed >_< face when hit). 
Each cell is an exact 35x29 pixel grid, so the whole sheet is 140x58 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, at most about 16 colours. Transparent background. No text, no grid lines.
```

## Word

첫 결과는 눈이 가늘고 날카롭게 나오고 입이 빠져 다시 그렸다. 후보 세 개 중 대화 초상화(스토리) 얼굴에 맞춘 것을 골랐다. 첨부: develop 원래 프레임 시트, 승인된 WordDialogue.png 의 얼굴 부분 8배 확대본. 둘째 줄 칸 위쪽에 그려진 칸 선 조각은 지웠다.

```text
Attached: (1) a sprite sheet of 7 animation frames of Word, the player character, laid out left to right, top to bottom in a 4 x 2 grid (the last cell is unused), each cell one frame; the other attachments are style references of the same game.
Redraw sheet (1) as true pixel art: a chibi red-haired mage girl with a yellow ribbon, black cloak, red scarf, blue dress, holding a long staff with a blue orb. Frames in order: idle, idle, idle, attack swing, attack follow-through, hit, death (sitting and crying).
She must look cute and gentle like reference (1): big ROUND magenta eyes with a small white highlight, a small calm mouth, a soft round face, soft rounded orange-red bob hair. Never draw narrow, slanted, sharp or angry eyes, and do not leave out the mouth.
Keep the same grid layout and frame order. In every frame keep her at the same position inside its cell (same baseline, same left and right extent), the same pose, action and expression as reference (1), the same colours. Keep areas clean and flat: no scattered single pixels.
Each cell is an exact 68x69 pixel grid, so the whole sheet is 272x138 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, at most about 20 colours, flat 2 to 3 tone shading lit from the top left. Transparent background. No text, no grid lines.
Attachment (2) is a close-up of her face from her approved dialogue portrait: draw her face, eyes, mouth and hair in the battle sprites the same way, simplified to this small size. Outline: a clean 1-pixel dark outline.
```

## Projectiles

```text
Attached: (1) a sprite sheet of 14 animation frames of one game character, laid out left to right, top to bottom in a 4 x 4 grid (empty cells are unused), each cell one frame; (2) and (3) style references: an approved dialogue portrait and an approved background of the same game.
Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small clean eyes, the same palette treatment and density as the references.
Small round magic projectiles fired by slimes: 7 colours (blue, dark green, green, light green, purple, sky blue, yellow), two animation frames each, in that order. Each is a glowing orb with a short trail to its right made of two or three solid streaks in lighter and darker shades of the orb's own colour. No brown or black specks, no scattered dots; the orb and trail fill most of the cell like the reference.
Keep the same grid layout and frame order. In every frame keep the character at the same position inside its cell (same baseline, same left and right extent), the same pose and action, the same colours and decorations, so the frames still animate together and the character is recognisable. Keep each frame's expression (for example a squeezed >_< face when hit). 
Keep areas clean and flat: no scattered single pixels, no specks or noise inside the body; texture is shown with a few clear marks only. Each cell is an exact 21x24 pixel grid, so the whole sheet is 84x96 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, at most about 16 colours. Transparent background. No text, no grid lines.
```

## WordHead

```text
Attached: (1) a sprite sheet of 2 animation frames of one game character, laid out left to right, top to bottom in a 4 x 1 grid (empty cells are unused), each cell one frame; (2) and (3) style references: an approved dialogue portrait and an approved background of the same game.
Redraw sheet (1) as true pixel art in the unified art style of references (2) and (3): a clean 1-pixel outline in a darker shade of each area's colour (not pure black), flat shading with 2 to 3 tones per colour and light from the top left, simple bold readable shapes, small clean eyes, the same palette treatment and density as the references.
A chibi head icon of Word: red-orange hair, big yellow ribbon on top, large magenta eyes, pink blushing cheeks, a small happy cat-like smile. The head fills the whole cell exactly like the reference: ribbon at the top edge, hair reaching the bottom edge, face in the lower middle. Frame 2 is the same head tilted slightly to the right.
Keep the same grid layout and frame order. In every frame keep the character at the same position inside its cell (same baseline, same left and right extent), the same pose and action, the same colours and decorations, so the frames still animate together and the character is recognisable. Keep each frame's expression (for example a squeezed >_< face when hit). 
Keep areas clean and flat: no scattered single pixels, no specks or noise inside the body; texture is shown with a few clear marks only. Each cell is an exact 31x30 pixel grid, so the whole sheet is 124x30 pixels: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, at most about 16 colours. Transparent background. No text, no grid lines.
```

## 제목 글자, 튜토리얼 손, 상태 이상 오버레이

같은 스타일 참고 그림 두 장과 develop 의 원래 그림을 첨부했다.

### NewCardTitle

```text
Attached: (1) the current game heading image, (2) and (3) style references. Redraw (1) as true pixel art in the unified art style of the attached approved portrait and background: a clean 1-pixel outline in a darker shade of each colour, flat 2 to 3 tone shading, simple bold readable shapes. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, no scattered single pixels. The heading reads exactly NEW CARD in one line of chunky blocky tan-gold letters with a lighter top highlight, a darker bottom shade and a thick dark brown outline, like the reference. Draw it on an exact 260x39 pixel grid filling the grid edge to edge (every art pixel one equal square cell), then enlarge with nearest-neighbour by an integer factor. Transparent background. No other text.
```

### GameOverTitle

```text
Attached: (1) the current game heading image, (2) and (3) style references. Redraw (1) as true pixel art in the unified art style of the attached approved portrait and background: a clean 1-pixel outline in a darker shade of each colour, flat 2 to 3 tone shading, simple bold readable shapes. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, no scattered single pixels. The heading reads exactly GAME OVER in one line of chunky blocky tan-gold letters with a lighter top highlight, a darker bottom shade and a thick dark brown outline, like the reference. Draw it on an exact 227x37 pixel grid filling the grid edge to edge (every art pixel one equal square cell), then enlarge with nearest-neighbour by an integer factor. Transparent background. No other text.
```

### DragHand

```text
Attached: (1) the current tutorial hand image, (2) and (3) style references. Redraw (1) as true pixel art in the unified art style of the attached approved portrait and background: a clean 1-pixel outline in a darker shade of each colour, flat 2 to 3 tone shading, simple bold readable shapes. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, no scattered single pixels. A white cartoon glove hand pointing with the index finger up and to the upper left, the fingertip near the upper-left of the image like the reference, with a yellow cuff. Draw it on an exact 18x18 pixel grid (every art pixel one equal square cell), then enlarge with nearest-neighbour by an integer factor. Transparent background.
```

### StatusOverlay

```text
Attached: (1) the current status-effect overlay atlas: 5 rows (burn flames, chill frost, frozen ice crystals, shock lightning, fracture rock cracks) by 4 columns (animation frames), each effect drawn as a ring around where an enemy stands, (2) and (3) style references. Redraw (1) as true pixel art in the unified art style of the attached approved portrait and background: a clean 1-pixel outline in a darker shade of each colour, flat 2 to 3 tone shading, simple bold readable shapes. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, no scattered single pixels. Keep the same 4 x 5 grid, the same effect in each row, the same ring shape and position inside each cell, and the frame-to-frame motion. Each cell is an exact 51x51 pixel grid, so the whole atlas is 204x255 pixels: every art pixel one equal square cell, then enlarge with nearest-neighbour by an integer factor. Transparent background, no soft glow, no text, no grid lines.
```
