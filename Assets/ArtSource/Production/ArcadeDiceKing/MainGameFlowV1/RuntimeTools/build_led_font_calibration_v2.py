from __future__ import annotations

import csv
import hashlib
import json
import math
from dataclasses import dataclass
from functools import lru_cache
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[6]
FLOW_ROOT = ROOT / "Assets" / "ArtSource" / "Production" / "ArcadeDiceKing" / "MainGameFlowV1"
OUTPUT_DIR = FLOW_ROOT / "LedFontCalibrationV2"
REFERENCE_1080 = FLOW_ROOT / "MotionPreviewsV1" / "_sources" / "01_ready_1920x1080.png"
RUNTIME_1080 = ROOT / "Docs" / "QA" / "20260716_main_game_runtime_ready_1920x1080.png"
RUNTIME_720 = ROOT / "Docs" / "QA" / "20260716_main_game_runtime_ready_1280x720.png"
V1_A_1080 = FLOW_ROOT / "LedFontCalibrationV1" / "led_font_candidate_a_coarse_ready_1920x1080_v1.png"
V1_A_720 = FLOW_ROOT / "LedFontCalibrationV1" / "led_font_candidate_a_coarse_ready_1280x720_v1.png"
COMMON_BASE = ROOT / "Assets" / "Resources" / "Art" / "MainGame" / "arcade_main_game_common_base.png"
RUNTIME_MAP = ROOT / "Assets" / "Resources" / "Art" / "MainGame" / "Flow" / "main_game_led_font_map.csv"
FONT_PATH = Path(r"C:\Windows\Fonts\msyhbd.ttc")
REVIEW_FONT_PATH = Path(r"C:\Windows\Fonts\msyhbd.ttc")

DESIGN_WIDTH = 1280
DESIGN_HEIGHT = 720
CORE_COLOR = (255, 184, 48)
HALO_COLOR = (255, 142, 28)
HALO_ALPHA = 34
HALO_RADIUS_PX = 1


@dataclass(frozen=True)
class Rect:
    x: float
    y: float
    width: float
    height: float

    @property
    def right(self) -> float:
        return self.x + self.width

    @property
    def bottom(self) -> float:
        return self.y + self.height


@dataclass(frozen=True)
class LampStyle:
    grid: int
    dot_px: int
    pitch_px: int
    alpha: int

    @property
    def cell_height_px(self) -> int:
        return self.grid * self.pitch_px - (self.pitch_px - self.dot_px)

    @property
    def line_height_px(self) -> int:
        return self.cell_height_px + self.pitch_px * 2


@dataclass(frozen=True)
class Pattern:
    width: int
    height: int
    points: tuple[tuple[int, int], ...]


@dataclass(frozen=True)
class TextSpec:
    name: str
    token: str
    rect: Rect
    text: str
    alignment: str
    wrapped: bool


TEXT_SPECS = (
    TextSpec("hud_primary", "hud_primary", Rect(50, 23, 760, 50), "第 1 章 · 第 1 关", "middle_left", False),
    TextSpec("hud_life", "hud_secondary", Rect(854, 23, 158, 50), "生命 --", "middle_center", False),
    TextSpec("hud_gold", "hud_secondary", Rect(1052, 23, 174, 50), "金币 8", "middle_center", False),
    TextSpec("rail_target", "rail", Rect(48, 191, 88, 28), "目标 15", "middle_center", False),
    TextSpec("rail_current", "rail", Rect(48, 220, 88, 28), "当前 0", "middle_center", False),
    TextSpec("terminal_title", "terminal_title", Rect(1062, 174, 162, 44), "本关规则", "middle_center", False),
    TextSpec("terminal_body", "terminal_body", Rect(1072, 230, 142, 174), "无额外规则，检查基础骰袋。", "middle_center", True),
    TextSpec("micro", "micro", Rect(320, 492, 570, 28), "拖动骰子调整结算顺序", "middle_center", False),
    TextSpec("action", "action", Rect(458, 574, 356, 72), "SPACE  投掷", "middle_center", False),
)


SAMPLE_CROPS = (
    ("hud_primary", Rect(36, 12, 500, 72)),
    ("rail", Rect(34, 122, 126, 150)),
    ("terminal", Rect(1043, 145, 196, 280)),
    ("action", Rect(438, 558, 398, 100)),
    ("micro", Rect(286, 470, 640, 72)),
)


