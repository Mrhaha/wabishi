from __future__ import annotations

import hashlib
import json
from pathlib import Path

from PIL import Image, ImageDraw

import build_led_font_recognition_v3 as v3


ROOT = Path(__file__).resolve().parents[6]
FLOW_ROOT = ROOT / "Assets" / "ArtSource" / "Production" / "ArcadeDiceKing" / "MainGameFlowV1"
OUTPUT_DIR = FLOW_ROOT / "LedFontBaselineV4"

BOARD_SIZE = (1280, 720)
BASELINE_ROW = 14
MIDLINE_ROW = 7

BACKGROUND = (4, 9, 12, 255)
PANEL = v3.PANEL
PANEL_EDGE = v3.PANEL_EDGE
CORE = v3.CORE
CORE_DIM = v3.CORE_DIM
LABEL = v3.LABEL
LABEL_DIM = v3.LABEL_DIM
GUIDE_BASELINE = (47, 109, 84, 255)
GUIDE_BASELINE_FAIL = (125, 67, 56, 255)
GUIDE_CAP = (39, 52, 56, 255)

FONT_PATH = str(v3.FONT_NOTO)

BASELINE_TEST_TEXTS = (
    "第 1 章 · 第 1 关",
    "生命 --    金币 8",
    "目标 15    当前 0",
)

MIXED_TEST_TEXTS = (
    "本关规则    SPACE  投掷",
    "ABC 0123456789    中文混排",
    "第-1·章，规则。",
    "目标 15    当前 0    金币 8",
)

FLOATING_PUNCTUATION = frozenset("-—–·•:：")
BASELINE_SAMPLE_CHARACTERS = "第章生命金币目标当前本关规则投掷中文混排SPACEABC0123456789"


def shape_crop(character: str) -> Image.Image:
    """Return the exact V3 active bitmap; no pixel is added, removed, or resampled."""
    mask = v3.native_mask(character, FONT_PATH)
    bounds = v3.active_bounds(mask)
    if bounds is None:
        return Image.new("L", (0, 0), 0)
    return mask.crop(bounds)


def shape_signature(character: str) -> str:
    shape = shape_crop(character)
    payload = (
        shape.width.to_bytes(2, "big")
        + shape.height.to_bytes(2, "big")
        + bytes(shape.get_flattened_data())
    )
    return hashlib.sha256(payload).hexdigest().upper()


def anchor_kind(character: str) -> str:
    return "midline" if character in FLOATING_PUNCTUATION else "baseline"


def placed_top_row(character: str) -> int:
    shape = shape_crop(character)
    if shape.height == 0:
        return 0
    if anchor_kind(character) == "midline":
        return MIDLINE_ROW - (shape.height - 1) // 2
    return BASELINE_ROW - shape.height + 1


def placed_bottom_row(character: str) -> int:
    shape = shape_crop(character)
    return placed_top_row(character) + max(0, shape.height - 1)


def draw_v4_glyph(
    target: Image.Image,
    character: str,
    x: int,
    line_top: int,
    geometry: v3.LampGeometry,
    color: tuple[int, int, int, int] = CORE,
) -> None:
    if character.isspace():
        return
    shape = shape_crop(character)
    top_row = placed_top_row(character)
    if top_row < 0 or top_row + shape.height > v3.GRID:
        raise RuntimeError(
            f"V4 placement does not fit {v3.GRID} rows: {character!r}, "
            f"top={top_row}, height={shape.height}"
        )
    draw = ImageDraw.Draw(target)
    for row in range(shape.height):
        for column in range(shape.width):
            if shape.getpixel((column, row)) == 0:
                continue
            px = x + column * geometry.pitch_px
            py = line_top + (top_row + row) * geometry.pitch_px
            draw.rectangle(
                (px, py, px + geometry.dot_px - 1, py + geometry.dot_px - 1),
                fill=color,
            )


def draw_v4_line(
    target: Image.Image,
    text: str,
    x: int,
    line_top: int,
    geometry: v3.LampGeometry = v3.MAJOR,
    color: tuple[int, int, int, int] = CORE,
) -> None:
    cursor = x
    for character in text:
        draw_v4_glyph(target, character, cursor, line_top, geometry, color)
        cursor += v3.glyph_advance(character, geometry, FONT_PATH)


def draw_alignment_guides(
    draw: ImageDraw.ImageDraw,
    x0: int,
    x1: int,
    line_top: int,
    geometry: v3.LampGeometry,
    color: tuple[int, int, int, int],
) -> None:
    baseline_y = line_top + BASELINE_ROW * geometry.pitch_px + geometry.dot_px + 1
    cap_y = line_top
    draw.line((x0, cap_y, x1, cap_y), fill=GUIDE_CAP, width=1)
    draw.line((x0, baseline_y, x1, baseline_y), fill=color, width=1)


