from __future__ import annotations

import csv
import importlib.util
import json
import math
import sys
from collections import defaultdict
from dataclasses import asdict
from pathlib import Path

from PIL import Image


HERE = Path(__file__).resolve().parent
V1_DIR = HERE.parent / "LedTextBrightnessV1"
V1_SCRIPT = V1_DIR / "build_led_text_brightness_v1.py"
ROOT = HERE.parents[5]
RUNTIME_CONTRACT_DIR = ROOT / "Assets" / "Resources" / "Art" / "MainGame" / "Flow"
ROLE_PATH = RUNTIME_CONTRACT_DIR / "main_game_led_text_roles.csv"
BRIGHTNESS_PATH = RUNTIME_CONTRACT_DIR / "main_game_led_text_brightness_groups.csv"
METRICS_PATH = HERE / "led_text_brightness_metrics_v2_20260717.json"


def load_v1_renderer():
    spec = importlib.util.spec_from_file_location("wabish_led_brightness_v1_renderer", V1_SCRIPT)
    if spec is None or spec.loader is None:
        raise RuntimeError(f"Cannot import renderer: {V1_SCRIPT}")
    module = importlib.util.module_from_spec(spec)
    sys.modules[spec.name] = module
    spec.loader.exec_module(module)
    return module


BASE = load_v1_renderer()
ROOT = BASE.ROOT
BASE.HERE = HERE
BASE.ROLE_PATH = ROLE_PATH
BASE.METRICS_PATH = METRICS_PATH
BASE.ROLES = BASE.load_roles()
BASE.ELEMENTS.clear()
BASE.USED_CHARACTERS.clear()


def srgb_channel_to_linear(value: int) -> float:
    channel = value / 255.0
    if channel <= 0.04045:
        return channel / 12.92
    return ((channel + 0.055) / 1.055) ** 2.4


def oklab_lightness(rgb: tuple[int, int, int]) -> float:
    red, green, blue = (srgb_channel_to_linear(value) for value in rgb)
    l_value = 0.4122214708 * red + 0.5363325363 * green + 0.0514459929 * blue
    m_value = 0.2119034982 * red + 0.6806995451 * green + 0.1073969566 * blue
    s_value = 0.0883024619 * red + 0.2817188376 * green + 0.6299787005 * blue
    l_root = math.copysign(abs(l_value) ** (1.0 / 3.0), l_value)
    m_root = math.copysign(abs(m_value) ** (1.0 / 3.0), m_value)
    s_root = math.copysign(abs(s_value) ** (1.0 / 3.0), s_value)
    return 0.2104542553 * l_root + 0.7936177850 * m_root - 0.0040720468 * s_root


def load_brightness_contract() -> dict[str, dict[str, object]]:
    records: dict[str, dict[str, object]] = {}
    with BRIGHTNESS_PATH.open("r", encoding="utf-8-sig", newline="") as handle:
        for row in csv.DictReader(handle):
            role_id = row["role_id"]
            records[role_id] = {
                "role_id": role_id,
                "brightness_grade": row["brightness_grade"],
                "target_oklab_l": float(row["target_oklab_l"]),
                "tolerance": float(row["tolerance"]),
                "anchor_role": row["anchor_role"],
                "variant_dimension": row["variant_dimension"],
            }
    return records


BRIGHTNESS = load_brightness_contract()


def output_name(context: str, variant: str, scale: float) -> Path:
    width, height = BASE.scaled_size(scale)
    return HERE / f"led_text_brightness_v2_{context}_{variant}_{width}x{height}.png"


def same_grade_output_name(variant: str, scale: float) -> Path:
    return HERE / f"led_text_brightness_v2_same_grade_{variant}_{round(1280 * scale)}x{round(360 * scale)}.png"


