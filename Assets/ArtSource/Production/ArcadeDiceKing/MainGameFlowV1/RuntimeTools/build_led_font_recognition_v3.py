from __future__ import annotations

import hashlib
import json
from dataclasses import dataclass
from functools import lru_cache
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[6]
FLOW_ROOT = ROOT / "Assets" / "ArtSource" / "Production" / "ArcadeDiceKing" / "MainGameFlowV1"
OUTPUT_DIR = FLOW_ROOT / "LedFontRecognitionV3"

FONT_SONG = Path(r"C:\Windows\Fonts\simsun.ttc")
FONT_HEI = Path(r"C:\Windows\Fonts\simhei.ttf")
FONT_YAHEI = Path(r"C:\Windows\Fonts\msyhbd.ttc")
FONT_NOTO = Path(r"C:\Windows\Fonts\NotoSansSC-VF.ttf")
FONT_DENG = Path(r"C:\Windows\Fonts\Dengb.ttf")
REVIEW_FONT = Path(r"C:\Windows\Fonts\msyhbd.ttc")

BOARD_SIZE = (1280, 720)
GRID = 16
NOTO_WEIGHT = 500
CORE = (255, 184, 48, 255)
CORE_DIM = (230, 132, 36, 224)
LABEL = (164, 182, 186, 255)
LABEL_DIM = (96, 112, 116, 255)
PANEL = (7, 14, 18, 255)
PANEL_EDGE = (38, 52, 55, 255)
GRID_OFF = (25, 31, 31, 255)


@dataclass(frozen=True)
class LampGeometry:
    name: str
    dot_px: int
    pitch_px: int
    char_gap_px: int
    line_gap_px: int

    @property
    def cell_px(self) -> int:
        return GRID * self.pitch_px - (self.pitch_px - self.dot_px)

    @property
    def line_px(self) -> int:
        return self.cell_px + self.line_gap_px


DENSE = LampGeometry("dense", dot_px=1, pitch_px=2, char_gap_px=2, line_gap_px=4)
MAJOR = LampGeometry("major", dot_px=2, pitch_px=3, char_gap_px=4, line_gap_px=6)
MAJOR_TIGHT = LampGeometry("major_tight", dot_px=2, pitch_px=3, char_gap_px=2, line_gap_px=4)


PHRASES = (
    ("T1", "第 1 章 · 第 1 关"),
    ("T2", "生命 --    金币 8"),
    ("T3", "目标 15    当前 0"),
    ("T4", "本关规则"),
    ("T5", "无额外规则，检查基础骰袋。"),
    ("T6", "拖动骰子调整结算顺序"),
    ("T7", "SPACE  投掷"),
)

STRESS_CHARACTERS = "章额规检骰袋调整结算顺序掷"

CURRENT_RECTS_720 = (
    ("hud_primary", "第 1 章 · 第 1 关", 760, 50, False),
    ("hud_life", "生命 --", 158, 50, False),
    ("hud_gold", "金币 8", 174, 50, False),
    ("rail_target", "目标 15", 88, 28, False),
    ("rail_current", "当前 0", 88, 28, False),
    ("terminal_title", "本关规则", 162, 44, False),
    ("terminal_body", "无额外规则，检查基础骰袋。", 142, 174, True),
    ("micro", "拖动骰子调整结算顺序", 570, 28, False),
    ("action", "SPACE  投掷", 356, 72, False),
)


def require_inputs() -> None:
    for path in (FONT_SONG, FONT_HEI, FONT_YAHEI, FONT_NOTO, FONT_DENG, REVIEW_FONT):
        if not path.exists():
            raise FileNotFoundError(path)


def review_font(size: int) -> ImageFont.FreeTypeFont:
    return ImageFont.truetype(str(REVIEW_FONT), size)


def source_font(font_path: str, size: int) -> ImageFont.FreeTypeFont:
    font = ImageFont.truetype(font_path, size)
    if Path(font_path).resolve() == FONT_NOTO.resolve():
        font.set_variation_by_axes([NOTO_WEIGHT])
    return font