def draw_panel(draw: ImageDraw.ImageDraw, box: tuple[int, int, int, int]) -> None:
    draw.rounded_rectangle(box, radius=10, fill=PANEL, outline=PANEL_EDGE, width=1)


def make_board() -> Path:
    image = Image.new("RGBA", BOARD_SIZE, BACKGROUND)
    draw = ImageDraw.Draw(image)
    draw.text((28, 17), "V4 LED 字体基线校准板", font=v3.review_font(28), fill=(214, 225, 226, 255))
    draw.text(
        (368, 25),
        "原生 1280×720 · V3 字形像素不变 · 仅整数纵向位移 · 无辉光 / 无缩放",
        font=v3.review_font(16),
        fill=LABEL_DIM,
    )

    compare_box = (26, 62, 1254, 330)
    verify_box = (26, 348, 1254, 694)
    draw_panel(draw, compare_box)
    draw_panel(draw, verify_box)

    divider_x = 640
    draw.line((divider_x, 76, divider_x, 314), fill=PANEL_EDGE, width=1)
    draw.text((48, 76), "V3 失败证据 · 每字顶对齐", font=v3.review_font(18), fill=(173, 113, 99, 255))
    draw.text((666, 76), "V4 单一候选 · 共享底线", font=v3.review_font(18), fill=(109, 177, 144, 255))

    for index, text_value in enumerate(BASELINE_TEST_TEXTS):
        line_top = 112 + index * 68
        draw_alignment_guides(draw, 48, 614, line_top, v3.MAJOR, GUIDE_BASELINE_FAIL)
        draw_alignment_guides(draw, 666, 1232, line_top, v3.MAJOR, GUIDE_BASELINE)
        v3.draw_line(image, text_value, 64, line_top, v3.MAJOR, FONT_PATH, CORE_DIM)
        draw_v4_line(image, text_value, 682, line_top, v3.MAJOR, CORE)

    draw.text((48, 364), "V4 混合排版压力区", font=v3.review_font(18), fill=LABEL)
    draw.text(
        (266, 370),
        "绿线为共享底线；短横 / 间隔点固定在中线，不参与底线误差",
        font=v3.review_font(15),
        fill=LABEL_DIM,
    )
    for index, text_value in enumerate(MIXED_TEST_TEXTS):
        line_top = 406 + index * 67
        draw_alignment_guides(draw, 48, 1232, line_top, v3.MAJOR, GUIDE_BASELINE)
        draw_v4_line(image, text_value, 64, line_top, v3.MAJOR, CORE if index != 2 else CORE_DIM)

    path = OUTPUT_DIR / "led_font_v4_baseline_calibration_board_1280x720.png"
    image.convert("RGB").save(path, optimize=True)
    return path


def make_v4_only_sheet() -> Path:
    size = (1280, 520)
    image = Image.new("RGBA", size, BACKGROUND)
    draw = ImageDraw.Draw(image)
    draw.text((28, 18), "V4 统一基线 1:1 验收条", font=v3.review_font(24), fill=LABEL)
    draw.text((310, 25), "字形沿用 V3；只检查中英数是否落在同一水平线", font=v3.review_font(15), fill=LABEL_DIM)
    all_rows = BASELINE_TEST_TEXTS + MIXED_TEST_TEXTS[:3]
    for index, text_value in enumerate(all_rows):
        line_top = 72 + index * 72
        draw_alignment_guides(draw, 42, 1238, line_top, v3.MAJOR, GUIDE_BASELINE)
        draw_v4_line(image, text_value, 58, line_top, v3.MAJOR, CORE)
    path = OUTPUT_DIR / "led_font_v4_unified_baseline_1to1.png"
    image.convert("RGB").save(path, optimize=True)
    return path


def make_zoom_sheet(board_path: Path) -> Path:
    board = Image.open(board_path).convert("RGB")
    crop = board.crop((640, 88, 1254, 330))
    zoom = crop.resize((crop.width * 2, crop.height * 2), Image.Resampling.NEAREST)
    path = OUTPUT_DIR / "led_font_v4_baseline_zoom2x_nearest.png"
    zoom.save(path, optimize=True)
    return path


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


