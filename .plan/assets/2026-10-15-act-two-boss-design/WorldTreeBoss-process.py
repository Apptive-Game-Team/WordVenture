"""Reproduce the enlarged final boss animation conversion and evidence."""
import json, re, shutil, subprocess, sys
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[3]
OUT = Path(__file__).parent
SCRIPTS = ROOT / '.agents/skills/pixel-art/scripts'
frames = json.loads((ROOT / 'docs/design/act-two-art/manifest.json').read_text())['enemies']['WorldTreeShell']['frames']
paths = [ROOT / f['path'] for f in frames]
raw = OUT / 'WorldTreeBoss-raw.png'
shutil.copy2(sys.argv[1], raw)
subprocess.run([sys.executable, str(SCRIPTS / 'char_sheet.py'), 'post', str(raw), '126', '126', *map(str,paths)], check=True)
grid = Image.new('RGBA',(504,252))
for i,path in enumerate(paths):
    grid.alpha_composite(Image.open(path).convert('RGBA'), ((i % 4) * 126, (i // 4) * 126))
# A transparent pixel is also a color; leave room below the 64 RGBA-color limit.
sys.path.insert(0,str(SCRIPTS))
from pixelate import pixelate
grid=pixelate(grid,504,252,colors=48,keep_lines=0).convert('RGBA')
for i,path in enumerate(paths):
    x,y=(i%4)*126,(i//4)*126
    grid.crop((x,y,x+126,y+126)).save(path)
grid.save(OUT / 'WorldTreeBoss-logical.png')
preview = Image.new('RGBA',grid.size,'#737984')
preview.alpha_composite(grid)
preview.resize((2016,1008),Image.Resampling.NEAREST).save(OUT / 'WorldTreeBoss-comparison-4x.png')
subprocess.run([sys.executable,str(SCRIPTS / 'trim_set.py'),*map(str,paths)],check=True)
report=[]
for path in paths:
    im=Image.open(path).convert('RGBA')
    colors=len(im.getcolors(100000))
    alpha=sorted(set(im.getchannel('A').get_flattened_data()))
    assert colors<=64 and alpha==[0,255]
    meta_path=Path(str(path)+'.meta')
    meta=meta_path.read_text()
    meta=re.sub(r'(spritePivot: \{x: [-\d.e]+, y: )[-\d.e]+',lambda m:m[1]+f'{13/im.height:.8f}',meta)
    meta_path.write_text(meta,newline='')
    assert 'spritePixelsToUnits: 24.57' in meta and 'filterMode: 0' in meta and 'enableMipMap: 0' in meta
    report.append(dict(path=str(path),size=im.size,colors=colors,alpha=alpha,ppu=24.57,pivotY=13/im.height,pivotRule='13 art pixels above shared trimmed union floor'))
(OUT/'WorldTreeBoss-validation.json').write_text(json.dumps(report,indent=2)+'\n')
print('Validated',len(paths),'frames',Image.open(paths[0]).size)