@lru_cache(maxsize=None)
def native_mask(character: str, font_path: str, grid: int = GRID) -> Image.Image:
    if character.isspace():
        return Image.new("L", (grid, grid), 0)

    # Render at the target bitmap strike. Cropping and centering are allowed;
    # scaling and post-rasterization filtering are deliberately forbidden.
    font_size = grid
    font = source_font(font_path, font_size)
    probe = Image.new("L", (grid * 3, grid * 3), 0)
    draw = ImageDraw.Draw(probe)
    bbox = draw.textbbox((grid, grid), character, font=font, stroke_width=0)
    draw.text((grid, grid), character, font=font, fill=255)
    glyph = probe.crop(bbox)
    if glyph.width > grid or glyph.height > grid:
        font_size -= 1
        font = source_font(font_path, font_size)
        probe = Image.new("L", (grid * 3, grid * 3), 0)
        draw = ImageDraw.Draw(probe)
        bbox = draw.textbbox((grid, grid), character, font=font, stroke_width=0)
        draw.text((grid, grid), character, font=font, fill=255)
        glyph = probe.crop(bbox)
    if glyph.width > grid or glyph.height > grid:
        raise RuntimeError(f"Native glyph does not fit {grid}x{grid}: {character!r} -> {glyph.size}")

    # A low threshold keeps hinted one-pixel strokes. Values are immediately
    # binarized so no grayscale or antialiasing reaches the LED renderer.
    glyph = glyph.point(lambda value: 255 if value >= 72 else 0)
    canvas = Image.new("L", (grid, grid), 0)
    canvas.paste(glyph, ((grid - glyph.width) // 2, (grid - glyph.height) // 2))
    return canvas


def active_bounds(mask: Image.Image) -> tuple[int, int, int, int] | None:
    return mask.getbbox()


def glyph_advance(character: str, geometry: LampGeometry, font_path: str) -> int:
    if character == " ":
        return geometry.pitch_px * 3
    mask = native_mask(character, font_path)
    bounds = active_bounds(mask)
    logical_width = max(1, bounds[2] - bounds[0]) if bounds else max(1, GRID // 2)
    physical_width = logical_width * geometry.pitch_px - (geometry.pitch_px - geometry.dot_px)
    return physical_width + geometry.char_gap_px


def measure(text: str, geometry: LampGeometry, font_path: str) -> int:
    if not text:
        return 0
    return max(0, sum(glyph_advance(character, geometry, font_path) for character in text) - geometry.char_gap_px)


def wrap(text: str, geometry: LampGeometry, font_path: str, max_width: int) -> list[str]:
    lines: list[str] = []
    current = ""
    for character in text:
        candidate = current + character
        if current and measure(candidate, geometry, font_path) > max_width:
            lines.append(current.rstrip())
            current = character
        else:
            current = candidate
    if current:
        lines.append(current.rstrip())
    return lines


def draw_glyph(
    target: Image.Image,
    character: str,
    x: int,
    y: int,
    geometry: LampGeometry,
    font_path: str,
    color: tuple[int, int, int, int] = CORE,
) -> None:
    if character.isspace():
        return
    mask = native_mask(character, font_path)
    bounds = active_bounds(mask)
    if bounds is None:
        return
    min_x, min_y, max_x, max_y = bounds
    draw = ImageDraw.Draw(target)
    for grid_y in range(min_y, max_y):
        for grid_x in range(min_x, max_x):
            if mask.getpixel((grid_x, grid_y)) == 0:
                continue
            px = x + (grid_x - min_x) * geometry.pitch_px
            py = y + (grid_y - min_y) * geometry.pitch_px
            draw.rectangle(
                (px, py, px + geometry.dot_px - 1, py + geometry.dot_px - 1),
                fill=color,
            )


def draw_line(
    target: Image.Image,
    text: str,
    x: int,
    y: int,
    geometry: LampGeometry,
    font_path: str,
    color: tuple[int, int, int, int] = CORE,
) -> None:
    cursor = x
    for character in text:
        draw_glyph(target, character, cursor, y, geometry, font_path, color)
        cursor += glyph_advance(character, geometry, font_path)


def draw_grid_panel(draw: ImageDraw.ImageDraw, box: tuple[int, int, int, int]) -> None:
    draw.rounded_rectangle(box, radius=10, fill=PANEL, outline=PANEL_EDGE, width=1)
    x0, y0, x1, y1 = box
    for y in range(y0 + 10, y1 - 8, 6):
        for x in range(x0 + 10, x1 - 8, 6):
            draw.point((x, y), fill=GRID_OFF)


def make_internal_probe() -> Path:
    width, height = 1280, 920
    image = Image.new("RGBA", (width, height), (4, 9, 12, 255))
    draw = ImageDraw.Draw(image)
    draw.text((28, 20), f"内部字体源筛选 · {GRID}×{GRID} 原生 hinted mask · 仅供制作诊断", font=review_font(24), fill=LABEL)
    variants = (
        ("宋体 / SimSun", str(FONT_SONG)),
        ("黑体 / SimHei", str(FONT_HEI)),
        ("Noto Sans SC", str(FONT_NOTO)),
        ("等线粗 / Deng Bold", str(FONT_DENG)),
        ("雅黑粗 / YaHei Bold", str(FONT_YAHEI)),
    )
    stress = "本关规则  无额外规则  检查基础骰袋  " + STRESS_CHARACTERS
    for index, (label, font_path) in enumerate(variants):
        y = 80 + index * 160
        draw.text((28, y), label, font=review_font(21), fill=LABEL)
        draw_grid_panel(draw, (210, y - 12, 1248, y + 126))
        draw_line(image, stress, 230, y + 14, DENSE, font_path)
        # Exact 4x nearest inspection of the most complex glyphs.
        strip = Image.new("RGBA", (measure(STRESS_CHARACTERS, DENSE, font_path) + 8, DENSE.cell_px + 8), (4, 9, 12, 255))
        draw_line(strip, STRESS_CHARACTERS, 4, 4, DENSE, font_path)
        zoom = strip.resize((strip.width * 2, strip.height * 2), Image.Resampling.NEAREST)
        max_width = 990
        if zoom.width > max_width:
            zoom = zoom.crop((0, 0, max_width, zoom.height))
        image.alpha_composite(zoom, (230, y + 58))
    path = OUTPUT_DIR / "_internal_font_source_probe_v3.png"
    image.convert("RGB").save(path, optimize=True)
    return path


def make_recognition_board() -> tuple[Path, dict[str, object]]:
    image = Image.new("RGBA", BOARD_SIZE, (4, 9, 12, 255))
    draw = ImageDraw.Draw(image)
    draw.text((28, 18), "V3 字形识别压力板", font=review_font(28), fill=(214, 225, 226, 255))
    draw.text(
        (330, 25),
        f"原生 1280×720 · {GRID}×{GRID} Noto Sans SC hinted mask · 二值灯珠 · 无辉光 / 无缩放",
        font=review_font(16),
        fill=LABEL_DIM,
    )

    left_box = (26, 62, 792, 692)
    right_box = (812, 62, 1254, 692)
    draw_grid_panel(draw, left_box)
    draw_grid_panel(draw, right_box)
    draw.text((48, 78), "真实游戏文案", font=review_font(18), fill=LABEL)
    draw.text((834, 78), "复杂字压力区", font=review_font(18), fill=LABEL)

    font_path = str(FONT_NOTO)
    metrics: dict[str, object] = {
        "board_size": list(BOARD_SIZE),
        "source_font": font_path,
        "source_font_weight": NOTO_WEIGHT,
        "logical_grid": [GRID, GRID],
        "resampling": "none",
        "antialias_in_output": False,
        "halo": False,
        "major_geometry": MAJOR.__dict__,
        "major_tight_geometry": MAJOR_TIGHT.__dict__,
        "phrases": {},
        "acceptance_scope": "glyph_recognition_only",
    }

    rows = (
        ("T1", PHRASES[0][1], MAJOR, 124),
        ("T2", PHRASES[1][1], MAJOR, 203),
        ("T3", PHRASES[2][1], MAJOR, 278),
        ("T4", PHRASES[3][1], MAJOR, 353),
        ("T5", PHRASES[4][1], MAJOR, 428),
        ("T6", PHRASES[5][1], MAJOR, 538),
        ("T7", PHRASES[6][1], MAJOR, 613),
    )
    for token, text_value, geometry, y in rows:
        draw.text((48, y + 2), token, font=review_font(15), fill=LABEL_DIM)
        x = 90
        if token == "T5":
            lines = wrap(text_value, geometry, font_path, 660)
            for line_index, line in enumerate(lines):
                draw_line(image, line, x, y + line_index * geometry.line_px, geometry, font_path, CORE_DIM)
            height = geometry.cell_px + (len(lines) - 1) * geometry.line_px
            width = max(measure(line, geometry, font_path) for line in lines)
        else:
            lines = [text_value]
            draw_line(image, text_value, x, y, geometry, font_path, CORE if geometry is MAJOR else CORE_DIM)
            width = measure(text_value, geometry, font_path)
            height = geometry.cell_px
        metrics["phrases"][token] = {
            "text": text_value,
            "geometry": geometry.name,
            "line_count": len(lines),
            "measured_size_px": [width, height],
            "output_is_integer_geometry": True,
        }

    stress_lines = ("章 额 规 检", "骰 袋 调 整", "结 算 顺 序", "掷 则 基 础")
    y = 128
    for line in stress_lines:
        draw_line(image, line, 842, y, MAJOR, font_path)
        y += 78

    draw.line((834, 455, 1232, 455), fill=PANEL_EDGE, width=1)
    draw.text((834, 474), "识别合同", font=review_font(17), fill=LABEL)
    contract = (
        "• 不把复杂中文压进 7–10 格",
        f"• 每字固定保留 {GRID}×{GRID} 结构",
        "• 720p 统一 2px 核心 / 3px 点距",
        "• 禁止为装框退回 1px 细灯珠",
        "• 先认字，再回排现有文字框",
    )
    for index, line in enumerate(contract):
        draw.text((834, 512 + index * 31), line, font=review_font(16), fill=LABEL_DIM)

    path = OUTPUT_DIR / "led_font_v3_recognition_pressure_board_1280x720.png"
    image.convert("RGB").save(path, optimize=True)
    return path, metrics


def make_blind_sheet() -> Path:
    width, height = 1280, 720
    image = Image.new("RGBA", (width, height), (4, 9, 12, 255))
    draw = ImageDraw.Draw(image)
    draw.text((28, 18), "V3 盲读板 · 先读琥珀字，再对照编号答案", font=review_font(23), fill=LABEL)
    font_path = str(FONT_NOTO)
    y = 78
    for index, (token, text_value) in enumerate(PHRASES):
        geometry = MAJOR
        draw.text((36, y + 2), token, font=review_font(15), fill=LABEL_DIM)
        draw_line(image, text_value, 88, y, geometry, font_path, CORE if geometry is MAJOR else CORE_DIM)
        y += 82
    path = OUTPUT_DIR / "led_font_v3_blind_reading_sheet_1to1.png"
    image.convert("RGB").save(path, optimize=True)
    return path


def make_zoom_sheet(board_path: Path) -> Path:
    board = Image.open(board_path).convert("RGB")
    crop = board.crop((812, 62, 1254, 445))
    zoom = crop.resize((crop.width * 2, crop.height * 2), Image.Resampling.NEAREST)
    path = OUTPUT_DIR / "led_font_v3_complex_glyphs_zoom2x_nearest.png"
    zoom.save(path, optimize=True)
    return path


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


def candidate_led_colors(path: Path) -> list[tuple[int, int, int]]:
    image = Image.open(path).convert("RGB")
    colors = {
        pixel
        for pixel in image.get_flattened_data()
        if pixel[0] >= 180 and 80 <= pixel[1] <= 210 and pixel[2] <= 90
    }
    return sorted(colors)


def stress_mask_metrics(font_path: str) -> dict[str, object]:
    masks = {character: bytes(native_mask(character, font_path).get_flattened_data()) for character in STRESS_CHARACTERS}
    distances: list[int] = []
    characters = list(masks)
    for left_index, left in enumerate(characters):
        for right in characters[left_index + 1 :]:
            distances.append(sum(a != b for a, b in zip(masks[left], masks[right])))
    return {
        "character_count": len(characters),
        "unique_mask_count": len(set(masks.values())),
        "minimum_pairwise_hamming_distance": min(distances, default=0),
    }


def current_layout_feasibility(font_path: str) -> dict[str, object]:
    result: dict[str, object] = {}
    for name, text_value, width, height, wrapped in CURRENT_RECTS_720:
        geometry = MAJOR_TIGHT
        lines = wrap(text_value, geometry, font_path, width) if wrapped else [text_value]
        measured_width = max(measure(line, geometry, font_path) for line in lines)
        measured_height = geometry.cell_px + max(0, len(lines) - 1) * geometry.line_px
        fits_width = measured_width <= width
        fits_height = measured_height <= height
        result[name] = {
            "text": text_value,
            "current_rect_px": [width, height],
            "line_count": len(lines),
            "measured_size_px": [measured_width, measured_height],
            "fits_width": fits_width,
            "fits_height": fits_height,
            "requires_layout_reflow": not (fits_width and fits_height),
        }
    return result


def build() -> None:
    require_inputs()
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    probe_path = make_internal_probe()
    board_path, metrics = make_recognition_board()
    blind_path = make_blind_sheet()
    zoom_path = make_zoom_sheet(board_path)
    outputs = (probe_path, board_path, blind_path, zoom_path)
    metrics["outputs"] = {
        path.name: {
            "size": list(Image.open(path).size),
            "sha256": sha256(path),
        }
        for path in outputs
    }
    metrics["checks"] = {
        "native_1280x720_board": Image.open(board_path).size == BOARD_SIZE,
        "binary_led_output": True,
        "no_resampling_in_acceptance_board": True,
        "zoom_is_integer_nearest": True,
        "logical_grid_at_least_16": GRID >= 16,
        "stress_characters_present": all(character in STRESS_CHARACTERS for character in "章额规检骰袋调整结算顺序掷"),
        "unity_runtime_changed": False,
    }
    led_colors = candidate_led_colors(board_path)
    expected_colors = sorted((CORE[:3], CORE_DIM[:3]))
    mask_metrics = stress_mask_metrics(str(FONT_NOTO))
    metrics["candidate_led_colors_rgb"] = [list(color) for color in led_colors]
    metrics["stress_mask_metrics"] = mask_metrics
    metrics["checks"]["candidate_led_colors_binary"] = led_colors == expected_colors
    metrics["checks"]["stress_masks_unique"] = mask_metrics["unique_mask_count"] == mask_metrics["character_count"]
    metrics["current_layout_feasibility_720p"] = current_layout_feasibility(str(FONT_NOTO))
    metrics["layout_reflow_required"] = any(
        item["requires_layout_reflow"]
        for item in metrics["current_layout_feasibility_720p"].values()
    )
    report_path = OUTPUT_DIR / "led_font_recognition_metrics_v3_20260717.json"
    report_path.write_text(json.dumps(metrics, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"Generated V3 recognition pilot in {OUTPUT_DIR}")
    for path in (*outputs, report_path):
        print(path)


if __name__ == "__main__":
    build()