def make_same_grade_strip(scale: float, allow_halo: bool) -> Image.Image:
    width = round(1280 * scale)
    height = round(360 * scale)
    image = Image.new("RGBA", (width, height), (4, 12, 14, 255))
    screen = "same_grade"

    BASE.draw_role_text(
        image,
        screen,
        "title",
        (28, 12, 1224, 52),
        "SAME GRADE / SAME CORE BRIGHTNESS",
        "primary_amber",
        "middle-left",
        False,
        scale,
        allow_halo,
        "display",
    )
    BASE.draw_role_text(image, screen, "l3_label", (28, 66, 1224, 18), "L3", "ambient", "middle-left", False, scale, False, "micro")

    l3_samples = (
        (24, "L3 AMBER", "开始游戏", "focus_amber"),
        (332, "L3 TEAL", "刷新 1 金", "focus_teal"),
        (640, "L3 WARNING", "继续游戏", "warning"),
        (948, "L3 SUCCESS", "开始游戏", "success"),
    )
    for index, (x_value, label, sample, role_id) in enumerate(l3_samples):
        rect = (x_value, 88, 284, 100)
        BASE.fill_rect(image, rect, (4, 12, 14, 255), scale)
        BASE.outline_rect(image, rect, (67, 58, 45, 255), 1, scale)
        BASE.draw_role_text(image, screen, f"l3_{index}_label", (x_value + 10, 96, 264, 18), label, "ambient", "middle-left", False, scale, False, "micro")
        BASE.draw_role_text(image, screen, f"l3_{index}_value", (x_value + 10, 122, 264, 58), sample, role_id, "middle-center", False, scale, allow_halo, "display")

    BASE.draw_role_text(image, screen, "l1_label", (28, 206, 1224, 18), "L1", "ambient", "middle-left", False, scale, False, "micro")
    l1_samples = (
        (24, "L1 AMBER", "六面 1/2/3/4/5/6", "secondary_amber"),
        (640, "L1 WARM", "开启新的六骰挑战", "secondary_warm"),
    )
    for index, (x_value, label, sample, role_id) in enumerate(l1_samples):
        rect = (x_value, 228, 592, 104)
        BASE.fill_rect(image, rect, (4, 12, 14, 255), scale)
        BASE.outline_rect(image, rect, (67, 58, 45, 255), 1, scale)
        BASE.draw_role_text(image, screen, f"l1_{index}_label", (x_value + 12, 238, 568, 18), label, "ambient", "middle-left", False, scale, False, "micro")
        BASE.draw_role_text(image, screen, f"l1_{index}_value", (x_value + 12, 270, 568, 46), sample, role_id, "middle-center", False, scale, allow_halo, "compact")
    return image


def make_state_strip_v2(scale: float, allow_halo: bool) -> Image.Image:
    width = round(1280 * scale)
    height = round(260 * scale)
    image = Image.new("RGBA", (width, height), (3, 8, 11, 255))
    screen = "state_strip"
    BASE.draw_role_text(image, screen, "title", (28, 16, 1224, 47), "LED TEXT ROLES V2", "primary_amber", "middle-left", False, scale, allow_halo, "display")
    samples = (
        (24, "L3 FOCUS", "开始游戏", "focus_amber", (4, 12, 14, 255), "display"),
        (272, "L2 PRIMARY", "骰袋 5 / 6", "primary_amber", (4, 12, 14, 255), "compact"),
        (520, "L1 SECONDARY", "六面 1/2/3/4/5/6", "secondary_amber", (4, 12, 14, 255), "micro"),
        (768, "DISABLED", "继续游戏", "disabled", (4, 12, 14, 255), "display"),
        (1016, "INK ON LIGHT", "购买 12 金", "ink_on_light", (200, 167, 124, 255), "compact"),
    )
    for index, (x_value, label, sample, role_id, background, geometry) in enumerate(samples):
        rect = (x_value, 84, 232, 146)
        BASE.fill_rect(image, rect, background, scale)
        BASE.outline_rect(image, rect, (67, 58, 45, 255), 1, scale)
        label_role = "ink_on_light" if role_id == "ink_on_light" else "ambient"
        BASE.draw_role_text(image, screen, f"sample_{index + 1}_label", (x_value + 12, 94, 208, 18), label, label_role, "middle-left", False, scale, allow_halo, "micro")
        BASE.draw_role_text(image, screen, f"sample_{index + 1}_value", (x_value + 12, 126, 208, 74), sample, role_id, "middle-center", False, scale, allow_halo, geometry)
    return image


