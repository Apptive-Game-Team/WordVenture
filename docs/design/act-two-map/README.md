# 2부 맵 그림

`Assets/Art/Map/ActTwoMap.png`는 2부 지역 5곳을 보여 주는 맵 배경이다. 1부를 끝낸 세이브(`StagePosition` 5 이상)로 맵에 들어가면 `MapMove`가 이 그림과 `Assets/ScriptableObjects/Map/ActTwoMap.asset`의 지점 좌표를 쓴다.

## 만드는 방법

Codex의 내장 이미지 생성 도구로 만들었다. 1부 맵(`Assets/ThirdParty/ArtResource/JinWook/WorldMap/Stage4BG.png`)을 참고 이미지로 첨부했다. 1부 맵과 같은 1536×1024이고 import 설정(100 px/unit)도 같아서, 씬의 배경 오브젝트에 그대로 올라간다.

## 지점 좌표

배경 그림의 픽셀 (px, py)는 월드 좌표 ((px − 768) / 100 × 1.1913, (512 − py) / 100)이다. 배경 오브젝트가 원점에 있고 가로로 1.1913배 늘어나 있기 때문이다.

| 스테이지 | 지역 | 픽셀 | 월드 |
| --- | --- | --- | --- |
| 5 | 서리 마을 | (345, 790) | (−5.039, −2.78) |
| 6 | 벼락 협곡 | (845, 700) | (0.917, −1.88) |
| 7 | 잿빛 유적 | (1135, 490) | (4.372, 0.22) |
| 8 | 뒤틀린 숲 | (845, 345) | (0.917, 1.67) |
| 9 | 세계수의 심장 | (1305, 350) | (6.397, 1.62) |

그림의 길은 잿빛 유적에서 뒤틀린 숲과 세계수로 갈라진다. 지점 순서는 기획대로 숲(8) 다음 세계수(9)다.

## 프롬프트

```text
Use your built-in image generation tool to create ONE new image file, then save it as exactly `act-two-map.png` in the current working directory. Do not edit or create any other files.

Reference: `reference-act-one-map.png` in this directory is the world map of Act 1 of a 2D pixel-art game. Attach it as the style reference. Match its style closely: same chunky hand-drawn pixel art, thick dark outlines, flat colors with simple shading, same camera (slightly top-down isometric-ish view of an island in the sea), same overall brightness, same scale of landmarks, and teal-blue ocean filling the background.

Image requirements:
- Exactly 1536 x 1024 pixels, landscape.
- A NEW, different island (Act 2) in the same teal ocean. Do not copy the Act 1 island shape.
- Five landmark locations connected in order by one brown dirt road with dark outlines, like the reference:
  1. Bottom-left: "Frost Village" - a small snowy village of 3-4 little houses with snow-covered roofs, white snow ground around it.
  2. Lower-middle: "Thunder Canyon" - a rocky canyon gorge with a small yellow lightning bolt and a dark storm cloud above it.
  3. Center or middle-right: "Ashen Ruins" - grey crumbling stone ruins and broken pillars on ash-grey ground.
  4. Upper-middle: "Twisted Forest" - a cluster of dark purple-green twisted trees.
  5. Top-right: "Heart of the World Tree" - one huge glowing world tree, the largest landmark, with a soft green-gold glow.
- The road starts at Frost Village and visits the landmarks in the order 1 -> 2 -> 3 -> 4 -> 5, zig-zagging across the island like the reference.
- Each landmark sits on a small flat base like the round platforms in the reference so a character sprite can stand on it.
- Keep every landmark fully inside the image with at least 120 px margin from the edges. Keep the top-left 520 x 130 px area as plain ocean (the game draws a title there).
- NO text, letters, labels, numbers, UI, or watermark anywhere in the image.

After saving, reply with the approximate pixel center (x, y) of each of the five landmark platforms in the saved 1536x1024 image, measured from the top-left corner.
```
