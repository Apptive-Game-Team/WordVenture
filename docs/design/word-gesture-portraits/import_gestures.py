"""Codex 이미지 생성 결과를 워드 몸짓 초상화 에셋으로 바꾼다.

사용법: python3 docs/design/word-gesture-portraits/import_gestures.py <생성 폴더>
생성 폴더에는 prompt.txt 의 파일 이름(WordNod.png 등)대로 1024x1024 마젠타 배경 그림이 있다.
저장소 루트에서 실행한다.
"""
import sys
from collections import deque
from pathlib import Path

from PIL import Image

ROOT = Path.cwd()
PORTRAITS = ROOT / "Assets/Art/Story/Portraits"
# 기존 워드 초상화의 import 설정을 그대로 복사하고 GUID만 바꾼다.
META_TEMPLATE = PORTRAITS / "WordDialogue.png.meta"
TEMPLATE_GUID = "4b11d118601649689dbff602bda3aab4"
PORTRAIT_SIZE = 1254
# 생성 파일 이름 → (에셋 이름, GUID). GUID 는 ActOneDialogues.asset 의 gesturePortraits 가 참조한다.
GESTURES = {
    "WordNod.png": ("WordNodDialogue.png", "933d020b58f44e65b0d785c1f7e7774a"),
    "WordShakeHead.png": ("WordShakeHeadDialogue.png", "06c672c795e34291a4245b7d873886b2"),
    "WordPoint.png": ("WordPointDialogue.png", "3532e203c5b44229b520289e7b422c05"),
    "WordTilt.png": ("WordTiltDialogue.png", "c206665f424b496090e9ddabe52c99e1"),
    "WordShowBook.png": ("WordShowBookDialogue.png", "e965a5282d084d5a89b6a9ce8305b556"),
    "WordRaiseCard.png": ("WordRaiseCardDialogue.png", "51d9ed3a93f642c0b64594803e842281"),
    "WordPalmOut.png": ("WordPalmOutDialogue.png", "53a3f8021a534d91b60f96e1f7dd3bef"),
    "WordNodFirm.png": ("WordNodFirmDialogue.png", "2c4d9a7194e0416fb11a1af27b085640"),
    "WordShakeHeadFirm.png": ("WordShakeHeadFirmDialogue.png", "e68a5ff30f78431291a1155709494872"),
    "WordPointWorried.png": ("WordPointWorriedDialogue.png", "ee1daf037c4e49b58018a3de2fd2b433"),
    "WordShowCover.png": ("WordShowCoverDialogue.png", "2893518f1c344927b7e4547cc52875f5"),
    "WordTouchThroat.png": ("WordTouchThroatDialogue.png", "ca6e1f029377475990077d7e067cf7c9"),
    "WordReadBook.png": ("WordReadBookDialogue.png", "fbbda2c859ba4701af1298c38aa6856c"),
}


def is_background(pixel) -> bool:
    r, g, b, _ = pixel
    # 배경 마젠타와 가장자리의 분홍빛 번짐을 함께 지운다.
    return r > 150 and b > 150 and g < 110 and abs(r - b) < 90


def is_pure_magenta(pixel) -> bool:
    r, g, b, _ = pixel
    return r > 200 and b > 200 and g < 70


def remove_background(image: Image.Image) -> Image.Image:
    # 워드의 눈이 분홍빛이라 넓은 기준으로 그림 전체를 지우면 눈까지 뚫린다. 그래서 가장자리와 순수한
    # 마젠타(지팡이와 머리카락 사이에 갇힌 배경 포함)에서 시작해, 거기에 이어진 분홍빛 번짐까지만 지운다.
    image = image.convert("RGBA")
    pixels = image.load()
    width, height = image.size
    queue = deque()
    seen = set()
    for x in range(width):
        queue.extend([(x, 0), (x, height - 1)])
    for y in range(height):
        queue.extend([(0, y), (width - 1, y)])
    queue.extend((x, y) for y in range(height) for x in range(width) if is_pure_magenta(pixels[x, y]))
    while queue:
        x, y = queue.popleft()
        if (x, y) in seen or not (0 <= x < width and 0 <= y < height):
            continue
        seen.add((x, y))
        if not is_background(pixels[x, y]):
            continue
        pixels[x, y] = (0, 0, 0, 0)
        queue.extend([(x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)])
    return image


def main(source: Path) -> None:
    template = META_TEMPLATE.read_text(encoding="utf-8")
    for generated, (asset_name, guid) in GESTURES.items():
        image = remove_background(Image.open(source / generated))
        # 기존 초상화와 같은 1254x1254 캔버스로 키운다.
        image = image.resize((PORTRAIT_SIZE, PORTRAIT_SIZE), Image.NEAREST)
        target = PORTRAITS / asset_name
        image.save(target)
        target.with_name(asset_name + ".meta").write_text(template.replace(TEMPLATE_GUID, guid), encoding="utf-8")
        print(target.relative_to(ROOT))


if __name__ == "__main__":
    main(Path(sys.argv[1]))