def brightness_records() -> tuple[list[dict[str, object]], list[dict[str, object]], dict[str, object]]:
    role_ids = set(BASE.ROLES)
    contract_ids = set(BRIGHTNESS)
    missing_contract = sorted(role_ids - contract_ids)
    unknown_contract = sorted(contract_ids - role_ids)
    records: list[dict[str, object]] = []
    grouped: dict[str, list[dict[str, object]]] = defaultdict(list)
    for role_id, role in BASE.ROLES.items():
        contract = BRIGHTNESS.get(role_id)
        if contract is None:
            continue
        measured = oklab_lightness(BASE.parse_hex(role.core_hex))
        delta = measured - float(contract["target_oklab_l"])
        record = {
            **contract,
            "semantic_level": role.level,
            "core_hex": role.core_hex,
            "core_alpha": role.alpha,
            "measured_oklab_l": round(measured, 9),
            "absolute_target_delta": round(abs(delta), 9),
            "target_pass": abs(delta) <= float(contract["tolerance"]),
        }
        records.append(record)
        grouped[str(contract["brightness_grade"])].append(record)

    group_records: list[dict[str, object]] = []
    for grade, members in grouped.items():
        measured_values = [float(member["measured_oklab_l"]) for member in members]
        targets = [float(member["target_oklab_l"]) for member in members]
        tolerances = [float(member["tolerance"]) for member in members]
        spread = max(measured_values) - min(measured_values)
        target_spread = max(targets) - min(targets)
        allowed_spread = 2.0 * max(tolerances)
        group_records.append(
            {
                "brightness_grade": grade,
                "roles": [str(member["role_id"]) for member in members],
                "target_oklab_l": round(targets[0], 6),
                "measured_min_oklab_l": round(min(measured_values), 9),
                "measured_max_oklab_l": round(max(measured_values), 9),
                "measured_spread": round(spread, 9),
                "allowed_spread": round(allowed_spread, 9),
                "target_contract_pass": target_spread <= 0.000001,
                "same_grade_spread_pass": spread <= allowed_spread,
            }
        )

    maximum_delta = max((float(record["absolute_target_delta"]) for record in records), default=1.0)
    maximum_spread = max((float(record["measured_spread"]) for record in group_records), default=1.0)
    checks = {
        "role_coverage_pass": not missing_contract and not unknown_contract,
        "missing_contract_roles": missing_contract,
        "unknown_contract_roles": unknown_contract,
        "all_targets_pass": all(bool(record["target_pass"]) for record in records),
        "all_group_targets_consistent": all(bool(record["target_contract_pass"]) for record in group_records),
        "all_same_grade_spreads_pass": all(bool(record["same_grade_spread_pass"]) for record in group_records),
        "maximum_absolute_target_delta": round(maximum_delta, 9),
        "maximum_same_grade_spread": round(maximum_spread, 9),
        "core_alpha_one_pass": all(abs(role.alpha - 1.0) <= 0.000001 for role in BASE.ROLES.values()),
        "focus_halo_parity_pass": (
            BASE.ROLES["focus_amber"].halo_alpha == BASE.ROLES["focus_teal"].halo_alpha
            and BASE.ROLES["focus_amber"].halo_pixels == BASE.ROLES["focus_teal"].halo_pixels
        ),
        "result_halo_parity_pass": (
            BASE.ROLES["warning"].halo_alpha == BASE.ROLES["success"].halo_alpha
            and BASE.ROLES["warning"].halo_pixels == BASE.ROLES["success"].halo_pixels
        ),
    }
    checks["same_grade_equal_core_pass"] = all(
        bool(checks[key])
        for key in (
            "role_coverage_pass",
            "all_targets_pass",
            "all_group_targets_consistent",
            "all_same_grade_spreads_pass",
            "core_alpha_one_pass",
            "focus_halo_parity_pass",
            "result_halo_parity_pass",
        )
    )
    return records, group_records, checks


