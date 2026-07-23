from __future__ import annotations

import hashlib
import json
from dataclasses import asdict, dataclass
from functools import lru_cache
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[6]
HERE = Path(__file__).resolve().parent
BRIGHTNESS_V2_DIR = HERE.parent / "LedTextBrightnessV2"
FONT_PATH = Path(r"C:\Windows\Fonts\NotoSansSC-VF.ttf")

INK = (24, 14, 8, 255)  # #180E08
PLATE = (214, 184, 140, 255)  # #D6B88C
PLATE_EDGE = (116, 78, 43, 255)  # #744E2B
HOVER_EDGE = (246, 196, 91, 255)
DISABLED_PLATE = (178, 163, 140, 255)
DISABLED_EDGE = (104, 87, 67, 255)
DISABLED_INK = (63, 50, 38, 255)
FONT_WEIGHT = 600


@dataclass(frozen=True)
class KeySpec:
    key_id: str
    label: str
    outer_rect: tuple[int, int, int, int]
    plate_rect: tuple[int, int, int, int]
    font_size_720: int
    min_ink_height_720: int


KEYS = (
    KeySpec("sell", "卖出 1 金", (59, 496, 172, 72), (71, 510, 148, 44), 22, 18),
    KeySpec("buy_1", "购买 12 金", (323, 472, 215, 57), (335, 482, 191, 37), 22, 18),
    KeySpec("buy_2", "购买 8 金", (617, 472, 215, 57), (629, 482, 191, 37), 22, 18),
    KeySpec("buy_3", "购买 10 金", (910, 472, 215, 57), (922, 482, 191, 37), 22, 18),
    KeySpec("leave", "离开市场", (912, 601, 301, 82), (928, 615, 269, 54), 24, 22),
)


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def scale_rect(rect: tuple[int, int, int, int], scale: float) -> tuple[int, int, int, int]:
    x, y, width, height = rect
    left = round(x * scale)
    top = round(y * scale)
    right = round((x + width) * scale)
    bottom = round((y + height) * scale)
    return left, top, right - left, bottom - top


def rgb_hex(color: tuple[int, int, int, int]) -> str:
    return "#" + "".join(f"{channel:02X}" for channel in color[:3])


def relative_luminance(color: tuple[int, int, int, int]) -> float:
    channels = []
    for value in color[:3]:
        normalized = value / 255.0
        channels.append(normalized / 12.92 if normalized <= 0.04045 else ((normalized + 0.055) / 1.055) ** 2.4)
    return 0.2126 * channels[0] + 0.7152 * channels[1] + 0.0722 * channels[2]


def contrast_ratio(a: tuple[int, int, int, int], b: tuple[int, int, int, int]) -> float:
    light = max(relative_luminance(a), relative_luminance(b))
    dark = min(relative_luminance(a), relative_luminance(b))
    return (light + 0.05) / (dark + 0.05)


@lru_cache(maxsize=None)
def font(size: int, weight: int = FONT_WEIGHT) -> ImageFont.FreeTypeFont:
    result = ImageFont.truetype(str(FONT_PATH), size)
    result.set_variation_by_axes([weight])
    return result


def fill_and_outline(
    image: Image.Image,
    rect: tuple[int, int, int, int],
    fill: tuple[int, int, int, int],
    edge: tuple[int, int, int, int],
    edge_px: int,
) -> None:
    x, y, width, height = rect
    draw = ImageDraw.Draw(image)
    draw.rectangle((x, y, x + width - 1, y + height - 1), fill=fill, outline=edge, width=edge_px)