STYLES_720 = {
    "hud_primary": LampStyle(10, 3, 4, 255),
    "hud_secondary": LampStyle(8, 2, 3, 248),
    "rail": LampStyle(7, 2, 3, 250),
    "terminal_title": LampStyle(9, 2, 3, 255),
    "terminal_body": LampStyle(9, 2, 3, 238),
    "action": LampStyle(9, 3, 4, 255),
    "micro": LampStyle(8, 2, 3, 232),
}


STYLES_1080 = {
    "hud_primary": LampStyle(10, 5, 7, 255),
    "hud_secondary": LampStyle(8, 3, 4, 248),
    "rail": LampStyle(7, 3, 4, 250),
    "terminal_title": LampStyle(9, 3, 4, 255),
    "terminal_body": LampStyle(9, 3, 4, 238),
    "action": LampStyle(9, 4, 6, 255),
    "micro": LampStyle(8, 3, 4, 232),
}


def require_inputs() -> None:
    for path in (
        REFERENCE_1080,
        RUNTIME_1080,
        RUNTIME_720,
        V1_A_1080,
        V1_A_720,
        COMMON_BASE,
        RUNTIME_MAP,
        FONT_PATH,
        REVIEW_FONT_PATH,
    ):
        if not path.exists():
            raise FileNotFoundError(path)


def review_font(size: int) -> ImageFont.FreeTypeFont:
    return ImageFont.truetype(str(REVIEW_FONT_PATH), size)


def runtime_characters() -> set[str]:
    result: set[str] = set()
    with RUNTIME_MAP.open("r", encoding="utf-8", newline="") as stream:
        for row in csv.DictReader(stream):
            result.add(chr(int(row["codepoint"])))
    return result