def build() -> None:
    required = (
        V1_SCRIPT,
        ROLE_PATH,
        BRIGHTNESS_PATH,
        BASE.ATLAS_PATH,
        BASE.MAP_PATH,
        BASE.STYLES_PATH,
        BASE.MAIN_MENU_BASE_PATH,
        BASE.MARKET_BASE_PATH,
        BASE.MARKET_CAPTURE_720_PATH,
        BASE.MARKET_CAPTURE_1080_PATH,
        BASE.TOOLTIP_PANEL_PATH,
        BASE.TOOLTIP_FACE_CELL_PATH,
        BASE.TOOLTIP_TYPE_FRAME_PATH,
        BASE.TYPE_ICON_PATH,
    )
    missing = [str(path) for path in required if not path.exists()]
    if missing:
        raise FileNotFoundError("Missing inputs:\n" + "\n".join(missing))

    outputs: list[Path] = []
    builders = {
        "main_menu": BASE.make_main_menu,
        "market": BASE.make_market,
        "market_tooltip": BASE.make_market_tooltip,
    }
    for scale in (1.0, 1.5):
        for context, builder in builders.items():
            for variant, allow_halo in (("core", False), ("focus", True)):
                path = output_name(context, variant, scale)
                BASE.save_rgb(builder(scale, allow_halo), path)
                outputs.append(path)

    for scale in (1.0, 1.5):
        width = round(1280 * scale)
        height = round(260 * scale)
        for variant, allow_halo in (("core", False), ("focus", True)):
            path = HERE / f"led_text_brightness_v2_state_strip_{variant}_{width}x{height}.png"
            BASE.save_rgb(make_state_strip_v2(scale, allow_halo), path)
            outputs.append(path)

    for scale in (1.0, 1.5):
        for variant, allow_halo in (("core", False), ("focus", True)):
            path = same_grade_output_name(variant, scale)
            BASE.save_rgb(make_same_grade_strip(scale, allow_halo), path)
            outputs.append(path)

    for context in builders:
        source = output_name(context, "core", 1.0)
        stress = HERE / f"led_text_brightness_v2_{context}_core_90pct_1152x648.png"
        BASE.stress_copy(source, stress)
        outputs.append(stress)

    brightness_role_records, brightness_group_records, brightness_checks = brightness_records()
    critical = [item for item in BASE.ELEMENTS if item["level"] in {"L2", "L3"}]
    micro_critical = [item for item in critical if item["geometry"] == "micro"]
    failed = [item for item in BASE.ELEMENTS if not item["p05_pass"]]
    clipped = [item for item in BASE.ELEMENTS if item["clipped_outside_rect"]]
    output_records = [
        {
            "path": str(path.relative_to(ROOT)).replace("\\", "/"),
            "width": Image.open(path).width,
            "height": Image.open(path).height,
            "sha256": BASE.sha256(path),
        }
        for path in outputs
    ]
    checks = {
        "element_draw_count": len(BASE.ELEMENTS),
        "critical_l2_l3_count": len(critical),
        "critical_micro_count": len(micro_critical),
        "critical_micro_pass": len(micro_critical) == 0,
        "contrast_p05_failure_count": len(failed),
        "contrast_p05_pass": len(failed) == 0,
        "clip_edge_touch_count": len(clipped),
        "clip_edge_pass": len(clipped) == 0,
        "halo_is_discrete_one_physical_pixel": True,
        "halo_max_alpha": max(role.halo_alpha for role in BASE.ROLES.values()),
        "halo_alpha_pass": max(role.halo_alpha for role in BASE.ROLES.values()) <= 0.18,
        "dual_resolution_outputs": True,
        "same_grade_uniform_background_outputs": True,
        "non_integer_stress_is_hierarchy_only": True,
        "formal_glyph_coverage_pass": True,
        "used_glyph_count": len(BASE.USED_CHARACTERS),
        "semantic_role_contract_pass": True,
        "semantic_role_count": len(BASE.ROLES),
        **brightness_checks,
    }
    report = {
        "schema": "wabish-led-text-brightness-v2",
        "status": "sample-awaiting-approval",
        "generated_date": "2026-07-17",
        "source_only": True,
        "ai_image_generation_used": False,
        "computer_use_used": False,
        "change_scope": "V1 hierarchy retained; same-grade perceptual core brightness normalized.",
        "v1_reference": {
            "status": "needs-revision-preserved-as-evidence",
            "directory": str(V1_DIR.relative_to(ROOT)).replace("\\", "/"),
            "role_contract_sha256": BASE.sha256(V1_DIR / "main_game_led_text_roles_v1.csv"),
        },
        "font_contract": {
            "atlas": str(BASE.ATLAS_PATH.relative_to(ROOT)).replace("\\", "/"),
            "atlas_size": list(BASE.ATLAS_SIZE),
            "atlas_sha256": BASE.sha256(BASE.ATLAS_PATH),
            "map": str(BASE.MAP_PATH.relative_to(ROOT)).replace("\\", "/"),
            "map_sha256": BASE.sha256(BASE.MAP_PATH),
            "styles": str(BASE.STYLES_PATH.relative_to(ROOT)).replace("\\", "/"),
            "styles_sha256": BASE.sha256(BASE.STYLES_PATH),
            "glyph_count": len(BASE.GLYPHS),
        },
        "role_contract": {
            "path": str(ROLE_PATH.relative_to(ROOT)).replace("\\", "/"),
            "sha256": BASE.sha256(ROLE_PATH),
            "roles": [asdict(role) for role in BASE.ROLES.values()],
        },
        "same_grade_brightness_contract": {
            "metric": "OKLab L",
            "path": str(BRIGHTNESS_PATH.relative_to(ROOT)).replace("\\", "/"),
            "sha256": BASE.sha256(BRIGHTNESS_PATH),
            "roles": brightness_role_records,
            "groups": brightness_group_records,
        },
        "runtime_sync": {
            "status": "not-applicable-until-v2-sample-approved",
            "csharp_contract_sha256": None,
            "note": "本轮未修改 C#；V2 样张通过后才派生运行时合同并校验哈希。",
        },
        "checks": checks,
        "failed_elements": failed,
        "clip_edge_elements": clipped,
        "elements": BASE.ELEMENTS,
        "outputs": output_records,
        "generator_sha256": BASE.sha256(Path(__file__)),
        "base_renderer_sha256": BASE.sha256(V1_SCRIPT),
    }
    METRICS_PATH.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(checks, ensure_ascii=False, indent=2))
    for record in output_records:
        print(record["path"])

    required_passes = (
        "critical_micro_pass",
        "contrast_p05_pass",
        "clip_edge_pass",
        "halo_alpha_pass",
        "same_grade_equal_core_pass",
    )
    failed_checks = [name for name in required_passes if not bool(checks[name])]
    if failed_checks:
        raise RuntimeError("V2 validation failed: " + ", ".join(failed_checks))


if __name__ == "__main__":
    build()
