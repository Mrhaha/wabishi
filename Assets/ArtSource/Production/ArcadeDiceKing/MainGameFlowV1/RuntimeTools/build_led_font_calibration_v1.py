from __future__ import annotations

import csv
import hashlib
import json
import math
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[6]
FLOW_ROOT = ROOT / "Assets" / "ArtSource" / "Production" / "ArcadeDiceKing" / "MainGameFlowV1"
OUTPUT_DIR = FLOW_ROOT / "LedFontCalibrationV1"
REFERENCE_1080 = FLOW_ROOT / "MotionPreviewsV1" / "_sources" / "01_ready_1920x1080.png"
RUNTIME_1080 = ROOT / "Docs" / "QA" / "20260716_main_game_runtime_ready_1920x1080.png"
RUNTIME_720 = ROOT / "Docs" / "QA" / "20260716_main_game_runtime_ready_1280x720.png"
COMMON_BASE = ROOT / "Assets" / "Resources" / "Art" / "MainGame" / "arcade_main_game_common_base.png"
RUNTIME_ATLAS = ROOT / "Assets" / "Resources" / "Art" / "MainGame" / "Flow" / "main_game_led_font_atlas.png"
RUNTIME_MAP = ROOT / "Assets" / "Resources" / "Art" / "MainGame" / "Flow" / "main_game_led_font_map.csv"
FONT_PATH = Path(r"C:\Windows\Fonts\msyhbd.ttc")
REVIEW_FONT_PATH = Path(r"C:\Windows\Fonts\msyhbd.ttc")

DESIGN_WIDTH = 1280
DESIGN_HEIGHT = 720
GLOW_INTENSITY = 0.72
GLOW_ALPHA_SCALE = 0.22
GLOW_EXPANSION = 1.35


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
class Glyph:
    box: tuple[int, int, int, int]
    advance: float


@dataclass(frozen=True)
class Profile:
    key: str
    label: str
    note: str
    tile: int
    columns: int
    grid: int
    cell: int
    dot: int
    offset: int
    threshold: int
    strong_threshold: int
    weak_alpha: int
    stroke_width: int
    sizes: dict[str, int]


CURRENT_SIZES = {
    "hud_primary": 22,
    "hud_secondary": 18,
    "rail": 16,
    "terminal_title": 18,
    "terminal_body": 14,
    "action": 24,
    "micro": 14,
}

PROFILES = (
    Profile(
        key="candidate_a_coarse",
        label="A 推荐：10×10 粗灯珠",
        note="优先接近批准预览；减少点阵密度并显著增粗灯珠。",
        tile=10,
        columns=32,
        grid=10,
        cell=1,
        dot=1,
        offset=0,
        threshold=96,
        strong_threshold=184,
        weak_alpha=220,
        stroke_width=0,
        sizes={
            "hud_primary": 28,
            "hud_secondary": 24,
            "rail": 22,
            "terminal_title": 24,
            "terminal_body": 22,
            "action": 30,
            "micro": 22,
        },
    ),
    Profile(
        key="candidate_b_compact",
        label="B 保守：11×11 紧凑灯珠",
        note="保留更多中文字形细节；尺寸较克制，核心灯珠仍不低于 2px。",
        tile=11,
        columns=32,
        grid=11,
        cell=1,
        dot=1,
        offset=0,
        threshold=108,
        strong_threshold=190,
        weak_alpha=216,
        stroke_width=0,
        sizes={
            "hud_primary": 26,
            "hud_secondary": 23,
            "rail": 22,
            "terminal_title": 24,
            "terminal_body": 22,
            "action": 28,
            "micro": 22,
        },
    ),
)


