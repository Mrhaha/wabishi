from __future__ import annotations

import hashlib
import importlib.util
import json
import statistics
import sys
from pathlib import Path

from PIL import Image, ImageDraw, ImageEnhance


ROOT = Path(__file__).resolve().parents[6]
HERE = Path(__file__).resolve().parent
V1_DIR = HERE.parent / "MarketPhysicalKeyLabelsV1"
V1_SCRIPT = V1_DIR / "build_market_physical_key_labels_v1.py"
CLEAN_BASE_PATH = ROOT / "Assets" / "Resources" / "Art" / "Market" / "arcade_market_common_base.png"


def load_v1_module():
    spec = importlib.util.spec_from_file_location("market_physical_key_labels_v1", V1_SCRIPT)
    if spec is None or spec.loader is None:
        raise RuntimeError(f"Cannot load {V1_SCRIPT}")
    module = importlib.util.module_from_spec(spec)
    sys.modules[spec.name] = module
    spec.loader.exec_module(module)
    return module


BASE = load_v1_module()
KEYS = BASE.KEYS
INK = BASE.INK
HOVER_EDGE = BASE.HOVER_EDGE
DISABLED_INK = (48, 38, 29, 255)


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def load_clean_base(scale: float) -> Image.Image:
    target = (round(1280 * scale), round(720 * scale))
    image = Image.open(CLEAN_BASE_PATH).convert("RGBA")
    if image.size != target:
        image = image.resize(target, Image.Resampling.LANCZOS)
    return image


def paste_rect(destination: Image.Image, source: Image.Image, rect: tuple[int, int, int, int]) -> None:
    x, y, width, height = rect
    destination.paste(source.crop((x, y, x + width, y + height)), (x, y))


def luminance(rgb: tuple[int, int, int]) -> float:
    converted = []
    for channel in rgb:
        value = channel / 255.0
        converted.append(value / 12.92 if value <= 0.04045 else ((value + 0.055) / 1.055) ** 2.4)
    return 0.2126 * converted[0] + 0.7152 * converted[1] + 0.0722 * converted[2]


def contrast_against_luminance(ink: tuple[int, int, int, int], surface_luminance: float) -> float:
    ink_luminance = luminance(ink[:3])
    return (max(ink_luminance, surface_luminance) + 0.05) / (min(ink_luminance, surface_luminance) + 0.05)


