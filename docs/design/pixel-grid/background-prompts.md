# 배경 재생성 프롬프트

#116 에서 배경을 화면 4px 격자에 맞춰 Codex image_gen 으로 다시 그릴 때 쓴 프롬프트다. 결과는 `pixelate.py --target <W>x<H> --despeckle`(지도는 `--despeckle` 없이)로 변환했다.

## 스토리·타이틀·엔딩 배경 6장(480x270)

`fix/pixel-perfect-backgrounds` 에서 만든 결과를 썼다. PPU 100, 월드 너비 4.8.

아트 1픽셀 = 화면 4px 격자에 맞추려고 Codex image_gen으로 다시 그렸다. 참고 이미지로 AI 원본(b7c2ed0)과 현재 480x270 PNG의 4배 확대본을 첨부. 생성 원본 1672x941을 `pixelate.py --target 480x270 --despeckle`로 변환.

- MountainHome: Redraw the attached reference image as true pixel art for the game WordVenture. Attached: (1) the original AI image, (2) the current in-game 480x270 version enlarged 4x. Peaceful remote mountain valley home: one modest timber and stone cottage with a warm ochre thatched roof and chimney on the left, small vegetable garden and wooden fence, forest and layered blue mountains behind, wide sandy path across the foreground, grass strip at the bottom, clear blue sky. Keep the same composition, layout, landmarks and colours as the reference. Draw it on an exact 480x270 pixel grid: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor (output 1920x1080 if possible, otherwise 16:9). Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, limited palette of about 48 colours. Keep the bottom quarter calm for the dialogue overlay. No text, UI or logos. Match the palette and density of the other attached game art.

- WorldTreeEpilogue: Redraw the attached reference image as true pixel art for the game WordVenture. Attached: (1) the original AI image, (2) the current in-game 480x270 version enlarged 4x. A single majestic world tree with a broad emerald canopy and warm brown trunk with spreading roots in a calm forest clearing, sparse pale cyan magical flowers among the roots, distant blue mountains, pale blue sky, sandy path and bushes in the foreground. Keep the same composition, layout, landmarks and colours as the reference. Draw it on an exact 480x270 pixel grid: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor (output 1920x1080 if possible, otherwise 16:9). Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, limited palette of about 48 colours. Keep the bottom quarter calm for the dialogue overlay. No text, UI or logos. Match the palette and density of the other attached game art.

- MountainValley: Redraw the attached reference image as true pixel art for the game WordVenture. Attached: (1) the original AI image, (2) the current in-game 480x270 version enlarged 4x. Title screen backdrop: mountain valley with a winding ochre trail leading toward distant layered blue mountains, framing trees at far left and right, low bushes and small rocks. Keep the central 60 percent open and uncluttered for the title and menu overlay, airy blue sky upper half. Keep the same composition, layout, landmarks and colours as the reference. Draw it on an exact 480x270 pixel grid: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor (output 1920x1080 if possible, otherwise 16:9). Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, limited palette of about 48 colours. Keep the bottom quarter calm for the dialogue overlay. No text, UI or logos. Match the palette and density of the other attached game art.

- MountainHomeAttack(새 MountainHome 원본을 편집 대상으로 첨부): Attached: (1) EDIT TARGET, the newly redrawn peaceful cottage background: keep its exact cottage, mountains, trees, path, camera and pixel grid, (2) the old version of this scene showing the content to depict, (3) the current in-game version of this scene enlarged 4x. Edit image 1 into the ACTIVE ATTACK moment: several bright orange-red fireballs with yellow cores and angular pixel flame tails fly diagonally from upper right toward the cottage at the lower left; one strikes the roof edge with pixel sparks and small flames spreading across the thatch; dark smoke above the house; dramatic smoky blue-gray sky. Roof still mostly standing. No people, monsters, text or UI. Keep the bottom quarter calm for dialogue. Draw it on an exact 480x270 pixel grid: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, limited palette of about 48 colours. No text, UI or logos. Match the palette and density of the other attached game art.

