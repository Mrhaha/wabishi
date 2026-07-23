from __future__ import annotations

import argparse
import hashlib
import json
import sys
from pathlib import Path

from fontTools import __version__ as fonttools_version
from fontTools.subset import Options, Subsetter
from fontTools.ttLib import TTFont
from fontTools.varLib.instancer import instantiateVariableFont


ROOT = Path(__file__).resolve().parents[6]
HERE = Path(__file__).resolve().parent
DEFAULT_SOURCE = Path(r"C:\Windows\Fonts\NotoSansSC-VF.ttf")
DEFAULT_OUTPUT = ROOT / "Assets/Resources/Art/MainGame/Flow/wabish_physical_key_sans_sc_semibold.ttf"
DEFAULT_MANIFEST = HERE / "wabish_physical_key_sans_sc_semibold_manifest.json"

# Every character reachable from the current light-key market labels in DiceKingDemo.
# Dark display surfaces continue to use the V4 LED atlas and are intentionally absent.
LABEL_TEXT = (
    "不可购买 已售出 无奉献目标 骰袋已满 差 金 血契吞吃 奉献 购买 "
    "卖出 交互已锁定 骰袋为空 离场结算中 离开市场 完成本轮"
)
CHARACTERS = "".join(sorted(set("0123456789- " + LABEL_TEXT)))
WEIGHT = 600
FAMILY = "Wabish Physical Key Sans SC"
SUBFAMILY = "SemiBold"
FULL_NAME = FAMILY + " " + SUBFAMILY
POSTSCRIPT_NAME = "WabishPhysicalKeySansSC-SemiBold"


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def set_name(font: TTFont, name_id: int, value: str) -> None:
    table = font["name"]
    table.removeNames(nameID=name_id)
    table.setName(value, name_id, 3, 1, 0x0409)
    table.setName(value, name_id, 1, 0, 0)


def rename_derivative(font: TTFont) -> None:
    # The upstream OFL reserves "Source". A project-specific family name also
    # prevents the subset from shadowing a full Noto/Source family in Unity.
    set_name(font, 1, FAMILY)
    set_name(font, 2, SUBFAMILY)
    set_name(font, 3, FULL_NAME + "; Wabish runtime subset v1")
    set_name(font, 4, FULL_NAME)
    set_name(font, 6, POSTSCRIPT_NAME)
    set_name(font, 16, FAMILY)
    set_name(font, 17, SUBFAMILY)
    set_name(
        font,
        13,
        "This derivative font is licensed under the SIL Open Font License, Version 1.1. "
        "Reserved Font Name: Source.",
    )
    set_name(font, 14, "https://scripts.sil.org/OFL")


def build(source: Path, output: Path, manifest: Path) -> None:
    if not source.is_file():
        raise FileNotFoundError(f"Source font not found: {source}")

    variable_font = TTFont(source, recalcTimestamp=False)
    if "fvar" not in variable_font:
        raise RuntimeError(f"Expected a variable font with a wght axis: {source}")

    font = instantiateVariableFont(variable_font, {"wght": WEIGHT}, inplace=False, optimize=True)
    variable_font.close()

    options = Options()
    options.name_IDs = ["*"]
    options.name_legacy = True
    options.name_languages = ["*"]
    options.layout_features = ["*"]
    options.notdef_glyph = True
    options.notdef_outline = True
    options.recommended_glyphs = True
    options.glyph_names = True
    options.recalc_timestamp = False

    subsetter = Subsetter(options=options)
    subsetter.populate(text=CHARACTERS)
    subsetter.subset(font)
    rename_derivative(font)

    if "OS/2" in font:
        font["OS/2"].usWeightClass = WEIGHT
    if "head" in font:
        font["head"].modified = font["head"].created

    output.parent.mkdir(parents=True, exist_ok=True)
    font.save(output, reorderTables=True)
    font.close()

    report = {
        "schema": "wabish-physical-key-runtime-font-v1",
        "family": FAMILY,
        "subfamily": SUBFAMILY,
        "weight": WEIGHT,
        "characters": CHARACTERS,
        "character_count": len(CHARACTERS),
        "source_path": str(source),
        "source_sha256": sha256(source),
        "output_path": str(output.relative_to(ROOT)).replace("\\", "/"),
        "output_bytes": output.stat().st_size,
        "output_sha256": sha256(output),
        "fonttools_version": fonttools_version,
        "license": "SIL Open Font License 1.1",
        "reserved_font_name": "Source",
    }
    manifest.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, ensure_ascii=False, indent=2))


def main() -> int:
    parser = argparse.ArgumentParser(description="Build the approved Wabish physical-key runtime font subset.")
    parser.add_argument("--source", type=Path, default=DEFAULT_SOURCE)
    parser.add_argument("--output", type=Path, default=DEFAULT_OUTPUT)
    parser.add_argument("--manifest", type=Path, default=DEFAULT_MANIFEST)
    args = parser.parse_args()
    build(args.source, args.output, args.manifest)
    return 0


if __name__ == "__main__":
    sys.exit(main())
