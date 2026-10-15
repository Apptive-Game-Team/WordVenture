"""Reproduce pixel-grid conversion and contact sheets of generated boss art."""
import json
import re
import shutil
import subprocess
import sys
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[3]
OUT = Path(__file__).parent
MANIFEST = ROOT / 'docs/design/act-two-art/manifest.json'
PY = sys.executable
SCRIPTS = ROOT / '.agents/skills/pixel-art/scripts'

def process(name, source):
    entry = json.loads(MANIFEST.read_text())['enemies'][name]
    paths = [ROOT / f['path'] for f in entry['frames']]
    raw = OUT / (name + '-raw.png')
    shutil.copy2(source, raw)
    if name == 'Snowball':
        subprocess.run([PY, str(SCRIPTS / 'pixelate.py'), str(raw), str(paths[0]), '--target', '48x48', '--keep-lines', '2'], check=True)
    else:
        subprocess.run([PY, str(SCRIPTS / 'char_sheet.py'), 'post', str(raw), '97', '97', *map(str, paths)], check=True)
    # Store untrimmed logical sheet alongside reference for stable baseline checks.
    grid = Image.new('RGBA', (4 * 97, 2 * 97))
    for index, path in enumerate(paths):
        im = Image.open(path).convert('RGBA')
        grid.alpha_composite(im, ((index % 4) * 97, (index // 4) * 97))
    grid.save(OUT / (name + '-logical.png'))
    subprocess.run([PY, str(SCRIPTS / 'trim_set.py'), *map(str, paths)], check=True)
    preview = Image.new('RGBA', (4 * 97, 2 * 97), '#737984')
    preview.alpha_composite(grid)
    preview.resize((1552, 776), Image.Resampling.NEAREST).save(OUT / (name + '-comparison-4x.png'))
    for path in paths:
        im = Image.open(path).convert('RGBA')
        assert len(im.getcolors(100000)) <= 64
        assert set(im.getchannel('A').getdata()) <= {0,255}
        meta_path = Path(str(path) + '.meta')
        s = meta_path.read_text()
        if name != 'Snowball':
            s = re.sub(r'(spritePivot: \{x: [-\d.e]+, y: )[-\d.e]+', lambda m: m[1] + f'{13/im.height:.8f}', s)
            meta_path.write_text(s, newline='')
        assert 'filterMode: 0' in s and 'enableMipMap: 0' in s
        assert 'spritePixelsToUnits: ' + ('10.8' if name == 'Snowball' else '18.9') in s
    print(name, 'validated', len(paths), 'frames', Image.open(paths[0]).size)

if __name__ == '__main__':
    process(sys.argv[1], sys.argv[2])