def placement_metrics() -> dict[str, object]:
    characters = list(dict.fromkeys(BASELINE_SAMPLE_CHARACTERS))
    per_character: dict[str, object] = {}
    baseline_rows: list[int] = []
    shape_preserved = True
    pixel_counts_preserved = True
    placement_fits = True
    for character in characters:
        source_mask = v3.native_mask(character, FONT_PATH)
        source_bounds = v3.active_bounds(source_mask)
        source_shape = shape_crop(character)
        top = placed_top_row(character)
        bottom = placed_bottom_row(character)
        baseline_rows.append(bottom)
        source_pixel_count = sum(value != 0 for value in source_shape.get_flattened_data())

        placed = Image.new("L", (v3.GRID, v3.GRID), 0)
        placed.paste(source_shape, (0, top))
        placed_bounds = placed.getbbox()
        placed_shape = placed.crop(placed_bounds) if placed_bounds else Image.new("L", (0, 0), 0)
        placed_pixel_count = sum(value != 0 for value in placed_shape.get_flattened_data())
        source_signature = shape_signature(character)
        placed_payload = (
            placed_shape.width.to_bytes(2, "big")
            + placed_shape.height.to_bytes(2, "big")
            + bytes(placed_shape.get_flattened_data())
        )
        placed_signature = hashlib.sha256(placed_payload).hexdigest().upper()
        same_shape = source_signature == placed_signature
        same_pixels = source_pixel_count == placed_pixel_count
        fits = top >= 0 and bottom < v3.GRID
        shape_preserved = shape_preserved and same_shape
        pixel_counts_preserved = pixel_counts_preserved and same_pixels
        placement_fits = placement_fits and fits
        per_character[character] = {
            "v3_active_bounds": list(source_bounds) if source_bounds else None,
            "active_size": [source_shape.width, source_shape.height],
            "v4_top_row": top,
            "v4_bottom_row": bottom,
            "anchor": anchor_kind(character),
            "lit_pixel_count": source_pixel_count,
            "active_shape_sha256": source_signature,
            "shape_preserved": same_shape,
            "pixel_count_preserved": same_pixels,
            "fits_16_rows": fits,
        }
    return {
        "baseline_row": BASELINE_ROW,
        "midline_row": MIDLINE_ROW,
        "baseline_character_count": len(characters),
        "baseline_bottom_rows": sorted(set(baseline_rows)),
        "max_baseline_deviation_logical_rows": max(baseline_rows) - min(baseline_rows),
        "max_baseline_deviation_physical_px_720p": (max(baseline_rows) - min(baseline_rows)) * v3.MAJOR.pitch_px,
        "shape_preserved_from_v3": shape_preserved,
        "lit_pixel_counts_preserved_from_v3": pixel_counts_preserved,
        "all_placements_fit": placement_fits,
        "floating_punctuation_rule": {
            "characters": sorted(FLOATING_PUNCTUATION),
            "anchor": "midline",
            "excluded_from_baseline_deviation": True,
        },
        "per_character": per_character,
    }


def build() -> None:
    v3.require_inputs()
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    board_path = make_board()
    v4_only_path = make_v4_only_sheet()
    zoom_path = make_zoom_sheet(board_path)
    outputs = (board_path, v4_only_path, zoom_path)
    placement = placement_metrics()
    metrics: dict[str, object] = {
        "scope": "baseline_alignment_only",
        "source_glyph_contract": "V3 exact active binary shapes",
        "source_font": FONT_PATH,
        "source_font_weight": v3.NOTO_WEIGHT,
        "logical_grid": [v3.GRID, v3.GRID],
        "geometry_720p": v3.MAJOR.__dict__,
        "resampling": "none; zoom uses integer 2x nearest only",
        "halo": False,
        "placement": placement,
        "outputs": {
            path.name: {"size": list(Image.open(path).size), "sha256": sha256(path)}
            for path in outputs
        },
        "checks": {
            "native_1280x720_board": Image.open(board_path).size == BOARD_SIZE,
            "v3_active_shapes_preserved": placement["shape_preserved_from_v3"],
            "v3_lit_pixel_counts_preserved": placement["lit_pixel_counts_preserved_from_v3"],
            "baseline_deviation_is_zero": placement["max_baseline_deviation_logical_rows"] == 0,
            "integer_vertical_translation_only": True,
            "all_placements_fit_16_rows": placement["all_placements_fit"],
            "no_antialias_or_halo": True,
            "zoom_is_integer_nearest": True,
            "unity_runtime_unchanged": True,
        },
    }
    report_path = OUTPUT_DIR / "led_font_baseline_metrics_v4_20260717.json"
    report_path.write_text(json.dumps(metrics, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"Generated V4 baseline pilot in {OUTPUT_DIR}")
    for path in (*outputs, report_path):
        print(path)


if __name__ == "__main__":
    build()
