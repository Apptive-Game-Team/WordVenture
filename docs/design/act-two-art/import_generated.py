"""Codex 이미지 생성 결과를 게임 에셋으로 바꾼다.

사용법: python3 docs/design/act-two-art/import_generated.py <생성 폴더>
생성 폴더에는 bg/, portraits/, enemies1/, enemies2/ 아래에 생성 그림이 있다.
경로와 GUID는 manifest.json을 따른다. 저장소 루트에서 실행한다.
"""
import json
import sys
from pathlib import Path

from PIL import Image, ImageOps

ROOT = Path.cwd()
MANIFEST = json.loads((ROOT / "docs/design/act-two-art/manifest.json").read_text(encoding="utf-8"))
# 기존 에셋의 import 설정을 그대로 복사해 쓴다. GUID만 바꾼다.
META_TEMPLATES = {
    "background": ("Assets/ThirdParty/ArtResource/JinWook/Desert/HIghland.png.meta", "f5eee149beadcfb4bb4bbb74ad433be6"),
    "portrait": ("Assets/Art/Story/Portraits/VillagerDialogue.png.meta", "bba01f481368450997f4104c698ce553"),
    "frame": ("Assets/ThirdParty/ArtResource/JinWook/Slime Resource/Ranged_Purple/{first}.meta", None),
    "folder": ("Assets/Scripts/Combat.meta", "3baa0310ea55e42ca8013a6ff0e05e39"),
}
SLIME_REFERENCE = ROOT / "Assets/ThirdParty/ArtResource/JinWook/Slime Resource/Ranged_Purple"
FRAME_SIZE = 512
# 최종 보스는 일반 슬라임보다 크게 보이도록 기준 폭에 곱한다.
WIDTH_SCALE = {"Surge": 1.4}


def write_meta(asset_path: Path, kind: str, guid: str) -> None:
    template_path, template_guid = META_TEMPLATES[kind]
    if kind == "frame":
        first = sorted(p.name for p in SLIME_REFERENCE.glob("*.png"))[0]
        template_path = template_path.format(first=first)
    text = (ROOT / template_path).read_text(encoding="utf-8")
    if template_guid is None:
        template_guid = next(line.split()[1] for line in text.splitlines() if line.startswith("guid:"))
    asset_path.with_name(asset_path.name + ".meta").write_text(text.replace(template_guid, guid), encoding="utf-8")


def ensure_folders() -> None:
    for folder, guid in MANIFEST["folders"].items():
        path = ROOT / folder
        path.mkdir(parents=True, exist_ok=True)
        meta = path.with_name(path.name + ".meta")
        if not meta.exists():
            write_meta(path, "folder", guid)


def remove_magenta(image: Image.Image) -> Image.Image:
    image = image.convert("RGBA")
    pixels = image.load()
    for y in range(image.height):
        for x in range(image.width):
            r, g, b, a = pixels[x, y]
            # 배경 마젠타와 가장자리의 분홍빛 번짐을 함께 지운다.
            if r > 150 and b > 150 and g < 110 and abs(r - b) < 90:
                pixels[x, y] = (0, 0, 0, 0)
    return image


def import_backgrounds(source: Path) -> None:
    for name, entry in MANIFEST["backgrounds"].items():
        image = Image.open(source / "bg" / f"{name}.png").convert("RGBA")
        # 1536x1024 중 가운데 2:1 띠(y 128~896)만 쓴다. 기존 배경은 256x128 그림을 4배로 키운 것처럼
        # 4픽셀 덩어리에 색이 30여 개라서, 256x128로 줄이고 색을 32개로 줄인 뒤 4배로 키운다.
        band = image.crop((0, 128, 1536, 896)).convert("RGB").resize((256, 128), Image.BOX)
        band = band.quantize(colors=32, method=Image.Quantize.MEDIANCUT).convert("RGBA")
        band = band.resize((1024, 512), Image.NEAREST)
        target = ROOT / entry["path"]
        band.save(target)
        write_meta(target, "background", entry["guid"])


def import_portraits(source: Path) -> None:
    for name, entry in MANIFEST["portraits"].items():
        image = remove_magenta(Image.open(source / "portraits" / f"{name}.png"))
        # 기존 초상화와 같은 1254x1254 캔버스에 인물 아래쪽을 맞춘다.
        image = image.resize((1254, 1254), Image.NEAREST)
        target = ROOT / entry["path"]
        image.save(target)
        write_meta(target, "portrait", entry["guid"])


def reference_box() -> tuple:
    first = sorted(SLIME_REFERENCE.glob("*.png"))[0]
    return Image.open(first).convert("RGBA").getbbox()


def import_enemies(source: Path) -> None:
    ref_left, _, ref_right, ref_bottom = reference_box()
    ref_width = ref_right - ref_left
    ref_center = (ref_left + ref_right) / 2
    for name, entry in MANIFEST["enemies"].items():
        atlas_path = next(p for p in (source / "enemies1" / f"{name}.png", source / "enemies2" / f"{name}.png") if p.exists())
        # 생성 그림은 왼쪽을 보고 그렸다. 기존 슬라임 그림은 오른쪽을 보고, 적 prefab이 x 크기를
        # 음수로 뒤집어 왼쪽을 보게 하므로 좌우를 뒤집어 기존 그림과 방향을 맞춘다.
        atlas = ImageOps.mirror(remove_magenta(Image.open(atlas_path)))
        # 뒤집은 atlas에서 프레임 순서는 칸마다 오른쪽부터 왼쪽이다.
        cells = [atlas.crop(((3 - i % 4) * 384, (i // 4) * 512, (3 - i % 4) * 384 + 384, (i // 4) * 512 + 512)) for i in range(8)]
        first_box = cells[0].getbbox()
        # 첫 프레임의 폭을 기존 슬라임 폭에 맞추고, 같은 배율을 8프레임 모두에 쓴다.
        scale = ref_width * WIDTH_SCALE.get(name, 1.0) / (first_box[2] - first_box[0])
        cell_center = (first_box[0] + first_box[2]) / 2
        cell_bottom = first_box[3]
        for cell, frame in zip(cells, entry["frames"]):
            scaled = cell.resize((round(cell.width * scale), round(cell.height * scale)), Image.NEAREST)
            canvas = Image.new("RGBA", (FRAME_SIZE, FRAME_SIZE), (0, 0, 0, 0))
            # 첫 프레임의 가로 중심과 바닥이 기존 슬라임의 중심·바닥선에 오도록 놓는다.
            offset_x = round(ref_center - cell_center * scale)
            offset_y = round(ref_bottom - cell_bottom * scale)
            canvas.paste(scaled, (offset_x, offset_y), scaled)
            target = ROOT / frame["path"]
            canvas.save(target)
            write_meta(target, "frame", frame["guid"])


def main() -> None:
    source = Path(sys.argv[1])
    ensure_folders()
    import_backgrounds(source)
    import_portraits(source)
    import_enemies(source)


if __name__ == "__main__":
    main()