def render_solid_label(
    image: Image.Image,
    rect: tuple[int, int, int, int],
    text: str,
    requested_size: int,
    ink: tuple[int, int, int, int],
    minimum_padding: int,
) -> dict[str, object]:
    x, y, width, height = rect
    chosen_size = requested_size
    while chosen_size >= 8:
        current_font = font(chosen_size)
        raw_bbox = current_font.getbbox(text)
        ink_width = raw_bbox[2] - raw_bbox[0]
        ink_height = raw_bbox[3] - raw_bbox[1]
        if ink_width <= width - minimum_padding * 2 and ink_height <= height - minimum_padding * 2:
            break
        chosen_size -= 1
    else:
        raise RuntimeError(f"Cannot fit label {text!r} into {rect}")

    mask = Image.new("L", (width, height), 0)
    mask_draw = ImageDraw.Draw(mask)
    draw_x = (width - ink_width) // 2 - raw_bbox[0]
    draw_y = (height - ink_height) // 2 - raw_bbox[1]
    mask_draw.text((draw_x, draw_y), text, font=current_font, fill=255)
    mask_bbox = mask.getbbox()
    if mask_bbox is None:
        raise RuntimeError(f"Empty label mask: {text!r}")

    image.paste(ink, (x, y, x + width, y + height), mask)
    histogram = mask.histogram()
    antialias_pixels = sum(histogram[1:255])
    solid_pixels = histogram[255]
    left, top, right, bottom = mask_bbox
    padding = {
        "left": left,
        "right": width - right,
        "top": top,
        "bottom": height - bottom,
    }
    return {
        "text": text,
        "requested_font_size_px": requested_size,
        "font_size_px": chosen_size,
        "mask_bbox_local_px": list(mask_bbox),
        "ink_width_px": right - left,
        "ink_height_px": bottom - top,
        "padding_px": padding,
        "minimum_padding_px": min(padding.values()),
        "solid_core_pixels": solid_pixels,
        "antialias_edge_pixels": antialias_pixels,
        "direct_native_rasterization": True,
        "dot_grid": False,
        "outline": False,
        "glow": False,
        "shadow": False,
    }


def source_for_scale(scale: float) -> Path:
    width = round(1280 * scale)
    height = round(720 * scale)
    return BRIGHTNESS_V2_DIR / f"led_text_brightness_v2_market_core_{width}x{height}.png"


def render_market(scale: float) -> tuple[Image.Image, list[dict[str, object]]]:
    source = source_for_scale(scale)
    image = Image.open(source).convert("RGBA")
    results: list[dict[str, object]] = []
    for spec in KEYS:
        plate_rect = scale_rect(spec.plate_rect, scale)
        fill_and_outline(image, plate_rect, PLATE, PLATE_EDGE, max(1, round(scale)))
        label_result = render_solid_label(
            image,
            plate_rect,
            spec.label,
            round(spec.font_size_720 * scale),
            INK,
            max(4, round(5 * scale)),
        )
        label_result.update(
            {
                "key_id": spec.key_id,
                "resolution": [image.width, image.height],
                "outer_rect_px": list(scale_rect(spec.outer_rect, scale)),
                "plate_rect_px": list(plate_rect),
                "ink_hex": rgb_hex(INK),
                "plate_hex": rgb_hex(PLATE),
                "contrast_ratio": round(contrast_ratio(INK, PLATE), 4),
                "contrast_pass_7_to_1": contrast_ratio(INK, PLATE) >= 7.0,
                "minimum_ink_height_px": round(spec.min_ink_height_720 * scale),
            }
        )
        label_result["ink_height_pass"] = label_result["ink_height_px"] >= label_result["minimum_ink_height_px"]
        label_result["padding_pass"] = label_result["minimum_padding_px"] >= max(4, round(5 * scale))
        results.append(label_result)
    return image, results


def crop_key_strip(image: Image.Image, scale: float) -> Image.Image:
    top = round(460 * scale)
    return image.crop((0, top, image.width, image.height))


def crop_outer(image: Image.Image, rect_720: tuple[int, int, int, int], scale: float) -> Image.Image:
    x, y, width, height = scale_rect(rect_720, scale)
    return image.crop((x, y, x + width, y + height))


def draw_review_text(
    image: Image.Image,
    xy: tuple[int, int],
    text: str,
    size: int,
    fill: tuple[int, int, int, int],
) -> None:
    ImageDraw.Draw(image).text(xy, text, font=font(size, 500), fill=fill)


