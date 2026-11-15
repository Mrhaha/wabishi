"""Focused offline UI review using the actual V4 LED glyph/style/role contracts.

This creates native-resolution source previews, not Unity Play Mode acceptance.
Existing atlas, runtime resources and historical generated artifacts are read only.
"""

from __future__ import annotations

import csv
import importlib.util
import json
import math
import sys
from dataclasses import replace
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "Docs" / "QA"
PREFIX = "20261106_ui_two_level_brightness"
FONT_DIR = ROOT / "Assets/Resources/Art/MainGame/Flow"
ROLE_PATH = FONT_DIR / "main_game_led_text_roles.csv"
GROUP_PATH = FONT_DIR / "main_game_led_text_brightness_groups.csv"
BASE_PATH = FONT_DIR.parent / "arcade_main_game_common_base.png"
RENDERER_PATH = ROOT / "Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedTextBrightnessV1/build_led_text_brightness_v1.py"

spec = importlib.util.spec_from_file_location("wabish_two_level_led_renderer", RENDERER_PATH)
assert spec and spec.loader
r = importlib.util.module_from_spec(spec)
sys.modules[spec.name] = r
spec.loader.exec_module(r)
# The historical renderer defaults to its historical role CSV. Always redirect it
# to the current runtime contract before rendering or validating any sample.
r.ROLE_PATH = ROLE_PATH
r.ROLES = r.load_roles()

NORMAL_ROLES = {"primary_amber", "secondary_amber", "secondary_warm", "ambient", "disabled"}
HIGHLIGHT_ROLES = {"focus_amber", "focus_teal", "warning", "success"}
OLD_COLORS = {
    "primary_amber": "#FFC04A", "secondary_amber": "#D99A4A",
    "secondary_warm": "#BDA57A", "ambient": "#806B4E", "disabled": "#8C8068",
}
CURRENT_ROLES = dict(r.ROLES)
BEFORE_ROLES = {key: replace(role, core_hex=OLD_COLORS.get(key, role.core_hex)) for key, role in r.ROLES.items()}
SAMPLES = [
    ("chapter", (50, 23, 760, 50), "第 1 章 · 第 1 关", "primary_amber", False),
    ("lives", (854, 23, 158, 50), "生命 3", "primary_amber", False),
    ("gold", (1052, 23, 174, 50), "金币 8", "primary_amber", False),
    ("target", (40, 191, 104, 31), "目标 14", "primary_amber", False),
    ("score", (40, 222, 104, 31), "当前 0", "primary_amber", False),
    ("terminal_title", (1062, 174, 162, 44), "本关规则", "primary_amber", False),
    ("terminal_detail", (1072, 230, 142, 174), "无额外规则，检查基础输出", "secondary_warm", True),
    ("sort_hint", (320, 492, 570, 28), "拖动骰子调整结算顺序", "secondary_amber", False),
    ("main_action", (458, 574, 356, 72), "SPACE  投掷", "focus_amber", False),
]
BUTTON = (844, 584, 184, 54)
BUTTON_LABEL = (852, 586, 168, 50)
TERMINAL_CROP = (1044, 145, 198, 282)
BUTTON_CROP = (828, 572, 216, 82)


def oklab_l(color: str) -> float:
    red, green, blue = [r.linear_channel(channel) for channel in r.parse_hex(color)]
    l = (0.4122214708 * red + 0.5363325363 * green + 0.0514459929 * blue) ** (1 / 3)
    m = (0.2119034982 * red + 0.6806995451 * green + 0.1073969566 * blue) ** (1 / 3)
    s = (0.0883024619 * red + 0.2817188376 * green + 0.6299787005 * blue) ** (1 / 3)
    return 0.2104542553 * l + 0.793617785 * m - 0.0040720468 * s


def annotation_font(size: int) -> ImageFont.FreeTypeFont:
    return ImageFont.truetype("C:/Windows/Fonts/msyh.ttc", size)


def annotate(image: Image.Image, xy: tuple[int, int], text: str, size: int = 18) -> None:
    ImageDraw.Draw(image).text(xy, text, font=annotation_font(size), fill=(214, 218, 214, 255))


def draw_sample(image, name, rect, text, role, wrapped, scale, variant):
    alignment = "middle-left" if name == "chapter" else "middle-center"
    r.draw_role_text(image, variant, name, rect, text, role, alignment, wrapped, scale, False)


def legacy_rect(image, rect, color, scale):
    layer = Image.new("RGBA", image.size, (0, 0, 0, 0))
    r.fill_rect(layer, rect, tuple(round(channel * 255) for channel in color), scale)
    image.alpha_composite(layer)


