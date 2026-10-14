#!/usr/bin/env python3
"""List every image under Assets/ with whether the game uses it and its transparent margins.

Usage (from the repository root):
    python3 .agents/skills/pixel-art/scripts/audit_textures.py > audit.json
    python3 .agents/skills/pixel-art/scripts/audit_textures.py --summary

An image counts as used when it is reachable through GUID references from a root:
the enabled build scenes, everything under Assets/Resources (shipped whole), and
asset paths written as "Assets/....ext" string literals in scripts and tests (a
literal with an interpolation counts its whole folder; a bare folder literal is
ignored, since tests use those to skip paths), and GUIDs that ProjectSettings
references (app icons, splash screen). Fonts, TextMeshPro, WebGL templates
and editor-only folders are skipped.
"""
import argparse
import json
import re
import subprocess
import sys
from pathlib import Path

from PIL import Image

ROOT = Path.cwd()
IMAGE_EXT = {".png", ".jpg", ".jpeg", ".psd", ".tga"}
SKIP_PARTS = ("TextMesh", "/Fonts/", "WebGLTemplates", "/Editor/", "/Plugins/")
YAML_EXT = (".unity", ".prefab", ".asset", ".anim", ".controller", ".mat",
            ".spriteatlas", ".overrideController", ".playable", ".guiskin")


def git(*args):
    return subprocess.run(["git", *args], capture_output=True, text=True, cwd=ROOT).stdout


def guid_of(meta: Path):
    m = re.search(r"^guid: ([0-9a-f]{32})", meta.read_text(encoding="utf-8", errors="ignore"), re.M)
    return m.group(1) if m else None


def build_scenes():
    text = (ROOT / "ProjectSettings/EditorBuildSettings.asset").read_text(encoding="utf-8")
    return re.findall(r"enabled: 1\s+path: (\S+)", text)


def reachable():
    """Paths of every asset reachable from the roots through GUID references."""
    guid_path = {}
    for meta in (ROOT / "Assets").rglob("*.meta"):
        g = guid_of(meta)
        if g:
            guid_path[g] = meta.relative_to(ROOT).as_posix()[:-5]
    all_paths = set(guid_path.values())
    refs = {}
    for f in git("ls-files", "Assets").split("\n"):
        if f.endswith(YAML_EXT):
            refs[f] = set(re.findall(r"guid: ([0-9a-f]{32})", (ROOT / f).read_text(encoding="utf-8", errors="ignore")))

    roots = set(build_scenes())
    roots |= {p for p in all_paths if p.startswith("Assets/Resources/")}
    for lit in re.findall(r'"(Assets/[^"]+)"', git("grep", "-h", "-o", "-E", r'"Assets/[^"]+"', "--", "*.cs")):
        if not re.search(r"\.\w+$", lit):
            continue
        if "{" in lit:
            folder = lit[:lit.index("{")].rsplit("/", 1)[0] + "/"
            roots |= {p for p in all_paths if p.startswith(folder)}
        else:
            roots.add(lit)
    for f in (ROOT / "ProjectSettings").glob("*.asset"):
        roots |= {guid_path[g] for g in re.findall(r"guid: ([0-9a-f]{32})", f.read_text(encoding="utf-8", errors="ignore")) if g in guid_path}

    seen, stack = set(), list(roots)
    while stack:
        p = stack.pop()
        if p in seen:
            continue
        seen.add(p)
        stack.extend(q for q in (guid_path.get(g) for g in refs.get(p, ())) if q and q not in seen)
    return seen


def margins(img: Image.Image):
    if img.mode not in ("RGBA", "LA", "P") and "transparency" not in img.info:
        return None
    box = img.convert("RGBA").getchannel("A").point(lambda v: 255 if v > 0 else 0).getbbox()
    if box is None:
        return {"empty": True}
    return {"left": box[0], "top": box[1], "right": img.width - box[2], "bottom": img.height - box[3]}


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--summary", action="store_true")
    args = ap.parse_args()
    live = reachable()
    entries = []
    for p in sorted((ROOT / "Assets").rglob("*")):
        rel = p.relative_to(ROOT).as_posix()
        meta = Path(str(p) + ".meta")
        if p.suffix.lower() not in IMAGE_EXT or any(s in "/" + rel for s in SKIP_PARTS) or not meta.exists():
            continue
        mt = meta.read_text(encoding="utf-8", errors="ignore")
        ppu = re.search(r"spritePixelsToUnits: ([\d.]+)", mt)
        mode = re.search(r"spriteMode: (\d)", mt)
        with Image.open(p) as im:
            entries.append({
                "path": rel, "guid": guid_of(meta), "used": rel in live, "size": list(im.size),
                "colors": len(set(im.convert("RGBA").get_flattened_data())) if im.width * im.height <= 4_000_000 else None,
                "margins": margins(im),
                "spriteMode": {"0": "none", "1": "single", "2": "multiple"}.get(mode.group(1) if mode else "", "?"),
                "ppu": float(ppu.group(1)) if ppu else None,
            })
    if args.summary:
        used = sum(e["used"] for e in entries)
        print(f"images {len(entries)}, used {used}, unused {len(entries) - used}")
        for e in entries:
            if not e["used"]:
                print("UNUSED", e["path"])
        return
    json.dump(entries, sys.stdout, ensure_ascii=False, indent=1)


if __name__ == "__main__":
    main()