- MountainHomeRuins(새 MountainHome 원본을 편집 대상으로 첨부): Attached: (1) EDIT TARGET, the newly redrawn peaceful cottage background: keep its exact cottage, mountains, trees, path, camera and pixel grid, (2) the old version of this scene showing the content to depict, (3) the current in-game version of this scene enlarged 4x. Edit image 1 into the aftermath: cottage roof collapsed inward with broken timber and stone rubble, damaged garden and fence, faint gray smoke rising, subdued blue-gray cloudy sky and slightly muted grass. The house must visibly be ruined. No people, bodies, monsters, text or UI. Keep the bottom quarter calm for dialogue. Draw it on an exact 480x270 pixel grid: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, limited palette of about 48 colours. No text, UI or logos. Match the palette and density of the other attached game art.

- MountainHomeMemorial(새 MountainHome 원본을 편집 대상으로 첨부): Attached: (1) EDIT TARGET, the newly redrawn peaceful cottage background: keep its exact cottage, mountains, trees, path, camera and pixel grid, (2) the old version of this scene showing the content to depict, (3) the current in-game version of this scene enlarged 4x. Edit image 1 into the bittersweet ending, long after the attack: the same cottage still damaged (collapsed roof, broken fence), grass growing around the rubble, no smoke or fire, a peaceful late-afternoon sky with warm golden light and soft clouds at the horizon, one small plain unlettered gray memorial stone to the right of the cottage with a few white wildflowers at its base. No people, text or UI. Keep the bottom quarter calm for dialogue. Draw it on an exact 480x270 pixel grid: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. Hard edges, no anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters, limited palette of about 48 colours. No text, UI or logos. Match the palette and density of the other attached game art.

## 전투·대화 배경과 게임오버·게임클리어·월드맵

월드맵 두 장(WorldMapDefault, ActTwoMap)은 아래 프롬프트로 다시 그렸지만 외곽선이 자글자글해서 쓰지 않고, 원래 지도를 줄여 만들었다.

참고 이미지로 develop 의 현재 그림 한 장을 첨부했다(월드맵은 씬 스케일 (1.19, 1)로 늘어난 화면 그대로). 크기: 전투·대화 배경 552x276(PPU 53.90625, 전투 배경 오브젝트 스케일 2), 게임오버·게임클리어 480x240(PPU 46.875), 월드맵 495x276(PPU 27, 오브젝트 스케일 1). 비 오는 배경은 비 없는 바탕만 그리고 빗줄기는 `rain_frames.py` 로 옮겼다. 게임오버 1·2프레임은 `gameover_frames.py`, 1부 월드맵 진행 4장은 `map_stages.py` 로 만들었다.

### Plain

```text
Attached: the current in-game image (composition, landmarks, colours and mood reference).
Redraw the attached reference image as true pixel art for the game WordVenture.
Plains battle backdrop: layered blue mountains under puffy light clouds, dark teal bushes, a bright grass band, a wide flat ochre dirt strip and a light green grass fringe at the bottom. Side-on battle backdrop; the bottom third is flat ground where characters stand.
Keep the same composition, layout, horizon heights, landmark positions and colours as the reference.
Draw it on an exact 552x276 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 40 colours. 
No text, UI or logos unless the reference has them. Output a landscape image with the same aspect ratio as the reference.
```

### Highland

```text
Attached: the current in-game image (composition, landmarks, colours and mood reference).
Redraw the attached reference image as true pixel art for the game WordVenture.
Desert highland battle backdrop: tall layered sandstone mesas and cliffs in tan, orange and cream, dusty pale sky, flat orange sand ground across the lower part. Side-on battle backdrop; the bottom third is flat ground where characters stand.
Keep the same composition, layout, horizon heights, landmark positions and colours as the reference.
Draw it on an exact 552x276 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 40 colours. 
No text, UI or logos unless the reference has them. Output a landscape image with the same aspect ratio as the reference.
```

### Sea

```text
Attached: the current in-game image (composition, landmarks, colours and mood reference).
Redraw the attached reference image as true pixel art for the game WordVenture.
Beach battle backdrop: blue sky with soft clouds, calm blue sea with a small island and a palm tree, a palm on the right edge, golden sand beach across the lower half with a few small starfish, crabs and grass tufts. Side-on battle backdrop; the bottom third is flat ground where characters stand.
Keep the same composition, layout, horizon heights, landmark positions and colours as the reference.
Draw it on an exact 552x276 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 40 colours. 
No text, UI or logos unless the reference has them. Output a landscape image with the same aspect ratio as the reference.
```

### DemonCastleHall (BossStage, 예전 이름 KakaoTalk_20240701_202830558)