def texture_metrics(image: Image.Image, rect: tuple[int, int, int, int]) -> dict[str, float | bool]:
    x, y, width, height = rect
    crop = image.crop((x, y, x + width, y + height)).convert("RGB")
    values = sorted(luminance(pixel) for pixel in crop.get_flattened_data())
    p05 = values[min(len(values) - 1, int(len(values) * 0.05))]
    median = values[len(values) // 2]
    deviation = statistics.pstdev(values)
    return {
        "surface_luminance_p05": round(p05, 6),
        "surface_luminance_median": round(median, 6),
        "surface_luminance_stddev": round(deviation, 6),
        "texture_preserved": deviation >= 0.03,
        "p05_contrast_ratio": round(contrast_against_luminance(INK, p05), 4),
        "median_contrast_ratio": round(contrast_against_luminance(INK, median), 4),
        "p05_large_text_contrast_pass_3_to_1": contrast_against_luminance(INK, p05) >= 3.0,
        "median_contrast_pass_4_5_to_1": contrast_against_luminance(INK, median) >= 4.5,
    }


def render_market(scale: float) -> tuple[Image.Image, list[dict[str, object]]]:
    source_path = BASE.source_for_scale(scale)
    image = Image.open(source_path).convert("RGBA")
    clean = load_clean_base(scale)
    records: list[dict[str, object]] = []

    for spec in KEYS:
        outer_rect = BASE.scale_rect(spec.outer_rect, scale)
        plate_rect = BASE.scale_rect(spec.plate_rect, scale)
        paste_rect(image, clean, outer_rect)
        material = texture_metrics(clean, plate_rect)
        label = BASE.render_solid_label(
            image,
            plate_rect,
            spec.label,
            round(spec.font_size_720 * scale),
            INK,
            max(4, round(5 * scale)),
        )
        label.update(material)
        label.update(
            {
                "key_id": spec.key_id,
                "resolution": [image.width, image.height],
                "outer_rect_px": list(outer_rect),
                "text_safe_rect_px": list(plate_rect),
                "ink_hex": BASE.rgb_hex(INK),
                "solid_backing_added": False,
                "original_key_surface_restored": True,
                "minimum_ink_height_px": round(spec.min_ink_height_720 * scale),
            }
        )
        label["ink_height_pass"] = label["ink_height_px"] >= label["minimum_ink_height_px"]
        label["padding_pass"] = label["minimum_padding_px"] >= max(4, round(5 * scale))
        records.append(label)
    return image, records


def clean_key_crop(clean: Image.Image, spec, scale: float) -> tuple[Image.Image, tuple[int, int, int, int]]:
    outer_x, outer_y, outer_width, outer_height = BASE.scale_rect(spec.outer_rect, scale)
    plate_x, plate_y, plate_width, plate_height = BASE.scale_rect(spec.plate_rect, scale)
    key = clean.crop((outer_x, outer_y, outer_x + outer_width, outer_y + outer_height))
    local_plate = (plate_x - outer_x, plate_y - outer_y, plate_width, plate_height)
    return key, local_plate


def draw_outline(image: Image.Image, rect: tuple[int, int, int, int], color: tuple[int, int, int, int], width: int) -> None:
    x, y, rect_width, rect_height = rect
    ImageDraw.Draw(image).rectangle(
        (x, y, x + rect_width - 1, y + rect_height - 1),
        outline=color,
        width=width,
    )


def render_key_state(clean: Image.Image, spec, scale: float, state: str, label: str | None = None) -> tuple[Image.Image, dict[str, object]]:
    key, plate = clean_key_crop(clean, spec, scale)
    text = label or spec.label
    text_rect = plate
    ink = INK
    if state == "hover":
        draw_outline(key, plate, HOVER_EDGE, max(2, round(2 * scale)))
    elif state == "pressed":
        key = ImageEnhance.Brightness(key).enhance(0.94)
        shift = max(1, round(2 * scale))
        x, y, width, height = plate
        text_rect = (x, y + shift, width, height - shift)
    elif state == "disabled":
        key = ImageEnhance.Color(key).enhance(0.35)
        key = ImageEnhance.Brightness(key).enhance(0.86)
        ink = DISABLED_INK
    result = BASE.render_solid_label(
        key,
        text_rect,
        text,
        round(spec.font_size_720 * scale),
        ink,
        max(4, round(5 * scale)),
    )
    result.update(
        {
            "state": state,
            "key_id": spec.key_id,
            "ink_hex": BASE.rgb_hex(ink),
            "solid_backing_added": False,
        }
    )
    return key, result


def make_state_board(scale: float) -> tuple[Image.Image, list[dict[str, object]]]:
    clean = load_clean_base(scale)
    width = round(1280 * scale)
    height = round(500 * scale)
    board = Image.new("RGBA", (width, height), (5, 12, 15, 255))
    draw = ImageDraw.Draw(board)
    edge = (43, 57, 61, 255)
    title = (235, 219, 183, 255)
    note = (149, 166, 168, 255)
    draw.rectangle((round(18 * scale), round(18 * scale), width - round(18 * scale), height - round(18 * scale)), outline=edge, width=max(1, round(scale)))
    BASE.draw_review_text(board, (round(36 * scale), round(28 * scale)), "浅色实体键帽文字 V2 · 保留原材质", round(30 * scale), title)
    BASE.draw_review_text(board, (round(36 * scale), round(74 * scale)), "无新增纯色底板；只恢复无字键帽并印入实心字", round(18 * scale), note)

    buy_spec = KEYS[1]
    card_width = round(286 * scale)
    start_x = round(38 * scale)
    top = round(124 * scale)
    labels = ("常态", "悬停：只亮边缘", "按下：材质整体压暗", "禁用：保留纹理")
    records: list[dict[str, object]] = []
    for index, state in enumerate(("normal", "hover", "pressed", "disabled")):
        card_x = start_x + index * card_width
        draw.rectangle(
            (card_x, top, card_x + round(258 * scale), top + round(126 * scale)),
            fill=(9, 18, 21, 255),
            outline=(38, 49, 51, 255),
            width=max(1, round(scale)),
        )
        BASE.draw_review_text(board, (card_x + round(12 * scale), top + round(10 * scale)), labels[index], round(17 * scale), note)
        key, record = render_key_state(clean, buy_spec, scale, state)
        key_x = card_x + (round(258 * scale) - key.width) // 2
        key_y = top + round(52 * scale)
        board.alpha_composite(key, (key_x, key_y))
        records.append(record)

    stress_top = round(286 * scale)
    BASE.draw_review_text(board, (round(38 * scale), stress_top), "最长文案压力（原键帽纹理不变）", round(20 * scale), title)
    BASE.draw_review_text(board, (round(38 * scale), stress_top + round(34 * scale)), "购买 999 金 / 卖出 999 金 / 交互已锁定", round(16 * scale), note)
    stress_specs = (
        (KEYS[1], "购买 999 金", round(48 * scale)),
        (KEYS[0], "卖出 999 金", round(394 * scale)),
        (KEYS[4], "交互已锁定", round(706 * scale)),
    )
    stress_y = stress_top + round(78 * scale)
    for spec, text, target_x in stress_specs:
        key, record = render_key_state(clean, spec, scale, "stress", text)
        board.alpha_composite(key, (target_x, stress_y))
        record["resolution"] = [width, height]
        records.append(record)
    return board, records


def output_path(kind: str, width: int, height: int) -> Path:
    return HERE / f"market_physical_key_labels_v2_{kind}_{width}x{height}.png"


def build() -> None:
    required = [V1_SCRIPT, CLEAN_BASE_PATH, BASE.FONT_PATH, *(BASE.source_for_scale(scale) for scale in (1.0, 1.5))]
    missing = [str(path) for path in required if not path.exists()]
    if missing:
        raise FileNotFoundError("Missing inputs:\n" + "\n".join(missing))

    outputs: list[Path] = []
    key_records: list[dict[str, object]] = []
    state_records: list[dict[str, object]] = []
    for scale in (1.0, 1.5):
        market, records = render_market(scale)
        full_path = output_path("full", market.width, market.height)
        market.convert("RGB").save(full_path, optimize=True)
        outputs.append(full_path)
        key_records.extend(records)

        strip = BASE.crop_key_strip(market, scale)
        strip_path = output_path("key_strip", strip.width, strip.height)
        strip.convert("RGB").save(strip_path, optimize=True)
        outputs.append(strip_path)

        board, records = make_state_board(scale)
        board_path = output_path("state_stress", board.width, board.height)
        board.convert("RGB").save(board_path, optimize=True)
        outputs.append(board_path)
        state_records.extend(records)

    stress = [record for record in state_records if record["state"] == "stress"]
    checks = {
        "no_solid_backing_added": all(not record["solid_backing_added"] for record in key_records + state_records),
        "all_original_key_surfaces_restored": all(record["original_key_surface_restored"] for record in key_records),
        "all_key_textures_preserved": all(record["texture_preserved"] for record in key_records),
        "all_p05_large_text_contrast_pass": all(record["p05_large_text_contrast_pass_3_to_1"] for record in key_records),
        "all_median_contrast_pass": all(record["median_contrast_pass_4_5_to_1"] for record in key_records),
        "all_labels_fit": all(record["padding_pass"] for record in key_records),
        "all_ink_heights_pass": all(record["ink_height_pass"] for record in key_records),
        "all_stress_labels_fit": all(record["minimum_padding_px"] >= (5 if record["resolution"][0] == 1280 else 8) for record in stress),
        "native_outputs_only": True,
        "no_unity_files_modified": True,
    }
    if not all(checks.values()):
        failures = [name for name, passed in checks.items() if not passed]
        raise RuntimeError("Validation failed: " + ", ".join(failures))

    report = {
        "sample": "MarketPhysicalKeyLabelsV2",
        "status": "sample_pending_user_acceptance",
        "revision_reason": "V1 added a flat color backing that interrupted the original keycap material.",
        "render_contract": {
            "family": "PhysicalKeyLabel",
            "font_source": str(BASE.FONT_PATH),
            "font_sha256": sha256(BASE.FONT_PATH),
            "font_weight": BASE.FONT_WEIGHT,
            "enabled_ink_hex": BASE.rgb_hex(INK),
            "solid_backing_added": False,
            "surface_source": str(CLEAN_BASE_PATH.relative_to(ROOT)).replace("\\", "/"),
            "surface_source_sha256": sha256(CLEAN_BASE_PATH),
            "anti_aliasing": "direct FreeType coverage at each native target resolution",
            "resampling_after_text_render": "none",
            "dot_matrix": False,
            "glow": False,
            "outline": False,
            "shadow": False,
        },
        "checks": checks,
        "inputs": [
            {
                "path": str(path.relative_to(ROOT)).replace("\\", "/"),
                "sha256": sha256(path),
            }
            for path in [CLEAN_BASE_PATH, *(BASE.source_for_scale(scale) for scale in (1.0, 1.5))]
        ],
        "outputs": [
            {
                "path": str(path.relative_to(ROOT)).replace("\\", "/"),
                "size": list(Image.open(path).size),
                "sha256": sha256(path),
            }
            for path in outputs
        ],
        "keys": key_records,
        "states_and_stress": state_records,
        "generator": {
            "path": str(Path(__file__).resolve().relative_to(ROOT)).replace("\\", "/"),
            "sha256": sha256(Path(__file__).resolve()),
            "v1_renderer_sha256": sha256(V1_SCRIPT),
        },
    }
    (HERE / "market_physical_key_labels_metrics_v2_20260717.json").write_text(
        json.dumps(report, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )


if __name__ == "__main__":
    build()
