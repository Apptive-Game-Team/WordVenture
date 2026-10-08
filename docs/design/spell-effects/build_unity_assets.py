"""원소 스크립트가 그린 시트를 Unity 에셋으로 연결한다.

만드는 파일 (원소마다):
- Assets/Art/Combat/Spells/<원소>/<원소><종류>.png.meta  스프라이트 슬라이스, PPU, Point 필터, 무압축
- Assets/Art/Combat/Spells/<원소>/<원소><종류>.anim      스프라이트 프레임 애니메이션
- Assets/Art/Combat/Spells/<원소>/<원소>{Shoot,Drop,Explode}.controller
  Shoot·Drop 은 Hit trigger 로 <원소>Hit clip 으로 넘어간다.

고치는 파일: Assets/Prefabs/Combat/Spells/{Shoot,Drop,Explode}/*.prefab 15개의
첫 스프라이트, animator controller, 회전. collider 와 scale 은 건드리지 않는다.

GUID 와 fileID 는 경로에서 계산하므로 다시 실행해도 바뀌지 않는다.
"""
from __future__ import annotations

import hashlib
import re
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent))
import common  # noqa: E402

ELEMENTS = ["Fire", "Ice", "Rock", "Lightning", "Holy"]
SHEET_KINDS = ["Shoot", "Drop", "Hit", "Explode"]
# prefab 파일 이름은 번개를 Electric 으로 쓴다.
PREFAB_ELEMENT_NAMES = {"Fire": "Fire", "Ice": "Ice", "Rock": "Rock", "Lightning": "Electric", "Holy": "Holy"}
PREFAB_ROOT = common.REPO_ROOT / "Assets" / "Prefabs" / "Combat" / "Spells"
SPRITE_CLASS_ID = 212


def digest(text: str) -> str:
    return hashlib.md5(f"WordVenture.SpellEffects/{text}".encode()).hexdigest()


def guid_for(path: Path) -> str:
    return digest(path.relative_to(common.REPO_ROOT).as_posix())


def file_id_for(text: str) -> int:
    """controller 안 오브젝트의 양수 fileID."""
    return int(digest(text)[:15], 16) + 1


def sprite_internal_id(sheet_name: str, index: int) -> int:
    """Unity 가 스프라이트에 붙이는 32bit 부호 있는 internalID 와 같은 범위의 값."""
    value = int(digest(f"{sheet_name}_{index}")[:8], 16)
    return value - (1 << 32) if value >= (1 << 31) else value


def write(path: Path, text: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text, encoding="utf-8", newline="\n")


def folder_meta(folder: Path) -> None:
    meta = folder.with_name(folder.name + ".meta")
    if meta.exists():
        return
    write(meta, f"""fileFormatVersion: 2
guid: {guid_for(folder)}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData:
  assetBundleName:
  assetBundleVariant:
""")


def platform_setting(target: str) -> str:
    return f"""  - serializedVersion: 3
    buildTarget: {target}
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
"""


def sprite_entry(sheet_name: str, index: int, frame: int, count: int) -> str:
    # Unity 의 rect y 는 아래에서 위로 센다. 시트는 위에서 아래로 쌓았다.
    y = (count - 1 - index) * frame
    return f"""    - serializedVersion: 2
      name: {sheet_name}_{index}
      rect:
        serializedVersion: 2
        x: 0
        y: {y}
        width: {frame}
        height: {frame}
      alignment: 0
      pivot: {{x: 0.5, y: 0.5}}
      border: {{x: 0, y: 0, z: 0, w: 0}}
      outline: []
      physicsShape: []
      tessellationDetail: 0
      bones: []
      spriteID: {digest(f"{sheet_name}_{index}/spriteID")}
      internalID: {sprite_internal_id(sheet_name, index)}
      vertices: []
      indices:
      edges: []
      weights: []
"""