TEXT_SPECS = (
    ("hud_primary", Rect(50, 23, 760, 50), "第 1 章 · 第 1 关", (255, 163, 48, 245), "middle_left", False),
    ("hud_secondary", Rect(854, 23, 158, 50), "生命 --", (242, 148, 46, 209), "middle_center", False),
    ("hud_secondary", Rect(1052, 23, 174, 50), "金币 8", (255, 163, 48, 245), "middle_center", False),
    ("rail", Rect(48, 191, 88, 28), "目标 15", (255, 158, 46, 245), "middle_center", False),
    ("rail", Rect(48, 220, 88, 28), "当前 0", (255, 158, 46, 245), "middle_center", False),
    ("terminal_title", Rect(1062, 174, 162, 44), "本关规则", (255, 163, 48, 245), "middle_center", False),
    ("terminal_body", Rect(1072, 230, 142, 174), "无额外规则，检查基础骰袋。", (237, 148, 56, 219), "middle_center", True),
    ("micro", Rect(320, 492, 570, 28), "拖动骰子调整结算顺序", (235, 143, 46, 184), "middle_center", False),
    ("action", Rect(458, 574, 356, 72), "SPACE  投掷", (255, 166, 51, 250), "middle_center", False),
)


SAMPLE_CROPS = (
    ("顶部主信息", "字高优先", Rect(36, 12, 500, 72)),
    ("积分塔读数", "当前槽宽压力", Rect(34, 122, 126, 150)),
    ("右侧终端", "标题与正文", Rect(1043, 145, 196, 280)),
    ("底部主按键", "主操作强调", Rect(438, 558, 398, 100)),
    ("最小辅助字", "不得继续缩到 14px", Rect(286, 470, 640, 72)),
)


def require_inputs() -> None:
    for path in (REFERENCE_1080, RUNTIME_1080, RUNTIME_720, COMMON_BASE, RUNTIME_ATLAS, RUNTIME_MAP, FONT_PATH, REVIEW_FONT_PATH):
        if not path.exists():
            raise FileNotFoundError(path)


def load_runtime_map() -> tuple[list[str], dict[str, float]]:
    characters: list[str] = []
    advances: dict[str, float] = {}
    with RUNTIME_MAP.open("r", encoding="utf-8", newline="") as stream:
        reader = csv.DictReader(stream)
        for row in reader:
            character = chr(int(row["codepoint"]))
            characters.append(character)
            advances[character] = float(row["advance"])
    return characters, advances