```text
Attached: the current in-game image (composition, landmarks, colours and mood reference).
Redraw the attached reference image as true pixel art for the game WordVenture.
Dark ruined temple battle backdrop: a row of pale gray stone pillars in deep black shadow, a blood-red moon at the upper left between pillars, rough gray cobblestone ground across the lower part. Side-on battle backdrop; the bottom third is flat ground where characters stand.
Keep the same composition, layout, horizon heights, landmark positions and colours as the reference.
Draw it on an exact 552x276 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 32 colours. 
No text, UI or logos unless the reference has them. Output a landscape image with the same aspect ratio as the reference.
```

### AshenRuins

```text
Attached: the current in-game image (composition, landmarks, colours and mood reference).
Redraw the attached reference image as true pixel art for the game WordVenture.
Ashen ruins battle backdrop: hazy peach-orange sky, layered gray-brown silhouettes of broken towers and a ruined stone arch, flat gray ash plain in the foreground. Side-on battle backdrop; the bottom third is flat ground where characters stand.
Keep the same composition, layout, horizon heights, landmark positions and colours as the reference.
Draw it on an exact 552x276 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 32 colours. 
No text, UI or logos unless the reference has them. Output a landscape image with the same aspect ratio as the reference.
```

### FrostVillage

```text
Attached: the current in-game image (composition, landmarks, colours and mood reference).
Redraw the attached reference image as true pixel art for the game WordVenture.
Frost village battle backdrop: blue sky with white clouds, snowy blue-white mountains, a row of snow-covered pine trees with small snowy village roofs between them, a wide flat snowfield in the foreground. Side-on battle backdrop; the bottom third is flat ground where characters stand.
Keep the same composition, layout, horizon heights, landmark positions and colours as the reference.
Draw it on an exact 552x276 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 32 colours. 
No text, UI or logos unless the reference has them. Output a landscape image with the same aspect ratio as the reference.
```

### ThunderCanyon

```text
Attached: the current in-game image (composition, landmarks, colours and mood reference).
Redraw the attached reference image as true pixel art for the game WordVenture.
Thunder canyon battle backdrop: dark purple storm clouds with small yellow lightning bolts, layered red-brown canyon mesas, flat reddish dirt ground in the foreground. Side-on battle backdrop; the bottom third is flat ground where characters stand.
Keep the same composition, layout, horizon heights, landmark positions and colours as the reference.
Draw it on an exact 552x276 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 32 colours. 
No text, UI or logos unless the reference has them. Output a landscape image with the same aspect ratio as the reference.
```

### TwistedForest

```text
Attached: the current in-game image (composition, landmarks, colours and mood reference).
Redraw the attached reference image as true pixel art for the game WordVenture.
Twisted forest battle backdrop: misty green-gray sky, huge dark purple twisted trees with spreading canopies, dark green bushes, flat muted green grass ground in the foreground. Side-on battle backdrop; the bottom third is flat ground where characters stand.
Keep the same composition, layout, horizon heights, landmark positions and colours as the reference.
Draw it on an exact 552x276 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 32 colours. 
No text, UI or logos unless the reference has them. Output a landscape image with the same aspect ratio as the reference.
```

### WorldTreeHeart

```text
Attached: the current in-game image (composition, landmarks, colours and mood reference).
Redraw the attached reference image as true pixel art for the game WordVenture.
World tree heart battle backdrop: pale yellow-green sky, a giant faint world tree silhouette in the background, layered green forest, a yellow-green grass band and a flat sandy ground in the foreground. Side-on battle backdrop; the bottom third is flat ground where characters stand.
Keep the same composition, layout, horizon heights, landmark positions and colours as the reference.
Draw it on an exact 552x276 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 32 colours. 
No text, UI or logos unless the reference has them. Output a landscape image with the same aspect ratio as the reference.
```

### RainyBase

```text
Attached: the current in-game image (composition, landmarks, colours and mood reference).
Redraw the attached reference image as true pixel art for the game WordVenture.
Rainy plains battle backdrop WITHOUT any rain streaks: purple-blue overcast sky, pale blue mountains with light cloud caps, dark green bushes, a bright grass band, a flat orange dirt strip with a few small stones, a light green grass fringe at the bottom. Draw no rain drops at all; rain is added separately. Side-on battle backdrop; the bottom third is flat ground where characters stand.
Keep the same composition, layout, horizon heights, landmark positions and colours as the reference.
Draw it on an exact 552x276 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 40 colours. 
No text, UI or logos unless the reference has them. Output a landscape image with the same aspect ratio as the reference.
```

