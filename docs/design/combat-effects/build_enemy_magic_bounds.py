"""Cache existing spell atlas alpha bounds without changing textures/import settings.

Unity's FullRect spell sprites have no physics outline and include wide transparent
margins. Enemy effects use these bounds to put the painted flame at the body center.
Run from the repository root after changing existing spell textures or slices.
"""
from pathlib import Path
import re
from PIL import Image


def build():
    sprites, bounds = [], []
    for image_path in sorted(Path('Assets/Art/Combat/Spells').rglob('*.png')):
        meta = Path(str(image_path) + '.meta').read_text(encoding='utf-8')
        guid = re.search(r'^guid: (\w+)', meta, re.M)[1]
        ppu = float(re.search(r'spritePixelsToUnits: ([\d.]+)', meta)[1])
        image = Image.open(image_path).convert('RGBA')
        for chunk in meta.split('      name: ')[1:]:
            if '      internalID:' not in chunk:
                continue
            rect = re.search(r'      rect:.*?        x: ([\d.]+)\s+        y: ([\d.]+)\s+        width: ([\d.]+)\s+        height: ([\d.]+)', chunk, re.S)
            if rect is None:
                continue
            x, y, width, height = map(float, rect.groups())
            pivot = re.search(r'pivot: \{x: ([\d.]+), y: ([\d.]+)\}', chunk)
            px, py = map(float, pivot.groups())
            frame = image.crop((int(x), int(image.height - y - height), int(x + width), int(image.height - y)))
            box = frame.getchannel('A').getbbox()
            if box is None:
                continue
            left, top, right, bottom = box
            center_x = ((left + right) / 2 - width * px) / ppu
            center_y = (height - (top + bottom) / 2 - height * py) / ppu
            file_id = re.search(r'internalID: (-?\d+)', chunk)[1]
            sprites.append(f'  - {{fileID: {file_id}, guid: {guid}, type: 3}}')
            bounds.append(f'  - {{x: {center_x:g}, y: {center_y:g}, z: {(right-left)/ppu:g}, w: {(bottom-top)/ppu:g}}}')
    asset = Path('Assets/Resources/Combat/EnemyMagicCatalog.asset')
    text = asset.read_text(encoding='utf-8').split('  boundedSprites:')[0].rstrip()
    asset.write_text(text + '\n  boundedSprites:\n' + '\n'.join(sprites) + '\n  visibleBounds:\n' + '\n'.join(bounds) + '\n', encoding='utf-8')
    print(f'{len(sprites)} existing spell frames: cached alpha bounds')


if __name__ == '__main__':
    build()
