# 배경 생성 기록

내장 image_gen 도구로 생성. 기존 전투·해변 배경을 스타일 참조로 사용.

- Assets/Art/Story/MountainHome.png: 평화로운 산골 집 (스토리 background 0)
- Assets/Art/Story/MountainHomeAttack.png: 불덩이가 날아와 지붕에 불이 붙는 습격 장면 (스토리 background 1, 3~6번째 대사)
- Assets/Art/Story/MountainHomeRuins.png: 같은 집의 습격 후 폐허 (스토리 background 2, 7번째 대사부터)
- Assets/Art/Title/MountainValley.png: 타이틀과 메뉴를 위한 중앙 여백이 있는 산골 풍경

원본 생성 크기 1672×941. 기존 배경의 월드 너비 4.8을 유지하도록 Pixels Per Unit 348.33333333을 사용. Point 필터, 무압축, mipmap 비활성화. 기존 프레임 애니메이터는 스프라이트 덮어쓰기를 막기 위해 비활성화하고 Controller 참조 제거.

## 최종 프롬프트

### WorldTreeEpilogue

Assets/Art/Story/WorldTreeEpilogue.png: 먼 훗날 워드가 세상을 떠난 자리에 자란 세계수. EndingScene background 1. background 0은 산골 집 귀환과 추모에 사용. 기준 문서: C:/Users/jys09/Projects/ArcaneCasters/Client/.art/WORLD.md. 신이 된 과정과 죽음의 원인·시점, 세계수의 지리적 위치는 단정하지 않음. 불타는 군단을 산골 습격의 주체로 변경하지 않음.

Use case: illustration-story. Asset: WordVenture far-future ending epilogue background in its shared world with ArcaneCasters. Style references only: match coarse pixel art, flat limited color clusters, layered blue mountains and teal forests, hard square pixels, simple readable side-on scenic 16:9 game composition. Scene: a single majestic living WORLD TREE grows where Word, the heroine who later became the god of magic, eventually died. Show a broad emerald leafy canopy and sturdy warm brown trunk with spreading roots on a calm forest clearing, distant blue mountains and quiet pale blue sky. Gentle pale cyan magical accents among the roots, restrained and sparse. Do not depict death, a grave, a cottage, a portal, a deity body, specific spirit characters or a cause of death. The exact geographic location and death cause are unspecified, so make an anonymous natural clearing. No inscriptions, words, UI, logos. Tree is one central strong silhouette, crown fits entirely in frame, bottom quarter calm grass/path space for dialogue overlay. Avoid photorealism, painterly textures, fine dithering, neon glow, cut-paper/3D style. Keep same pixel density as supplied game backgrounds. Output 1672x941 landscape.

### MountainHomeMemorial

Assets/Art/Story/MountainHomeMemorial.png: 복수를 마치고 고향으로 돌아와 할아버지를 추모하는 엔딩. 기존 산골 집의 폐허와 작은 비석, 들꽃, 저녁빛을 표현. 내장 image_gen으로 생성하고 EndingScene에 연결.

Use case: illustration-story / precise-object-edit. Edit target supplied ruined mountain cottage background. Create a grounded bittersweet ending after a young heroine defeats the slime king and returns home to remember her late grandfather. Preserve EXACT same cottage at left, damaged roof and broken fence, layered blue mountains, teal forest, sandy foreground path and camera composition. Time has passed: remove all smoke, fire and storm clouds, peaceful late afternoon blue sky with soft warm flat golden light at horizon. Add one small plain unlettered gray stone memorial beside the cottage on its right, with a modest handful of white wildflowers at the base, visible above bottom dialogue area. Grass is beginning to grow around rubble. House remains damaged, no newly rebuilt mansion. No characters, no bodies, no magic portals, no godhood imagery, no monsters, no words or inscriptions, no UI. Same simple chunky pixel art, broad flat limited color clusters, hard square pixels, low detail matching original battle game art. Landscape 16:9 at 1672x941 if possible. Bottom quarter quiet for dialogue.

### MountainHomeAttack

Use case: illustration-story / precise-object-edit. Edit target: supplied peaceful mountain cottage game backdrop. Create the ACTIVE ATTACK moment, between peaceful home and its later ruins. Preserve same cottage at left, valley, mountains, garden, path, framing, 16:9 composition and chunky pixel art palette. Several bright orange-red magic fireballs with yellow cores and long angular pixel flame tails fly diagonally FROM upper right TOWARD the cottage at lower left. Show one fireball striking roof edge with a burst of pixel sparks and small flames spreading across thatch, dark smoke above house; roof still mostly standing, only partial damage. Make incoming direction unambiguous: round bright leading cores on lower-left ends, trailing flames extending upper-right. Dramatic smoky blue-gray sky, retain readable blue mountains and green forest. No people, monsters, gore, text, UI or logos. No photoreal glow, no painterly gradients. Simple flat-color square pixel clusters matching original. Bottom quarter remains quiet for dialogue overlay. Output landscape at same dimensions as input if possible.

### MountainHome

Use case: illustration-story. Asset: WordVenture opening story game background, landscape 16:9. Reference images are STYLE references only: match their simple chunky low-resolution pixel art, broad flat color clusters, blue layered mountains, dark teal bushes, bright grass and sandy ochre path, limited palette, hard square pixels, no smooth painting or detailed dithering. Create a peaceful remote mountain valley home where a young girl lives with her grandfather: one modest timber and stone cottage with warm ochre roof on left-middle, a small vegetable garden and wooden fence, forest and blue mountains behind, open path across foreground. No people, text, UI, logos, monsters. Keep bottom quarter quiet for dialogue overlay. Wide scenic side-on game composition. Render as clean deliberately coarse pixel art, approximately 320x180 logical pixel grid, enlarged crisp.

### MountainHomeRuins

Use case: style-transfer / illustration-story. Edit target image 1: peaceful mountain cottage just generated. Keep EXACT same valley, cottage placement, mountains, path, camera and chunky simple pixel art style. Depict aftermath of slime army attack: cottage roof collapsed inward with broken timber and stone rubble, damaged garden and fence, faint gray smoke rising, subdued blue-gray cloudy sky and slightly muted grass. The house must visibly be ruined rather than merely recolored. No people, bodies, gore, monsters, text, UI. Keep bottom quarter simple for dialogue. 16:9 landscape. Hard square pixel clusters and limited flat colors matching reference game art. No painterly textures.

### MountainValley

Use case: illustration-story. Asset: WordVenture title screen background, 16:9 landscape. Input images STYLE references only. Match the simple coarse pixel art of game battle background: blue layered mountain silhouettes, flat teal forest, bright green grasses, ochre paths, limited flat color clusters, large square pixels. Scene: inviting remote mountain valley and winding ochre trail leading toward distant blue mountains, framing trees at far left and right, low bushes. Keep central 60 percent open and uncluttered for existing title and vertical menu overlay; airy blue sky upper half. No cottage in center, no characters, no text or letters, no logo/UI, no high detail dithering, no gradients or painterly texture. Approximately 320x180 logical pixel grid enlarged crisp, side-on scenic 2D RPG backdrop. Use same palette and visual density as reference 1.