### GameOver

워드 얼굴을 대화 초상화에 맞춰 다시 그렸다. 첨부: 그때의 게임오버 그림, 승인된 WordDialogue.png 의 얼굴 부분 8배 확대본. 결과는 3프레임(눈물이 뺨 아래까지 흐른 그림)으로 쓴다. 검은 배경에 16~20 사이 색이 섞여 있어서, 가장자리와 이어진 어두운 픽셀을 예전 배경색 (28, 28, 28) 한 색으로 칠했다. 그다음 `gameover_frames.py` 가 눈물 줄기를 위쪽 1/3, 2/3만 남겨 1·2프레임을 만든다.

```text
Attached: (1) the current game over screen, (2) a close-up of the heroine Word's face from her approved dialogue portrait.
Redraw (1) as true pixel art. On a flat near-black background the chibi heroine Word sits crying on a small yellow-green patch of ground, her staff with a blue orb lying across her lap, at exactly the same position and size as in (1) (her body centre just right of the image centre). Draw her face in the style of reference (2), simplified to chibi size: the same eye shape and colour, the same red-orange bob hair with bangs and the same yellow ribbon, eyes shut or teary with blue tears running down both cheeks, a small sad mouth. Her black cloak and blue dress as in (1).
Draw it on an exact 480x240 pixel grid: every art pixel is one equal square cell, then enlarge with nearest-neighbour by an integer factor. A clean 1-pixel dark outline, flat 2 to 3 tone shading, no scattered single pixels, no anti-aliasing, no gradients, no blur. Leave the rest of the background flat near-black. No text.
```

### GameClear

```text
Attached: the current in-game image (composition, landmarks, colours and mood reference).
Redraw the attached reference image as true pixel art for the game WordVenture.
Game clear night scene: dark blue night sky with a big pale yellow full moon and small gray clouds, dark blue mountains, dark green pine trees, the chibi red-haired mage girl Word with a yellow ribbon and black cloak cheering with her staff raised next to a bright campfire, the word Word in small blue pixel letters at the top right exactly as in the reference.
Keep the same composition, layout, horizon heights, landmark positions and colours as the reference.
Draw it on an exact 480x240 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 32 colours. 
No text, UI or logos unless the reference has them. Output a landscape image with the same aspect ratio as the reference.
```

### WorldMapDefault

```text
Attached: the current in-game image (composition, landmarks, colours and mood reference).
Redraw the attached reference image as true pixel art for the game WordVenture.
First chapter world map: teal sea, a small green island at the left, a large green landmass, a mushroom house at the lower left, a palm islet with a teal pool, a dark rocky ruin, a tree on a rocky crag at the top, wooden plank paths between them, and a dark castle in front of a red moon at the upper right. World map seen from above at an angle. Keep EVERY coastline, island, path and landmark at exactly the same place as in the reference; map node positions are fixed in the game. Thick 1-pixel dark outlines on land.
Keep the same composition, layout, horizon heights, landmark positions and colours as the reference.
Draw it on an exact 495x276 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 48 colours. 
No text, UI or logos unless the reference has them. Output a landscape image with the same aspect ratio as the reference.
```

### ActTwoMap

```text
Attached: the current in-game image (composition, landmarks, colours and mood reference).
Redraw the attached reference image as true pixel art for the game WordVenture.
Second chapter world map: deep blue sea, a long green island with pine trees, a snowy village at the left, a rocky canyon with a lightning spire in the middle, gray ruins, a twisted dark forest, and a glowing golden world tree at the right, joined by wooden plank paths. World map seen from above at an angle. Keep EVERY coastline, island, path and landmark at exactly the same place as in the reference; map node positions are fixed in the game. Thick 1-pixel dark outlines on land.
Keep the same composition, layout, horizon heights, landmark positions and colours as the reference.
Draw it on an exact 495x276 pixel grid: every art pixel is one equal square cell,
then enlarge with nearest-neighbour by an integer factor. Hard edges, no
anti-aliasing, no gradients, no dithering noise, no blur, flat colour clusters,
limited palette of about 48 colours. 
No text, UI or logos unless the reference has them. Output a landscape image with the same aspect ratio as the reference.
```