def texture_meta(png: Path, sheet_name: str, frame: int, count: int) -> str:
    platforms = "".join(platform_setting(t) for t in ["DefaultTexturePlatform", "Standalone", "WebGL"])
    sprites = "".join(sprite_entry(sheet_name, i, frame, count) for i in range(count))
    names = "".join(f"      {sheet_name}_{i}: {sprite_internal_id(sheet_name, i)}\n" for i in range(count))
    return f"""fileFormatVersion: 2
guid: {guid_for(png)}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 12
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMasterTextureLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 0
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 2
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: {frame}
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 0
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  cookieLightType: 1
  platformSettings:
{platforms}  spriteSheet:
    serializedVersion: 2
    sprites:
{sprites}    outline: []
    physicsShape: []
    bones: []
    spriteID:
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
    secondaryTextures: []
    nameFileIdTable:
{names}  spritePackingTag:
  pSDRemoveMatte: 0
  pSDShowRemoveMatteOption: 0
  userData:
  assetBundleName:
  assetBundleVariant:
"""


def native_meta(asset: Path, main_object_file_id: int) -> str:
    return f"""fileFormatVersion: 2
guid: {guid_for(asset)}
NativeFormatImporter:
  externalObjects: {{}}
  mainObjectFileID: {main_object_file_id}
  userData:
  assetBundleName:
  assetBundleVariant:
"""


def sprite_ref(png: Path, sheet_name: str, index: int) -> str:
    return f"{{fileID: {sprite_internal_id(sheet_name, index)}, guid: {guid_for(png)}, type: 3}}"


def animation_clip(png: Path, sheet_name: str, spec: dict) -> str:
    count, fps = spec["count"], spec["fps"]
    keys = "".join(
        f"    - time: {index / fps:.7g}\n      value: {sprite_ref(png, sheet_name, index)}\n"
        for index in range(count)
    )
    mapping = "".join(f"    - {sprite_ref(png, sheet_name, index)}\n" for index in range(count))
    return f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!74 &7400000
AnimationClip:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: {sheet_name}
  serializedVersion: 7
  m_Legacy: 0
  m_Compressed: 0
  m_UseHighQualityCurve: 1
  m_RotationCurves: []
  m_CompressedRotationCurves: []
  m_EulerCurves: []
  m_PositionCurves: []
  m_ScaleCurves: []
  m_FloatCurves: []
  m_PPtrCurves:
  - serializedVersion: 2
    curve:
{keys}    attribute: m_Sprite
    path:
    classID: {SPRITE_CLASS_ID}
    script: {{fileID: 0}}
    flags: 2
  m_SampleRate: {fps}
  m_WrapMode: 0
  m_Bounds:
    m_Center: {{x: 0, y: 0, z: 0}}
    m_Extent: {{x: 0, y: 0, z: 0}}
  m_ClipBindingConstant:
    genericBindings:
    - serializedVersion: 2
      path: 0
      attribute: 0
      script: {{fileID: 0}}
      typeID: {SPRITE_CLASS_ID}
      customType: 23
      isPPtrCurve: 1
      isIntCurve: 0
      isSerializeReferenceCurve: 0
    pptrCurveMapping:
{mapping}  m_AnimationClipSettings:
    serializedVersion: 2
    m_AdditiveReferencePoseClip: {{fileID: 0}}
    m_AdditiveReferencePoseTime: 0
    m_StartTime: 0
    m_StopTime: {count / fps:.7g}
    m_OrientationOffsetY: 0
    m_Level: 0
    m_CycleOffset: 0
    m_HasAdditiveReferencePose: 0
    m_LoopTime: {1 if spec["loop"] else 0}
    m_LoopBlend: 0
    m_LoopBlendOrientation: 0
    m_LoopBlendPositionY: 0
    m_LoopBlendPositionXZ: 0
    m_KeepOriginalOrientation: 0
    m_KeepOriginalPositionY: 1
    m_KeepOriginalPositionXZ: 0
    m_HeightFromFeet: 0
    m_Mirror: 0
  m_EditorCurves: []
  m_EulerEditorCurves: []
  m_HasGenericRootTransform: 0
  m_HasMotionFloatCurves: 0
  m_Events: []
