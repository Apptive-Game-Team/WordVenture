from pathlib import Path
import json,re
from PIL import Image

root=Path('.')
errors=[]
guid_paths={}
for meta in Path('Assets').rglob('*.meta'):
    match=re.search(r'^guid: ([0-9a-f]{32})',meta.read_text(encoding='utf-8',errors='replace'),re.M)
    if match: guid_paths[match[1]]=Path(str(meta)[:-5])

text=Path('Assets/ScriptableObjects/Combat/EnemyDataContainer.asset').read_text(encoding='utf-8')
entries=re.findall(r'  - id: (\d+)\n(.*?)(?=  - id: |\Z)',text,re.S)
table=json.loads(Path('Assets/Resources/Localization/English.json').read_text(encoding='utf-8'))['entries']
english={e['ko']:e['en'] for e in table}
if len(english)!=len(table): errors.append('duplicate localization key')
report=[]
for index,(raw_id,entry) in enumerate(entries):
    enemy_id=int(raw_id)
    if enemy_id!=index: errors.append(f'enemy index mismatch {index}/{enemy_id}')
    if enemy_id<29: continue
    encoded=re.search(r'    displayName: (.+)',entry)[1]
    name=json.loads(encoded)
    if not english.get(name): errors.append('name translation: '+name)
    guid=re.search(r'prefab: \{fileID: \d+, guid: ([0-9a-f]{32})',entry)[1]
    prefab=guid_paths.get(guid)
    if not prefab or not prefab.exists(): errors.append(f'missing prefab {enemy_id}'); continue
    prefab_text=prefab.read_text(encoding='utf-8')
    frames=re.search(r'  sprites:\n((?:  - .*\n)+)',prefab_text)
    if not frames: errors.append(f'missing animator {enemy_id}'); continue
    for sprite in re.findall(r'guid: ([0-9a-f]{32})',frames[1]):
        path=guid_paths.get(sprite)
        if not path or not path.exists(): errors.append(f'missing frame {enemy_id}/{sprite}')
    descriptions=[]
    for script in re.findall(r'm_Script: \{fileID: \d+, guid: ([0-9a-f]{32})',prefab_text):
        path=guid_paths.get(script)
        if path and path.suffix=='.cs':
            description=re.search(r'AttackDescription => "([^"]+)"',path.read_text(encoding='utf-8'))
            if description:
                descriptions.append(description[1])
                if not english.get(description[1]): errors.append('description translation: '+path.name)
    if not descriptions: errors.append(f'missing description {enemy_id}')
    report.append({'id':enemy_id,'name':name,'hp':int(re.search(r'maxHp: (\d+)',entry)[1]),
        'damage':int(re.search(r'damage: (\d+)',entry)[1]),'prefab':prefab.as_posix(),'frames':len(re.findall(r'guid:',frames[1]))})

for path in Path('Assets/ScriptableObjects/Combat').glob('*WaveData.asset'):
    for enemy in re.findall(r'enemyId: (\d+)',path.read_text(encoding='utf-8')):
        if int(enemy)>=len(entries): errors.append('wave unknown id '+path.name+'/'+enemy)

manifest=json.loads(Path('docs/design/act-two-art/manifest.json').read_text(encoding='utf-8'))
art=[]
for key in ['SnowBoss','ThunderBoss','FurnaceBoss','GraftBoss','Snowball','WorldTreeShell','WorldTreeCore']:
    for item in manifest['enemies'][key]['frames']:
        path=Path(item['path'])
        im=Image.open(path).convert('RGBA')
        colors=len(im.getcolors(im.width*im.height) or [])
        if colors>64: errors.append('palette '+path.name+'/'+str(colors))
        meta=Path(str(path)+'.meta').read_text(encoding='utf-8')
        if item['guid'] not in meta: errors.append('manifest guid '+path.name)
        if not re.search(r'filterMode: 0',meta) or not re.search(r'enableMipMap: 0',meta): errors.append('import '+path.name)
        art.append({'path':path.as_posix(),'size':list(im.size),'colors':colors})

output={'validation':'static asset/translation references only; does not compile or run Unity','errors':errors,'enemies':report,'art':art}
out=Path('.plan/assets/2026-10-15-act-two-boss-design/static-validation.json')
out.write_text(json.dumps(output,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'errors':errors,'enemies':len(report),'frames':len(art)},ensure_ascii=False))
raise SystemExit(bool(errors))