@lru_cache(maxsize=None)
def glyph_pattern(character: str, grid: int) -> Pattern:
    if character.isspace():
        return Pattern(max(1, int(round(grid * 0.38))), grid, tuple())

    font_size = grid * 12
    font = ImageFont.truetype(str(FONT_PATH), font_size)
    probe = Image.new("L", (font_size * 2, font_size * 2), 0)
    draw = ImageDraw.Draw(probe)
    bbox = draw.textbbox((0, 0), character, font=font, stroke_width=0)
    width = max(1, bbox[2] - bbox[0])
    height = max(1, bbox[3] - bbox[1])
    glyph = Image.new("L", (width, height), 0)
    ImageDraw.Draw(glyph).text((-bbox[0], -bbox[1]), character, font=font, fill=255)

    scale = min(grid / width, grid / height)
    target_width = max(1, min(grid, int(round(width * scale))))
    target_height = max(1, min(grid, int(round(height * scale))))
    glyph = glyph.resize((target_width, target_height), Image.Resampling.LANCZOS)
    canvas = Image.new("L", (grid, grid), 0)
    canvas.paste(glyph, ((grid - target_width) // 2, (grid - target_height) // 2))

    points: list[tuple[int, int]] = []
    for y in range(grid):
        for x in range(grid):
            if canvas.getpixel((x, y)) >= 108:
                points.append((x, y))
    if not points:
        center = grid // 2
        points = [(center, center)]

    min_x = min(point[0] for point in points)
    max_x = max(point[0] for point in points)
    min_y = min(point[1] for point in points)
    max_y = max(point[1] for point in points)
    normalized = tuple((x - min_x, y - min_y) for x, y in points)
    return Pattern(max_x - min_x + 1, max_y - min_y + 1, normalized)


def char_advance(character: str, style: LampStyle) -> int:
    pattern = glyph_pattern(character, style.grid)
    if character.isspace():
        return max(style.pitch_px * 2, pattern.width * style.pitch_px)
    return pattern.width * style.pitch_px + style.pitch_px


def measure_line(text: str, style: LampStyle) -> int:
    if not text:
        return 0
    width = sum(char_advance(character, style) for character in text)
    return max(0, width - style.pitch_px)


def wrap_lines(text: str, style: LampStyle, max_width: int) -> list[str]:
    result: list[str] = []
    for source in text.replace("\r", "").split("\n"):
        if not source:
            result.append("")
            continue
        current = ""
        for character in source:
            candidate = current + character
            if current and measure_line(candidate, style) > max_width:
                result.append(current.rstrip())
                current = character
            else:
                current = candidate
        result.append(current.rstrip())
    return result


def pixel_rect(rect: Rect, width: int, height: int, padding: float = 0.0) -> tuple[int, int, int, int]:
    sx = width / DESIGN_WIDTH
    sy = height / DESIGN_HEIGHT
    return (
        max(0, int(math.floor((rect.x - padding) * sx))),
        max(0, int(math.floor((rect.y - padding) * sy))),
        min(width, int(math.ceil((rect.right + padding) * sx))),
        min(height, int(math.ceil((rect.bottom + padding) * sy))),
    )


def erase_runtime_text(frame: Image.Image) -> None:
    base = Image.open(COMMON_BASE).convert("RGB")
    if base.size != frame.size:
        base = base.resize(frame.size, Image.Resampling.BILINEAR)
    for spec in TEXT_SPECS:
        box = pixel_rect(spec.rect, frame.width, frame.height, padding=3.0)
        frame.paste(base.crop(box), box)


def draw_lamp_glyph(
    core: Image.Image,
    halo: Image.Image | None,
    character: str,
    style: LampStyle,
    x: int,
    y: int,
) -> None:
    if character.isspace():
        return
    pattern = glyph_pattern(character, style.grid)
    core_draw = ImageDraw.Draw(core)
    halo_draw = ImageDraw.Draw(halo) if halo is not None else None
    for cell_x, cell_y in pattern.points:
        px = x + cell_x * style.pitch_px
        py = y + cell_y * style.pitch_px
        if halo_draw is not None:
            halo_draw.rectangle(
                (
                    px - HALO_RADIUS_PX,
                    py - HALO_RADIUS_PX,
                    px + style.dot_px - 1 + HALO_RADIUS_PX,
                    py + style.dot_px - 1 + HALO_RADIUS_PX,
                ),
                fill=(*HALO_COLOR, HALO_ALPHA),
            )
        core_draw.rectangle(
            (px, py, px + style.dot_px - 1, py + style.dot_px - 1),
            fill=(*CORE_COLOR, style.alpha),
        )


def draw_lamp_line(
    core: Image.Image,
    halo: Image.Image | None,
    text: str,
    style: LampStyle,
    x: int,
    y: int,
) -> None:
    cursor = x
    for character in text:
        pattern = glyph_pattern(character, style.grid)
        glyph_y = y + max(0, (style.cell_height_px - (pattern.height * style.pitch_px - (style.pitch_px - style.dot_px))) // 2)
        draw_lamp_glyph(core, halo, character, style, cursor, glyph_y)
        cursor += char_advance(character, style)


def render_spec(
    core: Image.Image,
    halo: Image.Image | None,
    spec: TextSpec,
    style: LampStyle,
) -> dict[str, object]:
    box = pixel_rect(spec.rect, core.width, core.height)
    rect_width = box[2] - box[0]
    rect_height = box[3] - box[1]
    lines = wrap_lines(spec.text, style, rect_width) if spec.wrapped else spec.text.replace("\r", "").split("\n")
    widths = [measure_line(line, style) for line in lines]
    block_height = style.cell_height_px + max(0, len(lines) - 1) * style.line_height_px

    if spec.alignment.startswith("middle"):
        y = box[1] + (rect_height - block_height) // 2
    elif spec.alignment.startswith("lower"):
        y = box[3] - block_height
    else:
        y = box[1]

    for index, line in enumerate(lines):
        line_width = widths[index]
        if spec.alignment.endswith("center"):
            x = box[0] + (rect_width - line_width) // 2
        elif spec.alignment.endswith("right"):
            x = box[2] - line_width
        else:
            x = box[0]
        draw_lamp_line(core, halo, line, style, x, y + index * style.line_height_px)

    return {
        "line_count": len(lines),
        "measured_width_px": max(widths, default=0),
        "measured_height_px": block_height,
        "rect_px": [box[0], box[1], rect_width, rect_height],
        "fits_width": max(widths, default=0) <= rect_width,
        "fits_height": block_height <= rect_height,
        "grid": style.grid,
        "core_dot_px": [style.dot_px, style.dot_px],
        "pitch_px": style.pitch_px,
        "integer_geometry": True,
        "core_alpha": style.alpha,
        "halo_radius_px": HALO_RADIUS_PX if halo is not None else 0,
        "halo_alpha": HALO_ALPHA if halo is not None else 0,
    }


def build_frame(size: tuple[int, int], with_halo: bool) -> tuple[Image.Image, dict[str, object]]:
    source = RUNTIME_1080 if size == (1920, 1080) else RUNTIME_720
    frame = Image.open(source).convert("RGB")
    erase_runtime_text(frame)
    core = Image.new("RGBA", size, (0, 0, 0, 0))
    halo = Image.new("RGBA", size, (0, 0, 0, 0)) if with_halo else None
    styles = STYLES_1080 if size == (1920, 1080) else STYLES_720
    metrics: dict[str, object] = {}
    for spec in TEXT_SPECS:
        metrics[spec.name] = render_spec(core, halo, spec, styles[spec.token])
    composed = frame.convert("RGBA")
    if halo is not None:
        composed = Image.alpha_composite(composed, halo)
    composed = Image.alpha_composite(composed, core)
    return composed.convert("RGB"), metrics


def make_exact_comparisons(
    resolution_label: str,
    size: tuple[int, int],
    sources: tuple[tuple[str, Image.Image], ...],
) -> list[Path]:
    outputs: list[Path] = []
    header_height = 54
    gap = 12
    for crop_name, rect in SAMPLE_CROPS:
        box = pixel_rect(rect, size[0], size[1])
        crops = [(label, image.crop(box).convert("RGB")) for label, image in sources]
        crop_width = crops[0][1].width
        crop_height = crops[0][1].height
        board_width = crop_width
        row_height = header_height + crop_height
        board_height = len(crops) * row_height + max(0, len(crops) - 1) * gap
        board = Image.new("RGB", (board_width, board_height), (7, 14, 20))
        draw = ImageDraw.Draw(board)
        for index, (label, crop) in enumerate(crops):
            y = index * (row_height + gap)
            draw.text((8, y + 10), label, font=review_font(24), fill=(233, 240, 242))
            board.paste(crop, (0, y + header_height))
        path = OUTPUT_DIR / f"led_font_v2_{resolution_label}_{crop_name}_comparison_1to1.png"
        board.save(path, optimize=True)
        outputs.append(path)
    return outputs


def make_zoom_checks(
    size: tuple[int, int],
    sources: tuple[tuple[str, Image.Image], ...],
    zoom: int = 4,
) -> list[Path]:
    outputs: list[Path] = []
    header_height = 56
    gap = 16
    selected = tuple(item for item in SAMPLE_CROPS if item[0] in {"hud_primary", "terminal", "action"})
    for crop_name, rect in selected:
        box = pixel_rect(rect, size[0], size[1])
        zoomed: list[tuple[str, Image.Image]] = []
        for label, image in sources:
            crop = image.crop(box).convert("RGB")
            zoomed.append((label, crop.resize((crop.width * zoom, crop.height * zoom), Image.Resampling.NEAREST)))
        width = max(image.width for _, image in zoomed)
        row_height = header_height + max(image.height for _, image in zoomed)
        height = len(zoomed) * row_height + gap * (len(zoomed) - 1)
        board = Image.new("RGB", (width, height), (7, 14, 20))
        draw = ImageDraw.Draw(board)
        for index, (label, image) in enumerate(zoomed):
            y = index * (row_height + gap)
            draw.text((8, y + 10), f"{label} · {zoom}× NEAREST", font=review_font(24), fill=(233, 240, 242))
            board.paste(image, (0, y + header_height))
        path = OUTPUT_DIR / f"led_font_v2_1280x720_{crop_name}_pixel_zoom{zoom}x.png"
        board.save(path, optimize=True)
        outputs.append(path)
    return outputs


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest().upper()


def build() -> None:
    require_inputs()
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    core_1080, core_metrics_1080 = build_frame((1920, 1080), with_halo=False)
    lamp_1080, lamp_metrics_1080 = build_frame((1920, 1080), with_halo=True)
    core_720, core_metrics_720 = build_frame((1280, 720), with_halo=False)
    lamp_720, lamp_metrics_720 = build_frame((1280, 720), with_halo=True)

    frame_paths = {
        "core_1920x1080": OUTPUT_DIR / "led_font_v2_hard_core_ready_1920x1080.png",
        "lamp_1920x1080": OUTPUT_DIR / "led_font_v2_controlled_halo_ready_1920x1080.png",
        "core_1280x720": OUTPUT_DIR / "led_font_v2_hard_core_ready_1280x720.png",
        "lamp_1280x720": OUTPUT_DIR / "led_font_v2_controlled_halo_ready_1280x720.png",
    }
    core_1080.save(frame_paths["core_1920x1080"], optimize=True)
    lamp_1080.save(frame_paths["lamp_1920x1080"], optimize=True)
    core_720.save(frame_paths["core_1280x720"], optimize=True)
    lamp_720.save(frame_paths["lamp_1280x720"], optimize=True)

    reference = Image.open(REFERENCE_1080).convert("RGB")
    current_1080 = Image.open(RUNTIME_1080).convert("RGB")
    current_720 = Image.open(RUNTIME_720).convert("RGB")
    v1_a_1080 = Image.open(V1_A_1080).convert("RGB")
    v1_a_720 = Image.open(V1_A_720).convert("RGB")

    comparison_paths = make_exact_comparisons(
        "1920x1080",
        (1920, 1080),
        (
            ("批准预览", reference),
            ("当前运行", current_1080),
            ("V1 A 已退回", v1_a_1080),
            ("V2 无辉光硬核心", core_1080),
            ("V2 受限 1px 辉光", lamp_1080),
        ),
    )
    comparison_paths += make_exact_comparisons(
        "1280x720",
        (1280, 720),
        (
            ("当前运行", current_720),
            ("V1 A 已退回", v1_a_720),
            ("V2 无辉光硬核心", core_720),
            ("V2 受限 1px 辉光", lamp_720),
        ),
    )
    zoom_paths = make_zoom_checks(
        (1280, 720),
        (
            ("V1 A 已退回", v1_a_720),
            ("V2 无辉光硬核心", core_720),
            ("V2 受限 1px 辉光", lamp_720),
        ),
    )

    all_metrics = {
        "hard_core": {"1920x1080": core_metrics_1080, "1280x720": core_metrics_720},
        "controlled_halo": {"1920x1080": lamp_metrics_1080, "1280x720": lamp_metrics_720},
    }
    fits = all(
        bool(metric["fits_width"]) and bool(metric["fits_height"]) and bool(metric["integer_geometry"])
        for mode in all_metrics.values()
        for resolution in mode.values()
        for metric in resolution.values()
    )
    required_text = "".join(spec.text for spec in TEXT_SPECS)
    available_characters = runtime_characters()
    missing_glyphs = sorted({character for character in required_text if not character.isspace() and character not in available_characters})

    output_files = list(frame_paths.values()) + comparison_paths + zoom_paths
    report = {
        "version": "V2",
        "date": "2026-07-17",
        "status": "像素级样张待用户验收",
        "v1_status": "REJECTED_BLUR",
        "runtime_changed": False,
        "acceptance_contract": {
            "full_frames_are_native_resolution": True,
            "comparison_crops_are_1to1_without_resampling": True,
            "pixel_zoom_uses_integer_nearest_only": True,
            "overview_thumbnail_can_approve_sharpness": False,
            "core_render_uses_antialiasing": False,
            "core_render_uses_texture_resizing": False,
            "core_dot_geometry_is_integer_and_square": True,
            "glow_is_separate_layer": True,
            "glow_filter": "none",
            "glow_radius_px": HALO_RADIUS_PX,
            "glow_alpha": HALO_ALPHA,
        },
        "required_text_missing_glyphs": missing_glyphs,
        "metrics": all_metrics,
        "machine_result": "PASS" if fits and not missing_glyphs else "FAIL",
        "user_visual_result": "PENDING",
        "outputs": [],
    }
    for path in output_files:
        report["outputs"].append(
            {
                "path": str(path.relative_to(ROOT)).replace("\\", "/"),
                "bytes": path.stat().st_size,
                "sha256": sha256(path),
            }
        )
    report_path = OUTPUT_DIR / "led_font_calibration_metrics_v2_20260717.json"
    report_path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    print(frame_paths["core_1280x720"])
    print(frame_paths["lamp_1280x720"])
    print(zoom_paths[1])
    print(report_path)


if __name__ == "__main__":
    build()