"""


def animator_state(file_id: int, name: str, clip: Path, transition_ids: list[int]) -> str:
    transitions = "".join(f"\n  - {{fileID: {t}}}" for t in transition_ids) or " []"
    return f"""--- !u!1102 &{file_id}
AnimatorState:
  serializedVersion: 6
  m_ObjectHideFlags: 1
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: {name}
  m_Speed: 1
  m_CycleOffset: 0
  m_Transitions:{transitions}
  m_StateMachineBehaviours: []
  m_Position: {{x: 50, y: 50, z: 0}}
  m_IKOnFeet: 0
  m_WriteDefaultValues: 1
  m_Mirror: 0
  m_SpeedParameterActive: 0
  m_MirrorParameterActive: 0
  m_CycleOffsetParameterActive: 0
  m_TimeParameterActive: 0
  m_Motion: {{fileID: 7400000, guid: {guid_for(clip)}, type: 2}}
  m_Tag:
  m_SpeedParameter:
  m_MirrorParameter:
  m_CycleOffsetParameter:
  m_TimeParameter:
"""


def hit_transition(file_id: int, destination_id: int) -> str:
    # 스프라이트 애니메이션이라 섞을 것이 없으므로 전환 시간 0 으로 바로 넘어간다.
    return f"""--- !u!1101 &{file_id}
AnimatorStateTransition:
  m_ObjectHideFlags: 1
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name:
  m_Conditions:
  - m_ConditionMode: 1
    m_ConditionEvent: Hit
    m_EventTreshold: 0
  m_DstStateMachine: {{fileID: 0}}
  m_DstState: {{fileID: {destination_id}}}
  m_Solo: 0
  m_Mute: 0
  m_IsExit: 0
  serializedVersion: 3
  m_TransitionDuration: 0
  m_TransitionOffset: 0
  m_ExitTime: 0
  m_HasExitTime: 0
  m_HasFixedDuration: 1
  m_InterruptionSource: 0
  m_OrderedInterruption: 1
  m_CanTransitionToSelf: 1
"""


def animator_controller(name: str, main_clip: Path, hit_clip: Path | None) -> str:
    """main_clip 을 기본 상태로 재생한다. hit_clip 이 있으면 Hit trigger 로 넘어간다.

    Explode 도 SpellObj 가 충돌 시 Hit trigger 를 보내므로 parameter 는 항상 둔다.
    전이가 없으면 trigger 는 아무 일도 하지 않는다.
    """
    machine_id = file_id_for(f"{name}/machine")
    main_id = file_id_for(f"{name}/main")
    hit_id = file_id_for(f"{name}/hit")
    transition_id = file_id_for(f"{name}/transition")
    child_states = f"""  - serializedVersion: 1
    m_State: {{fileID: {main_id}}}
    m_Position: {{x: 240, y: 120, z: 0}}
"""
    objects = animator_state(main_id, name, main_clip, [transition_id] if hit_clip else [])
    if hit_clip:
        child_states += f"""  - serializedVersion: 1
    m_State: {{fileID: {hit_id}}}
    m_Position: {{x: 240, y: 240, z: 0}}
"""
        objects += animator_state(hit_id, "Hit", hit_clip, [])
        objects += hit_transition(transition_id, hit_id)
    return f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!91 &9100000
AnimatorController:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: {name}
  serializedVersion: 5
  m_AnimatorParameters:
  - m_Name: Hit
    m_Type: 9
    m_DefaultFloat: 0
    m_DefaultInt: 0
    m_DefaultBool: 0
    m_Controller: {{fileID: 9100000}}
  m_AnimatorLayers:
  - serializedVersion: 5
    m_Name: Base Layer
    m_StateMachine: {{fileID: {machine_id}}}
    m_Mask: {{fileID: 0}}
    m_Motions: []
    m_Behaviours: []
    m_BlendingMode: 0
    m_SyncedLayerIndex: -1
    m_DefaultWeight: 0
    m_IKPass: 0
    m_SyncedLayerAffectsTiming: 0
    m_Controller: {{fileID: 9100000}}
--- !u!1107 &{machine_id}
AnimatorStateMachine:
  serializedVersion: 6
  m_ObjectHideFlags: 1
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: Base Layer
  m_ChildStates:
{child_states}  m_ChildStateMachines: []
  m_AnyStateTransitions: []
  m_EntryTransitions: []
  m_StateMachineTransitions: {{}}
  m_StateMachineBehaviours: []
  m_AnyStatePosition: {{x: 50, y: 20, z: 0}}
  m_EntryPosition: {{x: 50, y: 120, z: 0}}
  m_ExitPosition: {{x: 800, y: 120, z: 0}}
  m_ParentStateMachinePosition: {{x: 800, y: 20, z: 0}}
  m_DefaultState: {{fileID: {main_id}}}
{objects}"""


