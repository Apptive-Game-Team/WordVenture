"""다시 그린 그림의 색을 원래 그림의 색으로 되돌린다(#116).

사용법: python3 docs/design/pixel-grid/match_palette.py <원래 그림> <새 그림> [<새 그림>을 덮어씀]
저장소 루트에서 실행한다. Codex 로 다시 그린 지도는 배치는 같지만 색이 더 밝고 진했다. 원래 그림을 새 그림
크기로 늘려(가장 가까운 픽셀) 겹쳐 놓고, 새 그림의 색마다 같은 자리에 가장 많이 있던 원래 색으로 바꾼다.
새 그림의 칸 경계와 모양은 그대로이고 색만 원래 팔레트로 돌아간다.
"""
import sys
from collections import Counter

import numpy as np
from PIL import Image

old_path, new_path = sys.argv[1:3]
new = np.asarray(Image.open(new_path).convert("RGB"))
H, W = new.shape[:2]
old = np.asarray(Image.open(old_path).convert("RGB").resize((W, H), Image.NEAREST))
votes = {}
for n, o in zip(map(tuple, new.reshape(-1, 3)), map(tuple, old.reshape(-1, 3))):
    votes.setdefault(n, Counter())[o] += 1
mapping = {n: c.most_common(1)[0][0] for n, c in votes.items()}
out = np.array([mapping[tuple(p)] for p in new.reshape(-1, 3)], np.uint8).reshape(H, W, 3)
Image.fromarray(out).save(new_path, optimize=True)
print(f"{new_path}: {len(mapping)} colours -> {len(set(mapping.values()))} original colours")
