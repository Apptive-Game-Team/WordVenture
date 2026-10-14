# 대화 초상화 재생성 프롬프트

대화 초상화 25장을 170x170 픽셀 격자로 다시 그릴 때 쓴 Codex image_gen 프롬프트다(#116). 모든 결과는 `pixelate.py --target 170x170 --keep-lines 2`로 변환했다. 화면에서는 680x680px(아트 1픽셀 = 4px)로 보인다.

## 워드와 1부 인물 8장

WordDialogue.png 는 동작 초상화 12장과 머리 크기·위치를 맞추려고 한 번 더 그렸다. 참고 이미지로 develop 의 원본 WordDialogue, 새 WordNodDialogue 4배 확대본, 앞서 재생성한 WordDialogue 4배 확대본(밀도·외곽선 참고, 프레이밍은 따르지 않음)을 첨부했다.

`fix/pixel-perfect-backgrounds`에서 만들었다. 참고 이미지로 b7c2ed0 시점 원본, 당시 170x170 PNG의 4배 확대본, 워드 전투 스프라이트(워드) 또는 새 WordDialogue.png 4배 확대본(나머지 7장)을 첨부했다.


### WordDialogue (프레이밍 맞춤, 최종)

```text
Attached: (1) the original dialogue portrait of the heroine Word (composition, framing, head size and head position reference), (2) the approved pixel-art version of Word's nodding gesture portrait, enlarged 4x: it is drawn on the same 170x170 grid and its head size, head position and bust framing are exactly what this portrait must match, (3) an earlier pixel-art version of this same portrait, enlarged 4x: copy its pixel density, outline style and palette, but NOT its framing (its head is too large and too high).

Redraw reference 1 as true pixel art for the game WordVenture.
Dialogue portrait of Word, a young red-haired mage girl with a yellow ribbon, black cloak, red scarf, holding a staff with a blue orb, calm neutral face, bust shot.
Place the head, ribbon, shoulders and staff so that this portrait lines up with reference 2 when the two are swapped in the same dialogue slot: same head size, same eye height, same shoulder line.
Draw it on an exact 170x170 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 32 colours, clean 1-pixel dark outline.
Transparent background. No text, UI or logos.
```

## 2부 인물과 워드 동작 초상화 17장

원본 1254x1254 PNG, 새 WordDialogue.png 4배 확대본, 새 VillagerDialogue.png 4배 확대본(2부 인물만)을 첨부했다.


### CanyonRanger (attempt 1)
References: CanyonRangerDialogue.png (original 1254x1254); WordDialogue.png 4x nearest (680x680); VillagerDialogue.png 4x nearest (680x680)

```text
Attached: (1) the original dialogue portrait of this character (composition, design, pose and colour reference), (2) the already approved dialogue portrait of the heroine Word, enlarged 4x: match its pixel density, outline style, palette treatment, head size and bust framing exactly so all portraits look like one set, (3) another approved portrait of the same set.

Redraw the attached reference image as true pixel art for the game WordVenture.
A young woman canyon ranger, short dark brown hair, tan skin, brown leather vest with a small yellow star badge, coiled rope on one shoulder, serious face looking to the left, waist-up.
Keep the same composition, character design, pose and colours as reference 1.
Draw it on an exact 170x170 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 32 colours, clean 1-pixel dark outline.
Transparent background. No text, UI or logos. Match the palette and density of the other attached game art.
```

### ForestRefugee (attempt 1)
References: ForestRefugeeDialogue.png (original 1254x1254); WordDialogue.png 4x nearest (680x680); VillagerDialogue.png 4x nearest (680x680)

```text
Attached: (1) the original dialogue portrait of this character (composition, design, pose and colour reference), (2) the already approved dialogue portrait of the heroine Word, enlarged 4x: match its pixel density, outline style, palette treatment, head size and bust framing exactly so all portraits look like one set, (3) another approved portrait of the same set.

Redraw the attached reference image as true pixel art for the game WordVenture.
A girl about twelve, black hair in one long braid, torn grey-green cloak, holding a small glowing lantern, frightened but brave face, waist-up.
Keep the same composition, character design, pose and colours as reference 1.
Draw it on an exact 170x170 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 32 colours, clean 1-pixel dark outline.
Transparent background. No text, UI or logos. Match the palette and density of the other attached game art.
```

### FrostVillager (attempt 1)
References: FrostVillagerDialogue.png (original 1254x1254); WordDialogue.png 4x nearest (680x680); VillagerDialogue.png 4x nearest (680x680)

```text
Attached: (1) the original dialogue portrait of this character (composition, design, pose and colour reference), (2) the already approved dialogue portrait of the heroine Word, enlarged 4x: match its pixel density, outline style, palette treatment, head size and bust framing exactly so all portraits look like one set, (3) another approved portrait of the same set.

Redraw the attached reference image as true pixel art for the game WordVenture.
A young man from a snowy village, brown hood with white fur trim pushed back, short dark blond hair, rosy cheeks, blue scarf, hands held near chest, worried but hopeful face, waist-up.
Keep the same composition, character design, pose and colours as reference 1.
Draw it on an exact 170x170 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 32 colours, clean 1-pixel dark outline.
Transparent background. No text, UI or logos. Match the palette and density of the other attached game art.
```

### MagicSurge (attempt 1)
References: MagicSurgeDialogue.png (original 1254x1254); WordDialogue.png 4x nearest (680x680); VillagerDialogue.png 4x nearest (680x680)

```text
Attached: (1) the original dialogue portrait of this character (composition, design, pose and colour reference), (2) the already approved dialogue portrait of the heroine Word, enlarged 4x: match its pixel density, outline style, palette treatment, head size and bust framing exactly so all portraits look like one set, (3) another approved portrait of the same set.

Redraw the attached reference image as true pixel art for the game WordVenture.
The final boss: a big slime made of runaway magic, rounded translucent body with swirls of gold, green and violet, a few simple crystals inside, two big white eyes with a sad angry look. It fills the same area as a waist-up character.
Keep the same composition, character design, pose and colours as reference 1.
Draw it on an exact 170x170 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 32 colours, clean 1-pixel dark outline.
Transparent background. No text, UI or logos. Match the palette and density of the other attached game art.
```

### RuinsScholar (attempt 1)
References: RuinsScholarDialogue.png (original 1254x1254); WordDialogue.png 4x nearest (680x680); VillagerDialogue.png 4x nearest (680x680)

```text
Attached: (1) the original dialogue portrait of this character (composition, design, pose and colour reference), (2) the already approved dialogue portrait of the heroine Word, enlarged 4x: match its pixel density, outline style, palette treatment, head size and bust framing exactly so all portraits look like one set, (3) another approved portrait of the same set.

Redraw the attached reference image as true pixel art for the game WordVenture.
A kind old scholar, round glasses, short grey beard, wavy grey hair, dark green hooded robe, holding a small brown book, curious smiling face, waist-up.
Keep the same composition, character design, pose and colours as reference 1.
Draw it on an exact 170x170 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 32 colours, clean 1-pixel dark outline.
Transparent background. No text, UI or logos. Match the palette and density of the other attached game art.
```

### WordNod (attempt 1)
References: WordNodDialogue.png (original 1254x1254); WordDialogue.png 4x nearest (680x680)

```text
Attached: (1) the original dialogue portrait of this character (composition, design, pose and colour reference), (2) the already approved dialogue portrait of the heroine Word, enlarged 4x: match its pixel density, outline style, palette treatment, head size and bust framing exactly so all portraits look like one set.

Redraw the attached reference image as true pixel art for the game WordVenture.
Word, the heroine: red bob hair, yellow ribbon headband, black hooded cloak, red scarf, dark blue dress. Same character as reference 2, only pose and expression differ: gentle nod: head dipped slightly, eyes gently closed, small soft smile, staff with blue orb held at her right side as in reference 1.
Keep the same composition, character design, pose and colours as reference 1.
Draw it on an exact 170x170 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 32 colours, clean 1-pixel dark outline.
Transparent background. No text, UI or logos. Match the palette and density of the other attached game art.
```

### WordNodFirm (attempt 1)
References: WordNodFirmDialogue.png (original 1254x1254); WordDialogue.png 4x nearest (680x680)

```text
Attached: (1) the original dialogue portrait of this character (composition, design, pose and colour reference), (2) the already approved dialogue portrait of the heroine Word, enlarged 4x: match its pixel density, outline style, palette treatment, head size and bust framing exactly so all portraits look like one set.

Redraw the attached reference image as true pixel art for the game WordVenture.
Word, the heroine: red bob hair, yellow ribbon headband, black hooded cloak, red scarf, dark blue dress. Same character as reference 2, only pose and expression differ: firm nod of resolve: eyes open looking straight ahead, eyebrows low and straight, lips pressed, determined, staff with blue orb held as in reference 1.
Keep the same composition, character design, pose and colours as reference 1.
Draw it on an exact 170x170 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 32 colours, clean 1-pixel dark outline.
Transparent background. No text, UI or logos. Match the palette and density of the other attached game art.
```

### WordPalmOut (attempt 1)
References: WordPalmOutDialogue.png (original 1254x1254); WordDialogue.png 4x nearest (680x680)

```text
Attached: (1) the original dialogue portrait of this character (composition, design, pose and colour reference), (2) the already approved dialogue portrait of the heroine Word, enlarged 4x: match its pixel density, outline style, palette treatment, head size and bust framing exactly so all portraits look like one set.

Redraw the attached reference image as true pixel art for the game WordVenture.
Word, the heroine: red bob hair, yellow ribbon headband, black hooded cloak, red scarf, dark blue dress. Same character as reference 2, only pose and expression differ: 'wait here' pose: one arm extended toward the viewer's right with the open palm facing outward, calm face with a very slight smile; the other hand holds the staff with blue orb on the left as in reference 1.
Keep the same composition, character design, pose and colours as reference 1.
Draw it on an exact 170x170 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 32 colours, clean 1-pixel dark outline.
Transparent background. No text, UI or logos. Match the palette and density of the other attached game art.
```

### WordPoint (attempt 1)
References: WordPointDialogue.png (original 1254x1254); WordDialogue.png 4x nearest (680x680)

```text
Attached: (1) the original dialogue portrait of this character (composition, design, pose and colour reference), (2) the already approved dialogue portrait of the heroine Word, enlarged 4x: match its pixel density, outline style, palette treatment, head size and bust framing exactly so all portraits look like one set.

Redraw the attached reference image as true pixel art for the game WordVenture.
Word, the heroine: red bob hair, yellow ribbon headband, black hooded cloak, red scarf, dark blue dress. Same character as reference 2, only pose and expression differ: pointing, serious: one arm extended to the viewer's right with the index finger pointing, serious focused eyes; the other hand holds the staff with blue orb on the left as in reference 1.
Keep the same composition, character design, pose and colours as reference 1.
Draw it on an exact 170x170 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 32 colours, clean 1-pixel dark outline.
Transparent background. No text, UI or logos. Match the palette and density of the other attached game art.
```

### WordPointWorried (attempt 1)
References: WordPointWorriedDialogue.png (original 1254x1254); WordDialogue.png 4x nearest (680x680)

```text
Attached: (1) the original dialogue portrait of this character (composition, design, pose and colour reference), (2) the already approved dialogue portrait of the heroine Word, enlarged 4x: match its pixel density, outline style, palette treatment, head size and bust framing exactly so all portraits look like one set.

Redraw the attached reference image as true pixel art for the game WordVenture.
Word, the heroine: red bob hair, yellow ribbon headband, black hooded cloak, red scarf, dark blue dress. Same character as reference 2, only pose and expression differ: pointing, worried: one arm extended to the viewer's right with the index finger pointing, eyebrows raised and drawn together in worry; the other hand holds the staff with blue orb on the left as in reference 1.
Keep the same composition, character design, pose and colours as reference 1.
Draw it on an exact 170x170 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 32 colours, clean 1-pixel dark outline.
Transparent background. No text, UI or logos. Match the palette and density of the other attached game art.
```

### WordRaiseCard (attempt 2)
References: WordRaiseCardDialogue.png (original 1254x1254); WordDialogue.png 4x nearest (680x680)

```text
Attached: (1) the original dialogue portrait of this character (composition, design, pose and colour reference), (2) the already approved dialogue portrait of the heroine Word, enlarged 4x: match its pixel density, outline style, palette treatment, head size and bust framing exactly so all portraits look like one set.

Redraw the attached reference image as true pixel art for the game WordVenture.
Word, the heroine: red bob hair, yellow ribbon headband, black hooded cloak, red scarf, dark blue dress. Same character as reference 2, only pose and expression differ: raising a card, determined: one hand raised to shoulder height holding a plain cream card with a pink border and no text, lowered eyebrows and a flat serious determined expression: the mouth is a single short flat straight line, NOT a smile; the other hand holds the staff with blue orb as in reference 1.
Keep the same composition, character design, pose and colours as reference 1.
Draw it on an exact 170x170 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 32 colours, clean 1-pixel dark outline.
Transparent background. No text, UI or logos. Match the palette and density of the other attached game art.
```

Attempt 1 prompt differed only by lacking the 'flat serious determined expression: mouth a single short flat straight line, NOT a smile' clause; result smiled, so rejected.

### WordShakeHead (attempt 1)
References: WordShakeHeadDialogue.png (original 1254x1254); WordDialogue.png 4x nearest (680x680)

```text
Attached: (1) the original dialogue portrait of this character (composition, design, pose and colour reference), (2) the already approved dialogue portrait of the heroine Word, enlarged 4x: match its pixel density, outline style, palette treatment, head size and bust framing exactly so all portraits look like one set.

Redraw the attached reference image as true pixel art for the game WordVenture.
Word, the heroine: red bob hair, yellow ribbon headband, black hooded cloak, red scarf, dark blue dress. Same character as reference 2, only pose and expression differ: sad head shake: head turned slightly, eyes half closed looking down, eyebrows drawn up in sorrow, small closed frown, staff with blue orb as in reference 1.
Keep the same composition, character design, pose and colours as reference 1.
Draw it on an exact 170x170 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 32 colours, clean 1-pixel dark outline.
Transparent background. No text, UI or logos. Match the palette and density of the other attached game art.
```

### WordShakeHeadFirm (attempt 1)
References: WordShakeHeadFirmDialogue.png (original 1254x1254); WordDialogue.png 4x nearest (680x680)

```text
Attached: (1) the original dialogue portrait of this character (composition, design, pose and colour reference), (2) the already approved dialogue portrait of the heroine Word, enlarged 4x: match its pixel density, outline style, palette treatment, head size and bust framing exactly so all portraits look like one set.

Redraw the attached reference image as true pixel art for the game WordVenture.
Word, the heroine: red bob hair, yellow ribbon headband, black hooded cloak, red scarf, dark blue dress. Same character as reference 2, only pose and expression differ: defiant head shake: eyes open glaring firmly to the viewer's right, eyebrows lowered, lips pressed, staff with blue orb gripped tightly as in reference 1.
Keep the same composition, character design, pose and colours as reference 1.
Draw it on an exact 170x170 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 32 colours, clean 1-pixel dark outline.
Transparent background. No text, UI or logos. Match the palette and density of the other attached game art.
```

### WordShowBook (attempt 1)
References: WordShowBookDialogue.png (original 1254x1254); WordDialogue.png 4x nearest (680x680)

```text
Attached: (1) the original dialogue portrait of this character (composition, design, pose and colour reference), (2) the already approved dialogue portrait of the heroine Word, enlarged 4x: match its pixel density, outline style, palette treatment, head size and bust framing exactly so all portraits look like one set.

Redraw the attached reference image as true pixel art for the game WordVenture.
Word, the heroine: red bob hair, yellow ribbon headband, black hooded cloak, red scarf, dark blue dress. Same character as reference 2, only pose and expression differ: holding an open spellbook upright in front of her chest with both hands, cream pages with a few dark handwriting lines and one small circle diagram, thin brown leather border; earnest serious face; no staff.
Keep the same composition, character design, pose and colours as reference 1.
Draw it on an exact 170x170 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 32 colours, clean 1-pixel dark outline.
Transparent background. No text, UI or logos. Match the palette and density of the other attached game art.
```

### WordShowCover (attempt 1)
References: WordShowCoverDialogue.png (original 1254x1254); WordDialogue.png 4x nearest (680x680)

```text
Attached: (1) the original dialogue portrait of this character (composition, design, pose and colour reference), (2) the already approved dialogue portrait of the heroine Word, enlarged 4x: match its pixel density, outline style, palette treatment, head size and bust framing exactly so all portraits look like one set.

Redraw the attached reference image as true pixel art for the game WordVenture.
Word, the heroine: red bob hair, yellow ribbon headband, black hooded cloak, red scarf, dark blue dress. Same character as reference 2, only pose and expression differ: holding a closed dark-brown leather spellbook with a gold circle emblem and soot smudges upright in front of her chest with both hands, cover facing the viewer; eyes glancing slightly aside, hesitant; no staff.
Keep the same composition, character design, pose and colours as reference 1.
Draw it on an exact 170x170 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 32 colours, clean 1-pixel dark outline.
Transparent background. No text, UI or logos. Match the palette and density of the other attached game art.
```

### WordTilt (attempt 1)
References: WordTiltDialogue.png (original 1254x1254); WordDialogue.png 4x nearest (680x680)

```text
Attached: (1) the original dialogue portrait of this character (composition, design, pose and colour reference), (2) the already approved dialogue portrait of the heroine Word, enlarged 4x: match its pixel density, outline style, palette treatment, head size and bust framing exactly so all portraits look like one set.

Redraw the attached reference image as true pixel art for the game WordVenture.
Word, the heroine: red bob hair, yellow ribbon headband, black hooded cloak, red scarf, dark blue dress. Same character as reference 2, only pose and expression differ: puzzled: head tilted to one side, eyes open and curious, eyebrows slightly raised, one finger touching her chin, staff with blue orb leaning on her shoulder.
Keep the same composition, character design, pose and colours as reference 1.
Draw it on an exact 170x170 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 32 colours, clean 1-pixel dark outline.
Transparent background. No text, UI or logos. Match the palette and density of the other attached game art.
```

### WordTouchThroat (attempt 1)
References: WordTouchThroatDialogue.png (original 1254x1254); WordDialogue.png 4x nearest (680x680)

```text
Attached: (1) the original dialogue portrait of this character (composition, design, pose and colour reference), (2) the already approved dialogue portrait of the heroine Word, enlarged 4x: match its pixel density, outline style, palette treatment, head size and bust framing exactly so all portraits look like one set.

Redraw the attached reference image as true pixel art for the game WordVenture.
Word, the heroine: red bob hair, yellow ribbon headband, black hooded cloak, red scarf, dark blue dress. Same character as reference 2, only pose and expression differ: one hand lightly touching her own throat, eyes cast down, eyebrows drawn up in sadness; the other hand holds the staff with blue orb as in reference 1.
Keep the same composition, character design, pose and colours as reference 1.
Draw it on an exact 170x170 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 32 colours, clean 1-pixel dark outline.
Transparent background. No text, UI or logos. Match the palette and density of the other attached game art.
```

## 워드 13장 눈·눈썹 다시 그리기

위에서 만든 워드 초상화 13장은 속눈썹 픽셀이 흩어지고, 눈썹이 점으로 끊기고, 두 눈 크기가 달랐다. 그래서 얼굴만 한 번 더 그렸다. 첨부: develop 의 원본 1254x1254 PNG, 그때의 170x170 PNG 4배 확대본. 프롬프트는 각 초상화의 위 프롬프트 앞에 아래 첨부 설명을 붙이고, 뒤에 얼굴 규칙을 붙였다. 결과는 `pixelate.py --target 170x170 --keep-lines 2` 로 변환했다.

```text
Attached: (1) the original high-resolution dialogue portrait of this character (expression and design reference), (2) the approved current pixel-art version on the 170x170 grid, enlarged 4x (framing, pixel density, outline and palette reference).

[각 초상화의 위 프롬프트]

FACE FIX, the main point of this redraw: reference (2) is the approved current version; keep its framing, head size, head position, pose, hands, staff, clothes and palette exactly, and redraw only the face more cleanly, following reference (1) for the expression.
- Both eyes are the same size and shape and sit at the same height (a turned head may make the far eye slightly narrower). Each eye: a clean dark upper eyelash line 2 pixels thick with one small flick at the outer corner, a magenta iris with a darker pupil area and one white highlight, a thin 1-pixel lower line. No extra lash strokes, no stray dark pixels around the eyes, no double lines above the eyes.
- Eyebrows: one short, thin, smoothly curved 1-pixel line per eye in a dark red-brown hair-shadow colour, just above the eye and partly under the bangs. Never scattered dots or broken dashes.
- Nose: at most one or two pixels. Mouth: one short clean line matching the expression.
- Hair strands over the forehead are clean lines, not noise.
```