def legacy_border(image, rect, color, width, scale):
    x, y, w, h = rect
    for edge in ((x, y, w, width), (x, y + h - width, w, width), (x, y, width, h), (x + w - width, y, width, h)):
        legacy_rect(image, edge, color, scale)


def button(image: Image.Image, scale: float, before: bool, hovered: bool = False) -> None:
    x, y, width, height = BUTTON
    if before:
        # Legacy DrawScalableButtonFrame / DrawMiniCrown source colors and shapes.
        edge, fill = (.22, .15, .10, .92), (1, .93, .72, .92)
        legacy_rect(image, (x + 3, y + 4, width, height), (.10, .06, .03, .28), scale)
        legacy_rect(image, BUTTON, fill, scale)
        legacy_border(image, BUTTON, edge, 3, scale)
        legacy_border(image, (x + 6, y + 6, width - 12, height - 12), (1, .98, .84, .82), 2, scale)
        accent_width = min(42, max(28, height * .72))
        legacy_rect(image, (x + 8, y + 8, accent_width, height - 16), (.84, .57, .18, .72), scale)
        crown_x, crown_y = x + 8 + accent_width / 2 - 12, y + height / 2 - 10
        crown_fill = (1, .92, .66, .92)
        legacy_rect(image, (crown_x, crown_y + 20 * .68, 24, 20 * .22), edge, scale)
        for cx, cy, cw, ch in ((.08, .62, .84, .18), (.08, .32, .16, .36), (.76, .32, .16, .36), (.42, .05, .16, .6)):
            legacy_rect(image, (crown_x + 24 * cx, crown_y + 20 * cy, 24 * cw, 20 * ch), crown_fill, scale)
        role = replace(CURRENT_ROLES["primary_amber"], core_hex="#2E1C0F", preferred_geometry="compact", fallback_geometries=("micro",))
        r.ROLES["legacy_button_ink"] = role
        draw_sample(image, "spell_bag", BUTTON, "法术袋 0/3", "legacy_button_ink", False, scale, "before")
    else:
        r.fill_rect(image, BUTTON, (11, 18, 21, 255), scale)
        r.fill_rect(image, (x + 4, y + 4, width - 8, height - 8), (4, 9, 11, 255), scale)
        r.outline_rect(image, BUTTON, (61, 67, 59, 255), 1, scale)
        if hovered:
            r.outline_rect(image, (x + 1, y + 1, width - 2, height - 2), (255, 191, 74, 179), 1, scale)
        draw_sample(image, "spell_bag_hover" if hovered else "spell_bag", BUTTON_LABEL, "法术袋 0/3", "focus_amber" if hovered else "primary_amber", False, scale, "after")


def board(scale: float, before: bool) -> Image.Image:
    r.ROLES = dict(BEFORE_ROLES if before else CURRENT_ROLES)
    image = Image.open(BASE_PATH).convert("RGBA").resize(r.scaled_size(scale), Image.Resampling.LANCZOS)
    for name, rect, text, role, wrapped in SAMPLES:
        draw_sample(image, name, rect, text, role, wrapped, scale, "before" if before else "after")
    button(image, scale, before)
    # Review annotations are system-font labels, deliberately outside the source UI.
    annotate(image, (round(250 * scale), round(125 * scale)), "离线同源局部校验：普通文字等亮，仅必要操作高亮", max(12, round(17 * scale)))
    annotate(image, (round(250 * scale), round(232 * scale)), "此板仅检查文字、终端与法术入口，不代表 Unity 动态验收", max(11, round(15 * scale)))
    return image


def crop(image: Image.Image, rect: tuple[int, int, int, int], scale: float) -> Image.Image:
    return image.crop(r.physical_rect(rect, scale))


def comparison(before: Image.Image, after: Image.Image, scale: float) -> Image.Image:
    terminal_before, terminal_after = [crop(image, TERMINAL_CROP, scale) for image in (before, after)]
    button_before, button_after = [crop(image, BUTTON_CROP, scale) for image in (before, after)]
    width = max(terminal_before.width, button_before.width)
    pad, gap, heading = round(24 * scale), round(28 * scale), round(72 * scale)
    height = heading + terminal_before.height + gap + button_before.height + pad
    result = Image.new("RGBA", (2 * width + 3 * pad, height), (7, 13, 16, 255))
    for index, (terminal, bag) in enumerate(((terminal_before, button_before), (terminal_after, button_after))):
        x = pad + index * (width + pad)
        annotate(result, (x, round(12 * scale)), "修改前（源色重现）" if index == 0 else "修改后（运行合同）", max(12, round(17 * scale)))
        result.alpha_composite(terminal, (x, heading))
        result.alpha_composite(bag, (x, heading + terminal.height + gap))
    return result