def build_element(element: str) -> dict[str, tuple[Path, Path]]:
    """원소의 시트 4장에 meta·clip 을 쓰고 controller 3개를 만든다.

    반환: 종류 → (첫 프레임이 들어 있는 png, controller) — prefab 연결에 쓴다.
    """
    folder = common.ART_ROOT / element
    folder_meta(common.ART_ROOT)
    folder_meta(folder)
    clips: dict[str, Path] = {}
    for kind in SHEET_KINDS:
        spec = common.SHEET_SPECS[kind]
        sheet_name = f"{element}{kind}"
        png = folder / f"{sheet_name}.png"
        if not png.exists():
            raise FileNotFoundError(f"{png} 가 없다. {element.lower()}.py 를 먼저 실행한다.")
        write(png.with_name(png.name + ".meta"), texture_meta(png, sheet_name, spec["frame"], spec["count"]))
        clip = folder / f"{sheet_name}.anim"
        write(clip, animation_clip(png, sheet_name, spec))
        write(clip.with_name(clip.name + ".meta"), native_meta(clip, 7400000))
        clips[kind] = clip

    links = {}
    for kind in ["Shoot", "Drop", "Explode"]:
        name = f"{element}{kind}"
        controller = folder / f"{name}.controller"
        hit_clip = clips["Hit"] if kind != "Explode" else None
        write(controller, animator_controller(name, clips[kind], hit_clip))
        write(controller.with_name(controller.name + ".meta"), native_meta(controller, 9100000))
        links[kind] = (folder / f"{name}.png", controller)
    return links


def patch_prefab(prefab: Path, element: str, kind: str, png: Path, controller: Path) -> None:
    text = prefab.read_text(encoding="utf-8")
    first_sprite = sprite_ref(png, f"{element}{kind}", 0)
    text, sprites = re.subn(r"m_Sprite: \{[^}]*\}", f"m_Sprite: {first_sprite}", text)
    text, controllers = re.subn(
        r"m_Controller: \{fileID: 9100000, guid: [0-9a-f]+, type: 2\}",
        f"m_Controller: {{fileID: 9100000, guid: {guid_for(controller)}, type: 2}}",
        text,
    )
    # 예전 Drop 은 Shoot 스프라이트를 -90도 돌려 썼다. 새 시트는 방향대로 그렸으므로 회전을 없앤다.
    text = re.sub(r"m_LocalRotation: \{[^}]*\}", "m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}", text)
    text = re.sub(r"m_LocalEulerAnglesHint: \{[^}]*\}", "m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}", text)
    if sprites != 1 or controllers != 1:
        raise RuntimeError(f"{prefab}: SpriteRenderer {sprites}개, Animator {controllers}개. 1개씩이어야 한다")
    prefab.write_text(text, encoding="utf-8", newline="\n")


def main() -> None:
    for element in ELEMENTS:
        links = build_element(element)
        for kind, (png, controller) in links.items():
            prefab = PREFAB_ROOT / kind / f"{PREFAB_ELEMENT_NAMES[element]}{kind}.prefab"
            patch_prefab(prefab, element, kind, png, controller)
        print(f"{element}: 시트 4장, controller 3개, prefab 3개")


if __name__ == "__main__":
    main()