def source_mask(character: str, font: ImageFont.FreeTypeFont, grid: int, stroke_width: int) -> Image.Image:
    if character == " ":
        return Image.new("L", (grid, grid), 0)
    probe = Image.new("L", (1, 1), 0)
    bbox = ImageDraw.Draw(probe).textbbox((0, 0), character, font=font, stroke_width=stroke_width)
    width = max(1, bbox[2] - bbox[0])
    height = max(1, bbox[3] - bbox[1])
    glyph = Image.new("L", (width, height), 0)
    glyph_draw = ImageDraw.Draw(glyph)
    glyph_draw.text(
        (-bbox[0], -bbox[1]),
        character,
        font=font,
        fill=255,
        stroke_width=stroke_width,
        stroke_fill=255,
    )
    if width > grid or height > grid:
        scale = min(grid / width, grid / height)
        width = max(1, int(round(width * scale)))
        height = max(1, int(round(height * scale)))
        glyph = glyph.resize((width, height), Image.Resampling.LANCZOS)
    canvas = Image.new("L", (grid, grid), 0)
    canvas.paste(glyph, ((grid - width) // 2, (grid - height) // 2))
    return canvas


def build_candidate_font(
    profile: Profile,
    characters: list[str],
    advances: dict[str, float],
) -> tuple[Image.Image, dict[str, Glyph]]:
    rows = int(math.ceil(len(characters) / profile.columns))
    atlas = Image.new("RGBA", (profile.columns * profile.tile, rows * profile.tile), (0, 0, 0, 0))
    font = ImageFont.truetype(str(FONT_PATH), max(8, profile.grid - 1))
    glyphs: dict[str, Glyph] = {}
    map_rows: list[tuple[int, int, int, int, int, float]] = []
    for index, character in enumerate(characters):
        column = index % profile.columns
        row = index // profile.columns
        x = column * profile.tile
        y = row * profile.tile
        mask = source_mask(character, font, profile.grid, profile.stroke_width)
        tile = Image.new("RGBA", (profile.tile, profile.tile), (0, 0, 0, 0))
        draw = ImageDraw.Draw(tile)
        for grid_y in range(profile.grid):
            for grid_x in range(profile.grid):
                strength = mask.getpixel((grid_x, grid_y))
                if strength < profile.threshold:
                    continue
                alpha = 255 if strength >= profile.strong_threshold else profile.weak_alpha
                dot_x = profile.offset + grid_x * profile.cell
                dot_y = profile.offset + grid_y * profile.cell
                draw.rectangle(
                    (dot_x, dot_y, dot_x + profile.dot - 1, dot_y + profile.dot - 1),
                    fill=(255, 255, 255, alpha),
                )
        atlas.alpha_composite(tile, (x, y))
        glyphs[character] = Glyph((x, y, x + profile.tile, y + profile.tile), advances[character])
        map_rows.append((ord(character), x, y, profile.tile, profile.tile, advances[character]))

    atlas_path = OUTPUT_DIR / f"led_font_{profile.key}_atlas_v1.png"
    map_path = OUTPUT_DIR / f"led_font_{profile.key}_map_v1.csv"
    atlas.save(atlas_path, optimize=True)
    with map_path.open("w", encoding="utf-8", newline="") as stream:
        writer = csv.writer(stream, lineterminator="\n")
        writer.writerow(("codepoint", "x", "y", "width", "height", "advance"))
        writer.writerows((*row[:5], f"{row[5]:.2f}") for row in map_rows)
    return atlas, glyphs


def advance_for(character: str, glyphs: dict[str, Glyph]) -> float:
    if character == "\t":
        return 1.28
    if character == " ":
        return 0.46
    glyph = glyphs.get(character) or glyphs.get("?")
    return glyph.advance if glyph is not None else 0.72


def measure(text: str, size: float, glyphs: dict[str, Glyph]) -> float:
    return sum(advance_for(character, glyphs) * size for character in text)


def wrap_lines(text: str, size: float, max_width: float, glyphs: dict[str, Glyph]) -> list[str]:
    result: list[str] = []
    for source in text.replace("\r", "").split("\n"):
        if not source:
            result.append("")
            continue
        current = ""
        current_width = 0.0
        for character in source:
            advance = advance_for(character, glyphs) * size
            if current and current_width + advance > max_width:
                result.append(current.rstrip())
                current = ""
                current_width = 0.0
            current += character
            current_width += advance
        result.append(current.rstrip())
    return result


def colorize(tile: Image.Image, color: tuple[int, int, int, int], alpha_scale: float) -> Image.Image:
    result = Image.new("RGBA", tile.size, (color[0], color[1], color[2], 0))
    source_alpha = tile.getchannel("A")
    factor = color[3] / 255.0 * alpha_scale
    result.putalpha(source_alpha.point(lambda value: max(0, min(255, int(round(value * factor))))))
    return result


def render_line(
    target: Image.Image,
    atlas: Image.Image,
    glyphs: dict[str, Glyph],
    text: str,
    x_design: float,
    y_design: float,
    size_design: float,
    color: tuple[int, int, int, int],
    expansion_design: float,
    alpha_scale: float,
) -> None:
    scale_x = target.width / DESIGN_WIDTH
    scale_y = target.height / DESIGN_HEIGHT
    cursor = x_design
    for character in text:
        glyph = glyphs.get(character) or glyphs.get("?")
        advance = advance_for(character, glyphs) * size_design
        if glyph is not None and not character.isspace():
            glyph_width = max(1.0, advance - size_design * 0.04)
            x = int(round((cursor - expansion_design) * scale_x))
            y = int(round((y_design - expansion_design) * scale_y))
            width = max(1, int(round((glyph_width + expansion_design * 2) * scale_x)))
            height = max(1, int(round((size_design + expansion_design * 2) * scale_y)))
            tile = atlas.crop(glyph.box).resize((width, height), Image.Resampling.NEAREST)
            target.alpha_composite(colorize(tile, color, alpha_scale), (x, y))
        cursor += advance


def render_text(
    target: Image.Image,
    atlas: Image.Image,
    glyphs: dict[str, Glyph],
    rect: Rect,
    text: str,
    size: float,
    color: tuple[int, int, int, int],
    alignment: str,
    wrapped: bool,
) -> dict[str, float | int | bool]:
    lines = wrap_lines(text, size, max(1.0, rect.width), glyphs) if wrapped else text.replace("\r", "").split("\n")
    line_height = size * 1.24
    block_height = len(lines) * line_height
    if alignment.startswith("middle"):
        y = rect.y + (rect.height - block_height) * 0.5
    elif alignment.startswith("lower"):
        y = rect.bottom - block_height
    else:
        y = rect.y
    max_line_width = 0.0
    for line_index, line in enumerate(lines):
        line_width = measure(line, size, glyphs)
        max_line_width = max(max_line_width, line_width)
        if alignment.endswith("center"):
            x = rect.x + (rect.width - line_width) * 0.5
        elif alignment.endswith("right"):
            x = rect.right - line_width
        else:
            x = rect.x
        render_line(
            target,
            atlas,
            glyphs,
            line,
            x,
            y + line_index * line_height,
            size,
            color,
            GLOW_EXPANSION,
            GLOW_INTENSITY * GLOW_ALPHA_SCALE,
        )
        render_line(target, atlas, glyphs, line, x, y + line_index * line_height, size, color, 0.0, 1.0)
    return {
        "line_count": len(lines),
        "measured_width_design": round(max_line_width, 2),
        "measured_height_design": round(block_height, 2),
        "fits_width": max_line_width <= rect.width + 0.01,
        "fits_height": block_height <= rect.height + 0.01,
    }


def pixel_rect(rect: Rect, width: int, height: int, padding: float = 0.0) -> tuple[int, int, int, int]:
    sx = width / DESIGN_WIDTH
    sy = height / DESIGN_HEIGHT
    return (
        max(0, int(math.floor((rect.x - padding) * sx))),
        max(0, int(math.floor((rect.y - padding) * sy))),
        min(width, int(math.ceil((rect.right + padding) * sx))),
        min(height, int(math.ceil((rect.bottom + padding) * sy))),
    )


def erase_runtime_text(frame: Image.Image, base: Image.Image) -> None:
    for _, rect, _, _, _, _ in TEXT_SPECS:
        box = pixel_rect(rect, frame.width, frame.height, padding=2.5)
        frame.paste(base.crop(box), box[:2])


def build_candidate_frame(
    resolution: tuple[int, int],
    runtime_path: Path,
    profile: Profile,
    atlas: Image.Image,
    glyphs: dict[str, Glyph],
) -> tuple[Image.Image, dict[str, dict[str, float | int | bool]]]:
    width, height = resolution
    frame = Image.open(runtime_path).convert("RGBA")
    if frame.size != resolution:
        raise ValueError(f"Unexpected runtime frame size {frame.size}: {runtime_path}")
    base = Image.open(COMMON_BASE).convert("RGBA").resize(resolution, Image.Resampling.BILINEAR)
    erase_runtime_text(frame, base)
    metrics: dict[str, dict[str, float | int | bool]] = {}
    for token, rect, text, color, alignment, wrapped in TEXT_SPECS:
        result = render_text(frame, atlas, glyphs, rect, text, profile.sizes[token], color, alignment, wrapped)
        if token not in metrics:
            metrics[token] = result
    return frame.convert("RGB"), metrics


def contain_nearest(image: Image.Image, size: tuple[int, int], background: tuple[int, int, int]) -> Image.Image:
    target_width, target_height = size
    scale = min(target_width / image.width, target_height / image.height)
    width = max(1, int(round(image.width * scale)))
    height = max(1, int(round(image.height * scale)))
    resized = image.resize((width, height), Image.Resampling.NEAREST)
    result = Image.new("RGB", size, background)
    result.paste(resized, ((target_width - width) // 2, (target_height - height) // 2))
    return result


def review_font(size: int) -> ImageFont.FreeTypeFont:
    return ImageFont.truetype(str(REVIEW_FONT_PATH), size)


def draw_review_board(
    source_resolution: tuple[int, int],
    reference: Image.Image,
    current: Image.Image,
    candidates: list[Image.Image],
    output_path: Path,
) -> None:
    canvas_width = 3840
    canvas_height = 2160
    margin = 54
    label_width = 300
    gap = 24
    header_height = 210
    footer_height = 92
    available_width = canvas_width - margin * 2 - label_width - gap * 4
    cell_width = available_width // 4
    row_heights = (280, 350, 560, 300, 290)
    background = (7, 14, 20)
    panel = (12, 27, 36)
    border = (67, 91, 103)
    amber = (255, 177, 61)
    teal = (90, 221, 205)
    muted = (154, 173, 181)
    canvas = Image.new("RGB", (canvas_width, canvas_height), background)
    draw = ImageDraw.Draw(canvas)
    draw.text((margin, 42), f"LED 字体校准板 V1 · {source_resolution[0]}×{source_resolution[1]} 真实缩放", font=review_font(54), fill=(238, 244, 245))
    draw.text((margin, 115), "批准参考只提供视觉目标；当前列来自实际截图；A / B 使用真实字形映射、同一坐标与 Point 采样规则离线重建。", font=review_font(27), fill=muted)

    column_titles = (
        ("批准预览", "视觉目标", amber),
        ("当前运行时", "最小灯珠 0.88px · 失败", (255, 104, 78)),
        (PROFILES[0].label, "最小灯珠 2.20px · 推荐", teal),
        (PROFILES[1].label, "最小灯珠 2.00px · 备选", (116, 185, 255)),
    )
    x0 = margin + label_width + gap
    for index, (title, subtitle, color) in enumerate(column_titles):
        x = x0 + index * (cell_width + gap)
        draw.rounded_rectangle((x, 146, x + cell_width, header_height), radius=12, fill=panel, outline=color, width=2)
        draw.text((x + 18, 154), title, font=review_font(27), fill=color)
        draw.text((x + 18, 191), subtitle, font=review_font(20), fill=muted)

    images = [reference, current, *candidates]
    y = header_height + 24
    for row_index, (name, note, crop_rect) in enumerate(SAMPLE_CROPS):
        row_height = row_heights[row_index]
        draw.rounded_rectangle((margin, y, margin + label_width - gap, y + row_height - gap), radius=12, fill=panel, outline=border, width=2)
        draw.text((margin + 24, y + 32), name, font=review_font(31), fill=(235, 241, 242))
        draw.multiline_text((margin + 24, y + 86), note, font=review_font(22), fill=muted, spacing=8)
        for column_index, source in enumerate(images):
            x = x0 + column_index * (cell_width + gap)
            box = pixel_rect(crop_rect, source.width, source.height)
            crop = source.crop(box).convert("RGB")
            cell = contain_nearest(crop, (cell_width, row_height - gap), (5, 12, 17))
            canvas.paste(cell, (x, y))
            draw.rounded_rectangle((x, y, x + cell_width, y + row_height - gap), radius=12, outline=border, width=2)
        y += row_height

    footer_y = canvas_height - footer_height
    draw.rectangle((0, footer_y, canvas_width, canvas_height), fill=(9, 22, 29))
    draw.text((margin, footer_y + 24), "校准资产仅位于 Assets/ArtSource；本次未替换 Resources 字库，也未改 Unity 运行代码。", font=review_font(24), fill=muted)
    canvas.save(output_path, optimize=True)


def render_token_bbox(
    profile: Profile,
    atlas: Image.Image,
    glyphs: dict[str, Glyph],
    token: str,
    text: str,
) -> tuple[int, int]:
    canvas = Image.new("RGBA", (DESIGN_WIDTH, DESIGN_HEIGHT), (0, 0, 0, 0))
    render_text(canvas, atlas, glyphs, Rect(4, 4, 1100, 120), text, profile.sizes[token], (255, 180, 60, 255), "upper_left", False)
    bbox = canvas.getchannel("A").getbbox()
    if bbox is None:
        return 0, 0
    return bbox[2] - bbox[0], bbox[3] - bbox[1]


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest().upper()


def build() -> None:
    require_inputs()
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    characters, advances = load_runtime_map()
    required_text = "".join(spec[2] for spec in TEXT_SPECS)
    missing_glyphs = sorted({character for character in required_text if not character.isspace() and character not in advances})
    built_fonts: dict[str, tuple[Image.Image, dict[str, Glyph]]] = {}
    frames: dict[str, dict[str, Image.Image]] = {"1920x1080": {}, "1280x720": {}}
    layout_metrics: dict[str, dict[str, dict[str, float | int | bool]]] = {}

    for profile in PROFILES:
        atlas, glyphs = build_candidate_font(profile, characters, advances)
        built_fonts[profile.key] = (atlas, glyphs)
        frame_1080, metrics_1080 = build_candidate_frame((1920, 1080), RUNTIME_1080, profile, atlas, glyphs)
        frame_720, metrics_720 = build_candidate_frame((1280, 720), RUNTIME_720, profile, atlas, glyphs)
        path_1080 = OUTPUT_DIR / f"led_font_{profile.key}_ready_1920x1080_v1.png"
        path_720 = OUTPUT_DIR / f"led_font_{profile.key}_ready_1280x720_v1.png"
        frame_1080.save(path_1080, optimize=True)
        frame_720.save(path_720, optimize=True)
        frames["1920x1080"][profile.key] = frame_1080
        frames["1280x720"][profile.key] = frame_720
        layout_metrics[profile.key] = {"1920x1080": metrics_1080, "1280x720": metrics_720}

    reference_1080 = Image.open(REFERENCE_1080).convert("RGB")
    reference_720 = reference_1080.resize((1280, 720), Image.Resampling.BILINEAR)
    current_1080 = Image.open(RUNTIME_1080).convert("RGB")
    current_720 = Image.open(RUNTIME_720).convert("RGB")
    review_1080 = OUTPUT_DIR / "led_font_calibration_board_v1_1920x1080_source_20260717.png"
    review_720 = OUTPUT_DIR / "led_font_calibration_board_v1_1280x720_source_20260717.png"
    draw_review_board(
        (1920, 1080),
        reference_1080,
        current_1080,
        [frames["1920x1080"][profile.key] for profile in PROFILES],
        review_1080,
    )
    draw_review_board(
        (1280, 720),
        reference_720,
        current_720,
        [frames["1280x720"][profile.key] for profile in PROFILES],
        review_720,
    )
    preview_1080 = OUTPUT_DIR / "led_font_calibration_board_v1_1920x1080_preview_20260717.png"
    preview_720 = OUTPUT_DIR / "led_font_calibration_board_v1_1280x720_preview_20260717.png"
    Image.open(review_1080).convert("RGB").resize((1920, 1080), Image.Resampling.LANCZOS).save(preview_1080, optimize=True)
    Image.open(review_720).convert("RGB").resize((1920, 1080), Image.Resampling.LANCZOS).save(preview_720, optimize=True)

    token_samples = {
        "hud_primary": "第1章",
        "hud_secondary": "生命--",
        "rail": "目标15",
        "terminal_title": "本关规则",
        "terminal_body": "无额外规则，检查基础骰袋。",
        "action": "SPACE 投掷",
        "micro": "拖动骰子",
    }
    thresholds = {
        "minimum_core_dot_px_at_1280x720": 2.0,
        "minimum_primary_active_height_px": 22,
        "minimum_body_active_height_px": 18,
    }
    report: dict[str, object] = {
        "version": "V1",
        "date": "2026-07-17",
        "status": "样张待验收",
        "runtime_changed": False,
        "reference": str(REFERENCE_1080.relative_to(ROOT)).replace("\\", "/"),
        "current_runtime": {
            "atlas": str(RUNTIME_ATLAS.relative_to(ROOT)).replace("\\", "/"),
            "map": str(RUNTIME_MAP.relative_to(ROOT)).replace("\\", "/"),
            "tile_px": 32,
            "source_dot_px": 2,
            "sizes": CURRENT_SIZES,
            "minimum_core_dot_px_at_1280x720": round(2 * min(CURRENT_SIZES.values()) / 32, 3),
            "result": "FAIL",
        },
        "thresholds": thresholds,
        "required_text_missing_glyphs": missing_glyphs,
        "profiles": {},
    }
    output_files: list[Path] = [review_1080, review_720, preview_1080, preview_720]
    for profile in PROFILES:
        atlas, glyphs = built_fonts[profile.key]
        token_metrics: dict[str, object] = {}
        profile_pass = not missing_glyphs
        for token, text in token_samples.items():
            width, height = render_token_bbox(profile, atlas, glyphs, token, text)
            core = profile.dot * profile.sizes[token] / profile.tile
            height_threshold = thresholds["minimum_primary_active_height_px"] if token in {"hud_primary", "action"} else thresholds["minimum_body_active_height_px"]
            token_pass = core >= thresholds["minimum_core_dot_px_at_1280x720"] and height >= height_threshold
            profile_pass = profile_pass and token_pass
            token_metrics[token] = {
                "logical_size": profile.sizes[token],
                "core_dot_px_at_1280x720": round(core, 3),
                "active_bbox_px_at_1280x720": [width, height],
                "pass": token_pass,
            }
        fits = all(
            bool(metric["fits_width"]) and bool(metric["fits_height"])
            for metric in layout_metrics[profile.key]["1280x720"].values()
        )
        profile_pass = profile_pass and fits
        report["profiles"][profile.key] = {
            "label": profile.label,
            "note": profile.note,
            "tile_px": profile.tile,
            "grid": [profile.grid, profile.grid],
            "cell_px": profile.cell,
            "source_dot_px": profile.dot,
            "sizes": profile.sizes,
            "tokens": token_metrics,
            "layout_fits_1280x720": fits,
            "missing_glyphs": missing_glyphs,
            "layout_metrics": layout_metrics[profile.key],
            "result": "PASS" if profile_pass else "FAIL",
        }
        output_files.extend(
            (
                OUTPUT_DIR / f"led_font_{profile.key}_atlas_v1.png",
                OUTPUT_DIR / f"led_font_{profile.key}_map_v1.csv",
                OUTPUT_DIR / f"led_font_{profile.key}_ready_1920x1080_v1.png",
                OUTPUT_DIR / f"led_font_{profile.key}_ready_1280x720_v1.png",
            )
        )

    report_path = OUTPUT_DIR / "led_font_calibration_metrics_v1_20260717.json"
    report["outputs"] = []
    for path in output_files:
        report["outputs"].append(
            {
                "path": str(path.relative_to(ROOT)).replace("\\", "/"),
                "bytes": path.stat().st_size,
                "sha256": sha256(path),
            }
        )
    report_path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(review_1080)
    print(review_720)
    print(report_path)


if __name__ == "__main__":
    build()