def contract_report() -> dict:
    with GROUP_PATH.open(encoding="utf-8-sig", newline="") as handle:
        groups = list(csv.DictReader(handle))
    details = []
    for row in groups:
        role = CURRENT_ROLES[row["role_id"]]
        actual, target, tolerance = oklab_l(role.core_hex), float(row["target_oklab_l"]), float(row["tolerance"])
        details.append({"role": role.role_id, "grade": row["brightness_grade"], "core_hex": role.core_hex, "alpha": role.alpha, "oklab_l": round(actual, 8), "target": target, "pass": abs(actual - target) <= tolerance + 1e-6})
    grades = {row["brightness_grade"] for row in groups if row["role_id"] != "ink_on_light"}
    return {
        "non_material_brightness_grade_count": len(grades),
        "two_grades_only": len(grades) == 2,
        "ordinary_exact_color_and_alpha": all(CURRENT_ROLES[key].core_hex.upper() == "#FFC04A" and CURRENT_ROLES[key].alpha == 1.0 for key in NORMAL_ROLES),
        "highlight_alpha_one": all(CURRENT_ROLES[key].alpha == 1.0 for key in HIGHLIGHT_ROLES),
        "all_grade_targets_pass": all(row["pass"] for row in details),
        "roles": details,
        "ink_on_light_material_exception": "原市场浅色实体键帽保留深墨字；不计入 LED 两档亮度。",
    }


def main() -> None:
    r.ELEMENTS.clear()
    r.USED_CHARACTERS.clear()
    outputs = []
    after_1080 = None
    for scale in (1.0, 1.5):
        before, after = board(scale, True), board(scale, False)
        for label, image in (("after", after), ("comparison", comparison(before, after, scale))):
            path = OUT / f"{PREFIX}_{label}_{round(1280 * scale)}x{round(720 * scale)}.png"
            r.save_rgb(image, path)
            outputs.append(path)
        if scale == 1.5:
            after_1080 = after
    assert after_1080 is not None
    pressure = after_1080.resize((round(1920 * 0.606), round(1080 * 0.606)), Image.Resampling.LANCZOS)
    pressure_path = OUT / f"{PREFIX}_pressure_0606.png"
    r.save_rgb(pressure, pressure_path)
    outputs.append(pressure_path)
    after_elements = [item for item in r.ELEMENTS if item["screen"] == "after"]
    report = {
        "status": "offline-source-review-only",
        "generated_date": "2026-11-06",
        "notes": ["未启动 Unity Play Mode，本报告不宣告 UI 运行验收通过。", "原生板文字由实际 atlas/map/style/role 与运行坐标直接绘制；未对文字做二次缩放。", "0.606 压力图仅检查非整数显示可识别性，不替代原生锐度验收。", "前图采用修改前源色与旧按钮配色重现，不是历史运行截图。"],
        "contract": contract_report(),
        "font": {"atlas": str(r.ATLAS_PATH.relative_to(ROOT)), "atlas_sha256": r.sha256(r.ATLAS_PATH), "map_sha256": r.sha256(r.MAP_PATH), "styles_sha256": r.sha256(r.STYLES_PATH), "roles_sha256": r.sha256(ROLE_PATH), "groups_sha256": r.sha256(GROUP_PATH), "glyph_count": len(r.GLYPHS)},
        "layout_checks": {"native_resolutions": [[1280, 720], [1920, 1080]], "sample_missing_glyph_count": len(set(r.USED_CHARACTERS) - set(r.GLYPHS)), "after_clipped_count": sum(bool(item["clipped_outside_rect"]) for item in after_elements), "after_contrast_failure_count": sum(not item["p05_pass"] for item in after_elements)},
        "elements": after_elements,
        "outputs": [{"path": str(path.relative_to(ROOT)), "size": list(Image.open(path).size), "sha256": r.sha256(path)} for path in outputs],
        "generator_sha256": r.sha256(Path(__file__)),
    }
    metrics_path = OUT / f"{PREFIX}_metrics.json"
    metrics_path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    checks = report["contract"]
    print(json.dumps({"contract": {key: value for key, value in checks.items() if key != "roles"}, "layout_checks": report["layout_checks"], "outputs": [str(path.relative_to(ROOT)) for path in outputs], "metrics": str(metrics_path.relative_to(ROOT))}, ensure_ascii=False, indent=2))
    if not all(checks[key] for key in ("two_grades_only", "ordinary_exact_color_and_alpha", "highlight_alpha_one", "all_grade_targets_pass")):
        raise SystemExit("运行合同尚未达到两档等亮要求")


if __name__ == "__main__":
    main()