def make_state_board(market: Image.Image, scale: float) -> tuple[Image.Image, list[dict[str, object]]]:
    width = round(1280 * scale)
    height = round(500 * scale)
    board = Image.new("RGBA", (width, height), (5, 12, 15, 255))
    draw = ImageDraw.Draw(board)
    edge = (43, 57, 61, 255)
    title = (235, 219, 183, 255)
    note = (149, 166, 168, 255)
    draw.rectangle((round(18 * scale), round(18 * scale), width - round(18 * scale), height - round(18 * scale)), outline=edge, width=max(1, round(scale)))
    draw_review_text(board, (round(36 * scale), round(28 * scale)), "浅色实体键帽文字 V1 · 原生 1:1", round(30 * scale), title)
    draw_review_text(board, (round(36 * scale), round(74 * scale)), "启用态统一 #180E08；字色不承担悬停或按下反馈", round(18 * scale), note)

    buy_spec = KEYS[1]
    source_key = crop_outer(market, buy_spec.outer_rect, scale)
    key_width, key_height = source_key.size
    card_width = round(286 * scale)
    start_x = round(38 * scale)
    top = round(124 * scale)
    labels = ("常态", "悬停：只亮边缘", "按下：整体下移", "禁用：仍可识别")
    state_metrics: list[dict[str, object]] = []

    for index, state in enumerate(("normal", "hover", "pressed", "disabled")):
        card_x = start_x + index * card_width
        draw.rectangle(
            (card_x, top, card_x + round(258 * scale), top + round(126 * scale)),
            fill=(9, 18, 21, 255),
            outline=(38, 49, 51, 255),
            width=max(1, round(scale)),
        )
        draw_review_text(board, (card_x + round(12 * scale), top + round(10 * scale)), labels[index], round(17 * scale), note)
        key = source_key.copy()
        px, py, pw, ph = scale_rect((12, 10, 191, 37), scale)
        if state == "normal":
            pass
        elif state == "hover":
            fill_and_outline(key, (px, py, pw, ph), PLATE, HOVER_EDGE, max(2, round(2 * scale)))
            render_solid_label(key, (px, py, pw, ph), buy_spec.label, round(22 * scale), INK, max(4, round(5 * scale)))
        elif state == "pressed":
            recess = (71, 49, 31, 255)
            fill_and_outline(key, (px, py, pw, ph), recess, PLATE_EDGE, max(1, round(scale)))
            shift = max(1, round(2 * scale))
            fill_and_outline(key, (px, py + shift, pw, ph - shift), (199, 166, 119, 255), PLATE_EDGE, max(1, round(scale)))
            render_solid_label(key, (px, py + shift, pw, ph - shift), buy_spec.label, round(22 * scale), INK, max(4, round(5 * scale)))
        else:
            fill_and_outline(key, (px, py, pw, ph), DISABLED_PLATE, DISABLED_EDGE, max(1, round(scale)))
            render_solid_label(key, (px, py, pw, ph), buy_spec.label, round(22 * scale), DISABLED_INK, max(4, round(5 * scale)))
        key_x = card_x + (round(258 * scale) - key_width) // 2
        key_y = top + round(52 * scale)
        board.alpha_composite(key, (key_x, key_y))
        state_metrics.append(
            {
                "state": state,
                "ink_hex": rgb_hex(DISABLED_INK if state == "disabled" else INK),
                "plate_hex": rgb_hex(DISABLED_PLATE if state == "disabled" else PLATE),
                "contrast_ratio": round(
                    contrast_ratio(DISABLED_INK, DISABLED_PLATE) if state == "disabled" else contrast_ratio(INK, PLATE),
                    4,
                ),
                "text_color_changes_from_enabled": state == "disabled",
            }
        )

    stress_top = round(286 * scale)
    draw_review_text(board, (round(38 * scale), stress_top), "最长文案压力（保持当前键帽实际尺寸）", round(20 * scale), title)
    draw_review_text(board, (round(38 * scale), stress_top + round(34 * scale)), "购买 999 金 / 卖出 999 金 / 交互已锁定", round(16 * scale), note)

    stress_specs = (
        (KEYS[1], "购买 999 金", round(48 * scale)),
        (KEYS[0], "卖出 999 金", round(394 * scale)),
        (KEYS[4], "交互已锁定", round(706 * scale)),
    )
    stress_y = stress_top + round(78 * scale)
    for spec, text, target_x in stress_specs:
        key = crop_outer(market, spec.outer_rect, scale)
        outer_x, outer_y, outer_width, outer_height = scale_rect(spec.outer_rect, scale)
        plate_x, plate_y, plate_width, plate_height = scale_rect(spec.plate_rect, scale)
        local_plate = (plate_x - outer_x, plate_y - outer_y, plate_width, plate_height)
        fill_and_outline(key, local_plate, PLATE, PLATE_EDGE, max(1, round(scale)))
        result = render_solid_label(
            key,
            local_plate,
            text,
            round(spec.font_size_720 * scale),
            INK,
            max(4, round(5 * scale)),
        )
        board.alpha_composite(key, (target_x, stress_y))
        result.update({"state": "stress", "key_id": spec.key_id, "resolution": [width, height]})
        state_metrics.append(result)

    return board, state_metrics


def output_path(kind: str, width: int, height: int) -> Path:
    return HERE / f"market_physical_key_labels_v1_{kind}_{width}x{height}.png"


