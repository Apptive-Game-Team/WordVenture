import sys
from PIL import Image
G = {
'N':["##...##","###..##","####.##","##.####","##..###","##...##","##...##"],
'E':["######","##....","##....","#####.","##....","##....","######"],
'W':["##...##","##...##","##.#.##","##.#.##","#######","###.###","##...##"],
'C':[".#####","##....","##....","##....","##....","##....",".#####"],
'A':[".####.","##..##","##..##","######","##..##","##..##","##..##"],
'R':["#####.","##..##","##..##","#####.","##.##.","##..##","##..##"],
'D':["#####.","##..##","##..##","##..##","##..##","##..##","#####."],
}
CELL, M, SCALE = 3, 3, 6
FACE, HI, SHADE, LINE, DROP = (220,187,107), (238,214,150), (178,136,58), (55,35,2), (30,18,0)
cells=[]; x=0
for i,ch in enumerate("NEW CARD"):
    if ch==' ': x+=3; continue
    for r,row in enumerate(G[ch]):
        for c,v in enumerate(row):
            if v=='#': cells.append((x+c,r))
    x+=len(G[ch][0])+1
w=(x-1)*CELL+2*M; h=7*CELL+2*M+1
face=set()
for cx,cy in cells:
    for dx in range(CELL):
        for dy in range(CELL): face.add((M+cx*CELL+dx, M+cy*CELL+dy))
line={(px+dx,py+dy) for px,py in face for dx in(-1,0,1) for dy in(-1,0,1)}-face
drop={(px,py+1) for px,py in face|line}-face-line
im=Image.new('RGBA',(w,h),(0,0,0,0)); p=im.load()
for q in drop: p[q]=DROP+(255,)
for q in line: p[q]=LINE+(255,)
for (px,py) in face:
    if (px,py+1) not in face: col=SHADE
    elif (px,py-1) not in face: col=HI
    else: col=FACE
    p[px,py]=col+(255,)
im.resize((w*SCALE,h*SCALE),Image.NEAREST).save(sys.argv[1])
print(w*SCALE,h*SCALE)