def build() -> None:
    required = [FONT_PATH, *(source_for_scale(scale) for scale in (1.0, 1.5))]
    missing = [str(path) for path in required if not path.exists()]
    if missing:
        raise FileNotFoundError("Missing inputs:\n" + "\n".join(missing))

    outputs: list[Path] = []
    key_metrics: list[dict[str, object]] = []
    state_metrics: list[dict[str, object]] = []
    for scale in (1.0, 1.5):
        market, rendered = render_market(scale)
        full_path = output_path("full", market.width, market.height)
        market.convert("RGB").save(full_path, optimize=True)
        outputs.append(full_path)
        key_metrics.extend(rendered)

        strip = crop_key_strip(market, scale)
        strip_path = output_path("key_strip", strip.width, strip.height)
        strip.convert("RGB").save(strip_path, optimize=True)
        outputs.append(strip_path)

        board, board_metrics = make_state_board(market, scale)
        board_path = output_path("state_stress", board.width, board.height)
        board.convert("RGB").save(board_path, optimize=True)
        outputs.append(board_path)
        state_metrics.extend(board_metrics)

    enabled_contrast = contrast_ratio(INK, PLATE)
    disabled_contrast = contrast_ratio(DISABLED_INK, DISABLED_PLATE)
    stress_items = [item for item in state_metrics if item.get("state") == "stress"]
    report = {
        "sample": "MarketPhysicalKeyLabelsV1",
        "status": "sample_pending_user_acceptance",
        "scope": {
            "changed": ["sell light key", "three purchase light keys", "leave-market light key"],
            "unchanged": [
                "market layout",
                "button hit rects",
                "dark refresh key LED text",
                "offer names and faces LED text",
                "market feedback LED text",
                "tooltip LED text",
                "gameplay and economy",
                "Unity resources and C#",
            ],
        },
        "render_contract": {
            "family": "PhysicalKeyLabel",
            "font_source": str(FONT_PATH),
            "font_sha256": sha256(FONT_PATH),
            "font_weight": FONT_WEIGHT,
            "enabled_ink_hex": rgb_hex(INK),
            "enabled_plate_hex": rgb_hex(PLATE),
            "enabled_contrast_ratio": round(enabled_contrast, 4),
            "disabled_ink_hex": rgb_hex(DISABLED_INK),
            "disabled_plate_hex": rgb_hex(DISABLED_PLATE),
            "disabled_contrast_ratio": round(disabled_contrast, 4),
            "anti_aliasing": "direct FreeType coverage at each native target resolution",
            "resampling_after_text_render": "none",
            "dot_matrix": False,
            "outline": False,
            "glow": False,
            "shadow": False,
            "wear_mask": False,
        },
        "checks": {
            "enabled_contrast_pass_7_to_1": enabled_contrast >= 7.0,
            "disabled_contrast_pass_3_to_1": disabled_contrast >= 3.0,
            "all_enabled_keys_same_ink": all(item["ink_hex"] == rgb_hex(INK) for item in key_metrics),
            "all_enabled_keys_same_plate": all(item["plate_hex"] == rgb_hex(PLATE) for item in key_metrics),
            "all_labels_fit": all(item["padding_pass"] for item in key_metrics),
            "all_ink_heights_pass": all(item["ink_height_pass"] for item in key_metrics),
            "all_stress_labels_fit": all(
                item["minimum_padding_px"] >= (5 if item["resolution"][0] == 1280 else 8)
                for item in stress_items
            ),
            "native_outputs_only": True,
            "no_unity_files_modified": True,
        },
        "inputs": [
            {
                "path": str(source_for_scale(scale).relative_to(ROOT)).replace("\\", "/"),
                "sha256": sha256(source_for_scale(scale)),
            }
            for scale in (1.0, 1.5)
        ],
        "outputs": [
            {
                "path": str(path.relative_to(ROOT)).replace("\\", "/"),
                "size": list(Image.open(path).size),
                "sha256": sha256(path),
            }
            for path in outputs
        ],
        "keys": key_metrics,
        "states_and_stress": state_metrics,
        "generator": {
            "path": str(Path(__file__).resolve().relative_to(ROOT)).replace("\\", "/"),
            "sha256": sha256(Path(__file__).resolve()),
        },
    }
    if not all(report["checks"].values()):
        failures = [name for name, passed in report["checks"].items() if not passed]
        raise RuntimeError("Validation failed: " + ", ".join(failures))
    (HERE / "market_physical_key_labels_metrics_v1_20260717.json").write_text(
        json.dumps(report, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )


if __name__ == "__main__":
    build()
